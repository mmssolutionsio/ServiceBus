//-------------------------------------------------------------------------------
// <copyright file="IHandleMessage.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using JetBrains.Annotations;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public interface IHandleMessage<in TMessage>
    {
        void Handle([NotNull] TMessage message, [NotNull] IBusForHandler bus);
    }
}