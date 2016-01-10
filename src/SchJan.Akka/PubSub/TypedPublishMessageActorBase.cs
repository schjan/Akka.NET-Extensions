using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;

namespace SchJan.Akka.PubSub
{
    public abstract class TypedPublishMessageActorBase : TypedActor, IHandle<SubscribeMessage>, IHandle<UnsubscribeMessage>, IHandle<Terminated>
    {
        private readonly bool _autoWatchSubscriber;

        /// <summary>
        /// Messagetypes you can subscribe to.
        /// </summary>
        protected IReadOnlyList<Type> SubscribableMessages { get; }

        /// <summary>
        /// Subscriber List in style of Tuple(ActorRef, MessageType)
        /// </summary>
        protected IList<Tuple<IActorRef, Type>> Subscribers { get; }

        protected TypedPublishMessageActorBase(bool autoWatchSubscriber = true)
        {
            _autoWatchSubscriber = autoWatchSubscriber;

            SubscribableMessages =
                Attribute.GetCustomAttributes(GetType())
                    .OfType<PublishMessageAttribute>()
                    .Select(attr => attr.MessageType)
                    .ToList();

            Subscribers = new List<Tuple<IActorRef, Type>>();
        }

        protected void PublishMessage<T>(T message)
            where T : class
        {
            var type = typeof(T);

            if (!SubscribableMessages.Contains(type))
            {
                Context.GetLogger().Info("PublishMessage of Type {0} failed. Type not valid", type.Name);
                return;
            }

            var subscribers = Subscribers.Where(x => x.Item2 == type);

            foreach (var subscriber in subscribers)
            {
                subscriber.Item1.Tell(message);
            }
        }

        /// <summary>
        /// Handles the <see cref="SubscribeMessage"/> to handle subscribtions.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Handle(SubscribeMessage message)
        {
            if (_autoWatchSubscriber)
                Context.Watch(message.Subscriber);

            if (Subscribers.Any(x => Equals(x.Item1, message.Subscriber) && x.Item2 == message.MessageType))
                return;

            if (!SubscribableMessages.Contains(message.MessageType))
                return;

            Subscribers.Add(new Tuple<IActorRef, Type>(message.Subscriber, message.MessageType));
        }

        /// <summary>
        /// Handles unsubscribtions.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Handle(UnsubscribeMessage message)
        {
            if (message.UnsubscribeAllTypes)
            {
                RemoveActorFromSubscribers(message.Unsubscriber);
            }
            else
                Subscribers.Remove(
                    Subscribers.FirstOrDefault(
                        x => Equals(x.Item1, message.Unsubscriber) && x.Item2 == message.MessageType));
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Handle(Terminated message)
        {
            RemoveActorFromSubscribers(message.ActorRef);
        }

        private void RemoveActorFromSubscribers(IActorRef actor)
        {
            if (actor == null)
                return;

            Context.Unwatch(actor);

            foreach (var unsubscriber in Subscribers.Where(x => Equals(x.Item1, actor)).ToList())
            {
                Subscribers.Remove(unsubscriber);
            }
        }
    }
}