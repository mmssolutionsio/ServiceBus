//-------------------------------------------------------------------------------
// <copyright file="ISendMessages.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System.Threading.Tasks;

    public interface ISendMessages
    {
        Task SendAsync(TransportMessage message, SendOptions options);
    }
}