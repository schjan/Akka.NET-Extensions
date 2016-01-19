using System;

namespace SchJan.Akka.PubSub
{
    /// <summary>
    ///     Defines which MessageTypes a <see cref="TypedPublishMessageActorBase" /> can publish.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PublishMessageAttribute : Attribute
    {
        /// <summary>
        ///     Definiert welche Nachrichtentypen ein Aktor Publisht.
        /// </summary>
        /// <param name="messageType">Nachrichtentyp der Subscribed werden kann.</param>
        public PublishMessageAttribute(Type messageType)
        {
            MessageType = messageType;
        }

        /// <summary>
        ///     MessageType
        /// </summary>
        public Type MessageType { get; }
    }
}