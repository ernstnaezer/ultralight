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

namespace Ultralight.Client
{
    using System;
    using System.Collections.Generic;
    using Transport;

    /// <summary>
    ///   Ultra simple STOMP client with command buffering support
    /// </summary>
    public class StompClient
        : IDisposable
    {
        private readonly IDictionary<string, Action<StompMessage>> _messageConsumers;
        private readonly Queue<Action> _commandQueue = new Queue<Action>();
        private readonly StompMessageSerializer _serializer = new StompMessageSerializer();
        private readonly ITransport _transport;

        /// <summary>
        ///   Initializes a new instance of the <see cref = "StompClient" /> class.
        /// </summary>
        /// <param name = "transport">The transport channel.</param>
        public StompClient(ITransport transport)
        {
            _transport = transport;
            _messageConsumers = new Dictionary<string, Action<StompMessage>>
                                    {
                                        {"MESSAGE", msg => { if (OnMessage != null) OnMessage(msg); }},
                                        {"RECEIPT", msg => { if (OnReceipt != null) OnReceipt(msg); }},
                                        {"ERROR", msg => { if (OnError != null) OnError(msg); }},
                                        {"CONNECTED", OnStompConnected},
                                    };
        }

        public Action<StompMessage> OnMessage { get; set; }
        public Action<StompMessage> OnReceipt { get; set; }
        public Action<StompMessage> OnError { get; set; }

        /// <summary>
        ///   Gets a value indicating whether this <see cref = "StompClient" /> is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected { get; private set; }

        /// <summary>
        ///   Connects to the server on the specified address.
        /// </summary>
        /// <param name = "address">The address.</param>
        public void Connect()
        {
            _transport.OnOpen += () => _transport.Send(_serializer.Serialize(new StompMessage("CONNECT")));
            _transport.OnMessage += (msg) => HandleMessage(_serializer.Deserialize(msg));
            _transport.Connect();
        }

        /// <summary>
        ///   Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            ExecuteWhenConnected(
                () =>
                    {
                        var msg = new StompMessage("DISCONNECT");
                        _transport.Send(_serializer.Serialize(msg));

                        _transport.Close();

                        IsConnected = false;
                    });
        }

        /// <summary>
        ///   Sends a message to the specified address.
        /// </summary>
        /// <param name = "address">The address.</param>
        /// <param name = "message">The message.</param>
        public void Send(string address, string message)
        {
            Send(address, message, null);
        }

        /// <summary>
        ///   Sends a message to the specified address.
        /// </summary>
        /// <param name = "address">The address.</param>
        /// <param name = "message">The message.</param>
        /// <param name = "receiptId">The receipt id.</param>
        public void Send(string address, string message, string receiptId)
        {
            ExecuteWhenConnected(
                () =>
                    {
                        if (address.StartsWith("/") == false) address = string.Format("/{0}", address);

                        var msg = new StompMessage("SEND", message);
                        msg["destination"] = address;
                        msg["id"] = Guid.NewGuid().ToString();

                        if (!string.IsNullOrEmpty(receiptId))
                            msg["receipt"] = receiptId;

                        _transport.Send(_serializer.Serialize(msg));
                    });
        }

        /// <summary>
        ///   Subscribes to the specified destination.
        /// </summary>
        /// <param name = "destination">The destination.</param>
        public void Subscribe(string destination)
        {
            ExecuteWhenConnected(
                () =>
                    {
                        var msg = new StompMessage("SUBSCRIBE");
                        msg["destination"] = destination;
                        _transport.Send(_serializer.Serialize(msg));
                    });
        }

        /// <summary>
        ///   Unsubscribes the specified destination.
        /// </summary>
        /// <param name = "destination">The destination.</param>
        public void Unsubscribe(string destination)
        {
            ExecuteWhenConnected(
                () =>
                    {
                        var msg = new StompMessage("UNSUBSCRIBE");
                        msg["destination"] = destination;
                        _transport.Send(_serializer.Serialize(msg));
                    });
        }

        /// <summary>
        ///   Executes the given action when the client is connected to the server, otherwise store it for later use.
        /// </summary>
        /// <param name = "command">The command.</param>
        private void ExecuteWhenConnected(Action command)
        {
            if (IsConnected)
            {
                command();
            }
            else
            {
                _commandQueue.Enqueue(command);
            }
        }

        /// <summary>
        ///   Called when [connected] received.
        /// </summary>
        /// <param name = "obj">The obj.</param>
        private void OnStompConnected(StompMessage obj)
        {
            IsConnected = true;

            foreach (var action in _commandQueue)
            {
                action();
            }

            _commandQueue.Clear();
        }

        /// <summary>
        ///   Dispatches the given message to a registerd message consumer.
        /// </summary>
        /// <param name = "message">The message.</param>
        private void HandleMessage(StompMessage message)
        {
            if (message == null || message.Command == null) return;
            if (!_messageConsumers.ContainsKey(message.Command)) return;

            _messageConsumers[message.Command](message);
        }

        /// <summary>
        ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (IsConnected)
            {
                Disconnect();
            }

            _commandQueue.Clear();
        }
    }
}