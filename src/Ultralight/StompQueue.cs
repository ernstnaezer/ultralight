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

namespace Ultralight
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    /// <summary>
    /// Stomp message queue
    /// </summary>
    public class StompQueue
    {
        private readonly ConcurrentDictionary<IStompClient, Action> _clients =
            new ConcurrentDictionary<IStompClient, Action>();

        /// <summary>
        ///   Initializes a new instance of the <see cref = "StompQueue" /> class.
        /// </summary>
        /// <param name = "address">The address.</param>
        public StompQueue(string address)
        {
            if (address == null) throw new ArgumentNullException("address");
            if (!address.StartsWith("/")) address = string.Format("/{0}", address);

            Address = address;
        }

        /// <summary>
        ///   Gets the address.
        /// </summary>
        public string Address { get; private set; }


        /// <summary>
        /// Triggered when the last client got removed.
        /// </summary>
        /// <value>
        /// The on no more clients.
        /// </value>
        public Action<StompQueue> OnLastClientRemoved { get; set; }

        /// <summary>
        ///   Gets the clients.
        /// </summary>
        public IStompClient[] Clients
        {
            get { return _clients.Keys.ToArray(); }
        }

        /// <summary>
        ///   Adds the client.
        /// </summary>
        /// <param name = "client">The client.</param>
        public void AddClient(IStompClient client)
        {
            if (_clients.ContainsKey(client)) return;

            Action onClose = () => RemoveClient(client);
            client.OnClose += onClose;
            _clients.TryAdd(client, onClose);
        }

        /// <summary>
        ///   Removes the client.
        /// </summary>
        /// <param name = "client">The client.</param>
        public void RemoveClient(IStompClient client)
        {
            if (!_clients.ContainsKey(client)) return;

            Action onClose;
            if (_clients.TryRemove(client, out onClose))
                client.OnClose -= onClose;

            // raise the last client removed event if needed
            if (_clients.Count() == 0 && OnLastClientRemoved != null)
                OnLastClientRemoved(this);
        }

        /// <summary>
        ///   Publishes the specified message to all subscribed clients.
        /// </summary>
        /// <param name = "message">The message.</param>
        public void Publish(string message)
        {
            var envelope = new StompMessage("MESSAGE", message);
            envelope["message-id"] = Guid.NewGuid().ToString();

            foreach (var client in _clients.Keys)
            {
                client.Send(envelope);
            }
        }
    }
}