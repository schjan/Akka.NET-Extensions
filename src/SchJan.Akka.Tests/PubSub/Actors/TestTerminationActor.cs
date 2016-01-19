using Akka.Actor;
using Akka.Event;
using SchJan.Akka.PubSub;
using SchJan.Akka.Tests.PubSub.Messages;

namespace SchJan.Akka.Tests.PubSub.Actors
{
    // TestTerminationActor, because Termination doesn't work well with TestProbe.
    public class TestTerminationActor : ReceiveActor
    {
        public TestTerminationActor(IActorRef actorToSubscribe)
        {
            actorToSubscribe.Tell(new SubscribeMessage(Self, typeof(ActorUnsubscribedMessage)));
            actorToSubscribe.Tell(new SubscribeMessage(Self, typeof(FooMessage)));
        }

        protected override void PostStop()
        {
            Context.GetLogger().Warning("TestActor Terminated");
        }
    }
}
