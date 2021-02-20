// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.IrDA.IrDAService
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#region Using directives

using System;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace InTheHand.Net.IrDA
{
    /// <summary>
    /// Standard IrDA service names.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification="Windows CE Equivalents", Scope="type")]
    public static class IrDAService
    {

        /// <summary>
        /// Well-known Service Name &#x201C;IrDA:IrCOMM&#x201D;
        /// </summary>
#if CODE_ANALYSIS
        [SuppressMessage("Microsoft.Design", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Windows CE Equivalents"),
        SuppressMessage("Microsoft.Design", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Windows CE Equivalents")]
#endif
        public const string IrComm = "IrDA:IrCOMM";
        /// <summary>
        /// Well-known Service Name &#x201C;IrLPT&#x201D;
        /// </summary>
        public const string IrLpt = "IrLPT";
        /// <summary>
        /// Well-known Service Name &#x201C;OBEX&#x201D;
        /// </summary>
        public const string ObjectExchange = "OBEX";

    }
}
