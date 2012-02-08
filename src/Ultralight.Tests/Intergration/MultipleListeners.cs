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