using PX.Data;
using System;
using System.Collections;

namespace AnNhienCafe
{
    public class ZaloTokenMaint : PXGraph<ZaloTokenMaint>
    {
        public PXSelect<ZaloToken> ZaloToken;
        public PXSave<ZaloToken> Save;
        public PXCancel<ZaloToken> Cancel;

        // ✅ Constants cho thời gian hết hạn
        private const int ACCESS_TOKEN_EXPIRE_HOURS = 25; // 25 tiếng

        #region Event Handlers

        // ✅ RowInserting - Tự động tạo giá trị mặc định
        protected virtual void ZaloToken_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            if (e.Row is ZaloToken row)
            {
                // Tạo TokenID nếu chưa có
                if (row.TokenID == null || row.TokenID == Guid.Empty)
                {
                    row.TokenID = Guid.NewGuid();
                }

                PXTrace.WriteInformation($"✅ RowInserting: TokenID={row.TokenID}");
            }
        }

        // ✅ FieldDefaulting cho AccessTokenExpiredAt
        protected virtual void ZaloToken_AccessTokenExpiredAt_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = DateTime.Now.AddHours(ACCESS_TOKEN_EXPIRE_HOURS);
        }

        #endregion

        #region Helper Methods

        // ✅ Parse JSON response và cập nhật token
        public void UpdateTokenFromResponse(ZaloToken token, string jsonResponse)
        {
            var now = DateTime.Now;
            bool tokenUpdated = false;

            try
            {
                PXTrace.WriteInformation($"📝 Parsing JSON response: {jsonResponse.Substring(0, Math.Min(200, jsonResponse.Length))}...");

                // ✅ Parse access_token using regex for better accuracy
                var accessTokenMatch = System.Text.RegularExpressions.Regex.Match(jsonResponse, @"""access_token""\s*:\s*""([^""]*)""");
                if (accessTokenMatch.Success)
                {
                    string newAccessToken = accessTokenMatch.Groups[1].Value;
                    if (!string.IsNullOrEmpty(newAccessToken))
                    {
                        token.AccessToken = newAccessToken;
                        tokenUpdated = true;
                        PXTrace.WriteInformation($"✅ Updated access_token (length: {newAccessToken.Length})");
                    }
                }

                // ✅ Parse refresh_token
                var refreshTokenMatch = System.Text.RegularExpressions.Regex.Match(jsonResponse, @"""refresh_token""\s*:\s*""([^""]*)""");
                if (refreshTokenMatch.Success)
                {
                    string newRefreshToken = refreshTokenMatch.Groups[1].Value;
                    if (!string.IsNullOrEmpty(newRefreshToken))
                    {
                        token.RefreshToken = newRefreshToken;
                        PXTrace.WriteInformation($"✅ Updated refresh_token (length: {newRefreshToken.Length})");
                    }
                }

                // ✅ Parse expires_in (nhưng ta sẽ dùng logic cố định 25 tiếng)
                var expiresInMatch = System.Text.RegularExpressions.Regex.Match(jsonResponse, @"""expires_in""\s*:\s*""?(\d+)""?");
                if (expiresInMatch.Success)
                {
                    PXTrace.WriteInformation($"📝 API returned expires_in: {expiresInMatch.Groups[1].Value} (using fixed 25-hour rule instead)");
                }

                // ✅ Tự động cập nhật thời gian hết hạn theo logic 25 tiếng
                if (tokenUpdated)
                {
                    token.AccessTokenExpiredAt = now.AddHours(ACCESS_TOKEN_EXPIRE_HOURS);
                    PXTrace.WriteInformation($"✅ AccessToken expires at: {token.AccessTokenExpiredAt} (25 hours from now)");
                }
            }
            catch (Exception ex)
            {
                PXTrace.WriteError($"❌ JSON parsing failed: {ex.Message}");
            }
        }

        #endregion

        #region Actions

        public PXAction<ZaloToken> CallZaloApi;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Call Zalo API", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable callZaloApi(PXAdapter adapter)
        {
            PXTrace.WriteInformation("=== 🚀 CALLING ZALO API ===");

            ZaloToken current = ZaloToken.Current;

            // ✅ Tạo record mới nếu chưa có
            if (current == null)
            {
                PXTrace.WriteInformation("📝 Creating new record...");
                current = ZaloToken.Insert(new ZaloToken());
                if (current == null)
                {
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("Không thể tạo record mới");
                }
            }

            // ✅ Validate required fields
            if (string.IsNullOrWhiteSpace(current.AppID) ||
                string.IsNullOrWhiteSpace(current.AppSecret) ||
                string.IsNullOrWhiteSpace(current.RefreshToken))
            {
                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                throw new PXException("AppID, App Secret, và Refresh Token không được để trống.");
            }

            try
            {
                PXTrace.WriteInformation("🔄 Calling Zalo API...");
                string response = ZaloApiService.RefreshToken(
                    current.AppID,
                    current.AppSecret,
                    current.RefreshToken
                );

                PXTrace.WriteInformation($"📨 API Response received: {response.Length} characters");

                // ✅ Parse và cập nhật token
                UpdateTokenFromResponse(current, response);

                // ✅ Update record
                current = ZaloToken.Update(current);

                // ✅ Save to database
                this.Actions.PressSave();

                PXTrace.WriteInformation("✅ SUCCESS: Token updated and saved to database");
                PXTrace.WriteInformation($"✅ AccessToken expires at: {current.AccessTokenExpiredAt}");

                // ✅ Refresh view
                this.ZaloToken.View.RequestRefresh();
            }
            catch (Exception ex)
            {
                PXTrace.WriteError($"❌ ERROR: {ex.Message}");
                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                throw new PXException($"Lỗi khi gọi API Zalo: {ex.Message}");
            }

            return adapter.Get();
        }
        #endregion

        #region
        public PXProcessing<ZaloToken> AutoZaloTokens;
        public PXAction<ZaloToken> AutoRefreshToken;
        [PXProcessButton]
        [PXUIField(DisplayName = "Auto Refresh Token", Visible = false)] // Ẩn nút khỏi UI
        public virtual IEnumerable autoRefreshToken(PXAdapter adapter)
        {
            AutoZaloTokens.SetProcessDelegate(AutoProcessZaloToken);
            return adapter.Get();
        }

        public static void AutoProcessZaloToken(ZaloToken token)
        {
            if (token == null) return;

            if (token.AccessTokenExpiredAt.HasValue && token.AccessTokenExpiredAt.Value > DateTime.Now)
            {
                PXTrace.WriteInformation($"⏩ Token còn hạn đến {token.AccessTokenExpiredAt}, bỏ qua.");
                return;
            }

            var graph = PXGraph.CreateInstance<ZaloTokenMaint>();
            graph.ZaloToken.Current = token;

            try
            {
                PXTrace.WriteInformation("🔄 Đang tự động gọi API Zalo...");
                string response = ZaloApiService.RefreshToken(
                    token.AppID,
                    token.AppSecret,
                    token.RefreshToken
                );

                graph.UpdateTokenFromResponse(token, response);

                graph.ZaloToken.Update(token);
                graph.Actions.PressSave();

                PXTrace.WriteInformation($"✅ Token cho {token.AppID} đã được cập nhật.");
            }
            catch (Exception ex)
            {
                PXTrace.WriteError($"❌ Lỗi cập nhật token: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}