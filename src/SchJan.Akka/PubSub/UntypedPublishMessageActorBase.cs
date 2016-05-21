using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;

namespace SchJan.Akka.PubSub
{
    /// <summary>
    ///     UntypedActor which can publish defined types of messages to subscribers.
    /// </summary>
    public abstract class UntypedPublishMessageActorBase : UntypedActor, IPublishMessageActor
    {

        /// <summary>
        ///     True if actor should watch for <see cref="Terminated">Termination</see> of subscribers.
        /// </summary>
        public bool AutoWatchSubscriber { get; }

        /// <summary>
        ///     Creates a new instance of <see cref="UntypedPublishMessageActorBase" />.
        /// </summary>
        /// <param name="autoWatchSubscriber">
        ///     True if actor should watch for <see cref="Terminated">Termination</see> of
        ///     subscribers.
        /// </param>
        /// <param name="messageTypes">
        ///     Explicit set Messagetypes in case attributes are not available.
        /// </param>
        protected UntypedPublishMessageActorBase(bool autoWatchSubscriber = true, params Type[] messageTypes)
        {
            AutoWatchSubscriber = autoWatchSubscriber;

            SubscribableMessages = this.GetMessageTypesByAttributes();
            if (messageTypes != null)
            {
                SubscribableMessages = SubscribableMessages.Concat(messageTypes).ToArray();
            }

            Subscribers = new List<Tuple<IActorRef, Type>>();
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
        ///     Handles the <see cref="SubscribeMessage" /> to handle subscribtions.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual void HandleSubscriptionMessage(SubscribeMessage message)
        {
        }

        /// <summary>
        ///     Handles a Terminated message.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual void HandleTerminationMessage(Terminated message)
        {
        }

        /// <summary>
        ///     Handles unsubscribtions.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual void HandleUnsubscriptionMessage(UnsubscribeMessage message)
        {
        }

        /// <summary>
        ///     Subscriber List in style of Tuple(ActorRef, MessageType)
        /// </summary>
        public IList<Tuple<IActorRef, Type>> Subscribers { get; }

        /// <summary>
        ///     Called when a message is received.
        /// </summary>
        /// <param name="message">The message.</param>
        protected override void OnReceive(object message)
        {
            if (message is SubscribeMessage)
            {
                this.HandleSubscription((SubscribeMessage) message);
            }
            else if (message is UnsubscribeMessage)
            {
                this.HandleUnsubscription((UnsubscribeMessage) message);
            }
            else if (message is Terminated)
            {
                this.HandleTerminated((Terminated) message);
            }
        }

        /// <summary>
        ///     ActorContext Proxy.
        /// </summary>
        public IActorContext ActorContext => Context;
    }
}