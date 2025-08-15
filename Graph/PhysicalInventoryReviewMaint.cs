using AnNhienCafe;
using PX.Data;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnNhienCafe
{
    public class INPIReview_Extension : PXGraphExtension<INPIReview>
    {
        public static bool IsActive()
        {
            return true;
        }

        private bool _isSending = false;

        [PXOverride]
        public virtual IEnumerable FinishCounting(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseMethod)
        {
            var result = baseMethod(adapter); // Gọi action gốc

            try
            {
                if (!_isSending)
                {
                    _isSending = true;

                    // Lấy template đã setup cho IN305000
                    var template = GetZaloTemplateForIN305000();
                    if (template == null)
                        // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                        throw new PXException("No Zalo template found for screen IN305000.");

                    if (string.IsNullOrEmpty(template.To))
                        // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                        throw new PXException("Recipient (To) UserID is empty in Zalo template. Please configure recipients in the Zalo Template screen.");

                    if (string.IsNullOrWhiteSpace(template.Body))
                        // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                        throw new PXException("Message Body is empty in Zalo template. Please configure the Body in the Zalo Template screen.");

                    // Lấy access token
                    var tokenGraph = PXGraph.CreateInstance<ZaloTemplateMaint>();
                    var accessToken = tokenGraph.RefreshZaloToken();
                    if (string.IsNullOrEmpty(accessToken))
                        // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                        throw new PXException("Failed to obtain Zalo access token.");

                    // Nếu chưa có PreviewMessage thì merge nội dung
                    if (string.IsNullOrWhiteSpace(template.PreviewMessage))
                    {
                        // Sử dụng MergeInventoryReviewMessage từ ZaloTemplateMaint
                        var maintGraph = PXGraph.CreateInstance<ZaloTemplateMaint>();
                        template.PreviewMessage = maintGraph.MergeInventoryReviewMessage(template.Body, template.ReferenceNbr);
                        Base.Caches[typeof(ZaloTemplate)].Update(template);
                    }

                    // Lấy tất cả người nhận từ To, Cc, Bcc
                    var maintGraphForRecipients = PXGraph.CreateInstance<ZaloTemplateMaint>();
                    var allRecipients = maintGraphForRecipients.SplitRecipients(template.To, template.Cc, template.Bcc)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var sbResult = new StringBuilder();
                    sbResult.AppendLine("📤 KẾT QUẢ GỬI TIN NHẮN:");
                    sbResult.AppendLine($"⏰ Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                    sbResult.AppendLine($"📝 Nội dung: {(template.PreviewMessage.Length > 50 ? template.PreviewMessage.Substring(0, 50) + "..." : template.PreviewMessage)}");
                    sbResult.AppendLine($"👥 Số người nhận: {allRecipients.Count}");
                    sbResult.AppendLine("");

                    bool allSuccess = true;
                    int successCount = 0;
                    int failCount = 0;

                    foreach (string userID in allRecipients)
                    {
                        try
                        {
                            PXTrace.WriteInformation($"📤 Đang gửi tin nhắn đến ZaloUserID: {userID}");
                            string sendResult = ZaloApiService.SendTextMessage(accessToken, userID, template.PreviewMessage);
                            if (ZaloApiService.IsSuccessResponse(sendResult))
                            {
                                sbResult.AppendLine($"✅ {userID}: Gửi thành công");
                                successCount++;
                            }
                            else
                            {
                                sbResult.AppendLine($"❌ {userID}: {sendResult}");
                                allSuccess = false;
                                failCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            sbResult.AppendLine($"❌ {userID}: Lỗi - {ex.Message}");
                            allSuccess = false;
                            failCount++;
                            PXTrace.WriteError($"❌ Error sending to {userID}: {ex.Message}");
                        }
                    }

                    // Cập nhật kết quả vào database
                    sbResult.AppendLine("");
                    sbResult.AppendLine($"📊 TỔNG KẾT:");
                    sbResult.AppendLine($"✅ Thành công: {successCount}");
                    sbResult.AppendLine($"❌ Thất bại: {failCount}");

                    template.Result = sbResult.ToString();
                    template.Status = allSuccess ? "Sent" : (successCount > 0 ? "Partial" : "Failed");
                    Base.Caches[typeof(ZaloTemplate)].Update(template);
                    Base.Actions.PressSave();

                    if (allSuccess)
                        PXTrace.WriteInformation("✅ All messages sent successfully!");
                    else
                        PXTrace.WriteError("❌ Some messages failed to send.");
                }
            }
            catch (Exception ex)
            {
                PXUIFieldAttribute.SetError<ZaloTemplate.result>(
                    Base.Caches[typeof(ZaloTemplate)],
                    null,
                    $"Error sending Zalo message: {ex.Message}"
                );
            }
            finally
            {
                _isSending = false;
            }

            return result;
        }

        /// <summary>
        /// Lấy Zalo template theo màn hình IN305000, tương tự ZaloTemplateMaint
        /// </summary>
        private ZaloTemplate GetZaloTemplateForIN305000()
        {
            // Truy vấn template cho màn hình IN305000, tương tự ZaloTemplateMaint
            var result = PXSelect<ZaloTemplate,
                Where<ZaloTemplate.screen, Equal<Required<ZaloTemplate.screen>>,
                    And<ZaloTemplate.isActive, Equal<True>>>,
                OrderBy<Desc<ZaloTemplate.createdDateTime>>>
                .Select(Base, "IN305000");
            var template = result.RowCast<ZaloTemplate>().FirstOrDefault();

            if (template != null)
            {
                // Log thông tin template, giống ZaloTemplateMaint
                PXTrace.WriteInformation($"[Zalo Template] NotificationID: {template.NotificationID}");
                PXTrace.WriteInformation($"[Zalo Template] Screen: {template.Screen}");
                PXTrace.WriteInformation($"[Zalo Template] To: {template.To}");
                PXTrace.WriteInformation($"[Zalo Template] Cc: {template.Cc}");
                PXTrace.WriteInformation($"[Zalo Template] Bcc: {template.Bcc}");
                PXTrace.WriteInformation($"[Zalo Template] Body: {template.Body}");
                PXTrace.WriteInformation($"[Zalo Template] PreviewMessage: {template.PreviewMessage}");
                PXTrace.WriteInformation($"[Zalo Template] ReferenceNbr: {template.ReferenceNbr}");
                PXTrace.WriteInformation($"[Zalo Template] Result: {template.Result}");
                PXTrace.WriteInformation($"[Zalo Template] Status: {template.Status}");
            }

            return template;
        }
    }
}