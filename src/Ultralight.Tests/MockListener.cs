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
    using System;

    public class MockListener : IStompListener
    {
        public bool StopCalled { get; set; }

        /// <summary>
        ///   Start the listener
        /// </summary>
        public void Start()
        {
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            StopCalled = true;
        }

        /// <summary>
        ///   A new client connected
        /// </summary>
        public Action<IStompClient> OnConnect { get; set; }
    }
}