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
        public static bool IsActive() => true;

        private bool _isSending = false;

        [PXOverride]
        public virtual IEnumerable FinishCounting(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseMethod)
        {
            var result = baseMethod(adapter); // Gọi action gốc trước

            if (_isSending)
                return result;

            try
            {
                _isSending = true;

                // 1. Lấy template cho màn hình IN305000
                var template = GetZaloTemplateForIN305000();
                if (template == null)
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("No Zalo template found for screen IN305000.");

                if (string.IsNullOrWhiteSpace(template.To))
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("Recipient (To) is empty in Zalo template.");
                if (string.IsNullOrWhiteSpace(template.Body))
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("Message Body is empty in Zalo template.");

                // 2. Nếu chưa có PreviewMessage thì merge nội dung
                if (string.IsNullOrWhiteSpace(template.PreviewMessage))
                {
                    var maintGraph = PXGraph.CreateInstance<ZaloTemplateMaint>();
                    // Gọi hàm private MergeInventoryReviewMessage qua Reflection
                    var mergeMethod = typeof(ZaloTemplateMaint)
                        .GetMethod("MergeInventoryReviewMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (mergeMethod == null)
                        // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                        throw new PXException("MergeInventoryReviewMessage method not found in ZaloTemplateMaint.");

                    template.PreviewMessage = (string)mergeMethod.Invoke(maintGraph, new object[]
                    {
                        template.Body ?? string.Empty,
                        template.ReferenceNbr?.ToString() ?? string.Empty
                    });

                    Base.Caches[typeof(ZaloTemplate)].Update(template);
                }

                // 3. Lấy danh sách người nhận bằng SplitRecipients
                var maintGraphForRecipients = PXGraph.CreateInstance<ZaloTemplateMaint>();
                var splitMethod = typeof(ZaloTemplateMaint)
                    .GetMethod("SplitRecipients", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (splitMethod == null)
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("SplitRecipients method not found in ZaloTemplateMaint.");

                var allRecipients = (List<string>)splitMethod.Invoke(maintGraphForRecipients, new object[] { new string[] { template.To, template.Cc, template.Bcc } });

                allRecipients = allRecipients.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                // 4. Gửi tin nhắn
                var sbResult = new StringBuilder();
                bool allSuccess = true;

                var accessToken = ZaloTemplateMaint.RefreshZaloToken();
                if (string.IsNullOrEmpty(accessToken))
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("Không thể lấy access token từ Zalo");

                foreach (string userID in allRecipients)
                {
                    try
                    {
                        PXTrace.WriteInformation($"📤 Sending Zalo message to: {userID}");
                        string sendResult = ZaloApiService.SendMessage(accessToken, userID, template.PreviewMessage);
                        sbResult.AppendLine($"- {userID}: {sendResult}");
                        if (!sendResult.StartsWith("Success", StringComparison.OrdinalIgnoreCase))
                            allSuccess = false;
                    }
                    catch (Exception ex)
                    {
                        sbResult.AppendLine($"❌ {userID}: Error - {ex.Message}");
                        allSuccess = false;
                        PXTrace.WriteError(ex);
                    }
                }

                Base.Caches[typeof(ZaloTemplate)].Update(template);
                Base.Actions.PressSave();

                PXTrace.WriteInformation(allSuccess
                    ? "✅ All Zalo messages sent successfully."
                    : "❌ Some Zalo messages failed to send.");
            }
            catch (Exception ex)
            {
                PXTrace.WriteError($"Error sending Zalo message: {ex.Message}");
            }
            finally
            {
                _isSending = false;
            }

            return result;
        }

        /// <summary>
        /// Lấy template Zalo đang active cho màn hình IN305000
        /// </summary>
        private ZaloTemplate GetZaloTemplateForIN305000()
        {
            var result = PXSelect<ZaloTemplate,
                Where<ZaloTemplate.screen, Equal<Required<ZaloTemplate.screen>>>>
                .Select(Base, "IN305000");

            var template = result.RowCast<ZaloTemplate>().FirstOrDefault();

            if (template != null)
            {
                PXTrace.WriteInformation($"[Zalo Template] NotificationID: {template.NotificationID}");
                PXTrace.WriteInformation($"[Zalo Template] To: {template.To}");
                PXTrace.WriteInformation($"[Zalo Template] Body: {template.Body}");
                PXTrace.WriteInformation($"[Zalo Template] PreviewMessage: {template.PreviewMessage}");
            }

            return template;
        }
    }
}