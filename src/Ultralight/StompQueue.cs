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
    ///   Stomp message queue
    /// </summary>
    public class StompQueue
    {
        private class SubscriptionMetadata
        {
            public string Id { get; set; }
            public Action OnCloseHandler { get; set; }
        }

        private readonly ConcurrentDictionary<IStompClient, SubscriptionMetadata> _clients =
            new ConcurrentDictionary<IStompClient, SubscriptionMetadata>();

        private readonly ConcurrentQueue<string> _queuedMessages =
            new ConcurrentQueue<string>();

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
        ///   Triggered when the last client got removed.
        /// </summary>
        /// <value>
        ///   The on no more clients.
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
        ///   Gets the queued messages.
        /// </summary>
        public string[] QueuedMessages
        {
            get { return _queuedMessages.ToArray(); }
        }

        /// <summary>
        ///   Adds the client.
        /// </summary>
        /// <param name = "client">The client.</param>
        /// <param name = "subscriptionId">The subscription id.</param>
        public void AddClient(IStompClient client, string subscriptionId)
        {
            if (_clients.ContainsKey(client)) return;

            Action onClose = () => RemoveClient(client);
            client.OnClose += onClose;

            lock (_queuedMessages)
            {
                if (_clients.IsEmpty && _queuedMessages.IsEmpty == false)
                {
                    do
                    {
                        string body;
                        if (_queuedMessages.TryDequeue(out body))
                        {
                            SendMessage(client, body, Guid.NewGuid(), subscriptionId);
                        }
                    } while (_queuedMessages.IsEmpty == false);
                }
            }

            _clients.TryAdd(client, new SubscriptionMetadata {Id = subscriptionId, OnCloseHandler = onClose});
        }

        /// <summary>
        ///   Removes the client.
        /// </summary>
        /// <param name = "client">The client.</param>
        public void RemoveClient(IStompClient client)
        {
            if (!_clients.ContainsKey(client)) return;

            SubscriptionMetadata meta;
            if (_clients.TryRemove(client, out meta))
                if (meta.OnCloseHandler != null) client.OnClose -= meta.OnCloseHandler;

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
            if (_clients.IsEmpty)
            {
                _queuedMessages.Enqueue(message);
                return;
            }

            var messageId = Guid.NewGuid();
            foreach (var client in _clients)
            {
                var stompClient = client.Key;
                var metadata = client.Value;
                SendMessage(stompClient, message, messageId, metadata.Id);
            }
        }

        private void SendMessage(IStompClient client, string body, Guid messageId, string subscriptionId)
        {
            var stompMessage = new StompMessage("MESSAGE", body);
            stompMessage["message-id"] = messageId.ToString();
            stompMessage["destination"] = Address;

            if (!string.IsNullOrEmpty(subscriptionId))
            {
                stompMessage["subscription"] = subscriptionId;
            }

            client.Send(stompMessage);
        }
    }
}