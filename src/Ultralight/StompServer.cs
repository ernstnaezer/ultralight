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
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///   A small and light STOMP message broker
    /// </summary>
    public class StompServer
    {
        private readonly IDictionary<string, Action<IStompClient, StompMessage>> _actions;

        private readonly IStompListener _listener;

        private readonly List<StompQueue> _queues = new List<StompQueue>();

        /// <summary>
        ///   Initializes a new instance of the <see cref = "StompServer" /> class.
        /// </summary>
        /// <param name = "listener">The listener.</param>
        public StompServer(IStompListener listener)
        {
            if (listener == null) throw new ArgumentNullException("listener");
            _listener = listener;

            _actions = new Dictionary<string, Action<IStompClient, StompMessage>>
                           {
                               {"CONNECT", OnStompConnect},
                               {"SUBSCRIBE", OnStompSubscribe},
                               {"UNSUBSCRIBE", OnStompUnsubscribe},
                               {"SEND", OnStompSend},
                           };
        }

        /// <summary>
        ///   Gets the queues.
        /// </summary>
        public StompQueue[] Queues
        {
            get { return _queues.ToArray(); }
        }

        /// <summary>
        ///   Starts this instance.
        /// </summary>
        public void Start()
        {
            // attach to listener events
            _listener.OnConnect += client =>
                                      {
                                          client.OnMessage += msg => OnClientMessage(client, msg);
                                          client.OnClose += () => client.OnClose = null;
                                      };

            _listener.Start();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            _queues.ForEach(queue => queue.Clients.ToList().ForEach(client => client.Close()));
            _queues.Clear();
            _listener.Stop();
        }

        /// <summary>
        ///   Excutes the action assigned to the message command
        /// </summary>
        /// <param name = "client"></param>
        /// <param name = "message"></param>
        private void OnClientMessage(IStompClient client, StompMessage message)
        {
            if ( message == null || message.Command == null) return;
            
            if (!_actions.ContainsKey(message.Command)) return;

            if (message.Command != "CONNECT" && client.IsConnected() == false)
            {
                client.Send(new StompMessage("ERROR", "Please connect before sending '" + message.Command + "'"));
                return;
            }

            _actions[message.Command](client, message);

            // when a receipt is request, we send a receipt frame
            if (message.Command == "CONNECT" || message["receipt"] == string.Empty) return;
            var response = new StompMessage("RECEIPT");
            response["receipt-id"] = message["receipt"];
            client.Send(response);
        }

        /// <summary>
        /// Handles the CONNECT message
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="message">The message.</param>
        private static void OnStompConnect(IStompClient client, StompMessage message)
        {
            var result = new StompMessage("CONNECTED");

            client.SessionId = Guid.NewGuid();
            result["session-id"] = client.SessionId.ToString();

            client.Send(result);
        }

        /// <summary>
        /// Handles the SUBSCRIBE message
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="message">The message.</param>
        private void OnStompSubscribe(IStompClient client, StompMessage message)
        {
            string destination = message["destination"];

            var queue = _queues.FirstOrDefault(s => s.Address == destination) ?? AddNewQueue(destination);

            queue.AddClient(client, message["id"]);
        }

        /// <summary>
        /// Handles the UNSUBSCRIBE message
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="message">The message.</param>
        private void OnStompUnsubscribe(IStompClient client, StompMessage message)
        {
            string destination = message["destination"];

            if (string.IsNullOrEmpty(destination)) return;
            var queue = _queues.FirstOrDefault(q => q.Address == destination);
            if (queue == null || queue.Clients.Contains(client) == false)
            {
                client.Send(new StompMessage("ERROR", "You are not subscribed to queue '" + destination + "'"));
                return;
            }

            queue.RemoveClient(client);
        }

        /// <summary>
        /// Handles the SEND message
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="message">The message.</param>
        private void OnStompSend(IStompClient client, StompMessage message)
        {
            var destination = message["destination"];

            var queue = _queues.FirstOrDefault(s => s.Address == destination) ?? AddNewQueue(destination);

            queue.Publish(message.Body);
        }

        /// <summary>
        /// Adds the new queue.
        /// </summary>
        /// <param name="destination">The queue name.</param>
        /// <returns></returns>
        private StompQueue AddNewQueue(string destination)
        {
            var queue = new StompQueue(destination)
            {
                OnLastClientRemoved =
                    q =>
                    {
                        q.OnLastClientRemoved = null;
                        lock (this)
                        {
                            _queues.Remove(q);
                        }
                    }
            };

            lock (this)
            {
                _queues.Add(queue);
            }
            return queue;
        }

    }
}