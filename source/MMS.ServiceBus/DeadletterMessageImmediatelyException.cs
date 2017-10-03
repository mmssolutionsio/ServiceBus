//-------------------------------------------------------------------------------
// <copyright file="DeadletterMessageImmediatelyException.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------
namespace MMS.ServiceBus
{
    using System;

    public class DeadletterMessageImmediatelyException : Exception
    {
        public DeadletterMessageImmediatelyException(Exception innerException)
            : base("An critical errror during message handling! See InnerException for more details.",innerException)
        {
        }
    }
}