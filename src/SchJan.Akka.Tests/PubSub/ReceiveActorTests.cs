using System;
using System.Collections.Generic;
using Akka.Actor;
using NUnit.Framework;
using SchJan.Akka.PubSub;
using SchJan.Akka.Tests.PubSub.Messages;

namespace SchJan.Akka.Tests.PubSub
{
    [TestFixture]
    public class ReceiveActorTests :
        PublishMessageActorBaseTests<ReceiveActorTests.PublishMessageReceiveActorBaseProxy>
    {
        [PublishMessage(typeof (FooMessage))]
        [PublishMessage(typeof (TestMessage))]
        [PublishMessage(typeof (ActorUnsubscribedMessage))]
        public class PublishMessageReceiveActorBaseProxy : PublishMessageReceiveActorBase
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
            }

            public new IList<Tuple<IActorRef, Type>> Subscribers => base.Subscribers;

            public new IReadOnlyList<Type> SubscribableMessages => base.SubscribableMessages;

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

            protected override void Unhandled(object message)
            {
                Assert.Fail("Unhandled Message of Type {0} occured.", message.GetType());
            }
        }
    }
}