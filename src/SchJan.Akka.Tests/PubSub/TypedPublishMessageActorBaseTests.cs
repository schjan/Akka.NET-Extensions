using System;
using System.Collections.Generic;
using Akka.Actor;
using NUnit.Framework;
using SchJan.Akka.PubSub;

namespace SchJan.Akka.Tests.PubSub
{
    [TestFixture]
    public class TypedPublishMessageActorBaseTests :
        PublishMessageActorBaseTests<TypedPublishMessageActorBaseTests.TypedPublishMessageActorBaseProxy>
    {
        [PublishMessage(typeof (FooMessage))]
        [PublishMessage(typeof (TestMessage))]
        public sealed class TypedPublishMessageActorBaseProxy : TypedPublishMessageActorBase
        {
            public TypedPublishMessageActorBaseProxy()
                : base(true)
            {
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