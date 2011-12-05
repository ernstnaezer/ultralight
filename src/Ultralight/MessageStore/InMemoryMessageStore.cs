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

namespace Ultralight.MessageStore
{
    using System.Collections.Concurrent;

    public class InMemoryMessageStore
        : IMessageStore
    {
        private readonly ConcurrentQueue<string> _messages = new ConcurrentQueue<string>();

        /// <summary>
        /// Enqueues the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Enqueue(string message)
        {
            _messages.Enqueue(message);
        }

        /// <summary>
        /// Tries to dequeue a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public bool TryDequeue(out string message)
        {
            return _messages.TryDequeue(out message);
        }

        /// <summary>
        /// Determines whether this instance has messages.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance has messages; otherwise, <c>false</c>.
        /// </returns>
        public bool HasMessages()
        {
            return !_messages.IsEmpty;
        }
    }
}