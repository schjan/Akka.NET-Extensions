namespace SchJan.Akka.Tests.PubSub.Messages
{
    public sealed class OopsMessage
    {
        public OopsMessage(string content)
        {
            Content = content;
        }

        public string Content { get; }
    }
}
