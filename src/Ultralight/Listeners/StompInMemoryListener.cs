namespace Ultralight.Listeners
{
    using System;

    public class StompInMemoryListener
        : IStompListener
    {
        public void Start()
        {
        }

        public void Stop()
        {            
        }

        public Action<IStompClient> OnConnect { get; set; }
    }
}