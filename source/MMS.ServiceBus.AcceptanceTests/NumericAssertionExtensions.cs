//-------------------------------------------------------------------------------
// <copyright file="NumericAssertionExtensions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using FluentAssertions.Numeric;

    public static class NumericAssertionExtensions
    {
        public static void BeInvoked(this NumericAssertions<int> assertions, int ntimes)
        {
            assertions.Be(ntimes);
        }

        public static void BeInvokedTwice(this NumericAssertions<int> assertions)
        {
            assertions.Be(2);
        }

        public static void BeInvokedOnce(this NumericAssertions<int> assertions)
        {
            assertions.Be(1);
        }

        public static void NotBeInvoked(this NumericAssertions<int> assertions)
        {
            assertions.Be(0);
        }
    }
}