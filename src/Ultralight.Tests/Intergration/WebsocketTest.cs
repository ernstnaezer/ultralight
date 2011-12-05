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
    public class WebsocketTest
    {
        private StompServer _server;
        private StompClient _client;

        [SetUp]
        public void Setup()
        {
            const string pipeName = "ws://localhost:8080/";

            _server = new StompServer(new StompWebsocketListener(pipeName));
            _client = new StompClient(new WebTransportTransport(pipeName));

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

        [Test]
        public void disconnecting_the_client_should_remove_all_traces()
        {
            Assert.That(_server.Queues, Is.Empty, "initial queue list should be empty");

            _client.Subscribe("reutel");
            Thread.Sleep(TimeSpan.FromSeconds(2));

            Assert.That(_server.Queues, Is.Not.Empty, "There should have been a queue called 'reutel'");

            _client.Disconnect();

            Thread.Sleep(TimeSpan.FromSeconds(2));

            Assert.That(_server.Queues, Is.Empty, "All queues should have been removed");
        }
    }
}