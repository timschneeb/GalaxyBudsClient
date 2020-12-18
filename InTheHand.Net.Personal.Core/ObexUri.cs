// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.ObexUri
// 
// Copyright (c) 2003-2006 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Text;

namespace InTheHand
{
    /// <summary>
    /// Contains <see cref="Uri"/> helper functionality.
    /// </summary>
    public static class ObexUri
    {

        #region Schemes

        /// <summary>
        /// Specifies that the URI is accessed through the Object Exchange (OBEX) protocol.
        /// </summary>
        public const string UriSchemeObex = "obex";

        /// <summary>
        /// Specifies that the URI is accessed through the Object Exchange (OBEX) Push protocol.
        /// </summary>
        public const string UriSchemeObexPush = "obex-push";

        /// <summary>
        /// Specifies that the URI is accessed through the Object Exchange (OBEX) FTP protocol.
        /// </summary>
        public const string UriSchemeObexFtp = "obex-ftp";

        /// <summary>
        /// Specifies that the URI is accessed through the Object Exchange (OBEX) Sync protocol.
        /// </summary>
        public const string UriSchemeObexSync = "obex-sync";

        #endregion

        #region Unescape Data String
#if V1 && WinCE
        /// <summary>
        /// Converts a string to its unescaped representation.
        /// </summary>
        /// <param name="stringToUnescape">The string to unescape.</param>
        /// <returns>The unescaped representation of stringToUnescape.</returns>
        public static string UnescapeDataString(string stringToUnescape)
        {
            //store unescaped string
            StringBuilder sb = new StringBuilder();
            int newchar, sumb = 0;
            int more = -1;

            for (int i = 0; i < stringToUnescape.Length; i++)
            {

                //find escape sequence
                switch (stringToUnescape[i])
                {
                    case '%':
                        newchar = Int32.Parse(stringToUnescape[++i].ToString() + stringToUnescape[++i].ToString(), System.Globalization.NumberStyles.HexNumber);
                        break;
                    case '+':
                        newchar = 0x20;
                        break;
                    default:
                        newchar = Convert.ToInt32(stringToUnescape[i]);
                        break;
                }
                /* Decode byte b as UTF-8, sumb collects incomplete chars */
                if ((newchar & 0xc0) == 0x80)
                {			// 10xxxxxx (continuation byte)
                    sumb = (sumb << 6) | (newchar & 0x3f);	// Add 6 bits to sumb
                    if (--more == 0) sb.Append(Convert.ToChar(sumb)); // Add char to sbuf
                }
                else if ((newchar & 0x80) == 0x00)
                {		// 0xxxxxxx (yields 7 bits)
                    sb.Append(Convert.ToChar(newchar));			// Store in sbuf
                }
                else if ((newchar & 0xe0) == 0xc0)
                {		// 110xxxxx (yields 5 bits)
                    sumb = newchar & 0x1f;
                    more = 1;				// Expect 1 more byte
                }
                else if ((newchar & 0xf0) == 0xe0)
                {		// 1110xxxx (yields 4 bits)
                    sumb = newchar & 0x0f;
                    more = 2;				// Expect 2 more bytes
                }
                else if ((newchar & 0xf8) == 0xf0)
                {		// 11110xxx (yields 3 bits)
                    sumb = newchar & 0x07;
                    more = 3;				// Expect 3 more bytes
                }
                else if ((newchar & 0xfc) == 0xf8)
                {		// 111110xx (yields 2 bits)
                    sumb = newchar & 0x03;
                    more = 4;				// Expect 4 more bytes
                }
                else /*if ((b & 0xfe) == 0xfc)*/
                {	// 1111110x (yields 1 bit)
                    sumb = newchar & 0x01;
                    more = 5;				// Expect 5 more bytes
                }
            }

            return sb.ToString();
        }
#endif
        #endregion

    }
}
