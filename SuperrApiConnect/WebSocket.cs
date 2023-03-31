using System.Net.WebSockets;
using System.Runtime.InteropServices;

namespace SuperrApiConnect
{
    public delegate void OnErrorHandler(string Message);
    public delegate void OnCloseHandler();
    public delegate void OnDataHandler(byte[] Data);
    public class WebSocket
    {
        private string WSRootUrl = "ws://inmob.stoxkart.com:7763";
        private string WSUrl;
        private int _buffer;
        private string _userID;
        private string _authToken;

        private ClientWebSocket client;
        private int _cancelAfter;
        CancellationTokenSource cTs = new CancellationTokenSource();

        public WebSocket(string ApiKey, string access_token, string UserId, int bufferLength) {
            WSUrl = string.Format("{0}?api_key={1}&request_token={2}", WSRootUrl, ApiKey, access_token);
            _userID = UserId;
            _authToken = access_token;
            _buffer = bufferLength;
        }

        public void SetCancellationTimeInSeconds(int duration) {
            string eod = "23:59:59.9999999";
            DateTime time = DateTime.Parse(eod);
            int timeDiff = (time - DateTime.Now).Seconds;
            if(duration > timeDiff) {
                Console.WriteLine("The duration provided is greater than today EOD, hence setting it todays EOD.");
                _cancelAfter = timeDiff;
            }
            else {
                _cancelAfter = duration;
            }
        }
        public bool IsConnected()
        {
            if(client is null)
                return false;
            
            return client.State == WebSocketState.Open;
        }

        // public async Task Connect() {
        //     using(client = new ClientWebSocket())
        //     {
        //         Uri WSUriUrl = new Uri(WSUrl);
        //         cTs.CancelAfter(TimeSpan.FromSeconds(_cancelAfter));
        //         try {
        //             await client.ConnectAsync(WSUriUrl, cTs.Token);
        //             while(client.State == WebSocketState.Open) {
        //                 Console.WriteLine("Connected to socket");
        //                 Console.WriteLine("Sending message");
        //                 WSSubscribeRequest subscribeRequest = GetBasicRequestStructure();
        //                 subscribeRequest.scripId = new SCRIPID[Tokens.Length];    
        //                 for(int i=0; i<Tokens.Length; i++) {
        //                     WSSubscribeRequest currentSubscribeRequest = subscribeRequest;
        //                     currentSubscribeRequest.ScripCount = (byte)1;
        //                     currentSubscribeRequest.ExchSeg = GetExchangeCode(Tokens[i].Split(':')[0].Trim());
        //                     currentSubscribeRequest.scripId[i] = new SCRIPID(Tokens[i].Split(':')[1].Trim());
        //                     currentSubscribeRequest.bHeader.iRequestCode = (byte)71;
        //                     ArraySegment<byte> byteToSend = new ArraySegment<byte>(Utils.StructToBytes(currentSubscribeRequest, currentSubscribeRequest.bHeader.iMsgLength));
        //                     client.SendAsync(byteToSend, WebSocketMessageType.Text, true, cTs.Token);
        //                     while(true) {
        //                         var responseBuffer = new byte[1024];
        //                         var offset =0;
        //                         var packet = 1024;
        //                         ArraySegment<byte> byteReceived = new ArraySegment<byte>(responseBuffer, offset, packet);
        //                         WebSocketReceiveResult response = await client.ReceiveAsync(byteReceived, cTs.Token);
        //                         ProcessDataReceived(responseBuffer);
        //                     }
        //                 }
        //             }
        //         } catch (WebSocketException e) {
        //             Console.WriteLine(e.Message);
        //         }  
        //     }
        // }

        public async Task Connect() {
            using( client = new ClientWebSocket())
            {
                Uri WSUriUrl = new Uri(WSUrl);
                cTs.CancelAfter(TimeSpan.FromSeconds(_cancelAfter));
                try
                {
                    await client.ConnectAsync(WSUriUrl, cTs.Token);
                }
                catch(Exception e) {
                    Console.WriteLine("Exception caugnt in connecting WebSocket. Message ::" + e.Message);
                }
            }
            
            Console.WriteLine("WebSocket Connected!!");
        }

