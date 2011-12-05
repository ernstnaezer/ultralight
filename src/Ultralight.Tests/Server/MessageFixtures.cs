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

namespace Ultralight.Tests.Server
{
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class MessageFixtures
    {
        private MockListener _listener;
        private StompServer _server;

        [SetUp]
        public void Setup()
        {
            _listener = new MockListener();
            _server = new StompServer(_listener);
            _server.Start();
        }

        [Test]
        public void when_sending_a_message_to_a_unexisting_queue_the_message_should_be_stored()
        {
            var client = _listener.GetAConnectedClient();
            client.Send("hi there", "/new/queue");

            Assert.IsNotEmpty(_server.Queues,"there should be a queue");

            var queue = _server.Queues.First();

            Assert.IsEmpty(queue.Clients,"clients should be empty");
            Assert.IsTrue(queue.Store.HasMessages(), "queue should not be empty");

            string message;
            Assert.IsTrue(queue.Store.TryDequeue(out message));
            Assert.AreEqual(message, "hi there");
        }

        [Test]
        public void when_connecting_to_a_queue_with_messages_the_client_should_receive_them()
        {
            var client1 = _listener.GetAConnectedClient();
            client1.Send("hi there", "/new/queue");
            client1.Send("and hello again", "/new/queue");

            var client2 = _listener.GetAConnectedClient();
            
            var receivedMessages1 = new List<StompMessage>();
            client2.OnServerMessage += receivedMessages1.Add;
            client2.Subscribe("/new/queue");

            Assert.IsNotEmpty(receivedMessages1, "there should be received messages");

            // check order
            Assert.AreEqual(receivedMessages1[0].Body, "hi there");
            Assert.AreEqual(receivedMessages1[1].Body, "and hello again");
            
            var client3 = _listener.GetAConnectedClient();
            
            var receivedMessages2 = new List<StompMessage>();
            client3.OnServerMessage += receivedMessages2.Add;
            client3.Subscribe("/new/queue");

            Assert.IsEmpty(receivedMessages2, "there should be no received messages");
        }        
    }
}