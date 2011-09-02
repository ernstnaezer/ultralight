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
    public static class TestHelpers
    {
        public static MockClient GetAConnectedClient(this IStompListener listener)
        {
            var client = new MockClient();
            listener.OnConnect(client);
            client.OnMessage(new StompMessage("CONNECT"));

            return client;
        }

        public static MockClient GetASubscribedClient(this IStompListener listener, string queue)
        {
            return listener.GetASubscribedClient(queue, string.Empty);
        }

        public static MockClient GetASubscribedClient(this IStompListener listener, string queue, string subscriptionId)
        {
            var client = listener.GetAConnectedClient();

            var message = new StompMessage("SUBSCRIBE");
            message["destination"] = queue;
            message["id"] = subscriptionId;

            client.OnMessage(message);

            return client;
        }
    }
}