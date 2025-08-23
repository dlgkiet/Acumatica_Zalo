using PX.Common;
using PX.Data;
using PX.Data.Licensing;
using PX.Metadata;
using PX.Objects.IN;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Compilation;
using PX.Objects.CR;

namespace AnNhienCafe
{
    [PXCacheName("Zalo Message Templates")]
    public class ZaloTemplateMaint : PXGraph<ZaloTemplateMaint>
    {
        public PXSave<ZaloTemplate> Save;
        public PXCancel<ZaloTemplate> Cancel;

        public PXSelect<ZaloTemplate> Templates;
        public PXSelect<ZaloTemplate, Where<ZaloTemplate.notificationID, Equal<Current<ZaloTemplate.notificationID>>>> CurrentNotification;
        public PXSelect<INPIHeader, Where<INPIHeader.pIID, Equal<Current<ZaloTemplate.referenceNbr>>>> PIHeader;
        public PXSelect<SiteMap> DummySiteMap;

        public PXSelect<EntityItem> EntityItems;
        public IEnumerable entityItems()
        {
            var result = new List<EntityItem>();

            var current = Templates.Current;
            if (current == null || string.IsNullOrEmpty(current.Screen))
                return result;

            var info = PX.Api.ScreenUtils.ScreenInfo.TryGet(current.Screen);
            if (info == null)
                return result;

            var graphType = PXBuildManager.GetType(info.GraphName, false);
            if (graphType == null)
                return result;

            var graph = (PXGraph)Activator.CreateInstance(graphType);

            var systemAdded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in info.Views)
            {
                var viewName = kv.Key;
                if (viewName.StartsWith("$", StringComparison.OrdinalIgnoreCase)
                || viewName.StartsWith("_CACHE#", StringComparison.OrdinalIgnoreCase)
                || viewName.StartsWith("_", StringComparison.OrdinalIgnoreCase))
                    continue;

                var layoutFields = (kv.Value ?? Array.Empty<string>())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (!graph.Views.ContainsKey(viewName))
                    continue;

                var cache = graph.Views[viewName]?.Cache;
                if (cache == null)
                    continue;

                var dacType = cache.GetItemType();
                var dacName = dacType?.Name ?? string.Empty;

                // Node cha = View name
                result.Add(new EntityItem
                {
                    Key = viewName,
                    Name = viewName,
                    Path = viewName,
                    Icon = "Folder",
                    ParentKey = null
                });

                // Chọn field để hiển thị: ưu tiên layout, nếu trống thì lấy DAC
                var fieldsToProcess = layoutFields.Count > 0 ? layoutFields : cache.Fields.ToList();

                var addedInThisView = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var field in fieldsToProcess)
                {
                    if (!string.IsNullOrEmpty(dacName) &&
                        string.Equals(field, dacName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!addedInThisView.Add(field))
                        continue;

                    var uiAttr = cache.GetAttributes(field).OfType<PXUIFieldAttribute>().FirstOrDefault();
                    var displayName = uiAttr?.DisplayName ?? field;

                    if (IsSystemField(field))
                    {
                        // Bỏ qua ở đây, vì system field sẽ được ép add riêng ở root
                        continue;
                    }

                    // Field thường → con của view
                    result.Add(new EntityItem
                    {
                        Key = $"{viewName}.{field}",
                        Name = displayName,
                        Path = $"[{field}]",
                        Icon = "Doc",
                        ParentKey = viewName
                    });
                }

                // ✅ Ép add toàn bộ system field của DAC này vào root
                foreach (var sysField in cache.Fields.Where(IsSystemField))
                {
                    if (systemAdded.Add(sysField))
                    {
                        var uiAttr = cache.GetAttributes(sysField).OfType<PXUIFieldAttribute>().FirstOrDefault();
                        var displayName = uiAttr?.DisplayName ?? sysField;

                        string extraInfo = null;

                        // Nếu là CreatedByID thì lookup Users
                        if (string.Equals(sysField, "CreatedByID", StringComparison.OrdinalIgnoreCase))
                        {
                            Guid? createdBy = (Guid?)cache.GetValue(cache.Current, sysField);
                            if (createdBy != null)
                            {
                                Users user = PXSelect<Users,
                                    Where<Users.pKID, Equal<Required<Users.pKID>>>>
                                    .Select(graph, createdBy);
                                extraInfo = user?.FullName;
                            }
                        }
                        result.Add(new EntityItem
                        {
                            Key = sysField,
                            Name = string.IsNullOrEmpty(extraInfo)
                                        ? displayName
                                        : $"{displayName} ({extraInfo})",
                            Path = $"[{sysField}]",
                            Icon = "Doc",
                            ParentKey = null
                        });
                    }
                }
            }

