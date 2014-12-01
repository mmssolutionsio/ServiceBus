//-------------------------------------------------------------------------------
// <copyright file="MessageHandler.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------
namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System;
    using System.Threading.Tasks;

    public class MessageHandler
    {
        public object Instance { get; set; }

        public Func<object, object, Task> Invocation { get; set; }
    }
}