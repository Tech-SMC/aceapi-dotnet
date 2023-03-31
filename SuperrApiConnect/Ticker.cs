namespace SuperrApiConnect
{
    public class Ticker
    {
        public WebSocket _ws;
        System.Timers.Timer _timer;
        int _timerTick = 5;
        private int _interval = 5;
        private byte _subscriptionMode;
        public Ticker(string APIKey, string APISecret, string UserId, int bufferLen = 2000) {
            _ws = new WebSocket(APIKey, APISecret, UserId, bufferLen);
            SetCancellationTimeInSeconds(300);
            Connect();
        }

        public void SetCancellationTimeInSeconds(int timeInSeconds = Constants.EOD) {
            _ws.SetCancellationTimeInSeconds(timeInSeconds);
        }

        public void Subscribe(String[] Tokens, UInt32 Mode = Constants.MODE_LTP) {
            if (Tokens.Length == 0) return;

            if (IsConnected)
                _ws.Subscribe(Tokens, Mode);
        }

        private bool IsConnected {
            get { return _ws.IsConnected(); }
        }

        public void Connect() {
            if (!IsConnected)
            {
                _ws.Connect();
            }
        }
    }
}