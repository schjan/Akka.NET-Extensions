using System;
using System.Collections.Generic;
using Akka.Actor;
using NUnit.Framework;
using SchJan.Akka.PubSub;
using SchJan.Akka.Tests.PubSub.Messages;

namespace SchJan.Akka.Tests.PubSub
{
    [TestFixture]
    public class ReceiveActorTest :
        PublishMessageActorBaseTests<ReceiveActorTest.PublishMessageReceiveActorBaseProxy>
    {
        [PublishMessage(typeof (FooMessage))]
        [PublishMessage(typeof (TestMessage))]
        [PublishMessage(typeof(ActorUnsubscribedMessage))]
        public sealed class PublishMessageReceiveActorBaseProxy : PublishMessageReceiveActorBase
        {
            private int _terminationMessages, _subscribeMessages, _unsubscribeMessages;


            public PublishMessageReceiveActorBaseProxy()
                : base(true)
            {
                Receive<AskMessageReceivedCountMessage>(m =>
                {
                    Sender.Tell(new MessageReceivedCountMessage(_subscribeMessages, _unsubscribeMessages,
                        _terminationMessages));
                });

                ReceiveAny(m => { Assert.Fail("Unhandled Message occured."); });
            }

            public new IList<Tuple<IActorRef, Type>> Subscribers => base.Subscribers;

            public new IReadOnlyList<Type> SubscribableMessages => base.SubscribableMessages;

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