Ultralight
==========

Ultralight is a small and fast STOMP message broker written in C# / .net 4.0. Out of the box it handles websockets connections 
but other transport protocols can easily be added by implementing both *IStompListener* and *IStompClient*.

Supported command
-----------------

Currently Ultralight support the following STOMP commands:

* CONNECT
* SUBSCRIBE		(id is supported)
* UNSUBSCRIBE	(id is unsupported)
* SEND

The server can return with

* MESSAGE
* ERROR
* RECEIPT

Transactions and ack commands are currently unsupported. Also wildcards in queues are ignored.

Sample
======
Check out the Example folder, a both a server and an HTML web client written by [Jeff Mesnil](https://github.com/jmesnil/stomp-websocket) are included.


Protocol details
================
For more information about stomp see the [codehause project](http://stomp.codehaus.org/Protocol)

Credits
================

Websockets are provided by [Fleck](https://github.com/statianzo/Fleck)
The code is loosly based on [acid stomp](https://github.com/danielbenzvi/acidstomp)

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

