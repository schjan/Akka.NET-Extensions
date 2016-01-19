using System;
using Akka.Actor;

namespace SchJan.Akka.PubSub
{
    /// <summary>
    ///     Message which is send to an <see cref="IPublishMessageActor" /> to unsubscribe from a defined message type.
    /// </summary>
    public class UnsubscribeMessage
    {
        /// <summary>
        ///     Creates a new <see cref="UnsubscribeMessage" /> to unsubscribe from a given Type of message.
        /// </summary>
        /// <param name="unsubscriber">Unsubscriber. Most of the time <i>self</i></param>
        /// <param name="messageType">MessageType to unsubscribe from.</param>
        public UnsubscribeMessage(IActorRef unsubscriber, Type messageType)
        {
            MessageType = messageType;
            UnsubscribeAllTypes = false;
            Unsubscriber = unsubscriber;
        }

        /// <summary>
        ///     Creates a new <see cref="UnsubscribeMessage" /> to unsubscribe from all message types of an actor.
        /// </summary>
        /// <param name="unsubscriber">Unsubscriber. Most of the time <i>self</i></param>
        public UnsubscribeMessage(IActorRef unsubscriber)
        {
            MessageType = null;
            UnsubscribeAllTypes = true;
            Unsubscriber = unsubscriber;
        }

        /// <summary>
        ///     MessageType
        /// </summary>
        public Type MessageType { get; }

        /// <summary>
        ///     True if you want to unsubscribe from all types.
        /// </summary>
        public bool UnsubscribeAllTypes { get; }

        /// <summary>
        ///     The <see cref="IActorRef" /> which unsubscribes.
        /// </summary>
        public IActorRef Unsubscriber { get; private set; }
    }
}