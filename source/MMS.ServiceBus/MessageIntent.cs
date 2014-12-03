//-------------------------------------------------------------------------------
// <copyright file="MessageIntent.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    /// <summary>
    /// Enumeration defining different kinds of message intent like Send and Publish.
    /// </summary>
    public enum MessageIntent
    {
        /// <summary>
        /// Regular point-to-point send
        /// </summary>
        Send = 1, 

        /// <summary>
        /// Publish, not a regular point-to-point send
        /// </summary>
        Publish = 2, 

        /// <summary>
        /// Indicates that this message is a reply
        /// </summary>
        Reply = 3
    }
}