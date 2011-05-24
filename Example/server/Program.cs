namespace Server
{
    using System;
    using Ultralight;
    using Ultralight.Listeners;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var wsListener = new StompWsListener(new Uri("ws://localhost:8181/"));

            wsListener
                .OnConnect += client =>
                                  {
                                      Console.WriteLine("a new client connected!");
                                      client.OnMessage +=
                                          msg =>
                                          Console.Out.WriteLine("msg received: {0} {1}", msg.Command, msg.Body);
                                  };

            var server = new StompServer(wsListener);
            server.Start();

            Console.Out.WriteLine("Press [Enter] to stop the server");
            Console.ReadLine();
        }
    }
}