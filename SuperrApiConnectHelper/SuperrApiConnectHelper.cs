using SuperrApiConnect;

namespace SuperrApiConnectHelper
{
    class SuperrApiConnectHelper
    {
        static SuperrApi superrApi;
        static Ticker ticker;

        // Initialize API_Key and API_Secret
        static string API_Key = "RyuLDVjtufsk0OAL";
        static string API_Secret = "tIFgrfYuaFNpsefdWUeDog";
        static string UserID = "SAU123";


        // Main method 
        static void Main(string[] args) {
            Console.WriteLine("Please enter the password for UserID(" + UserID + ") ::");
            string password = Console.ReadLine();

            superrApi = new SuperrApi(UserID, password, API_Key, API_Secret);
            superrApi.LoginAndSetAccessToken();

            ticker = new Ticker(API_Key, API_Secret, UserID);
            ticker.Subscribe(Tokens: new String[] { "NSE_CASH:11536" }, 71);
        }
    }
}