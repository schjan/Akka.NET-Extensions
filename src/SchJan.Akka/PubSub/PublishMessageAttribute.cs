using System;

namespace SchJan.Akka.PubSub
{
    /// <summary>
    /// Definiert welche Nachrichtentypen ein Aktor Publisht.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PublishMessageAttribute : Attribute
    {
        public Type MessageType { get; }

        /// <summary>
        ///  Definiert welche Nachrichtentypen ein Aktor Publisht.
        /// </summary>
        /// <param name="messageType">Nachrichtentyp der Subscribed werden kann.</param>
        public PublishMessageAttribute(Type messageType)
        {
            MessageType = messageType;
        }
    }
}