        private WSSubscribeRequest GetBasicRequestStructure() {
            WSRequestHeader requestHeader = new WSRequestHeader();
            requestHeader.sClientId = _userID;
            requestHeader.sAuthToken = _authToken;
            
            WSSubscribeRequest subscribeRequest = new WSSubscribeRequest();
            requestHeader.iMsgLength = (ushort)Marshal.SizeOf(subscribeRequest);
            subscribeRequest.bHeader = requestHeader;

            subscribeRequest.WName = _userID;
            subscribeRequest.secIdxCode = -1;
            subscribeRequest.ScripCount = (byte)1;
            return subscribeRequest;
        }

        private byte GetExchangeCode(string Exchange) {
            Exchange = Exchange.ToUpper();
            switch(Exchange) {
                case "NSE_CASH":
                    return Constants.NSE_CASH;
                case "NSE_FNO":
                    return Constants.NSE_FNO;
                case "NSE_CURRENCY":
                    return Constants.NSE_CURRENCY;
                case "BSE_CASH":
                    return Constants.BSE_CASH;
                case "MCX_COMMODITIES":
                    return Constants.MCX_COMMODITIES;
                case "NCEDEXCX_COMMODITIES":
                    return Constants.NCEDEXCX_COMMODITIES;
                default:
                    return (byte)255;
            }
        }

        public void Subscribe(string[] Tokens, UInt32 Mode) {
            WSSubscribeRequest subscribeRequest = GetBasicRequestStructure();
            subscribeRequest.scripId = new SCRIPID[Tokens.Length];    
            for(int i=0; i<Tokens.Length; i++) {
                try
                {
                    WSSubscribeRequest currentSubscribeRequest = subscribeRequest;
                    currentSubscribeRequest.ScripCount = (byte)1;
                    currentSubscribeRequest.ExchSeg = GetExchangeCode(Tokens[i].Split(':')[0].Trim());
                    currentSubscribeRequest.scripId[i] = new SCRIPID(Tokens[i].Split(':')[1].Trim());
                    currentSubscribeRequest.bHeader.iRequestCode = (byte)Mode;
                    
                    ArraySegment<byte> byteToSend = new ArraySegment<byte>(Utils.StructToBytes(currentSubscribeRequest, currentSubscribeRequest.bHeader.iMsgLength));
                    if(IsConnected())
                        client.SendAsync(byteToSend, WebSocketMessageType.Text, true, cTs.Token);
                } catch(Exception e) {
                    Console.WriteLine("Exception in struct formation. Message ::" + e.Message);
                }
            }
            ReceiveTicks();
        }

        private async Task ReceiveTicks() {
            byte[] buffer = new byte[_buffer];
            while(true) {
                ArraySegment<byte> byteReceived = new ArraySegment<byte>(buffer, 0, _buffer);
                //WebSocketReceiveResult response = await client.ReceiveAsync(byteReceived, cTs.Token);
                ProcessDataReceived(buffer);
            }
        }

        private void ProcessDataReceived(byte[] buffer)
        {
            WSResponseHeader wsResponseHeader = Utils.ByteArrayToStructure<WSResponseHeader>(buffer, Constants.RESPONSE_HEADER_SIZE);
            switch(wsResponseHeader.MsgCode.ToString()) {
                case "29":
                    ReadMarketStatusData(buffer, wsResponseHeader);
                    break;
                case "61":
                    ReadLTPMode(buffer, wsResponseHeader);
                    break;
                case "62":
                    ReadQUOTEMode(buffer, wsResponseHeader);
                    break;
                case "63":
                    ReadFULLMode(buffer, wsResponseHeader);
                    break;
                case "65":
                    ReadIDXQUOTEMode(buffer, wsResponseHeader);
                    break;
                default:
                    Console.WriteLine("incorrect Message Code (" + wsResponseHeader.MsgCode.ToString() + ") Received");
                    break;
            }
        }

