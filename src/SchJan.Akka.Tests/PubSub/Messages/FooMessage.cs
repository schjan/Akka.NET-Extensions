namespace SchJan.Akka.Tests.PubSub.Messages
{
    public sealed class FooMessage
    {
        public FooMessage(string content)
        {
            Content = content;
        }

        public string Content { get; }
    }
}
