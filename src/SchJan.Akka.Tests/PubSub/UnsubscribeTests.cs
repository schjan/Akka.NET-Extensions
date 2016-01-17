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
            receiverActor.Send(pubSubActor,
                new SubscribeMessage(receiverActor, typeof (ActorUnsubscribedMessage)));

            terminatedActor.Tell(PoisonPill.Instance);

            receiverActor.ExpectMsg<ActorUnsubscribedMessage>(msg => msg.Terminated && msg.Actor.Equals(terminatedActor));
        }

        [Test]
        public void UnsubscribeWatchUnsubscription()
        {
            var pubSubActor = Sys.ActorOf<SendsUnsubscribtionMessagesActor>();

            var terminatedActor = CreateTestProbe();
            terminatedActor.Send(pubSubActor,
                new SubscribeMessage(terminatedActor, typeof (ActorUnsubscribedMessage)));

            var receiverActor = CreateTestProbe();
            receiverActor.Send(pubSubActor,
                new SubscribeMessage(receiverActor, typeof (ActorUnsubscribedMessage)));

            terminatedActor.Send(pubSubActor, new UnsubscribeMessage(terminatedActor));

            receiverActor.ExpectMsg<ActorUnsubscribedMessage>(
                msg => !msg.Terminated && msg.Actor.Equals(terminatedActor));
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
