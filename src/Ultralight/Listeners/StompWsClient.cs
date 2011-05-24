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
    /// Wraps a client connecting over websockets
    /// </summary>
    public class StompWsClient : IStompClient
    {
        private readonly IWebSocketConnection _socket;
        private readonly StompMessageSerializer _messageSerializer = new StompMessageSerializer();

        /// <summary>
        ///   Initializes a new instance of the <see cref = "StompWsClient" /> class.
        /// </summary>
        /// <param name = "socket">The socket.</param>
        public StompWsClient(IWebSocketConnection socket)
        {
            if (socket == null) throw new ArgumentNullException("socket");
            _socket = socket;

            socket.OnClose = () => OnClose();
            socket.OnMessage = message => OnMessage(_messageSerializer.Deserialize(message));
        }

        #region IStompClient Members

        /// <summary>
        ///   Client ID
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        ///   Sends a message to the client
        /// </summary>
        /// <param name = "message"></param>
        public void Send(StompMessage message)
        {
            _socket.Send(_messageSerializer.Serialize(message));
        }

        /// <summary>
        ///   A message from the client is received
        /// </summary>
        public Action<StompMessage> OnMessage { get; set; }

        /// <summary>
        ///   The client disconnected
        /// </summary>
        public Action OnClose { get; set; }

        #endregion
    }
}