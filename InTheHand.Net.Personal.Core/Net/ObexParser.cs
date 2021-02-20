// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.ObexParser
// 
// Copyright (c) 2003-2006 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace InTheHand.Net
{
    internal static class ObexParser
    {

        internal static void ParseHeaders(byte[] packet, bool isConnectPacket, ref ushort remoteMaxPacket, Stream bodyStream, WebHeaderCollection headers)
        {
            ObexMethod method = (ObexMethod)packet[0];
            int packetLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(packet, 1));

            int pos = 3;
            int lastPos = int.MinValue;

            while (pos < packetLength) {
                if (pos == lastPos) {
                    Debug.Fail("Infinite Loop!");
                    throw new InvalidOperationException("Infinite Loop!");
                }
                lastPos = pos;

                ObexHeader header = (ObexHeader)packet[pos];
                switch (header) {
                    case ObexHeader.None:
                        return;
                    case (ObexHeader)0x10:
                        Debug.Assert(isConnectPacket, "NOT isConnectPacket");
                        Debug.Assert(pos == 3, "NOT before any headers!");
                        remoteMaxPacket = unchecked((ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(packet, pos + 2)));
                        pos += 4;
                        break;

                    case ObexHeader.ConnectionID:
                    case ObexHeader.Count:
                    case ObexHeader.Length:
                    case ObexHeader.CreatorID:
                    case ObexHeader.Time4Byte:
                        int intValue = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(packet, pos + 1));
                        pos += 5;
                        string key = header.ToString().ToUpper();
                        string value = intValue.ToString();
                        // Duplicate headers causes comma-separated HTTP list!!
                        if (-1 != Array.IndexOf<string>(headers.AllKeys, key)) {
                            string existing = headers.Get(key);
                            if (value == existing) {
                                // Just discard it then.
                                break;
                            } else {
                                Debug.Assert(-1 == Array.IndexOf<string>(headers.AllKeys, header.ToString().ToUpper()),
                                    "Duplicate headers causes comma-separated HTTP list!!: " + header.ToString().ToUpper());
                            }
                        }
                        headers.Add(header.ToString().ToUpper(), intValue.ToString());
                        break;

                    case ObexHeader.Who:
                        short whoSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(packet, pos + 1));
                        byte[] whoBytes = new byte[16];
                        Buffer.BlockCopy(packet, pos + 3, whoBytes, 0, whoSize - 3);
                        Guid service = new Guid(whoBytes);
                        headers.Add(header.ToString().ToUpper(), service.ToString());
                        pos += whoSize;
                        break;

                    case ObexHeader.Body:
                    case ObexHeader.EndOfBody:
                        short bodySize = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(packet, pos + 1));
                        bodyStream.Write(packet, pos + 3, bodySize - 3);
                        pos += bodySize;
                        break;

                    default:
                        int headerSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(packet, pos + 1));

                        if (headerSize > 3) {
                            string headerString = System.Text.Encoding.BigEndianUnicode.GetString(packet, pos + 3, headerSize - 5);
                            if (headerString != null) {
                                int nullindex = headerString.IndexOf('\0');
                                if (nullindex > -1) {
                                    headerString = headerString.Substring(0, nullindex);
                                }

                                if (headerString != string.Empty) {
                                    headers.Add(header.ToString().ToUpper(), headerString);
                                }
                            }
                        }

                        pos += headerSize;
                        break;
                }


            }
        }

    }
}
