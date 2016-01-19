namespace SchJan.Akka.Tests.PubSub.Messages
{
    public class MessageReceivedCountMessage
    {
        public MessageReceivedCountMessage(int subscriptionMessages, int unsubscriptionMessages, int terminationMessages)
        {
            SubscriptionMessages = subscriptionMessages;
            UnsubscriptionMessages = unsubscriptionMessages;
            TerminationMessages = terminationMessages;
        }
        
        public int SubscriptionMessages { get; }

        public int UnsubscriptionMessages { get; }

        public int TerminationMessages { get; }
    }
}
