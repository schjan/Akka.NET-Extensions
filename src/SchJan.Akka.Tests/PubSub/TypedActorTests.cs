using System;
using System.Collections.Generic;
using Akka.Actor;
using NUnit.Framework;
using SchJan.Akka.PubSub;
using SchJan.Akka.Tests.PubSub.Messages;

namespace SchJan.Akka.Tests.PubSub
{
    [TestFixture]
    public class TypedActorTests :
        PublishMessageActorBaseTests<TypedActorTests.TypedPublishMessageActorBaseProxy>
    {
        [PublishMessage(typeof (FooMessage))]
        [PublishMessage(typeof (TestMessage))]
        [PublishMessage(typeof (ActorUnsubscribedMessage))]
        public sealed class TypedPublishMessageActorBaseProxy : TypedPublishMessageActorBase,
            IHandle<AskMessageReceivedCountMessage>
        {
            private int _terminationMessages, _subscribeMessages, _unsubscribeMessages;

            public TypedPublishMessageActorBaseProxy()
                : base(true)
            {
            }

            public new IList<Tuple<IActorRef, Type>> Subscribers => base.Subscribers;

            public new IReadOnlyList<Type> SubscribableMessages => base.SubscribableMessages;

            protected override void Unhandled(object message)
            {
                Assert.Fail("Unhandled Message occured.");
            }

            public override void HandleTerminationMessage(Terminated message)
            {
                this.PublishMessage(new ActorUnsubscribedMessage(message.ActorRef, true));
                _terminationMessages++;

                base.HandleTerminationMessage(message);
            }

            public override void HandleUnsubscriptionMessage(UnsubscribeMessage message)
            {
                this.PublishMessage(new ActorUnsubscribedMessage(message.Unsubscriber, false));
                _unsubscribeMessages++;

                base.HandleUnsubscriptionMessage(message);
            }

            public override void HandleSubscriptionMessage(SubscribeMessage message)
            {
                _subscribeMessages++;

                base.HandleSubscriptionMessage(message);
            }

            public void Handle(AskMessageReceivedCountMessage message)
            {
                Sender.Tell(new MessageReceivedCountMessage(_subscribeMessages, _unsubscribeMessages,
                    _terminationMessages));
            }
        }
    }
}