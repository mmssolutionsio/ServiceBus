//-------------------------------------------------------------------------------
// <copyright file="DateTimeExtensions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System;
    using System.Globalization;

    internal static class DateTimeExtensions
    {
        private const string Format = "yyyy-MM-dd HH:mm:ss:ffffff Z";

        /// <summary>
        /// Converts the <see cref="DateTime"/> to a <see cref="string"/> suitable for transport over the wire
        /// </summary>
        public static string ToWireFormattedString(this DateTimeOffset dateTime)
        {
            return dateTime.ToUniversalTime().ToString(Format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a wire formatted <see cref="string"/> from <see cref="ToWireFormattedString"/> to a UTC <see cref="DateTime"/>
        /// </summary>
        public static DateTimeOffset ToUtcDateTime(this string wireFormattedString)
        {
            return DateTime.ParseExact(wireFormattedString, Format, CultureInfo.InvariantCulture).ToUniversalTime();
        }
    }
}