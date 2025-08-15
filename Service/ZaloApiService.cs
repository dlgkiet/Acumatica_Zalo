using System.IO;
using System.Net;
using System.Text;
using System;

namespace AnNhienCafe
{
    public static class ZaloApiService
    {
        public static string RefreshToken(string appId, string appSecret, string refreshToken)
        {
            var url = "https://oauth.zaloapp.com/v4/oa/access_token";
            var postData = $"app_id={appId}&refresh_token={refreshToken}&grant_type=refresh_token";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            // ✅ Thêm app_secret vào headers
            request.Headers["secret_key"] = appSecret;

            using (var stream = new StreamWriter(request.GetRequestStream()))
            {
                stream.Write(postData);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Gửi tin nhắn text qua Zalo OA
        /// </summary>
        /// <param name="accessToken">Access token từ Zalo</param>
        /// <param name="zaloUserId">Zalo User ID người nhận</param>
        /// <param name="message">Nội dung tin nhắn</param>
        /// <returns>JSON response từ Zalo API (thành công hoặc error)</returns>
        public static string SendTextMessage(string accessToken, string zaloUserId, string message)
        {
            var url = "https://openapi.zalo.me/v3.0/oa/message/cs";

            // Tạo JSON payload
            var jsonPayload = $@"{{
                ""recipient"": {{
                    ""user_id"": ""{zaloUserId}""
                }},
                ""message"": {{
                    ""text"": ""{EscapeJsonString(message)}""
                }}
            }}";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers["access_token"] = accessToken;

            // Ghi JSON data vào request body
            byte[] data = Encoding.UTF8.GetBytes(jsonPayload);
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException webEx)
            {
                // Read error response body để lấy detail từ Zalo
                string errorResponse = string.Empty;
                if (webEx.Response != null)
                {
                    using (var errorStream = webEx.Response.GetResponseStream())
                    using (var errorReader = new StreamReader(errorStream))
                    {
                        errorResponse = errorReader.ReadToEnd();
                    }
                }
                // Throw lại với detail
                throw new Exception($"Zalo API error: {webEx.Message}. Response: {errorResponse}");
            }
        }

        /// <summary>
        /// Escape chuỗi để tránh lỗi JSON
        /// </summary>
        private static string EscapeJsonString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input
                .Replace("\\", "\\\\")  // \ -> \\
                .Replace("\"", "\\\"")  // " -> \"
                .Replace("\n", "\\n")   // newline -> \n
                .Replace("\r", "\\r")   // carriage return -> \r
                .Replace("\t", "\\t");  // tab -> \t
        }

        /// <summary>
        /// Parse access token từ JSON response
        /// </summary>
        public static string ParseAccessToken(string jsonResponse)
        {
            try
            {
                var match = System.Text.RegularExpressions.Regex.Match(
                    jsonResponse,
                    @"""access_token""\s*:\s*""([^""]*)"""
                );

                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể parse access token: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Kiểm tra response có thành công không
        /// </summary>
        public static bool IsSuccessResponse(string jsonResponse)
        {
            if (string.IsNullOrEmpty(jsonResponse))
                return false;

            // Zalo API trả về error code = 0 khi thành công
            var errorMatch = System.Text.RegularExpressions.Regex.Match(
                jsonResponse,
                @"""error""\s*:\s*(\d+)"
            );

            if (errorMatch.Success)
            {
                return errorMatch.Groups[1].Value == "0";
            }

            // Nếu không có error code, kiểm tra có message_id không
            var messageIdMatch = System.Text.RegularExpressions.Regex.Match(
                jsonResponse,
                @"""message_id""\s*:\s*""([^""]*)"""
            );

            return messageIdMatch.Success;
        }

        /// <summary>
        /// Parse error message từ response
        /// </summary>
        public static string ParseErrorMessage(string jsonResponse)
        {
            if (string.IsNullOrEmpty(jsonResponse))
                return "Unknown error";

            var messageMatch = System.Text.RegularExpressions.Regex.Match(
                jsonResponse,
                @"""message""\s*:\s*""([^""]*)"""
            );

            if (messageMatch.Success)
            {
                return messageMatch.Groups[1].Value;
            }

            return jsonResponse;
        }
    }
}