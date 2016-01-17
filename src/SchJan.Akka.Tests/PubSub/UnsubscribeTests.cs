using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.NUnit;
using NUnit.Framework;
using SchJan.Akka.PubSub;

namespace SchJan.Akka.Tests.PubSub
{
    [TestFixture]
    public class UnsubscribeTests : TestKit
    {
        [Test]
        public void UnsubscribeWatchTermination()
        {
            var pubSubActor = Sys.ActorOf<SendsUnsubscribtionMessagesActor>();

            var terminatedActor = CreateTestProbe();
            terminatedActor.Send(pubSubActor,
                new SubscribeMessage(terminatedActor, typeof (ActorUnsubscribedMessage)));

            var receiverActor = CreateTestProbe();
            terminatedActor.Send(pubSubActor,
                new SubscribeMessage(receiverActor, typeof (ActorUnsubscribedMessage)));

            terminatedActor.Tell(PoisonPill.Instance);

            var message = receiverActor.ExpectMsg<ActorUnsubscribedMessage>();

            Assert.That(message.Terminated, Is.True);
            Assert.That(message.Actor, Is.EqualTo(terminatedActor));
        }

        [Test]
        public void UnsubscribeWatchUnsubscription()
        {
            var pubSubActor = Sys.ActorOf<SendsUnsubscribtionMessagesActor>();

            var terminatedActor = CreateTestProbe();
            terminatedActor.Send(pubSubActor,
                new SubscribeMessage(terminatedActor, typeof (ActorUnsubscribedMessage)));

            var receiverActor = CreateTestProbe();
            terminatedActor.Send(pubSubActor,
                new SubscribeMessage(receiverActor, typeof (ActorUnsubscribedMessage)));

            terminatedActor.Send(pubSubActor, new UnsubscribeMessage(terminatedActor));

            var message = receiverActor.ExpectMsg<ActorUnsubscribedMessage>();

            Assert.That(message.Terminated, Is.False);
            Assert.That(message.Actor, Is.EqualTo(terminatedActor));
        }

        [PublishMessage(typeof (ActorUnsubscribedMessage))]
        public class SendsUnsubscribtionMessagesActor : TypedPublishMessageActorBase
        {
            public SendsUnsubscribtionMessagesActor() : base(true)
            {

            }

            public override void Handle(UnsubscribeMessage message)
            {
                base.Handle(message);

                this.PublishMessage(new ActorUnsubscribedMessage(message.Unsubscriber, false));
            }

            public override void Handle(Terminated message)
            {
                base.Handle(message);

                this.PublishMessage(new ActorUnsubscribedMessage(message.ActorRef, true));
            }
        }

        public class ActorUnsubscribedMessage
        {
            public IActorRef Actor { get; }

            public bool Terminated { get; }

            public ActorUnsubscribedMessage(IActorRef actor, bool terminated)
            {
                Actor = actor;

                Terminated = terminated;
            }
        }
    }
}
