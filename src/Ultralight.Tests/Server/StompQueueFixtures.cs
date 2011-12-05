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
    using System.Linq;
    using MessageStore;
    using NUnit.Framework;

    [TestFixture]
    public class StompQueueFixtures
    {
        private static StompQueue GetStompQueue(string address)
        {
            return new StompQueue(address, new InMemoryMessageStore());
        }

        [Test]
        public void AddressShouldAlwaysStartWithASlash()
        {
            var q = GetStompQueue("queue/test");
            Assert.AreEqual(q.Address,"/queue/test");
        }

        [Test]
        public void AddingAClient_ShouldAddTheClientToTheClientList()
        {
            var q = GetStompQueue(string.Empty);
            var mockClient = new MockClient();
            q.AddClient(mockClient, string.Empty);

            Assert.That(q.Clients.Contains(mockClient));
        }
        
        [Test]
        public void AddingAClientTwice_ShouldAddTheClientToTheClientListOnce()
        {
            var q = GetStompQueue(string.Empty);
            var mockClient = new MockClient();
            q.AddClient(mockClient, string.Empty);
            q.AddClient(mockClient, string.Empty);

            Assert.That(q.Clients.Contains(mockClient));
            Assert.AreEqual(q.Clients.Length, 1);
        }

        [Test]
        public void AddingAClient_ShouldSubscribeToTheOnCloseAction()
        {
            var q = GetStompQueue(string.Empty);
            var mockClient = new MockClient();
            mockClient.OnClose += () => { };
            q.AddClient(mockClient, string.Empty);

            Assert.AreEqual(mockClient.OnClose.GetInvocationList().Length, 2);
        }
        
        [Test]
        public void RemovingAAClient_ShouldRemoveTheClientFromTheClientListAndUnsubscribeFromTheOnCloseEvent()
        {
            var q = GetStompQueue(string.Empty);
            var mockClient = new MockClient();
            mockClient.OnClose += () => { };
            q.AddClient(mockClient, string.Empty);

            Assert.That(q.Clients.Contains(mockClient));

            q.RemoveClient(mockClient);

            Assert.IsEmpty(q.Clients);
            Assert.AreEqual(mockClient.OnClose.GetInvocationList().Length, 1);
        }

        [Test]
        public void RemovingTheLastClient_ShouldTriggerTheLastClientRemovedEvent()
        {
            var q = GetStompQueue(string.Empty);
            var mockClient = new MockClient();
            q.AddClient(mockClient, string.Empty);

            var fired = false;
            q.OnLastClientRemoved += x => fired = true;

            Assert.IsFalse(fired);

            q.RemoveClient(mockClient);

            Assert.IsTrue(fired);
        }

        [Test]
        public void WhenAClientDisconnects_ItShouldBeRemovedFromTheQueue()
        {
            var q = GetStompQueue("/queue/test");
            var mockClient = new MockClient();

            q.AddClient(mockClient, string.Empty);
            Assert.IsNotEmpty(q.Clients);

            mockClient.OnClose();
            Assert.IsEmpty(q.Clients);
        }
    }
}