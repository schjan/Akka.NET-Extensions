namespace SchJan.Akka.PubSub
{
    /// <summary>
    ///     Message will be sent to subscriber on request.
    /// </summary>
    public class SubscribedMessage
    {
        /// <summary>
        ///     Creates a new <see cref="SubscribedMessage" />
        /// </summary>
        /// <param name="success">True if successfull subcribed</param>
        public SubscribedMessage(bool success)
        {
            Success = success;
        }

        /// <summary>
        ///     True if successfull subscribed
        /// </summary>
        public bool Success { get; }
    }
}
