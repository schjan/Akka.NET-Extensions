using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;

namespace SchJan.Akka.PubSub
{
    /// <summary>
    ///     ReceiveActor which can publish defined types of messages to subscribers.
    /// </summary>
    public class PublishMessageReceiveActorBase : ReceiveActor, IPublishMessageActor
    {
        /// <summary>
        ///     True if actor should watch for <see cref="Terminated">Termination</see> of subscribers.
        /// </summary>
        public bool AutoWatchSubscriber { get; }

        /// <summary>
        ///     Creates a new instance of <see cref="PublishMessageReceiveActorBase" />.
        /// </summary>
        /// <param name="autoWatchSubscriber">
        ///     True if actor should watch for <see cref="Terminated">Termination</see> of
        ///     subscribers.
        /// </param>
        public PublishMessageReceiveActorBase(bool autoWatchSubscriber = true)
        {
            AutoWatchSubscriber = autoWatchSubscriber;

            SubscribableMessages = this.GetMessageTypesByAttributes();

            Subscribers = new List<Tuple<IActorRef, Type>>();

            Receive<SubscribeMessage>(message =>
            {
                this.HandleSubscription(message);
            });

            Receive<UnsubscribeMessage>(message =>
            {
                this.HandleUnsubscription(message);
            });

            Receive<Terminated>(message =>
            {
                this.RemoveFromSubscribers(message.ActorRef);
            });
        }

        /// <summary>
        ///     Subscriber List in style of Tuple(ActorRef, MessageType)
        /// </summary>
        public IList<Tuple<IActorRef, Type>> Subscribers { get; }

        /// <summary>
        ///     Messagetypes you can subscribe to.
        /// </summary>
        public IReadOnlyList<Type> SubscribableMessages { get; }

        /// <summary>
        ///     Logs a message with the Info level.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void LogInfo(string format, params object[] args)
        {
            Context.GetLogger().Info(format, args);
        }

        /// <summary>
        ///     ActorContext Proxy.
        /// </summary>
        public IActorContext ActorContext => Context;
    }
}