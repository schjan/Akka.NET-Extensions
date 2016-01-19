using System;
using System.Collections.Generic;
using Akka.Actor;
using NUnit.Framework;
using SchJan.Akka.PubSub;
using SchJan.Akka.Tests.PubSub.Messages;

namespace SchJan.Akka.Tests.PubSub
{
    public class UntypedActorTests :
        PublishMessageActorBaseTests<UntypedActorTests.UntypedPublishMessageActorBaseProxy>
    {
        [PublishMessage(typeof (FooMessage))]
        [PublishMessage(typeof (TestMessage))]
        [PublishMessage(typeof(ActorUnsubscribedMessage))]
        public sealed class UntypedPublishMessageActorBaseProxy : UntypedPublishMessageActorBase
        {
            private int _terminationMessages, _subscribeMessages, _unsubscribeMessages;

            public UntypedPublishMessageActorBaseProxy()
                : base(true)
            {
            }

            public new IList<Tuple<IActorRef, Type>> Subscribers => base.Subscribers;

            public new IReadOnlyList<Type> SubscribableMessages => base.SubscribableMessages;

            protected override void OnReceive(object message)
            {
                if (message is AskMessageReceivedCountMessage)
                {
                    Sender.Tell(new MessageReceivedCountMessage(_subscribeMessages, _unsubscribeMessages,
                        _terminationMessages));
                    return;
                }

                base.OnReceive(message);
            }

            public override void HandleTerminationMessage(Terminated message)
            {
                this.PublishMessage(new ActorUnsubscribedMessage(message.ActorRef, true));
                _terminationMessages++;
            }

            public override void HandleUnsubscriptionMessage(UnsubscribeMessage message)
            {
                this.PublishMessage(new ActorUnsubscribedMessage(message.Unsubscriber, false));
                _unsubscribeMessages++;
            }

            public override void HandleSubscriptionMessage(SubscribeMessage message)
            {
                _subscribeMessages++;
            }

            protected override void Unhandled(object message)
            {
                Assert.Fail("Unhandled Message occured.");
            }
        }
    }
}