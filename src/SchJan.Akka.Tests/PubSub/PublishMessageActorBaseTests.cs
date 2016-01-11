using System;
using System.Linq;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using NUnit.Framework;
using SchJan.Akka.PubSub;

namespace SchJan.Akka.Tests.PubSub
{
    [TestFixture]
    public abstract class PublishMessageActorBaseTests<T> : TestKit
        where T : ActorBase, IPublishMessageActor, new()
    {
        public TestActorRef<T> Subject;

        [SetUp]
        public void SetUp()
        {
            Subject = ActorOfAsTestActorRef<T>(Props.Create(() => new T()));
        }

        [Test(Description = "Get subscribable messages by attributes.")]
        public void AttributeTest()
        {
            CollectionAssert.AreEquivalent(new[] {typeof (FooMessage), typeof (TestMessage)},
                Subject.UnderlyingActor.SubscribableMessages);
        }

        [Test(Description = "Unsubscribe from all Messages")]
        public void TestActorNullUnsubscribe()
        {
            var testProbe = CreateTestProbeAndSubscribeToFooAndTest();

            testProbe.Send(Subject, new UnsubscribeMessage(null));

            Assert.That(Subject.UnderlyingActor.Subscribers.Count, Is.EqualTo(2));
        }

        [Test(Description = "Publish a message and receive it.")]
        public void TestPublishMessage()
        {
            var testProbe = CreateTestProbeAndSubscribeToFoo();

            Subject.UnderlyingActor.PublishMessage(new FooMessage("FooFoo"));

            testProbe.ExpectMsg<FooMessage>();
        }


        [Test(Description = "Publish a message and dont receive it.")]
        public void TestPublishNotSubscribedMessage()
        {
            var testProbe = CreateTestProbeAndSubscribeToFoo();

            Subject.UnderlyingActor.PublishMessage(new TestMessage("TestFoo"));

            testProbe.ExpectNoMsg(TimeSpan.FromSeconds(0.3));
        }

        [Test(Description = "Publish a message that is not valid to subscribe to.")]
        public void TestPublishNotSupporteddMessage()
        {
            var testProbe = CreateTestProbeAndSubscribeToFoo();

            EventFilter.Info("PublishMessage of Type OopsMessage failed. Type not valid").ExpectOne(() =>
            {
                Subject.UnderlyingActor.PublishMessage(new OopsMessage("Oops"));

                testProbe.ExpectNoMsg(TimeSpan.FromSeconds(0.3));
            });
        }

        [Test(Description = "Subscribe to a Message")]
        public void TestSubscribeMessage()
        {
            var testProbe = CreateTestProbe();

            testProbe.Send(Subject, new SubscribeMessage(testProbe, typeof (TestMessage)));

            Assert.That(Subject.UnderlyingActor.Subscribers.First().Item1, Is.EqualTo(testProbe));
            Assert.That(Subject.UnderlyingActor.Subscribers.First().Item2, Is.EqualTo(typeof (TestMessage)));
            Assert.That(Subject.UnderlyingActor.Subscribers.Count, Is.EqualTo(1));
        }

        [Test(Description = "Subscribe to a message which is not supported.")]
        public void TestSubscribeMessageToNoType()
        {
            var testProbe = CreateTestProbe();

            testProbe.Send(Subject, new SubscribeMessage(testProbe, typeof (OopsMessage)));

            Assert.That(Subject.UnderlyingActor.Subscribers.Count, Is.EqualTo(0));
        }

        [Test(Description = "Subscribe to a Message twice")]
        public void TestSubscribeMessageTwice()
        {
            var testProbe = CreateTestProbe();

            testProbe.Send(Subject, new SubscribeMessage(testProbe, typeof (FooMessage)));

            Assert.That(Subject.UnderlyingActor.Subscribers.First().Item1, Is.EqualTo(testProbe));
            Assert.That(Subject.UnderlyingActor.Subscribers.First().Item2, Is.EqualTo(typeof (FooMessage)));
            Assert.That(Subject.UnderlyingActor.Subscribers.Count, Is.EqualTo(1));

            testProbe.Send(Subject, new SubscribeMessage(testProbe, typeof (FooMessage)));

            Assert.That(Subject.UnderlyingActor.Subscribers.First().Item1, Is.EqualTo(testProbe));
            Assert.That(Subject.UnderlyingActor.Subscribers.First().Item2, Is.EqualTo(typeof (FooMessage)));
            Assert.That(Subject.UnderlyingActor.Subscribers.Count, Is.EqualTo(1));
        }

        [Test(Description = "Unsubscribe only from one Message")]
        public void TestUnsubscribe()
        {
            var testProbe = CreateTestProbeAndSubscribeToFooAndTest();

            testProbe.Send(Subject, new UnsubscribeMessage(testProbe, typeof (FooMessage)));

            Assert.That(Subject.UnderlyingActor.Subscribers.First().Item1, Is.EqualTo(testProbe));
            Assert.That(Subject.UnderlyingActor.Subscribers.First().Item2, Is.EqualTo(typeof (TestMessage)));
            Assert.That(Subject.UnderlyingActor.Subscribers.Count, Is.EqualTo(1));
        }

        [Test(Description = "Unsubscribe from all Messages")]
        public void TestUnsubscribeAll()
        {
            var testProbe = CreateTestProbeAndSubscribeToFooAndTest();

            testProbe.Send(Subject, new UnsubscribeMessage(testProbe));

            Assert.That(Subject.UnderlyingActor.Subscribers.Count, Is.EqualTo(0));
        }

        [Test(Description = "Unsubscribe from all Messages with Terminated Message")]
        [Ignore("Terminated message not received by Subject...")]
        public void TestUnsubscribeWatch()
        {
            var testProbe = CreateTestProbe("t1");

            testProbe.Send(Subject, new SubscribeMessage(testProbe, typeof (FooMessage)));

            testProbe.Send(Subject, new Terminated(testProbe, true, true));

            Assert.That(Subject.UnderlyingActor.Subscribers.Count, Is.EqualTo(0));
        }

        [Test(Description = "Unsubscribe from all Messages with Actor Terminated")]
        [Ignore("Terminated message not received by Subject...")]
        public async void TestUnsubscribeWatchTerminated()
        {
            var testProbe = CreateTestProbe("t1");

            testProbe.Send(Subject, new SubscribeMessage(testProbe, typeof (FooMessage)));

            await testProbe.GracefulStop(TimeSpan.FromSeconds(1));

            Assert.That(Subject.UnderlyingActor.Subscribers.Count, Is.EqualTo(0));
        }

        #region Helper methods

        private TestProbe CreateTestProbeAndSubscribeToFoo()
        {
            var testProbe = CreateTestProbe();
            Subject.UnderlyingActor.Subscribers.Add(new Tuple<IActorRef, Type>(testProbe, typeof(FooMessage)));

            return testProbe;
        }

        private TestProbe CreateTestProbeAndSubscribeToFooAndTest()
        {
            var testProbe = CreateTestProbeAndSubscribeToFoo();
            Subject.UnderlyingActor.Subscribers.Add(new Tuple<IActorRef, Type>(testProbe, typeof(TestMessage)));

            return testProbe;
        }

        #endregion

        #region Messages

        public sealed class TestMessage
        {
            public TestMessage(string content)
            {
                Content = content;
            }

            public string Content { get; }
        }

        public sealed class FooMessage
        {
            public FooMessage(string content)
            {
                Content = content;
            }

            public string Content { get; }
        }

        public sealed class OopsMessage
        {
            public OopsMessage(string content)
            {
                Content = content;
            }

            public string Content { get; }
        }

        #endregion
    }
}