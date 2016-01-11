using System;
using System.Collections.Generic;
using Akka.Actor;
using NUnit.Framework;
using SchJan.Akka.PubSub;

namespace SchJan.Akka.Tests.PubSub
{
    [TestFixture]
    public class PublishMessageReceiveActorBaseTests :
        PublishMessageActorBaseTests<PublishMessageReceiveActorBaseTests.PublishMessageReceiveActorBaseProxy>
    {
        [PublishMessage(typeof (FooMessage))]
        [PublishMessage(typeof (TestMessage))]
        public sealed class PublishMessageReceiveActorBaseProxy : PublishMessageReceiveActorBase
        {
            public PublishMessageReceiveActorBaseProxy()
                : base(true)
            {
                ReceiveAny(m => { Assert.Fail("Unhandled Message occured."); });
            }

            public new IList<Tuple<IActorRef, Type>> Subscribers => base.Subscribers;

            public new IReadOnlyList<Type> SubscribableMessages => base.SubscribableMessages;

            protected override void Unhandled(object message)
            {
                Assert.Fail("Unhandled Message occured.");
            }
        }
    }
}