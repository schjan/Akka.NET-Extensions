using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;

namespace SchJan.Akka.PubSub
{
    /// <summary>
    ///     Extensionmethods for <see cref="IPublishMessageActor" />
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IPublishMessageActorExtensions
    {
        /// <summary>
        ///     Removes all subscriptions of a given actor.
        /// </summary>
        /// <returns>True if <see cref="IActorRef" /> is not null.</returns>
        internal static bool RemoveFromSubscribers(this IPublishMessageActor self, IActorRef actor)
        {
            if (actor == null)
                return false;

            foreach (var unsubscriber in self.Subscribers.Where(x => Equals(x.Item1, actor)).ToList())
            {
                self.Subscribers.Remove(unsubscriber);
            }

            if (self.AutoWatchSubscriber)
                self.ActorContext.Unwatch(actor);

            return true;
        }

        internal static IReadOnlyList<Type> GetMessageTypesByAttributes(this IPublishMessageActor self)
        {
            return
                Attribute.GetCustomAttributes(self.GetType())
                    .OfType<PublishMessageAttribute>()
                    .Select(attr => attr.MessageType)
                    .ToArray();
        }

        /// <summary>
        /// Handles a <see cref="SubscribeMessage"/>
        /// </summary>
        /// <param name="self">The <see cref="IPublishMessageActor"/></param>
        /// <param name="message">The <see cref="SubscribeMessage"/></param>
        internal static void HandleSubscription(this IPublishMessageActor self, SubscribeMessage message)
        {
            self.HandleSubscriptionMessage(message);

            if (self.AutoWatchSubscriber)
                self.ActorContext.Watch(message.Subscriber);

            if (self.Subscribers.Any(x => Equals(x.Item1, message.Subscriber) && x.Item2 == message.MessageType))
            {
                if (message.Confirmation)
                    message.Subscriber.Tell(new SubscribedMessage(true));
                return;
            }

            if (!self.SubscribableMessages.Contains(message.MessageType))
            {
                if (message.Confirmation)
                    message.Subscriber.Tell(new SubscribedMessage(false));
                return;
            }



            self.Subscribers.Add(new Tuple<IActorRef, Type>(message.Subscriber, message.MessageType));

            if (message.Confirmation)
                message.Subscriber.Tell(new SubscribedMessage(true));
        }

        /// <summary>
        /// Handles an <see cref="UnsubscribeMessage"/>.
        /// </summary>
        /// <param name="self">The <see cref="IPublishMessageActor"/></param>
        /// <param name="message">The <see cref="UnsubscribeMessage"/></param>
        /// <returns>True if <see cref="IActorRef"/> is removed from subscribers.</returns>
        internal static bool HandleUnsubscription(this IPublishMessageActor self, UnsubscribeMessage message)
        {
            self.HandleUnsubscriptionMessage(message);

            if (message.UnsubscribeAllTypes)
                return RemoveFromSubscribers(self, message.Unsubscriber);

            var result = self.Subscribers.Remove(
                self.Subscribers.FirstOrDefault(
                    x => Equals(x.Item1, message.Unsubscriber) && x.Item2 == message.MessageType));

            if (result && self.AutoWatchSubscriber)
                self.ActorContext.Unwatch(message.Unsubscriber);

            return result;
        }

        /// <summary>
        /// Handles <see cref="Terminated"/>.
        /// </summary>
        /// <param name="self">The <see cref="IPublishMessageActor"/></param>
        /// <param name="message">The <see cref="Terminated"/></param>
        internal static void HandleTerminated(this IPublishMessageActor self, Terminated message)
        {
            self.HandleTerminationMessage(message);

            RemoveFromSubscribers(self, message.ActorRef);
        }

        /// <summary>
        ///     Publishes a message to all subscribers.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="message"></param>
        /// <returns><b>false</b> if message is not in subscribable messages.</returns>
        public static bool PublishMessage(this IPublishMessageActor self, object message)
        {
            // Do not use typeof (T) here! In some cases message is declared as object by compiler but of a different type at runtime.
            var type = message.GetType();

            if (!self.SubscribableMessages.Contains(type))
            {
                self.LogInfo("PublishMessage of Type {0} failed. Type not valid", type.Name);
                return false;
            }

            var subscribers = self.Subscribers.Where(x => x.Item2 == type);

            foreach (var subscriber in subscribers)
            {
                subscriber.Item1.Tell(message);
            }

            return true;
        }
    }
}