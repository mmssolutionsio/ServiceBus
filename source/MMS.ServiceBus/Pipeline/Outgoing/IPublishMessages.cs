//-------------------------------------------------------------------------------
// <copyright file="IPublishMessages.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System.Threading.Tasks;

    public interface IPublishMessages
    {
        Task PublishAsync(TransportMessage message, PublishOptions options);
    }
}