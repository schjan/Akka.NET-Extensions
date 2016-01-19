namespace SchJan.Akka.Tests.PubSub.Messages
{
    public sealed class TestMessage
    {
        public TestMessage(string content)
        {
            Content = content;
        }

        public string Content { get; }
    }
}
