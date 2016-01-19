using Akka.Actor;

namespace SchJan.Akka.Tests.PubSub.Messages
{
    public sealed class ActorUnsubscribedMessage
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
