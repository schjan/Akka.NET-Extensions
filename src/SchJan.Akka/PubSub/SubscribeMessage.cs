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
        /// <param name="confirmation">Set to true if subscriber wants to receive a confirmation</param>
        public SubscribeMessage(IActorRef subscriber, Type messageType, bool confirmation = false)
        {
            MessageType = messageType;
            Subscriber = subscriber;
            Confirmation = confirmation;
        }

        /// <summary>
        ///     MessageType
        /// </summary>
        public Type MessageType { get; }

        /// <summary>
        ///     Subscriber
        /// </summary>
        public IActorRef Subscriber { get; }

        /// <summary>
        ///     True if subscriber wants to receive a confirmation.
        /// </summary>
        public bool Confirmation { get; }
    }
}