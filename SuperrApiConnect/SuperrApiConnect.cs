using System.Text;

namespace SuperrApiConnect
{
     public class SuperrApi
     {
         private string _apiKey;
         private string _apiSecret;
         private string _root = "https://uatauth.smcindiaonline.org";
         private string _accessToken;

         static User _user;
         static Ticker _ticker;
         public SuperrApi(string UserID, string Password, string API_Key, string API_Secret) {
            _user = new User(UserID, Password);
            _apiKey = API_Key;
            _apiSecret = API_Secret;
         }

         private Dictionary<string, string> GetAdditionalHeaders() {
            var additionalHeaders = new Dictionary<string, string>();
            string sessionKey = _apiKey + ":" + _accessToken;
            additionalHeaders.Add("x-session", sessionKey);
            additionalHeaders.Add("x-client-code", _user.GetUserId());
            additionalHeaders.Add("x-source", "api");
            return additionalHeaders;
         }

         private readonly Dictionary<string, string> _routes = new Dictionary<string, string> {
            ["login"] = "/auth/login",
            ["2FAverify"] = "/auth/twofa/verify",
            ["accessToken"] = "/auth/token"
         };

         private string GetLoginWithAPIKeyUrl() {
            return string.Format("{0}{1}?api-key={2}", _root, _routes["login"], _apiKey);
         }

         private string Get2FA_VerifyUrl() {
            return string.Format("{0}{1}?api-key={2}", _root, _routes["2FAverify"], _apiKey);
         }

         private string GetAccessTokenUrl() {
            return string.Format("{0}{1}", _root, _routes["accessToken"]);
         }

         private string LoginWithAPIKey(string Url) {
            Dictionary<string, dynamic> result = _user.Login(Url);
            if(result["status"] == "success")
               return result["data"]["request_token"];
            else
               return "failure";
         }

         private string Verify2FA(string Url, string request_token) {
            Dictionary<string, dynamic> result = _user.Verify2FA(Url, request_token);
            if(result["status"] == "success")
               return result["data"]["request_token"];
            else
               return "failure";
         }

         private string GenerateSignature(string auth_token) {
            string key = _apiKey + auth_token;
            byte[] keyInBytes = Encoding.UTF8.GetBytes(key);
            byte[] secretInBytes = Encoding.UTF8.GetBytes(_apiSecret);
            return Utils.GenerateHMACSignature(keyInBytes, secretInBytes);
         }

         private string GetAccessToken(string Url, string auth_token) {
            string signature = GenerateSignature(auth_token);
            var RequestBody = new Dictionary<string, string> {
               {"api_key", _apiKey},
               {"signature", signature},
               {"req_token", auth_token}
            };
            string Response = Utils.SendHttpRequest("POST", Url, RequestBody);
            Dictionary<string, dynamic> parsedResponse = Utils.JsonDeserialize(Response);
            if(parsedResponse["status"] == "success")
               return parsedResponse["data"]["access_token"];
            else
               return "failure";
         }

         public void LoginAndSetAccessToken() {
            string request_token = LoginWithAPIKey(GetLoginWithAPIKeyUrl());
            string auth_token = Verify2FA(Get2FA_VerifyUrl(), request_token);
            _accessToken = GetAccessToken(GetAccessTokenUrl(), auth_token);
            Console.WriteLine("access Token ::" + _accessToken);
            return;
         }
     }   
}