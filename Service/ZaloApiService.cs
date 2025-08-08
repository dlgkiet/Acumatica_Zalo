using System.IO;
using System.Net;

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
    }
}
