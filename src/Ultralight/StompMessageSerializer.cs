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

namespace Ultralight
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class StompMessageSerializer
    {
        /// <summary>
        ///   Serializes the specified message.
        /// </summary>
        /// <param name = "message">The message.</param>
        /// <returns>A serialized version of the given <see cref="StompMessage"/></returns>
        public string Serialize(StompMessage message)
        {
            var buffer =
                new StringBuilder((message.Body != null ? message.Body.Length : 0) + message.Command.Length +
                                  (message.Headers != null ? message.Headers.Count*32 : 0) + 100);

            buffer.Append(message.Command + "\n");

            if (message.Headers != null)
            {
                foreach (var header in message.Headers)
                {
                    buffer.Append(header.Key + ":" + header.Value + "\n");
                }
            }

            buffer.Append("\n");
            buffer.Append(message.Body);
            buffer.Append('\0');

            return buffer.ToString();
        }

        /// <summary>
        /// Deserializes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A <see cref="StompMessage"/> instance</returns>
        public StompMessage Deserialize(string message)
        {
            var reader = new StringReader(message);

            string command = reader.ReadLine();

            var headers = new Dictionary<string, string>();

            string header = reader.ReadLine();
            while (!string.IsNullOrEmpty(header))
            {
                string[] split = header.Split(':');
                if (split.Length < 2) throw new Exception("Malformed header: " + header);
                headers[split[0].Trim()] = split[1].Trim();
                header = reader.ReadLine() ?? string.Empty;
            }

            string body = reader.ReadToEnd() ?? string.Empty;
            body = body.TrimEnd('\r', '\n', '\0');

            return new StompMessage(command, body, headers);
        }
    }
}