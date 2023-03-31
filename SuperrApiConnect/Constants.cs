namespace SuperrApiConnect
{
    public class Constants
    {
        // Exchange Codes For Ticks
        public const byte NSE_CASH = (byte)1;
        public const byte NSE_FNO = (byte)2;
        public const byte NSE_CURRENCY = (byte)3;
        public const byte BSE_CASH = (byte)4;
        public const byte MCX_COMMODITIES = (byte)5;
        public const byte NCEDEXCX_COMMODITIES = (byte)6;

        // Subscription Modes
        public const UInt32 MODE_LTP = 71;

        // Response Structure Sizes
        public const int RESPONSE_HEADER_SIZE = 10;

        // 24 hrs time in Seconds (24*60*60)
        public const int EOD = 86400;
    }
}