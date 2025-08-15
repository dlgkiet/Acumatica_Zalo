using PX.Common;
using PX.Data;
using PX.Metadata;
using PX.Objects.IN;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Compilation;

namespace AnNhienCafe
{
    [PXCacheName("Zalo Message Templates")]
    public class ZaloTemplateMaint : PXGraph<ZaloTemplateMaint, ZaloTemplate>
    {
        [PXViewName("Templates")]
        public PXSelect<ZaloTemplate> Templates;
        public PXSelect<ZaloTemplate, Where<ZaloTemplate.notificationID, Equal<Current<ZaloTemplate.notificationID>>>> CurrentNotification;
        public PXSelect<INPIHeader, Where<INPIHeader.pIID, Equal<Current<ZaloTemplate.referenceNbr>>>> PIHeader;

        public PXSelect<EntityItem> EntityItems;

        public IEnumerable entityItems()
        {
            var current = Templates.Current; // Hoặc CurrentNotification.Current
            if (current == null || string.IsNullOrEmpty(current.Screen))
                yield break;

            var info = PX.Api.ScreenUtils.ScreenInfo.TryGet(current.Screen);
            if (info == null)
                yield break;

            // Lấy Graph instance
            var graphType = PXBuildManager.GetType(info.GraphName, false);
            if (graphType == null)
                yield break;

            var graph = (PXGraph)Activator.CreateInstance(graphType);

            // Lấy view chính
            var view = graph.Views[info.PrimaryView];
            var cache = view.Cache;

            foreach (var field in cache.Fields)
            {
                var displayName = PXUIFieldAttribute.GetDisplayName(cache, field) ?? field;
                yield return new EntityItem
                {
                    Key = field,
                    Name = displayName,
                    Path = $"[{field}]",
                    Icon = "Doc"
                };
            }
        }

        #region Actions
        #region Show Preview Message
        public PXAction<ZaloTemplate> showPreview;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Show Preview Message", MapEnableRights = PXCacheRights.Select)]
        protected virtual IEnumerable ShowPreview(PXAdapter adapter)
        {
            ZaloTemplate current = Templates.Current;

            if (current == null || string.IsNullOrWhiteSpace(current.Body))
            {
                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                throw new PXException("Vui lòng nhập nội dung tin nhắn trong Body (tab Message) trước khi xem Preview.");
            }

            if (string.IsNullOrEmpty(current.ReferenceNbr))
            {
                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                throw new PXException("Vui lòng chọn Reference Number trước khi xem Preview.");
            }

            try
            {
                PXTrace.WriteInformation("🔍 Đang tạo Preview Message từ Body...");

                // Merge Body với dữ liệu, không lưu DB
                string previewMessage = MergeInventoryReviewMessage(current.Body, current.ReferenceNbr);

                // Set giá trị preview lên cache (chỉ UI)
                Templates.Cache.SetValueExt<ZaloTemplate.previewMessage>(current, previewMessage);

                // Refresh UI để hiển thị preview trên edPreviewMessage
                Templates.View.RequestRefresh();

                PXTrace.WriteInformation("✅ Preview message được tạo thành công từ Body và hiển thị trên UI (chưa lưu DB)");
            }
            catch (Exception ex)
            {
                PXTrace.WriteError($"❌ Lỗi khi merge và hiển thị preview: {ex.Message}\nStackTrace: {ex.StackTrace}");
                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                throw new PXException($"Lỗi tạo preview từ Body: {ex.Message}. Vui lòng kiểm tra log để biết chi tiết.");
            }

            // Di chuyển Ask ra đây: Framework sẽ xử lý exception dialog tự nhiên (hiển thị popup info, không log error)
            // Nếu không muốn popup, comment khối này
            Templates.Ask(
                "📋 Xem Trước Tin Nhắn",
                "Tin nhắn từ Body đã được merge và hiển thị ở trường Preview Message trên UI. \n\n" +
                "Lưu ý: Tin nhắn này chưa được lưu vào database. Nếu muốn gửi, nhấn 'Send Zalo Message'.",
                MessageButtons.OK
            );

            return adapter.Get();
        }
        #endregion

