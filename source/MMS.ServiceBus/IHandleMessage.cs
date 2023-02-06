//-------------------------------------------------------------------------------
// <copyright file="IHandleMessage.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    public interface IHandleMessage<in TMessage>
    {
        void Handle(TMessage message, IBusForHandler bus);
    }
}