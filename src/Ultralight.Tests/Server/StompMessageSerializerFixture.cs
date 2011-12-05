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

            Assert.AreEqual(r, "CONNECT\ncontent-length:6\ndestination:/my/queue\nfoo:bar\n\nlorum!\0");
        }

        [Test]
        public void DeserializingAStompMessage()
        {
            var msg = new StompMessageSerializer().Deserialize("CONNECT\ndestination:/my/queue\nfoo:  bar  \n\nlorum!\0");

            Assert.IsNotNull(msg);
            Assert.AreEqual("CONNECT", msg.Command);
            Assert.AreEqual("lorum!", msg.Body);
            Assert.AreEqual(3, msg.Headers.Count);
            Assert.AreEqual("/my/queue", msg["destination"]);
            Assert.AreEqual("bar", msg["foo"]);
            Assert.AreEqual("6", msg["content-length"]);
        }       
       
        [Test]
        public void MalformedHeaderShouldBeIgnored()
        {
            var msg = new StompMessageSerializer().Deserialize("CONNECT\ndestination:/my/queue\nfoo\n\nlorum!\0");

            Assert.IsNotNull(msg);
            Assert.AreEqual("CONNECT", msg.Command);
            Assert.AreEqual("lorum!", msg.Body);
            Assert.AreEqual(2, msg.Headers.Count);
            Assert.AreEqual("/my/queue", msg["destination"]);
            Assert.AreEqual("6", msg["content-length"]);
        }
    }
}