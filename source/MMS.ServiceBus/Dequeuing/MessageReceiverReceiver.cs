//-------------------------------------------------------------------------------
// <copyright file="MessageReceiverReceiver.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Dequeuing
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using log4net;
    using Microsoft.ServiceBus.Messaging;

    public class MessageReceiverReceiver : IReceiveMessages
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageReceiverReceiver));

        private readonly MessagingFactory factory;

        public MessageReceiverReceiver(MessagingFactory factory)
        {
            this.factory = factory;
        }

        public async Task<AsyncClosable> StartAsync(
            EndpointConfiguration.ReadOnly configuration, 
            Func<TransportMessage, Task> onMessage)
        {
            MessageReceiver client =
                await this.factory.CreateMessageReceiverAsync(configuration.DestinationQueue(), ReceiveMode.PeekLock)
                    .ConfigureAwait(false);

            OnMessageOptions options = configuration.Options();
            options.ExceptionReceived += (sender, args) => HandleExceptionReceived(configuration.DestinationQueue(), args);

            configuration.Configure(client).OnMessageAsync(brokeredMessage => onMessage(new TransportMessage(brokeredMessage)), options);

            return new AsyncClosable(client.CloseAsync);
        }

        private static void HandleExceptionReceived(object sender, ExceptionReceivedEventArgs e)
        {
            if (e != null && e.Exception != null)
            {
                var queue = (string)sender;
                Logger.Info(string.Format(CultureInfo.InvariantCulture, "Exception occurred on queue {0}.", queue), e.Exception);
            }
        }
    }
}