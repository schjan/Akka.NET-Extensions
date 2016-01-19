using System;
using Akka.Actor;

namespace SchJan.Akka.PubSub
{
    /// <summary>
    ///     Message which is send to an <see cref="IPublishMessageActor" /> to subscribe to a defined message type.
    /// </summary>
    public class SubscribeMessage
    {
        /// <summary>
        ///     Creates a new <see cref="SubscribeMessage" />
        /// </summary>
        /// <param name="subscriber">Subscriber, most of the time <i>Self</i></param>
        /// <param name="messageType">MessageType to subscribe to.</param>
        public SubscribeMessage(IActorRef subscriber, Type messageType)
        {
            MessageType = messageType;
            Subscriber = subscriber;
        }

        /// <summary>
        ///     MessageType
        /// </summary>
        public Type MessageType { get; }

        /// <summary>
        ///     Subscriber
        /// </summary>
        public IActorRef Subscriber { get; }
    }
}