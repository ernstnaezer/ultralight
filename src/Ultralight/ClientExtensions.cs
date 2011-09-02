namespace Ultralight
{
    public static class ClientExtensions
    {
        /// <summary>
        ///   Sends a 'CONNECT' message
        /// </summary>
        public static void Connect(this IStompClient client)
        {
            var stompMsg = new StompMessage("CONNECT");

            client.Send(stompMsg);
        }

        /// <summary>
        ///   Sends a 'SEND' message
        /// </summary>
        public static void Send(this IStompClient client, string message, string destination)
        {
            var stompMsg = new StompMessage("SEND", message);
            stompMsg["destination"] = destination;

            client.Send(stompMsg);
        }

        /// <summary>
        ///   Sends a 'SUBSCRIBE' message
        /// </summary>
        public static void Subscribe(this IStompClient client, string destination)
        {
            var stompMsg = new StompMessage("SUBSCRIBE");
            stompMsg["destination"] = destination;

            client.Send(stompMsg);
        }

        /// <summary>
        ///   Sends an 'UNSUBSCRIBE' message
        /// </summary>
        public static void UnSubscribe(this IStompClient client, string destination)
        {
            var stompMsg = new StompMessage("UNSUBSCRIBE");
            stompMsg["destination"] = destination;

            client.Send(stompMsg);
        }
    }
}