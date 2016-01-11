using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;

namespace SchJan.Akka.PubSub
{
    /// <summary>
    ///     TypedActor which can publish defined types of messages to subscribers.
    /// </summary>
    public abstract class TypedPublishMessageActorBase : TypedActor, IHandle<SubscribeMessage>,
        IHandle<UnsubscribeMessage>, IHandle<Terminated>, IPublishMessageActor
    {
        private readonly bool _autoWatchSubscriber;

        /// <summary>
        ///     Creates a new instance of <see cref="TypedPublishMessageActorBase" />.
        /// </summary>
        /// <param name="autoWatchSubscriber">
        ///     True if actor should watch for <see cref="Terminated">Termination</see> of
        ///     subscribers.
        /// </param>
        protected TypedPublishMessageActorBase(bool autoWatchSubscriber = true)
        {
            _autoWatchSubscriber = autoWatchSubscriber;

            SubscribableMessages = this.GetMessageTypesByAttributes();

            Subscribers = new List<Tuple<IActorRef, Type>>();
        }

        /// <summary>
        ///     Handles the <see cref="SubscribeMessage" /> to handle subscribtions.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Handle(SubscribeMessage message)
        {
            if (_autoWatchSubscriber)
                Context.Watch(message.Subscriber);

            this.HandleSubscription(message);
        }

        /// <summary>
        ///     Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Handle(Terminated message)
        {
            if (this.RemoveFromSubscribers(message.ActorRef) && _autoWatchSubscriber)
                Context.Unwatch(message.ActorRef);
        }

        /// <summary>
        ///     Handles unsubscribtions.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Handle(UnsubscribeMessage message)
        {
            if (this.HandleUnsubscription(message) && _autoWatchSubscriber)
                Context.Unwatch(message.Unsubscriber);
        }

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
        ///     Subscriber List in style of Tuple(ActorRef, MessageType)
        /// </summary>
        public IList<Tuple<IActorRef, Type>> Subscribers { get; }
    }
}