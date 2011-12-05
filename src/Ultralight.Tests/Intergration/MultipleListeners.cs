namespace Ultralight.Tests.Intergration
{
    using System;
    using System.Linq;
    using System.Threading;
    using Client;
    using Client.Transport;
    using Listeners;
    using NUnit.Framework;

    [TestFixture]
    public class MultipleListeners
    {
        private StompServer _server;
        private StompClient _client1;
        private StompClient _client2;

        [SetUp]
        public void Setup()
        {
            var inMemoryListener = new StompInMemoryListener();
            var wsListener = new StompWebsocketListener("ws://localhost:8080/");

            _server = new StompServer(inMemoryListener, wsListener);
            _client1 = new StompClient(new InMemoryTransport(inMemoryListener));
            _client2 = new StompClient(new WebTransportTransport("ws://localhost:8080/"));

            _server.Start();

            _client1.Connect();
            _client2.Connect();
        }

        [Test]
        public void server_should_receive_messages_send_by_client()
        {
            Assert.That(_server.Queues, Is.Empty);

            _client1.Send("123", "foo:bar1");
            _client2.Send("123", "foo:bar2");

            Thread.Sleep(TimeSpan.FromSeconds(3));

            var stompQueue = _server.Queues.First();
            var store = stompQueue.Store;

            Assert.That(store.HasMessages(), Is.True);
            string msg;
            store.TryDequeue(out msg);
            Assert.That(msg, Is.EqualTo("foo:bar1"));

            store.TryDequeue(out msg);
            Assert.That(msg, Is.EqualTo("foo:bar2"));
        }
    }
}