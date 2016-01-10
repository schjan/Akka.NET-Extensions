using System;
using Akka.Actor;

namespace SchJan.Akka.PubSub
{
    public class SubscribeMessage
    {
        public SubscribeMessage(IActorRef subscriber, Type messageType)
        {
            MessageType = messageType;
            Subscriber = subscriber;
        }

        public Type MessageType { get; }

        public IActorRef Subscriber { get; }
    }
}