using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
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

            receiverActor.Send(subject,
                new SubscribeMessage(receiverActor, typeof (FooMessage)));

            receiverActor.Send(subject, "Hello");
            
            receiverActor.ExpectMsg<string>(s => s == "'sup");

            var result =
                await
                    subject.Ask<MessageReceivedCountMessage>(new AskMessageReceivedCountMessage(),
                        TimeSpan.FromSeconds(2));

            Assert.That(result.SubscriptionMessages, Is.EqualTo(1));
            Assert.That(result.UnsubscriptionMessages, Is.EqualTo(0));
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
                    Context.GetLogger().Warning("Received {0} from {1}", s, Sender);

                    Sender.Tell("Hello");
                });

                BecomeStacked(Cool);
            }

            public void Cool()
            {
                Receive<string>(s =>
                {
                    Context.GetLogger().Warning("Received {0} from {1}", s, Sender);
                    
                    Sender.Tell("'sup");
                });
            }
            

            public new IList<Tuple<IActorRef, Type>> Subscribers => base.Subscribers;

            public new IReadOnlyList<Type> SubscribableMessages => base.SubscribableMessages;
        }
    }
}
