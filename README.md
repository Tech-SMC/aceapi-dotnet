# SupperAPI Connect .Net library
This is the official .Net SDK for communicating with [Superr Trade Connect API](https://superrtrade.stoxkart.com).

The Superr API is a powerful & a set of REST-like HTTP APIs through which you can fetch live/historical data, automate your trading strategies, and monitor your portfolio in real time. An ApiKey and secret_key pair is issued that allows you to connect to the server. You have to register a redirect url where you will be sent after the login.

[StoxKart India Pvt. Ltd.](https://www.stoxkart.com) &copy; 2023. Licensed under the [MIT License](/license/).

## Documentation

* [HTTP API](https://developers.stoxkart.com/api-documentation)


## Getting started
```csharp
// Import library
using SuperrApiConnect;

// Initialize SuperrApiConnect using user_id, password, ApiKey and ApiSecret.
SuperrApi superrApi = new SuperrApi(UserID, password, API_Key, API_Secret);

// LoginAndSetAccessToken will do the following work:
// => fetch the login url and use the credentials provided while Initializing.
// => verify the 2FA, for which an OTP will be sent to registered mobile number.
// => generate access token, for which a signature will be generated using the APIKey and APISecret.
// Each step will be executed one after the other.
// Any failure in any of the step will return false and set the status as failure.
// In case of failure the programme will EXIT.

bool status = superrApi.LoginAndSetAccessToken();

// Example call for functions like "PlaceOrder" that returns Dictionary
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
Console.WriteLine("Order_ID ::" + PlaceOrderResponse["data"]["order_id"]);

```
For more examples, take a look at [Program.cs](https://github.com/StoxKart-Tech/SuperrApi-DotNet/blob/main/SuperrApiConnectHelper/SuperrApiConnectHelper.cs) of **SuperrApiConnectHelper** project in this repository.

## WebSocket live streaming data

This library runs a seperate thread to receive Ticks on WebSocket.

```csharp
/* 
To get live price use Ticker Object. 
It is recommended to use only one websocket connection at any point of time and make sure you stop connection, 
once user goes out of app. However, during initialization of Ticker object it can be set how long do one want to receive the ticks.
The cancellation time is set in seconds which can go to a max of EOD of the day the code the run. 
*/

// Create a new Ticker instance
Ticker ticker = new Ticker(API_Key, API_Secret, UserID, cancelTimeInSeconds: 300);

// Subscribing to NSE_CASH TCS and INFOSYS and setting mode to LTP
ticker.Subscribe(Tokens: new String[] { "NSE_CASH:11536", "NSE_CASH:1594" }, Constants.MODE_LTP);

```

For more details about different mode of quotes and subscribing for them, take a look at **SuperrApiConnectHelper** project in this repository and [SuperrApi Connect HTTP API documentation](https://developers.stoxkart.com/api-documentation).
