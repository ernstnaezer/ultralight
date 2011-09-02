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

namespace Ultralight.Tests
{
    using System.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class StompServerFixtures
    {
        private StompServer _server;
        private MockListener _listener;

        [SetUp]
        public void Setup()
        {
            _listener = new MockListener();
            _server = new StompServer(_listener);

            _server.Start();
        }

        [Test]
        public void WhenAClientConnects_ItShouldGetAConnectedResponseMessage()
        {
            var client = new MockClient();
            _listener.OnConnect(client);

            StompMessage r = null;
            client.OnServerMessage += m => r = m;

            client.OnMessage(new StompMessage("CONNECT"));

            Assert.IsNotNull(r);
            Assert.AreEqual(r.Command, "CONNECTED");
            Assert.AreEqual(r.Body, string.Empty);
            Assert.AreEqual(r["session-id"], client.SessionId.ToString());

            Assert.IsTrue(client.IsConnected());
        }

        [Test]
        public void WhenAClientSubscribesToANonExistingQueue_TheQueueShouldBeCreatedAndTheClientShouldBeAdded()
        {
            var client = _listener.GetAConnectedClient();

            var message = new StompMessage("SUBSCRIBE");
            message["destination"] = "/queue/test";

            Assert.IsEmpty(_server.Queues);

            client.OnMessage(message);

            Assert.IsNotEmpty(_server.Queues);
            Assert.That(_server.Queues.First().Address.Equals("/queue/test"));
            Assert.That(_server.Queues.First().Clients.Contains(client));
        }

        [Test]
        public void WhenAClientSubscribesToAnExistingQueue_TheClientShouldBeAdded()
        {
            var client1 = _listener.GetASubscribedClient("/test");
            var client2 = _listener.GetASubscribedClient("/test");

            Assert.That(_server.Queues.First().Clients.Contains(client1));
            Assert.That(_server.Queues.First().Clients.Contains(client2));
        }

        [Test]
        public void WhenAClientUnSubscribes_TheClientShouldBeRemovedFromTheQueue()
        {
            var client1 = _listener.GetASubscribedClient("/test");
            var client2 = _listener.GetASubscribedClient("/test");

            var message = new StompMessage("UNSUBSCRIBE");
            message["destination"] = "/test";
            client2.OnMessage(message);

            Assert.That(_server.Queues.First().Clients.Contains(client1));
            Assert.That(_server.Queues.First().Clients.Contains(client2) == false);
        }
        
        [Test]
        public void WhenAClientUnSubscribesFromAnInValidQueue_AnErrorShouldBeReturned()
        {
            var client = _listener.GetASubscribedClient("/test");

            var message = new StompMessage("UNSUBSCRIBE");
            message["destination"] = "/test2";
            
            StompMessage r = null;
            client.OnServerMessage += m => r = m;
            client.OnMessage(message);

            Assert.That(_server.Queues.First().Clients.Contains(client));
            
            Assert.NotNull(r);
            Assert.AreEqual(r.Command,"ERROR");
            Assert.AreEqual(r.Body, "You are not subscribed to queue '/test2'");
        }

        [Test]
        public void WhenTheLastClientUnSubscribs_TheQueueShouldBeRemoved()
        {
            var client = _listener.GetASubscribedClient("/test");

            Assert.IsNotEmpty(_server.Queues);

            var message = new StompMessage("UNSUBSCRIBE");
            message["destination"] = "/test";
            client.OnMessage(message);

            Assert.IsEmpty(_server.Queues);
        }

        [Test]
        public void WhenAClientDisconnects_ItsEventHandlersShouldBeCleared()
        {
            var client = _listener.GetAConnectedClient();
            
            Assert.IsNotNull(client.OnClose);

            client.OnClose();

            Assert.IsNull(client.OnClose);            
        }

        [Test]
        public void WhenASubscribedClientDisconnects_ItsEventHandlersShouldBeClearedAndItShouldBeRemovedFromTheQueue()
        {
            var client = _listener.GetASubscribedClient("/test");

            Assert.IsNotEmpty(_server.Queues);

            client.OnClose();

            Assert.IsEmpty(_server.Queues);
            Assert.IsNull(client.OnClose);            
        }

        [Test]
        public void WhenAClientSendsAnInvalidCommand_ItShouldBeIgnored()
        {
            var client = new MockClient();
            _listener.OnConnect(client);

            client.OnMessage(new StompMessage("INVALID"));
        }

        [Test]
        public void WhenAnUnconnectedClientIssuesACommandOtherThenConnect_AnErrorShouldBeReturned()
        {
            var client = new MockClient();
            _listener.OnConnect(client);
            
            StompMessage r = null;
            client.OnServerMessage += m => r = m;
            
            client.OnMessage(new StompMessage("SUBSCRIBE"));

            Assert.IsNotNull(r);
            Assert.AreEqual(r.Command, "ERROR");
            Assert.AreEqual(r.Body, "Please connect before sending 'SUBSCRIBE'");
        }

        [Test]
        public void WhenASubscribedClientSendsAMessage_ItShouldBePublishedToAllTheClienstInTheQueue()
        {
            var client1 = _listener.GetASubscribedClient("/test");
            var client2 = _listener.GetASubscribedClient("/test");

            int cnt = 0;
            client1.OnServerMessage = msg => { if (msg.Command == "MESSAGE" && msg.Body == "my body" && msg["destination"] == "/test") cnt++; };
            client2.OnServerMessage = msg => { if (msg.Command == "MESSAGE" && msg.Body == "my body" && msg["destination"] == "/test") cnt++; };
                
            var message = new StompMessage("SEND","my body");
            message["destination"] = "/test";
            client1.OnMessage(message);
            
            Assert.AreEqual(cnt, 2);
        }

        [Test]
        public void WhenASubscribedClientSendsAMessageToANonSubscribedQueue_TheMessageShouldBePublished()
        {
            var unsubscribedClient = _listener.GetAConnectedClient();
            var client = _listener.GetASubscribedClient("/test");

            var message = new StompMessage("SEND","Hi, you don't know me but...");
            message["destination"] = "/test";

            StompMessage r = null;
            client.OnServerMessage += m => r = m;

            unsubscribedClient.OnMessage(message);

            Assert.That(_server.Queues.First().Clients.Contains(client));

            Assert.NotNull(r);
            Assert.AreEqual(r.Command, "MESSAGE");
            Assert.AreEqual(r.Body, "Hi, you don't know me but...");
        }
        
        [Test]
        public void WhenClientIncludedAnIdOnSubscription_TheIdShouldBeInTheMessageResponse()
        {
            var client1 = _listener.GetASubscribedClient("/test", "123");
            var client2 = _listener.GetASubscribedClient("/test", "456");

            var message = new StompMessage("SEND");
            message["destination"] = "/test";

            StompMessage r1 = null;
            client1.OnServerMessage += m => r1 = m;
            StompMessage r2 = null;
            client2.OnServerMessage += m => r2 = m;

            client1.OnMessage(message);

            Assert.AreEqual(r1.Command, "MESSAGE");
            Assert.AreEqual(r1["subscription"], "123");

            Assert.AreEqual(r2.Command, "MESSAGE");
            Assert.AreEqual(r2["subscription"], "456");            
        }

        [Test]
        public void WhenRequestingAReceipt_AReceiptFrameShouldBeReturned()
        {
            var client = _listener.GetAConnectedClient();

            var message = new StompMessage("SUBSCRIBE");
            message["destination"] = "/test";
            message["receipt"] = "message-12345";

            StompMessage r = null;
            client.OnServerMessage += x => r = x;
            client.OnMessage(message);

            Assert.IsNotNull(r);
            Assert.AreEqual(r.Command,"RECEIPT");
            Assert.AreEqual(r["receipt-id"], "message-12345");
        }

        [Test]
        public void WhenClosingTheServer_ConnectedClientsShouldBeDisconnected_StopListenerShouldBeCalled()
        {
            var client = _listener.GetASubscribedClient("/test", "123");

            var called = false;
            client.OnClose += () => { called = true; };

            _server.Stop();

            Assert.IsTrue(called);
            Assert.IsTrue(_listener.StopCalled);
        }
    }
}