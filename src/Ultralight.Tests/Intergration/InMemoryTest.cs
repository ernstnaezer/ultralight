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
    public class InMemoryTest
    {
        private StompServer _server;
        private StompClient _client;

        [SetUp]
        public void Setup()
        {
            var inMemoryListener = new StompInMemoryListener();
            _server = new StompServer(inMemoryListener);
            _client = new StompClient(new InMemoryTransport(inMemoryListener));

            _server.Start();
            _client.Connect();
        }

        [Test]
        public void server_should_receive_messages_send_by_client()
        {
            Assert.That(_server.Queues, Is.Empty);

            _client.Send("123", "foo:bar");

            Thread.Sleep(TimeSpan.FromSeconds(3));

            var stompQueue = _server.Queues.First();
            var store = stompQueue.Store;

            Assert.That(store.HasMessages(), Is.True);
            string msg;
            store.TryDequeue(out msg);
            Assert.That(msg, Is.EqualTo("foo:bar"));
        }
    }
}