        private void ReadMarketStatusData(byte[] buffer, WSResponseHeader wsResponseHeader) {
            MarketStatusStruct response = Utils.ByteArrayToStructure<MarketStatusStruct>(buffer, wsResponseHeader.MsgLength);
            Console.WriteLine("MKT_STATUS ::\n" + 
                              "{\"bHeader\" : " + 
                                    "{\"ExchSeg\" : \"" + wsResponseHeader.ExchSeg.ToString() + "\", " +
                                    "\"MsgLength\" : " + wsResponseHeader.MsgLength + ", " +
                                    "\"MsgCode\" : \"" + wsResponseHeader.MsgCode.ToString() + "\", " +
                                    "\"ScripId\" : " + wsResponseHeader.ScripId + 
                                    "}, " + 
                                "\"Mkt_Type\" : \"" + response.Mkt_Type + "\", " +
                                "\"Mkt_Status\" : " + response.Mkt_Type + "}");
            return;
        }

        private void ReadLTPMode(byte[] buffer, WSResponseHeader wsResponseHeader) {
            WSLTPResponse response = Utils.ByteArrayToStructure<WSLTPResponse>(buffer, wsResponseHeader.MsgLength);
            Console.WriteLine("LTP_MODE ::\n" + 
                                "{\"bHeader\" : " + 
                                    "{\"ExchSeg\" : \"" + wsResponseHeader.ExchSeg.ToString() + "\", " +
                                    "\"MsgLength\" : " + wsResponseHeader.MsgLength + ", " +
                                    "\"MsgCode\" : \"" + wsResponseHeader.MsgCode.ToString() + "\", " +
                                    "\"ScripId\" : " + wsResponseHeader.ScripId + 
                                    "}, " + 
                                "\"LTP\" : " + response.LTP + ", " +
                                "\"LTT\" : " + response.LTT + ", " +
                                "\"iSecId\" : " + response.iSecId + ", " +
                                "\"traded\" : \"" + response.traded.ToString() + "\", " +
                                "\"mode\" : \"" + response.mode.ToString() + "\", " +
                                "\"fchange\" : " + response.fchange + ", " +
                                "\"fperChange\" : " + response.fperChange + "}");
            return;
        }

        private void ReadQUOTEMode(byte[] buffer, WSResponseHeader wsResponseHeader) {
            WSQuoteResponse response = Utils.ByteArrayToStructure<WSQuoteResponse>(buffer, wsResponseHeader.MsgLength);
            Console.WriteLine("QUOTE_MODE ::\n" + 
                                "{\"bHeader\" : " + 
                                    "{\"ExchSeg\" : \"" + wsResponseHeader.ExchSeg.ToString() + "\", " +
                                    "\"MsgLength\" : " + wsResponseHeader.MsgLength + ", " +
                                    "\"MsgCode\" : \"" + wsResponseHeader.MsgCode.ToString() + "\", " +
                                    "\"ScripId\" : " + wsResponseHeader.ScripId + 
                                    "}, " + 
                                "\"LTP\" : " + response.LTP + ", " +
                                "\"LTT\" : " + response.LTT + ", " +
                                "\"iSecId\" : " + response.iSecId + ", " +
                                "\"traded\" : \"" + response.traded.ToString() + "\", " +
                                "\"mode\" : \"" + response.mode.ToString() + "\", " +
                                "\"LTQ\" : " + response.LTQ + ", " +
                                "\"APT\" : " + response.APT + ", " +
                                "\"Vtraded\" : " + response.Vtraded + ", " +
                                "\"TotalBuyQ\" : " + response.TotalBuyQ + ", " +
                                "\"TotalSellQ\" : " + response.TotalSellQ + ", " +
                                "\"fOpen\" : " + response.fOpen + ", " +
                                "\"fClose\" : " + response.fClose + ", " +
                                "\"fHigh\" : " + response.fHigh + ", " +
                                "\"fLow\" : " + response.fLow + ", " +
                                "\"fperChange\" : " + response.fperChange + ", " +
                                "\"fchange\" : " + response.fchange + ", " +
                                "\"f52WKHigh\" : " + response.f52WKHigh + ", " +
                                "\"f52WKLow\" : " + response.f52WKLow + "}");
            return;
        }

