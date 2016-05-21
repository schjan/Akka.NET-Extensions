using Akka.Actor;
using Akka.TestKit.NUnit;
using NUnit.Framework;
using SchJan.Akka.PubSub;

namespace SchJan.Akka.Tests.PubSub
{
    [TestFixture]
    public class GenericMessagesTests : TestKit
    {
        [Test]
        public void SendsTMessage()
        {
            var probe = CreateTestProbe();
            var act = Sys.ActorOf<CoolTActor>();

            probe.Send(act, new SubscribeMessage(probe, typeof(CoolMessage)));

            act.Tell("a");

            probe.ExpectMsg<CoolMessage>();
        }

        [Test]
        public void SendsTMessageOtherWay()
        {
            var probe = CreateTestProbe();
            var act = Sys.ActorOf<CoolTActor>();

            probe.Send(act, new SubscribeMessage(probe, typeof(CoolMessage)));

            act.Tell("b");

            probe.ExpectMsg<CoolMessage>();
        }
    }

    public class TestActorWithoutAttribute : PublishMessageReceiveActorBase
    {
        public TestActorWithoutAttribute()
        {
            Receive<string>(s => Sender.Tell(s));
        }
    }

    public class CoolTActor : TestTActor<CoolMessage>
    {
        public override void Publish()
        {
            this.PublishMessage(new CoolMessage());
        }

        public override CoolMessage GetMessage()
        {
            return new CoolMessage();
        }
    }

    public abstract class TestTActor<T> : PublishMessageReceiveActorBase
        where T : class, ICoolMessage
    {
        protected TestTActor() : base(messageTypes: typeof(T))
        {
            Receive<string>(s =>
            {
                switch (s)
                {
                    case "a":
                        Publish();
                        break;
                    case "b":
                        this.PublishMessage(GetMessage());
                        break;
                }
            });
        }

        public abstract void Publish();
        public abstract T GetMessage();
    }

    public class CoolMessage : ICoolMessage
    {

    }

    public interface ICoolMessage
    {

    }
}
