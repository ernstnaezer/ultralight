namespace Ultralight.Client.Transport
{
    using System;
    using Listeners;

    public class InMemoryTransport
        : ITransport
    {
        private readonly StompMessageSerializer _serializer = new StompMessageSerializer();
        private readonly StompInMemoryListener _stompInMemoryListener;
        private readonly StompInMemoryClient _client = new StompInMemoryClient();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryTransport"/> class.
        /// </summary>
        /// <param name="stompInMemoryListener">The in memory listener.</param>
        public InMemoryTransport(StompInMemoryListener stompInMemoryListener)
        {
            _stompInMemoryListener = stompInMemoryListener;
        }

        public Action OnOpen { get; set; }
        public Action<string> OnMessage { get; set; }

        public void Connect()
        {
            _client.OnSend += m => OnMessage(_serializer.Serialize(m));

            if (_stompInMemoryListener.OnConnect != null)
            {
                _stompInMemoryListener.OnConnect(_client);
            }

            if (OnOpen != null) OnOpen();
        }

        public void Send(string message)
        {
            if (_client.OnMessage != null) _client.OnMessage(_serializer.Deserialize(message));
        }

        public void Close()
        {
        }
    }
}