namespace Ultralight.Listeners
{
    using System;

    public class StompInMemoryClient
        : IStompClient
    {
        public Action OnClose { get; set; }
        public Guid SessionId { get; set; }

        public Action<StompMessage> OnSend { get; set; }
        public Action<StompMessage> OnMessage { get; set; }

        public void Send(StompMessage message)
        {
            if (OnSend != null) OnSend(message);
        }

        public void Close()
        {
        }

        public int CompareTo(IStompClient other)
        {
            return SessionId.CompareTo(other.SessionId);
        }

        public bool Equals(StompInMemoryClient other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || other.SessionId.Equals(SessionId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof (StompInMemoryClient) && Equals((StompInMemoryClient) obj);
        }

        public override int GetHashCode()
        {
            return SessionId.GetHashCode();
        }
    }
}