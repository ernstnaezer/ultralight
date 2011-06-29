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

namespace Ultralight.Listeners
{
    using System;
    using Fleck;

    /// <summary>
    /// Listens to a websocket for incomming clients
    /// </summary>
    public class StompWsListener : IStompListener
    {
        private readonly WebSocketServer _server;

        /// <summary>
        /// Initializes a new instance of the <see cref="StompWsListener"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        public StompWsListener(Uri address)
        {
            _server = new WebSocketServer(address.ToString());
        }

        #region IStompListener Members

        /// <summary>
        /// Start the listener
        /// </summary>
        public void Start()
        {
            _server.Start(socket => socket.OnOpen = () => OnConnect(new StompWsClient(socket)));
        }

        public void Stop()
        {
            _server.Dispose();
        }

        /// <summary>
        /// A new client connected
        /// </summary>
        public Action<IStompClient> OnConnect { get; set; }

        #endregion
    }
}