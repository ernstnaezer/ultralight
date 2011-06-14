namespace Ultralight.Client
{
    using System;
    using System.Collections.Generic;
    using WebSocketSharp;

    /// <summary>
    /// Ultra simple STOMP client with command buffering support
    /// </summary>
    public class StompClient
    {
        private readonly IDictionary<string, Action<StompMessage>> _actions;
        private readonly Queue<Action> _commandQueue = new Queue<Action>();
        private readonly StompMessageSerializer _serializer = new StompMessageSerializer();

        private WebSocket _sock;
        private bool _connected;

        /// <summary>
        /// Initializes a new instance of the <see cref="StompClient"/> class.
        /// </summary>
        public StompClient()
        {
            _actions = new Dictionary<string, Action<StompMessage>>
                           {
                               {"MESSAGE", msg => { if (OnMessage != null) OnMessage(msg);}},
                               {"RECEIPT", msg => { if (OnReceipt != null) OnReceipt(msg);}},
                               {"ERROR", msg => { if (OnError != null) OnError(msg);}},
                               {"CONNECTED", OnStompConnected},
                           };
        }

        public Action<StompMessage> OnMessage { get; set; }
        public Action<StompMessage> OnReceipt { get; set; }
        public Action<StompMessage> OnError { get; set; }

        /// <summary>
        /// Connects to the server on the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        public void Connect(Uri address)
        {
            _sock = new WebSocket(address.ToString());
            _sock.OnOpen += (o, e) => _sock.Send(_serializer.Serialize(new StompMessage("CONNECT")));
            _sock.OnMessage += (o, s) => HandleMessage(_serializer.Deserialize(s));

            _sock.Connect();
        }

        /// <summary>
        /// Sends a message to the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="message">The message.</param>
        public void Send(string address, string message)
        {
            Send(address, message, null);
        }

        /// <summary>
        /// Sends a message to the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="message">The message.</param>
        /// <param name="receiptId">The receipt id.</param>
        public void Send(string address, string message, string receiptId)
        {
            ExecuteWhenConnected(
                () =>
                    {
                        if (address.StartsWith("/") == false) address = string.Format("/{0}", address);

                        var msg = new StompMessage("SEND", message);
                        msg["destination"] = address;

                        if (!string.IsNullOrEmpty(receiptId))
                            msg["receipt"] = receiptId;

                        _sock.Send(_serializer.Serialize(msg));
                    });
        }

        /// <summary>
        /// Subscribes to the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public void Subscribe(string path)
        {
            ExecuteWhenConnected(
                () =>
                    {
                        var msg = new StompMessage("SUBSCRIBE");
                        msg["destination"] = path;
                        _sock.Send(_serializer.Serialize(msg));
                    });
        }

        private void ExecuteWhenConnected(Action command)
        {
            if (_connected)
            {
                command();
            }
            else
            {
                _commandQueue.Enqueue(command);
            }
        }

        private void OnStompConnected(StompMessage obj)
        {
            _connected = true;

            foreach (var action in _commandQueue)
            {
                action();
            }
            _commandQueue.Clear();
        }

        private void HandleMessage(StompMessage message)
        {
            if (message == null || message.Command == null) return;
            if (!_actions.ContainsKey(message.Command)) return;

            _actions[message.Command](message);
        }
    }
}