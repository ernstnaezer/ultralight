namespace Server
{
    using System;
    using Ultralight;
    using Ultralight.Client;
    using Ultralight.Client.Transport;
    using Ultralight.Listeners;

    internal class Program
    {
        private static void Main(string[] args)
        {
            const string address = "ws://localhost:8181/";
            var wsListener = new StompWebsocketListener(address);

            wsListener.OnConnect
                += stompClient =>
                       {
                           Console.WriteLine("a new client connected!");
                           stompClient.OnMessage += msg => Console.Out.WriteLine("msg received: {0} {1}", msg.Command, msg.Body);
                       };

            var server = new StompServer(wsListener);
            server.Start();

            var client = new StompClient(new WebTransportTransport(address));
            client.Connect();
            client.Send("/queue/test", "hi there. you are the first to connect");

            Console.Out.WriteLine("Press [Enter] to stop the server");
            Console.ReadLine();
        }
    }
}