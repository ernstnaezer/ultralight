Ultralight
==========
Ultralight is a small and fast, but partial, STOMP 1.0 message broker and client written in C#. 

Transports
----------
Out of the box Ultralight communicates via websockets. Other transport protocols can easily be added by implementing both the *IStompListener* and *IStompClient* interfaces and registering them at startup.

Supported commands
------------------
Currently Ultralight support a subset of the STOMP 1.0 commands:

* CONNECT
* SUBSCRIBE		(id is supported)
* UNSUBSCRIBE	(id is unsupported)
* SEND

The server can return with:

* MESSAGE
* ERROR
* RECEIPT

Transactions and ack commands are currently unsupported. Wildcards in queues names are ignored, they are treated as string values.

Sample
======
Check out the Example folder, a both a server and an HTML web client written by [Jeff Mesnil](https://github.com/jmesnil/stomp-websocket) are included.

When you start the server a new 'hello there' message is queued to demonstrate message buffering. The first chat client that connects receives this message, subsequent clients won't.

Protocol details
================
For more information about STOMP see:

* [STOMP at Github.com](http://stomp.github.com/)
* [1.0 protocol definitions](http://stomp.github.com/stomp-specification-1.0.html)
* [codehause project](http://stomp.codehaus.org/Protocol)

Credits
=======
* Websockets are provided by [Fleck](https://github.com/statianzo/Fleck)
* The code is loosly based on [acid stomp](https://github.com/danielbenzvi/acidstomp)
* The libraty uses the concurrent collections and it depends on the .net 4.x runtime.

License
-------
Copyright 2011 Ernst Naezer, et. al.
 
Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
this file except in compliance with the License. You may obtain a copy of the 
License at 

    http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software distributed 
under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
CONDITIONS OF ANY KIND, either express or implied. See the License for the 
specific language governing permissions and limitations under the License.

