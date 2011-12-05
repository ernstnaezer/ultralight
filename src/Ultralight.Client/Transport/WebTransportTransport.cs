// Copyright 2011 Ernst Naezer, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

namespace Ultralight.Client.Transport
{
    using System;

    /// <summary>
    /// Websocket wrapper
    /// </summary>
    public class WebTransportTransport
        : ITransport
    {
        private readonly string _address;
        private WebSocketSharp.WebSocket _webSocket;

        public Action OnOpen { get; set; }
        public Action<string> OnMessage { get; set; }

        public WebTransportTransport(string address)
        {
            _address = address;
        }

        public void Connect()
        {
            _webSocket = new WebSocketSharp.WebSocket(_address);

            _webSocket.OnOpen += (o, e) => { if (OnOpen != null) OnOpen(); };
            _webSocket.OnMessage += (o, s) => { if (OnMessage != null) OnMessage(s); };
            _webSocket.Connect();
        }

        public void Send(string message)
        {
            _webSocket.Send(message);
        }

        public void Close()
        {
            _webSocket.Close();
            _webSocket.Dispose();
        }
    }
}