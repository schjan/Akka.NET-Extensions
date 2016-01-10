using System;
using Akka.Actor;

namespace SchJan.Akka.PubSub
{
    public class UnsubscribeMessage
    {
        public UnsubscribeMessage(IActorRef unsubscriber, Type messageType)
        {
            MessageType = messageType;
            UnsubscribeAllTypes = false;
            Unsubscriber = unsubscriber;
        }

        public UnsubscribeMessage(IActorRef unsubscriber)
        {
            MessageType = null;
            UnsubscribeAllTypes = true;
            Unsubscriber = unsubscriber;
        }

        public Type MessageType { get; }

        public bool UnsubscribeAllTypes { get; }
        
        public IActorRef Unsubscriber { get; private set; }
    }
}
