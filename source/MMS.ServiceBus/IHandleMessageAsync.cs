//-------------------------------------------------------------------------------
// <copyright file="IHandleMessageAsync.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public interface IHandleMessageAsync<in TMessage>
    {
        Task Handle([NotNull] TMessage message, [NotNull] IBusForHandler bus);
    }
}