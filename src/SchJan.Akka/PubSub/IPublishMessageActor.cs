using System;
using System.Collections.Generic;
using Akka.Actor;

namespace SchJan.Akka.PubSub
{
    /// <summary>
    ///     Interface for Actors which can publish defined types of messages to subscribers.
    /// </summary>
    public interface IPublishMessageActor
    {
        /// <summary>
        ///     List of Subscribers in style of Tuple(ActorRef, MessageType)
        /// </summary>
        IList<Tuple<IActorRef, Type>> Subscribers { get; }

        /// <summary>
        ///     Messagetypes you can subscribe to.
        /// </summary>
        IReadOnlyList<Type> SubscribableMessages { get; }

        /// <summary>
        ///     Logs a message with the Info level.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        void LogInfo(string format, params object[] args);
        
        /// <summary>
        ///     ActorContext Proxy.
        /// </summary>
        IActorContext ActorContext { get; }

        /// <summary>
        ///     True if actor should watch for <see cref="Terminated">Termination</see> of subscribers.
        /// </summary>
        bool AutoWatchSubscriber { get; }
    }
}