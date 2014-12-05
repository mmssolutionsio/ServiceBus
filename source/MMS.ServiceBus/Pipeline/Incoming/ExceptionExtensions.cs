//-------------------------------------------------------------------------------
// <copyright file="ExceptionExtensions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System;

    internal static class ExceptionExtensions
    {
        public static string GetMessage(this Exception exception)
        {
            try
            {
                return exception.Message;
            }
            catch (Exception)
            {
                return string.Format("Could not read Message from exception type '{0}'.", exception.GetType());
            }
        }
    }
}