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
    using NUnit.Framework;

    [TestFixture]
    public class StompMessageSerializerFixture
    {
        [Test]
        public void SerializingAStompMessage()
        {
            var msg = new StompMessage("CONNECT", "lorum!");
            msg["destination"] = "/my/queue";
            msg["foo"] = "bar";

            string r = new StompMessageSerializer().Serialize(msg);

            Assert.AreEqual(r, "CONNECT\ndestination:/my/queue\nfoo:bar\n\nlorum!\0");
        }

        [Test]
        public void DeserializingAStompMessage()
        {
            var msg = new StompMessageSerializer().Deserialize("CONNECT\ndestination:/my/queue\nfoo:  bar  \n\nlorum!\0");

            Assert.IsNotNull(msg);
            Assert.AreEqual(msg.Command, "CONNECT");
            Assert.AreEqual(msg.Body, "lorum!");
            Assert.AreEqual(msg.Headers.Count, 2);
            Assert.AreEqual(msg["destination"], "/my/queue");
            Assert.AreEqual(msg["foo"], "bar");
        }
    }
}