            return result;
        }

        private static bool IsSystemField(string fieldName)
        {
            string[] systemFields =
            {
                "CreatedByID", "CreatedDateTime", "CreatedByScreenID",
                "LastModifiedByID", "LastModifiedDateTime", "LastModifiedByScreenID",
                "NoteID", "tstamp"
            };
            return systemFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase);
        }

        public PXAction<ZaloTemplate> PreviewMess;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Show Message Preview")]
        protected virtual IEnumerable previewMess(PXAdapter adapter)
        {
            ZaloTemplate template = Templates.Current;
            if (template == null || string.IsNullOrWhiteSpace(template.Body))
                return adapter.Get();
            try
            {
                object currentRecord = null;
                PXCache cache = null;
                // 1. Lấy ScreenInfo từ screen trong template
                var info = PX.Api.ScreenUtils.ScreenInfo.TryGet(template.Screen);
                if (info != null)
                {
                    var graphType = PXBuildManager.GetType(info.GraphName, false);
                    if (graphType != null)
                    {
                        var graph = (PXGraph)PXGraph.CreateInstance(graphType);
                        // 2. Lấy Primary DAC của screen
                        var primaryDAC = graph.PrimaryItemType;
                        if (primaryDAC != null)
                        {
                            cache = graph.Caches[primaryDAC];
                            currentRecord = cache.Current;
                            // 3. Nếu Current null → tự lấy record mới nhất
                            if (currentRecord == null)
                            {
                                var view = new PXView(
                                    graph,
                                    true,
                                    BqlCommand.CreateInstance(typeof(Select<>).MakeGenericType(primaryDAC))
                                );
                                var records = view.SelectMulti();
                                if (records != null && records.Count > 0)
                                {
                                    // Nếu DAC có field CreatedDateTime thì chọn record mới nhất
                                    if (cache.Fields.Contains("CreatedDateTime"))
                                    {
                                        currentRecord = records
                                            .OfType<object>()
                                            .OrderByDescending(r =>
                                                cache.GetValue(r, "CreatedDateTime") as DateTime?
                                            )
                                            .FirstOrDefault();
                                    }
                                    else
                                    {
                                        // fallback: lấy record đầu tiên
                                        currentRecord = records[0];
                                    }
                                }
                            }
                        }
                    }
                }
                // 4. Merge body
                string mergedHtml = template.Body;
                if (currentRecord != null && cache != null)
                    mergedHtml = MergeRecordIntoBody(cache, currentRecord, template.Body);
                // 5. Chuẩn hoá xuống dòng và strip HTML
                // Replace xuống dòng cho các block tag
                mergedHtml = Regex.Replace(mergedHtml, @"<(br|BR)\s*/?>", "\n", RegexOptions.IgnoreCase);
                mergedHtml = Regex.Replace(mergedHtml, @"</(p|div|h[1-6]|li)>", "\n\n", RegexOptions.IgnoreCase);
                // Xoá script, style
                mergedHtml = Regex.Replace(mergedHtml, "<script.*?>.*?</script>", string.Empty,
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);
                mergedHtml = Regex.Replace(mergedHtml, "<style.*?>.*?</style>", string.Empty,
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);
                // ❌ Xoá comment
                mergedHtml = Regex.Replace(mergedHtml, @"<!--.*?-->", string.Empty,
                    RegexOptions.Singleline);
                mergedHtml = Regex.Replace(mergedHtml, @"/\*.*?\*/", string.Empty,
                    RegexOptions.Singleline);
                // Bỏ tag HTML còn lại
                string mergedText = Regex.Replace(mergedHtml, "<.*?>", string.Empty);
                // Decode HTML entities
                mergedText = System.Net.WebUtility.HtmlDecode(mergedText);
                // Chuẩn hoá newline về \n
                mergedText = mergedText.Replace("\r\n", "\n").Replace("\r", "\n");
                // Trim khoảng trắng thừa
                mergedText = Regex.Replace(mergedText, @"[ \t]+\n", "\n");   // xoá space trước newline
                mergedText = Regex.Replace(mergedText, @"\n{3,}", "\n\n");   // gộp >2 dòng trống về 2
                
                // 6. Save preview
                Templates.Cache.SetValueExt<ZaloTemplate.previewMessage>(template, mergedText);
                Templates.Cache.Update(template);
                PXTrace.WriteInformation("Preview plain text: " + mergedText);
            }
            catch (Exception ex)
            {
                PXTrace.WriteError(ex);
                throw;
            }
            return adapter.Get();
        }
        private string MergeRecordIntoBody(PXCache cache, object record, string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return body;
            string result = body;
            var graph = cache.Graph;
            var info = PX.Api.ScreenUtils.ScreenInfo.TryGet(Templates.Current.Screen);
            if (info != null)
            {
                foreach (var kv in info.Views)
                {
                    var viewName = kv.Key;
                    if (!graph.Views.ContainsKey(viewName))
                        continue;
                    var view = graph.Views[viewName];
                    var viewCache = view.Cache;
                    try
                    {
                        var records = view.SelectMulti();
                        if (records != null && records.Count > 0)
                        {
                            // Tìm dòng nào trong body có chứa field detail
                            var lines = result.Split('\n');
                            var newLines = new List<string>();
                            foreach (var line in lines)
                            {
                                bool isDetailLine = viewCache.Fields.Any(f => line.Contains($"[{f}]"));
                                if (isDetailLine)
                                {
                                    // Nhân bản dòng này cho tất cả records
                                    foreach (var rec in records)
                                    {
                                        string section = line;
                                        foreach (string field in viewCache.Fields)
                                        {
                                            var state = viewCache.GetStateExt(rec, field) as PXFieldState;
                                            string value = state?.Value?.ToString() ?? string.Empty;
                                            section = section.Replace($"[{field}]", value);
                                        }
                                        newLines.Add(section);
                                    }
                                }
                                else
                                {
                                    newLines.Add(line);
                                }
                            }
                            result = string.Join("\n", newLines);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            return result;
        }

        private bool _isSending = false;

        public PXAction<ZaloTemplate> SendZaloMessage;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Send Zalo Message")]
        public virtual IEnumerable sendZaloMessage(PXAdapter adapter)
        {
            if (_isSending)
                return adapter.Get();

            _isSending = true;

            try
            {
                var template = Templates.Current;
                if (template == null)
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("No template selected.");

                if (string.IsNullOrEmpty(template.To))
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("Please enter the recipient (To) UserID.");

                try
                {
                    // 1. Tái sử dụng PreviewMess để merge message trước khi gửi
                    previewMess(adapter);

                    // Reload lại template sau khi update PreviewMessage
                    template = Templates.Current;
                    PXTrace.WriteInformation("Reloaded Template PreviewMessage: " + (template.PreviewMessage ?? "null"));

                    var accessToken = RefreshZaloToken();
                    if (string.IsNullOrEmpty(accessToken))
                        // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                        throw new PXException("Không thể lấy access token từ Zalo");

                    // 2. Lấy danh sách recipients
                    var allRecipients = SplitRecipients(template.To, template.Cc, template.Bcc)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    PXTrace.WriteInformation("Total recipients: " + allRecipients.Count + ", List: " + string.Join(", ", allRecipients));

                    var sbResult = new StringBuilder();
                    bool allSuccess = true;
                    int successCount = 0;
                    int failCount = 0;

                    // 3. Gửi tin nhắn đến từng recipient
                    foreach (string userID in allRecipients)
                    {
                        PXTrace.WriteInformation("Sending message to ZaloUserID: " + userID);
                        string result = ZaloApiService.SendMessage(accessToken, userID, template.PreviewMessage ?? "");
                        PXTrace.WriteInformation("API Response for " + userID + ": " + (result ?? "null"));

                        sbResult.AppendLine($"- {userID}: {result ?? "No response"}");

                        if (result != null && result.Contains("Success"))
                        {
                            successCount++;
                        }
                        else
                        {
                            failCount++;
                            allSuccess = false;
                        }
                    }

                    // 4. Cập nhật kết quả gửi
                    template.Result = sbResult.ToString();
                    template.Status = allSuccess ? "Sent" : (successCount > 0 ? "Partial" : "Failed");
                    Templates.Cache.Update(template); // Cập nhật cache trước
                    PXTrace.WriteInformation("Cache updated - Status: " + template.Status + ", Result: " + template.Result);

                    // Lưu vào database
                    this.Actions.PressSave();
                    PXTrace.WriteInformation("Database save completed for Status: " + template.Status);

                    // 5. Hiển thị popup kết quả
                    string title = allSuccess ? "✅ Success" : (successCount > 0 ? "⚠️ Partial Success" : "❌ Failed");
                    string message = allSuccess
                        ? "Tất cả tin nhắn đã gửi thành công!"
                        : (successCount > 0
                            ? $"Gửi thành công {successCount} tin nhắn, thất bại {failCount} tin nhắn."
                            : "Gửi tin nhắn thất bại. Vui lòng kiểm tra lại.");
                    PXTrace.WriteInformation("Popup to display - Title: " + title + ", Message: " + message);

                    var dialogResult = Templates.Ask(title, message, MessageButtons.OK);
                    PXTrace.WriteInformation("Popup closed with result: " + dialogResult);
                }
                catch (Exception ex)
                {
                    PXTrace.WriteError("Inner exception during message sending: " + ex.Message + ", StackTrace: " + ex.StackTrace);
                    if (Templates.Current != null)
                    {
                        Templates.Current.Status = "Error";
                        Templates.Current.Result = ex.Message;
                        Templates.Cache.Update(Templates.Current);
                        this.Actions.PressSave(); // Đảm bảo lưu lỗi
                        PXTrace.WriteInformation("Error state saved - Status: Error, Result: " + ex.Message);
                    }
                    Templates.Ask("❌ Error", ex.Message, MessageButtons.OK);
                }
            }
            catch (Exception ex)
            {
                PXTrace.WriteError("Outer exception during sendZaloMessage: " + ex.Message + ", StackTrace: " + ex.StackTrace);
                if (Templates.Current != null)
                {
                    Templates.Current.Status = "Error";
                    Templates.Current.Result = ex.Message;
                    Templates.Cache.Update(Templates.Current);
                    this.Actions.PressSave();
                    PXTrace.WriteInformation("Error state saved - Status: Error, Result: " + ex.Message);
                }
                Templates.Ask("❌ Error", ex.Message, MessageButtons.OK);
            }
            finally
            {
                _isSending = false; // reset flag
                PXTrace.WriteInformation("Ending sendZaloMessage action");
            }

            return adapter.Get();
        }

        private List<string> SplitRecipients(params string[] fields)
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

    }
}