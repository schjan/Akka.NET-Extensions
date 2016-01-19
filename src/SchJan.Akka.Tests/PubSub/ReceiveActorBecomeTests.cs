using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.TestKit.NUnit;
using NUnit.Framework;
using SchJan.Akka.PubSub;
using SchJan.Akka.Tests.PubSub.Messages;

namespace SchJan.Akka.Tests.PubSub
{
    [TestFixture]
    public class ReceiveActorBecomeTests : TestKit
    {
        [Test]
        public async void TestBecome()
        {
            var receiverActor = CreateTestProbe();

            var subject = Sys.ActorOf<BecomeActor>("subject");

            receiverActor.Send(subject, new SubscribeMessage(receiverActor, typeof (TestMessage)));

            receiverActor.Send(subject, "Hello");
            receiverActor.ExpectMsg<TestMessage>();


            receiverActor.Send(subject, "become");

            receiverActor.Send(subject, new SubscribeMessage(receiverActor, typeof (FooMessage)));
            receiverActor.Send(subject, new UnsubscribeMessage(receiverActor, typeof (TestMessage)));
            receiverActor.Send(subject, "Hello");

            receiverActor.ExpectMsg<FooMessage>();


            receiverActor.Send(subject, "unbecome");

            receiverActor.Send(subject, "Hello");
            receiverActor.ExpectNoMsg(TimeSpan.FromSeconds(0.5));

            var result =
                await
                    subject.Ask<MessageReceivedCountMessage>(new AskMessageReceivedCountMessage(),
                        TimeSpan.FromSeconds(0.5));

            Assert.That(result.SubscriptionMessages, Is.EqualTo(2));
            Assert.That(result.UnsubscriptionMessages, Is.EqualTo(1));
            Assert.That(result.TerminationMessages, Is.EqualTo(0));
        }

        [PublishMessage(typeof (FooMessage))]
        [PublishMessage(typeof (TestMessage))]
        public sealed class BecomeActor : ReceiveActorTests.PublishMessageReceiveActorBaseProxy
        {
            public BecomeActor()
            {
                Receive<string>(s =>
                {
                    if (s == "become")
                        BecomeStacked(Cool);
                    else
                        this.PublishMessage(new TestMessage("I am normal"));
                });
            }

            public void Cool()
            {
                RegisterPubSubMessageHandler();

                Receive<string>(s =>
                {
                    if (s == "unbecome")
                        UnbecomeStacked();
                    else
                        this.PublishMessage(new FooMessage("I am cool"));
                });
            }


            public new IList<Tuple<IActorRef, Type>> Subscribers => base.Subscribers;

            public new IReadOnlyList<Type> SubscribableMessages => base.SubscribableMessages;
        }
    }
}
