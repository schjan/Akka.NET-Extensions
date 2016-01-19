using System;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using NUnit.Framework;
using SchJan.Akka.PubSub;
using SchJan.Akka.Tests.PubSub.Messages;

namespace SchJan.Akka.Tests.PubSub
{
    [TestFixture]
    public abstract class PublishMessageActorBaseTests<T> : TestKit
        where T : ActorBase, IPublishMessageActor, new()
    {
        #region TestActorTests

        public TestActorRef<T> SetUpTestActorRef()
        {
            return ActorOfAsTestActorRef<T>(Props.Create(() => new T()));
        }

        [Test(Description = "Get subscribable messages by attributes.")]
        public void AttributeTest()
        {
            var subject = SetUpTestActorRef();

            CollectionAssert.AreEquivalent(
                new[] {typeof (FooMessage), typeof (TestMessage), typeof (ActorUnsubscribedMessage)},
                subject.UnderlyingActor.SubscribableMessages);
        }

        [Test(Description = "Unsubscribe from all Messages")]
        public void TestActorNullUnsubscribe()
        {
            var subject = SetUpTestActorRef();

            var testProbe = CreateTestProbeAndSubscribeToFooAndTest(subject);

            testProbe.Send(subject, new UnsubscribeMessage(null));

            Assert.That(subject.UnderlyingActor.Subscribers.Count, Is.EqualTo(2));
        }

        [Test(Description = "Publish a message and receive it.")]
        public void TestPublishMessage()
        {
            var subject = SetUpTestActorRef();

            var testProbe = CreateTestProbeAndSubscribeToFoo(subject);

            subject.UnderlyingActor.PublishMessage(new FooMessage("FooFoo"));

            testProbe.ExpectMsg<FooMessage>();
        }


        [Test(Description = "Publish a message and dont receive it.")]
        public void TestPublishNotSubscribedMessage()
        {
            var subject = SetUpTestActorRef();

            var testProbe = CreateTestProbeAndSubscribeToFoo(subject);

            subject.UnderlyingActor.PublishMessage(new TestMessage("TestFoo"));

            testProbe.ExpectNoMsg(TimeSpan.FromSeconds(0.3));
        }

        [Test(Description = "Publish a message that is not valid to subscribe to.")]
        public void TestPublishNotSupporteddMessage()
        {
            var subject = SetUpTestActorRef();

            var testProbe = CreateTestProbeAndSubscribeToFoo(subject);

            EventFilter.Info("PublishMessage of Type OopsMessage failed. Type not valid").ExpectOne(() =>
            {
                subject.UnderlyingActor.PublishMessage(new OopsMessage("Oops"));

                testProbe.ExpectNoMsg(TimeSpan.FromSeconds(0.3));
            });
        }

        [Test(Description = "Subscribe to a Message")]
        public void TestSubscribeMessage()
        {
            var subject = SetUpTestActorRef();

            var testProbe = CreateTestProbe();

            testProbe.Send(subject, new SubscribeMessage(testProbe, typeof (TestMessage)));

            Assert.That(subject.UnderlyingActor.Subscribers.First().Item1, Is.EqualTo(testProbe));
            Assert.That(subject.UnderlyingActor.Subscribers.First().Item2, Is.EqualTo(typeof (TestMessage)));
            Assert.That(subject.UnderlyingActor.Subscribers.Count, Is.EqualTo(1));
        }

        [Test(Description = "Subscribe to a message which is not supported.")]
        public void TestSubscribeMessageToNoType()
        {
            var subject = SetUpTestActorRef();

            var testProbe = CreateTestProbe();

            testProbe.Send(subject, new SubscribeMessage(testProbe, typeof (OopsMessage)));

            Assert.That(subject.UnderlyingActor.Subscribers.Count, Is.EqualTo(0));
        }

        [Test(Description = "Subscribe to a Message twice")]
        public void TestSubscribeMessageTwice()
        {
            var subject = SetUpTestActorRef();

            var testProbe = CreateTestProbe();

            testProbe.Send(subject, new SubscribeMessage(testProbe, typeof (FooMessage)));

            Assert.That(subject.UnderlyingActor.Subscribers.First().Item1, Is.EqualTo(testProbe));
            Assert.That(subject.UnderlyingActor.Subscribers.First().Item2, Is.EqualTo(typeof (FooMessage)));
            Assert.That(subject.UnderlyingActor.Subscribers.Count, Is.EqualTo(1));

            testProbe.Send(subject, new SubscribeMessage(testProbe, typeof (FooMessage)));

            Assert.That(subject.UnderlyingActor.Subscribers.First().Item1, Is.EqualTo(testProbe));
            Assert.That(subject.UnderlyingActor.Subscribers.First().Item2, Is.EqualTo(typeof (FooMessage)));
            Assert.That(subject.UnderlyingActor.Subscribers.Count, Is.EqualTo(1));
        }

        [Test(Description = "Unsubscribe only from one Message")]
        public void TestUnsubscribe()
        {
            var subject = SetUpTestActorRef();

            var testProbe = CreateTestProbeAndSubscribeToFooAndTest(subject);

            testProbe.Send(subject, new UnsubscribeMessage(testProbe, typeof (FooMessage)));

            Assert.That(subject.UnderlyingActor.Subscribers.First().Item1, Is.EqualTo(testProbe));
            Assert.That(subject.UnderlyingActor.Subscribers.First().Item2, Is.EqualTo(typeof (TestMessage)));
            Assert.That(subject.UnderlyingActor.Subscribers.Count, Is.EqualTo(1));
        }

        [Test(Description = "Unsubscribe from all Messages")]
        public void TestUnsubscribeAll()
        {
            var subject = SetUpTestActorRef();

            var testProbe = CreateTestProbeAndSubscribeToFooAndTest(subject);

            testProbe.Send(subject, new UnsubscribeMessage(testProbe));

            Assert.That(subject.UnderlyingActor.Subscribers.Count, Is.EqualTo(0));
        }

        #region Helper methods

        private TestProbe CreateTestProbeAndSubscribeToFoo(TestActorRef<T> subject)
        {
            var testProbe = CreateTestProbe();
            subject.UnderlyingActor.Subscribers.Add(new Tuple<IActorRef, Type>(testProbe, typeof (FooMessage)));

            return testProbe;
        }

        private TestProbe CreateTestProbeAndSubscribeToFooAndTest(TestActorRef<T> subject)
        {
            var testProbe = CreateTestProbeAndSubscribeToFoo(subject);
            subject.UnderlyingActor.Subscribers.Add(new Tuple<IActorRef, Type>(testProbe, typeof (TestMessage)));

            return testProbe;
        }

        #endregion

        #endregion

        #region Async Tests

        public IActorRef SetUpActorRef()
        {
            return Sys.ActorOf<T>();
        }

        [Test]
        public async void UnsubscribeWatchTermination()
        {
            var subject = SetUpActorRef();

            var receiverActor = CreateTestProbe();
            receiverActor.Send(subject,
                new SubscribeMessage(receiverActor, typeof (ActorUnsubscribedMessage)));

            var terminatedActor = Sys.ActorOf(Props.Create(() => new TestTerminationActor(subject)));


            terminatedActor.Tell(PoisonPill.Instance);


            receiverActor.ExpectMsg<ActorUnsubscribedMessage>(msg => msg.Terminated && msg.Actor.Equals(terminatedActor));

            var result = await subject.Ask<MessageReceivedCountMessage>(new AskMessageReceivedCountMessage());

            Assert.That(result.SubscriptionMessages, Is.EqualTo(2));
            Assert.That(result.UnsubscriptionMessages, Is.EqualTo(0));
            Assert.That(result.TerminationMessages, Is.EqualTo(1));
        }

        [Test]
        public async void UnsubscribeWatchUnsubscription()
        {
            var subject = SetUpActorRef();

            var receiverActor = CreateTestProbe();
            receiverActor.Send(subject,
                new SubscribeMessage(receiverActor, typeof (ActorUnsubscribedMessage)));

            var unsubscriber = CreateTestProbe();
            unsubscriber.Send(subject,
                new SubscribeMessage(unsubscriber, typeof (ActorUnsubscribedMessage)));

            unsubscriber.Send(subject, new UnsubscribeMessage(unsubscriber));

            receiverActor.ExpectMsg<ActorUnsubscribedMessage>(
                msg => !msg.Terminated && msg.Actor.Equals(unsubscriber));

            var result = await subject.Ask<MessageReceivedCountMessage>(new AskMessageReceivedCountMessage());

            Assert.That(result.SubscriptionMessages, Is.EqualTo(2));
            Assert.That(result.UnsubscriptionMessages, Is.EqualTo(1));
            Assert.That(result.TerminationMessages, Is.EqualTo(0));
        }

        // TestTerminationActor, because Termination doesn't work well with TestProbe.
        public class TestTerminationActor : ReceiveActor
        {
            public TestTerminationActor(IActorRef actorToSubscribe)
            {
                actorToSubscribe.Tell(new SubscribeMessage(Self, typeof (ActorUnsubscribedMessage)));
            }

            protected override void PostStop()
            {
                Context.GetLogger().Warning("TestActor Terminated");
            }
        }

        #endregion
    }
}