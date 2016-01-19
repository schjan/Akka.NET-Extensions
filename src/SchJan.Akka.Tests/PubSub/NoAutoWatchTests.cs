using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.TestKit.NUnit;
using NUnit.Framework;
using SchJan.Akka.PubSub;
using SchJan.Akka.Tests.PubSub.Actors;
using SchJan.Akka.Tests.PubSub.Messages;

namespace SchJan.Akka.Tests.PubSub
{
    [TestFixture]
    public class NoAutoWatchTests : TestKit
    {
        [Test]
        public void Subscribe()
        {
            var subject = Sys.ActorOf<NoAutowatchActor>("subject");

            var testProbe = CreateTestProbe();

            testProbe.Send(subject, new SubscribeMessage(testProbe, typeof(FooMessage)));

            subject.Tell(new FooMessage("Hello"));

            testProbe.ExpectMsg<FooMessage>(m => m.Content == "Hello");
        }

        [Test]
        public void NoTerminatedUnsubscribing()
        {
            var subject = ActorOfAsTestActorRef<NoAutowatchActor>("subject");

            var testProbe = Sys.ActorOf(Props.Create(() => new TestTerminationActor(subject)));

            testProbe.Tell(PoisonPill.Instance);

            subject.Tell(new FooMessage("Hello"));

            Assert.That(subject.UnderlyingActor.Subscribers,
                Contains.Item(new Tuple<IActorRef, Type>(testProbe, typeof (FooMessage))));
        }

        [PublishMessage(typeof(FooMessage))]
        [PublishMessage(typeof(ActorUnsubscribedMessage))]
        public class NoAutowatchActor : PublishMessageReceiveActorBase
        {
            private int _terminationMessages, _subscribeMessages, _unsubscribeMessages;


            public NoAutowatchActor()
                : base(false)
            {
                Receive<AskMessageReceivedCountMessage>(m =>
                {
                    Sender.Tell(new MessageReceivedCountMessage(_subscribeMessages, _unsubscribeMessages,
                        _terminationMessages));
                });

                Receive<FooMessage>(message => this.PublishMessage(message));
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