        #region Send Zalo Message
        public PXAction<ZaloTemplate> sendZaloMessage;
        [PXButton(CommitChanges = true)] 
        [PXUIField(DisplayName = "Send Zalo Message", MapEnableRights = PXCacheRights.Select)]
        protected virtual IEnumerable SendZaloMessage(PXAdapter adapter)
        {
            if (_isSending)
                return adapter.Get();

            _isSending = true;
            try
            {
                var current = Templates.Current;

                if (current == null)
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("Vui lòng chọn template để gửi");

                if (string.IsNullOrEmpty(current.To))
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("Vui lòng nhập Zalo User ID trong trường 'To Users'");

                if (string.IsNullOrWhiteSpace(current.Body))
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("Vui lòng nhập nội dung tin nhắn trong Body");

                PXTrace.WriteInformation("🚀 Bắt đầu quá trình gửi tin nhắn Zalo...");

                // 1. Merge message và LUU VÀO DATABASE
                string mergedMessage = MergeInventoryReviewMessage(current.Body, current.ReferenceNbr);
                current.PreviewMessage = mergedMessage;
                Templates.Cache.Update(current); // Lưu vào cache

                PXTrace.WriteInformation("✅ Tin nhắn đã được merge và lưu vào database");

                // 2. Lấy access token
                var accessToken = RefreshZaloToken();
                if (string.IsNullOrEmpty(accessToken))
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("Không thể lấy access token từ Zalo");

                // 3. Gửi tin nhắn
                var allRecipients = SplitRecipients(current.To, current.Cc, current.Bcc)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var sbResult = new StringBuilder();
                sbResult.AppendLine("📤 KẾT QUẢ GỬI TIN NHẮN:");
                sbResult.AppendLine($"⏰ Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                sbResult.AppendLine($"📝 Nội dung: {(mergedMessage.Length > 50 ? mergedMessage.Substring(0, 50) + "..." : mergedMessage)}");
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
                        string response = ZaloApiService.SendTextMessage(accessToken, userID, mergedMessage);

                        if (ZaloApiService.IsSuccessResponse(response))
                        {
                            sbResult.AppendLine($"✅ {userID}: Gửi thành công");
                            successCount++;
                        }
                        else
                        {
                            sbResult.AppendLine($"❌ {userID}: {response}");
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

                // 4. Cập nhật kết quả vào database
                sbResult.AppendLine("");
                sbResult.AppendLine($"📊 TỔNG KẾT:");
                sbResult.AppendLine($"✅ Thành công: {successCount}");
                sbResult.AppendLine($"❌ Thất bại: {failCount}");

                current.Result = sbResult.ToString();
                current.Status = allSuccess ? "Sent" : (successCount > 0 ? "Partial" : "Failed");
                Templates.Cache.Update(current);

                // 5. Lưu vào database
                this.Actions.PressSave();

                PXTrace.WriteInformation($"✅ Hoàn tất gửi tin nhắn. Thành công: {successCount}, Thất bại: {failCount}");

                // 6. Thông báo kết quả cho user
                string title = allSuccess ? "🎉 GỬI THÀNH CÔNG" :
                              (successCount > 0 ? "⚠️ GỬI THÀNH CÔNG MỘT PHẦN" : "❌ GỬI THẤT BẠI");

                // Nếu user bấm OK -> chỉ return adapter.Get(), không chạy lại action gửi
                if (Templates.Ask(title, sbResult.ToString(), MessageButtons.OK) == WebDialogResult.OK)
                {
                    return adapter.Get();
                }
            }
            catch (PXException)
            {
                throw;
            }
            catch (Exception ex)
            {
                PXTrace.WriteError($"❌ Send message error: {ex.Message}");
                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                throw new PXException($"Lỗi gửi tin nhắn Zalo: {ex.Message}");
            }
            finally
            {
                _isSending = false;
            }

            return adapter.Get();
        }
        #endregion

        #endregion

        #region Helper Methods

        private bool _isSending = false;

        public string RefreshZaloToken()
        {
            try
            {
                var tokenGraph = PXGraph.CreateInstance<ZaloTokenMaint>();
                var zaloToken = PXSelect<ZaloToken>.SelectSingleBound(this, null);

                if (zaloToken == null)
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("Không tìm thấy Zalo Token configuration. Vui lòng cấu hình Zalo Token trước.");

                var token = (ZaloToken)zaloToken;

                PXTrace.WriteInformation("🔄 Đang refresh Zalo access token...");
                string response = ZaloApiService.RefreshToken(token.AppID, token.AppSecret, token.RefreshToken);
                string newAccessToken = ZaloApiService.ParseAccessToken(response);

                if (string.IsNullOrEmpty(newAccessToken))
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("Không thể parse access token từ response API");

                tokenGraph.ZaloToken.Current = token;
                tokenGraph.UpdateTokenFromResponse(token, response);
                tokenGraph.ZaloToken.Update(token);
                tokenGraph.Actions.PressSave();

                PXTrace.WriteInformation("✅ Access token đã được refresh thành công");
                return newAccessToken;
            }
            catch (Exception ex)
            {
                PXTrace.WriteError($"❌ Refresh token error: {ex.Message}");
                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                throw new PXException($"Lỗi refresh Zalo token: {ex.Message}");
            }
        }

        private List<string> ParseZaloUserIds(string userIdsString)
        {
            if (string.IsNullOrEmpty(userIdsString))
                return new List<string>();

            return userIdsString
                .Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();
        }

        public List<string> SplitRecipients(params string[] fields)
        {
            var all = new List<string>();
            foreach (var field in fields)
            {
                if (string.IsNullOrWhiteSpace(field)) continue;
                var items = field.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(x => x.Trim())
                                 .Where(x => !string.IsNullOrWhiteSpace(x));
                all.AddRange(items);
            }
            return all.Distinct().ToList();
        }

        public string MergeInventoryReviewMessage(string templateBody, string referenceNbr)
        {
            try
            {
                PXTrace.WriteInformation($"🔄 Đang merge template với Reference Nbr: {referenceNbr}");

                INPIHeader pi = PXSelect<INPIHeader,
                    Where<INPIHeader.pIID, Equal<Required<INPIHeader.pIID>>>>
                    .Select(this, referenceNbr);

                if (pi == null)
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException($"Không tìm thấy phiếu kiểm kê với mã PIID = {referenceNbr}");

                INSite site = PXSelect<INSite,
                    Where<INSite.siteID, Equal<Required<INSite.siteID>>>>
                    .Select(this, pi.SiteID);

                string chiNhanhTen = site != null ? $"{site.SiteCD}" : pi.SiteID?.ToString() ?? "Không rõ";

                Users user = PXSelect<Users,
                    Where<Users.pKID, Equal<Required<Users.pKID>>>>
                    .Select(this, pi.CreatedByID);

                string nguoiKiemKe = user?.FullName ?? "Không rõ";

                var chiTietList = new List<ChenhlechItem>();
                foreach (INPIDetail detail in PXSelect<INPIDetail,
                    Where<INPIDetail.pIID, Equal<Required<INPIDetail.pIID>>>>
                    .Select(this, pi.PIID))
                {
                    if (detail.InventoryID == null) continue;
                    InventoryItem item = PXSelect<InventoryItem,
                        Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                        .Select(this, detail.InventoryID);

                    string tenSP = item?.InventoryCD ?? $"ID {detail.InventoryID}";
                    int bookQty = (int)Math.Round(detail.BookQty ?? 0);
                    int thucTe = (int)Math.Round(detail.PhysicalQty ?? 0);
                    int chenhlech = (int)Math.Round(detail.VarQty ?? 0);
                    decimal tienChenh = detail.ExtVarCost ?? 0m;

                    chiTietList.Add(new ChenhlechItem
                    {
                        TenSP = tenSP,
                        SoSach = bookQty,
                        ThucTe = thucTe,
                        ChenhLech = chenhlech,
                        TienChenhLech = $"{(tienChenh >= 0 ? "" : "-")}{string.Format("{0:#,0}", Math.Abs(tienChenh))} đ"
                    });
                }

                string formattedChiTiet = ZaloMessageBuilder.FormatChenhlechLines(chiTietList);
                int tongQty = (int)(pi.TotalVarQty ?? 0);
                int tongTien = (int)(pi.TotalVarCost ?? 0);
                string tongText = $"{tongQty} sản phẩm ({ZaloMessageBuilder.FormatTien(tongTien)})";

                string mergedMessage = ZaloMessageBuilder.BuildMessage(templateBody, new
                {
                    ChiNhanh = chiNhanhTen,
                    NgayKiemKe = pi.CreatedDateTime?.ToString("dd/MM/yyyy") ?? "",
                    NguoiKiemKe = nguoiKiemKe,
                    SoPhieu = pi.PIID,
                    TongChenhlech = tongText,
                    ChiTietChenhlech = formattedChiTiet
                });

                PXTrace.WriteInformation($"✅ Template đã được merge thành công. Độ dài: {mergedMessage.Length} ký tự");
                return mergedMessage;
            }
            catch (Exception ex)
            {
                PXTrace.WriteError($"❌ Error in MergeInventoryReviewMessage: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Event Handlers

        protected virtual void ZaloTemplate_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            ZaloTemplate row = e.Row as ZaloTemplate;
            if (row == null) return;

            // Luôn enable các nút Show Preview và Send Zalo Message
            showPreview.SetEnabled(true);
            sendZaloMessage.SetEnabled(true);
        }

        protected virtual void ZaloTemplate_Body_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            // Clear preview message khi user thay đổi body
            ZaloTemplate row = e.Row as ZaloTemplate;
            if (row != null && !string.IsNullOrEmpty(row.PreviewMessage))
            {
                // Chỉ clear preview trên UI, không update DB
                sender.SetValueExt<ZaloTemplate.previewMessage>(row, null);
            }
        }

        protected virtual void ZaloTemplate_ReferenceNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            // Clear preview message khi user thay đổi reference number
            ZaloTemplate row = e.Row as ZaloTemplate;
            if (row != null && !string.IsNullOrEmpty(row.PreviewMessage))
            {
                // Chỉ clear preview trên UI, không update DB
                sender.SetValueExt<ZaloTemplate.previewMessage>(row, null);
            }
        }

        #endregion
    }

    public class ChenhlechItem
    {
        public string TenSP { get; set; }
        public int SoSach { get; set; }
        public int ThucTe { get; set; }
        public int ChenhLech { get; set; }
        public string TienChenhLech { get; set; }
    }
}