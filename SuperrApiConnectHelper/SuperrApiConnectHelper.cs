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
            bool status = superrApi.LoginAndSetAccessToken();
            if(!status) {
                return;
            }

            ticker = new Ticker(API_Key, API_Secret, UserID, cancelTimeInSeconds: 300);
            Thread t = new Thread(() => ticker.Subscribe(Tokens: new String[] { "NSE_CASH:11536" }, 71));
            t.Start();
            //ticker.Subscribe(Tokens: new String[] { "NSE_CASH:11536" }, 71);

            //-----------------------------------------------//
            //------------- Place order Normal---------------//
            //-----------------------------------------------//
            Dictionary<string, dynamic> PlaceOrderResponse =  superrApi.PlaceOrder(
                variety: "NORMAL",
                action: "BUY",
                exchange: "NSE",
                token: "11536",
                order_type: "LIMIT",
                product_type: "DELIVERY",
                quantity: "1",
                disclose_quantity: "0",
                price: "3400",
                trigger_price: "0",
                stop_loss_price: "0",
                trailing_stop_loss: "0",
                validity: "DAY",
                tag: ""
            );

            if(PlaceOrderResponse["status"] == "success") {
                Console.WriteLine("Order_ID ::" + PlaceOrderResponse["data"]["order_id"]);
            } else {
                Console.WriteLine("Place Order Transaction Failed ::" + PlaceOrderResponse["message"]);
            }

            //-----------------------------------------------//
            //---------- Modify Order Normal-----------------//
            //-----------------------------------------------//
            Dictionary<string, dynamic> ModifyOrderResponse =  superrApi.ModifyOrder(
                order_id: PlaceOrderResponse["data"]["order_id"],
                variety: "NORMAL",
                exchange: "NSE",
                token: "11536",
                order_type: "LIMIT",
                quantity: "10",
                disclose_quantity: "0",
                price: "3150",
                trigger_price: "0",
                stop_loss_price: "0",
                validity: ""
            );

            if(ModifyOrderResponse["status"] == "success") {
                Console.WriteLine("Order_ID ::" + ModifyOrderResponse["data"]["order_id"] );
            } else {
                Console.WriteLine("Modify Order Transaction Failed ::" + ModifyOrderResponse["message"]);
            }


            //-----------------------------------------------//
            //--------------Cancel Order Normal--------------//
            //-----------------------------------------------//
            Dictionary<string, dynamic> CancelOrderResponse = superrApi.CancelOrder(
                variety: "NORMAL",
                order_id: PlaceOrderResponse["data"]["order_id"]
            );

            if(CancelOrderResponse["status"] == "success") {
                Console.WriteLine("Cancel Order_ID ::" + CancelOrderResponse["data"]["order_id"] );
            } else {
                Console.WriteLine("Cancel Order Transaction Failed ::" + CancelOrderResponse["message"]);
            }

            //-----------------------------------------------//
            //------------------Fund Details-----------------//
            //-----------------------------------------------//
            Dictionary<string, dynamic> FundDetailsResponse = superrApi.FundDetails();

            if(FundDetailsResponse["status"] == "success") {    
                var size = FundDetailsResponse["data"].Count;
                if(size > 0) {
                    Console.WriteLine("Fund Details ::\n{");
                    foreach (KeyValuePair<string, dynamic> record in FundDetailsResponse["data"]) {
                        Console.Write("\t\"" + record.Key + "\" : \"" + record.Value + "\"");
                        size--;
                        if(size > 0) {
                            Console.WriteLine(", ");
                        } else {
                            Console.WriteLine("\n}");
                        }
                    }
                }
            } else {
                Console.WriteLine("Fund Details Transaction Failed ::" + FundDetailsResponse["message"]);
            }

            //-----------------------------------------------//
            //----------------Holding Details----------------//
            //-----------------------------------------------//
            Dictionary<string, dynamic> HoldingDetailsResponse = superrApi.HoldingDetails();
            
            if(HoldingDetailsResponse["status"] == "success") {
                var size = HoldingDetailsResponse["data"].Count;
                if(size > 0) {
                    Console.WriteLine("Holding Details ::\n{\n\t\"holding_details\" :\n\t[");
                    foreach (var record in HoldingDetailsResponse["data"]) {
                       var recordCount = record.Count;
                       if(recordCount > 0) {
                            Console.WriteLine("\t\t{");
                            foreach (KeyValuePair<string, dynamic> CurrentRecord in record) {
                                Console.Write("\t\t\t\"" + CurrentRecord.Key + "\" : \"" + CurrentRecord.Value + "\"");
                                recordCount--;
                                if(recordCount > 0) {
                                    Console.WriteLine(",");
                                } else {
                                    size--;
                                    if(size > 0) {
                                        Console.WriteLine("\n\t\t},");
                                    } else {
                                        Console.WriteLine("\n\t\t}");
                                    }
                                }
                            }
                        }
                    }
                    Console.WriteLine("\t]\n}");
                }
            } else {
                Console.WriteLine("Holding Details Transaction Failed ::" + HoldingDetailsResponse["message"]);
            }

            //-----------------------------------------------//
            //----------------Position Details---------------//
            //-----------------------------------------------//
            Dictionary<string, dynamic> PositionDetailsResponse = superrApi.PositionDetails();

            if(PositionDetailsResponse["status"] == "success") {
                var size = PositionDetailsResponse["data"].Count;
                if(size > 0) {
                    Console.WriteLine("Position Details ::\n{\n\t\"position_details\" :\n\t[");
                    foreach (var record in PositionDetailsResponse["data"]) {
                       var recordCount = record.Count;
                       if(recordCount > 0) {
                            Console.WriteLine("\t\t{");
                            foreach (KeyValuePair<string, dynamic> CurrentRecord in record) {
                                Console.Write("\t\t\t\"" + CurrentRecord.Key + "\" : \"" + CurrentRecord.Value + "\"");
                                recordCount--;
                                if(recordCount > 0) {
                                    Console.WriteLine(",");
                                } else {
                                    size--;
                                    if(size > 0) {
                                        Console.WriteLine("\n\t\t},");
                                    } else {
                                        Console.WriteLine("\n\t\t}");
                                    }
                                }
                            }
                        }
                    }
                    Console.WriteLine("\t]\n}");
                }
            } else {
                Console.WriteLine("Position Details Transaction Failed ::" + PositionDetailsResponse["message"]);
            }

            //-----------------------------------------------//
            //--------------------Order Book-----------------//
            //-----------------------------------------------//
            Dictionary<string, dynamic> OrderBookResponse = superrApi.OrderBook();

            if(OrderBookResponse["status"] == "success") {
                var size = OrderBookResponse["data"].Count;
                if(size > 0) {
                    Console.WriteLine("Order Book ::\n{\n\t\"order_book\" :\n\t[");
                    foreach (var record in OrderBookResponse["data"]) {
                       var recordCount = record.Count;
                       if(recordCount > 0) {
                            Console.WriteLine("\t\t{");
                            foreach (KeyValuePair<string, dynamic> CurrentRecord in record) {
                                Console.Write("\t\t\t\"" + CurrentRecord.Key + "\" : \"" + CurrentRecord.Value + "\"");
                                recordCount--;
                                if(recordCount > 0) {
                                    Console.WriteLine(",");
                                } else {
                                    size--;
                                    if(size > 0) {
                                        Console.WriteLine("\n\t\t},");
                                    } else {
                                        Console.WriteLine("\n\t\t}");
                                    }
                                }
                            }
                        }
                    }
                    Console.WriteLine("\t]\n}");
                }
            } else {
                Console.WriteLine("Order Book Transaction Failed ::" + OrderBookResponse["message"]);
            }

            //-----------------------------------------------//
            //--------------------Trade Book-----------------//
            //-----------------------------------------------//
            Dictionary<string, dynamic> TradeBookResponse = superrApi.TradeBook();

            if(TradeBookResponse["status"] == "success") {
                var size = TradeBookResponse["data"].Count;
                if(size > 0) {
                    Console.WriteLine("Trade Book ::\n{\n\t\"trade_book\" :\n\t[");
                    foreach (var record in TradeBookResponse["data"]) {
                       var recordCount = record.Count;
                       if(recordCount > 0) {
                            Console.WriteLine("\t\t{");
                            foreach (KeyValuePair<string, dynamic> CurrentRecord in record) {
                                Console.Write("\t\t\t\"" + CurrentRecord.Key + "\" : \"" + CurrentRecord.Value + "\"");
                                recordCount--;
                                if(recordCount > 0) {
                                    Console.WriteLine(",");
                                } else {
                                    size--;
                                    if(size > 0) {
                                        Console.WriteLine("\n\t\t},");
                                    } else {
                                        Console.WriteLine("\n\t\t}");
                                    }
                                }
                            }
                        }
                    }
                    Console.WriteLine("\t]\n}");
                }
            } else {
                Console.WriteLine("Trade Book Transaction Failed ::" + TradeBookResponse["message"]);
            }

            //-----------------------------------------------//
            //-----------------Script-Master-----------------//
            //-----------------------------------------------//
            Dictionary<string, dynamic> ScripMasterResponse = superrApi.ScripMaster(
                exchange: "NSE"
            );

            if(ScripMasterResponse["status"] == "success") {
                var size = ScripMasterResponse["data"].Count;
                if(size > 0) {
                    Console.WriteLine("Script Master ::\n{");
                    foreach (KeyValuePair<string, dynamic> record in ScripMasterResponse["data"]) {
                        if(record.Key != "script_master_data") {
                            Console.Write("\t\"" + record.Key + "\" : \"" + record.Value + "\"");
                        } else {
                            var count = ScripMasterResponse["data"]["script_master_data"].Count;
                            if(count > 0) {
                                Console.WriteLine("\t\"" + record.Key + "\" : [");
                                foreach (var ScripMasterData in ScripMasterResponse["data"]["script_master_data"]) {
                                    var recordCount = ScripMasterData.Count;
                                    if(recordCount > 0) {
                                        Console.WriteLine("\t\t{");
                                        foreach (KeyValuePair<string, dynamic> CurrentRecord in ScripMasterData) {
                                            Console.Write("\t\t\t\"" + CurrentRecord.Key + "\" : \"" + CurrentRecord.Value + "\"");
                                            recordCount--;
                                            if(recordCount > 0) {
                                                Console.WriteLine(",");
                                            } else {
                                                count--;
                                                if(count > 0) {
                                                    Console.WriteLine("\n\t\t},");
                                                } else {
                                                    Console.WriteLine("\n\t\t}");
                                                }
                                                
                                            }
                                        }
                                    }
                                }
                                Console.WriteLine("\t]");
                            }
                        }
                        size--;
                        if(size != 0) {
                            Console.WriteLine(",");
                        } else {
                            Console.WriteLine("\n}");
                        }
                    }
                }
            } else {
                Console.WriteLine("Script Master Transaction Failed ::" + ScripMasterResponse["message"]);
            }
        }
    }
}