        private void ReadFULLMode(byte[] buffer, WSResponseHeader wsResponseHeader) {
            WSFullModeResponse response = Utils.ByteArrayToStructure<WSFullModeResponse>(buffer, wsResponseHeader.MsgLength);
            string MBPRow = "[";
                                for(int i=0; i<response.submbp.Length ; i++) {
                                    MBPRow += "{" +
                                                    "\"iBuyqty\" : " + response.submbp[i].iBuyqty + ", " + 
                                                    "\"iSellqty\" : " + response.submbp[i].iSellqty + ", " +
                                                    "\"iBuyordno\" : " + response.submbp[i].iBuyordno + ", " +
                                                    "\"iSellordno\" : " + response.submbp[i].iSellqty + ", " +
                                                    "\"fBuyprice\" : " + response.submbp[i].fBuyprice + ", " +
                                                    "\"fSellprice\" : " + response.submbp[i].fSellprice + "}";
                                    if( i != (response.submbp.Length - 1)) {
                                        MBPRow += ", ";
                                    }
                                }
                                MBPRow += "]";
                                Console.WriteLine("Quote_Mode ::\n" + 
                                "{\"bHeader\" : " + 
                                    "{\"ExchSeg\" : \"" + wsResponseHeader.ExchSeg.ToString() + "\", " +
                                    "\"MsgLength\" : " + wsResponseHeader.MsgLength + ", " +
                                    "\"MsgCode\" : \"" + wsResponseHeader.MsgCode.ToString() + "\", " +
                                    "\"ScripId\" : " + wsResponseHeader.ScripId + 
                                    "}, " +
                                "\"submbp\" : " + MBPRow + ", " +
                                "\"LTP\" : " + response.LTP + ", " +
                                "\"LTT\" : " + response.LTT + ", " +
                                "\"iSecId\" : " + response.iSecId + ", " +
                                "\"traded\" : \"" + response.traded.ToString() + "\", " +
                                "\"mode\" : \"" + response.mode.ToString() + "\", " +
                                "\"LTQ\" : " + response.LTQ + ", " +
                                "\"APT\" : " + response.APT + ", " +
                                "\"Vtraded\" : " + response.Vtraded + ", " +
                                "\"TotalBuyQ\" : " + response.TotalBuyQ + ", " +
                                "\"TotalSellQ\" : " + response.TotalSellQ + ", " +
                                "\"fOpen\" : " + response.fOpen + ", " +
                                "\"fClose\" : " + response.fClose + ", " +
                                "\"fHigh\" : " + response.fHigh + ", " +
                                "\"fLow\" : " + response.fLow + ", " +
                                "\"fperChange\" : " + response.fperChange + ", " +
                                "\"fchange\" : " + response.fchange + ", " +
                                "\"f52WKHigh\" : " + response.f52WKHigh + ", " +
                                "\"f52WKLow\" : " + response.f52WKLow + ", " +
                                "\"OI\" : " + response.OI + ", " +
                                "\"OIChange\" : " + response.OIChange + "}");
            return;
        }

        private void ReadIDXQUOTEMode(byte[] buffer, WSResponseHeader wsResponseHeader) {
            WSIndexQuoteResponse response = Utils.ByteArrayToStructure<WSIndexQuoteResponse>(buffer, wsResponseHeader.MsgLength);
            Console.WriteLine("Quote_Mode ::\n" + 
                                "{\"bHeader\" : " + 
                                    "{\"ExchSeg\" : \"" + wsResponseHeader.ExchSeg.ToString() + "\", " +
                                    "\"MsgLength\" : " + wsResponseHeader.MsgLength + ", " +
                                    "\"MsgCode\" : \"" + wsResponseHeader.MsgCode.ToString() + "\", " +
                                    "\"ScripId\" : " + wsResponseHeader.ScripId + 
                                    "}, " +
                                "\"LTP\" : " + response.LTP + ", " +
                                "\"iSecId\" : " + response.iSecId + ", " +
                                "\"traded\" : \"" + response.traded.ToString() + "\", " +
                                "\"mode\" : \"" + response.mode.ToString() + "\", " +
                                "\"fOpen\" : " + response.fOpen + ", " +
                                "\"fClose\" : " + response.fClose + ", " +
                                "\"fHigh\" : " + response.fHigh + ", " +
                                "\"fLow\" : " + response.fLow + ", " +
                                "\"fperChange\" : " + response.fperChange + ", " +
                                "\"fchange\" : " + response.fchange + ", " +
                                "\"f52WKHigh\" : " + response.f52WKHigh + ", " +
                                "\"f52WKLow\" : " + response.f52WKLow + "}");
            return;
        }
    }
}