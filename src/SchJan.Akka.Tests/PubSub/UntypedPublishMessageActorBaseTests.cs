using System;
using System.Collections.Generic;
using Akka.Actor;
using NUnit.Framework;
using SchJan.Akka.PubSub;

namespace SchJan.Akka.Tests.PubSub
{
    public class UntypedPublishMessageActorBaseTests :
        PublishMessageActorBaseTests<UntypedPublishMessageActorBaseTests.UntypedPublishMessageActorBaseProxy>
    {
        [PublishMessage(typeof (FooMessage))]
        [PublishMessage(typeof (TestMessage))]
        public sealed class UntypedPublishMessageActorBaseProxy : UntypedPublishMessageActorBase
        {
            public UntypedPublishMessageActorBaseProxy()
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