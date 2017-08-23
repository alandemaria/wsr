using System;
using System.Collections.Generic;
using System.Linq;
using WSr.Framing;

using static WSr.ListConstruction;

namespace WSr.Tests
{
    internal static class Bytes
    {
        internal static IEnumerable<byte> L0UMasked { get; } = new byte[] { 0x81, 0x00 };
        internal static IEnumerable<byte> L0Masked { get; } = new byte[] { 0x81, 0x80, 0x00, 0x00, 0x00, 0x00 };
        
        internal static IEnumerable<byte> L2UMasked { get; } = new byte[] { 0x81, 0x02, 0x00, 0x00 };
        
        internal static IEnumerable<byte> L28Masked { get; } = new byte[] { 0x81, 0x9c, 0x06, 0xa2, 0xa0, 0x74, 0x54, 0xcd, 0xc3, 0x1f, 0x26, 0xcb, 0xd4, 0x54, 0x71, 0xcb, 0xd4, 0x1c, 0x26, 0xea, 0xf4, 0x39, 0x4a, 0x97, 0x80, 0x23, 0x63, 0xc0, 0xf3, 0x1b, 0x65, 0xc9, 0xc5, 0x00 };
        internal static IEnumerable<byte> L28UMasked { get; } = new byte[] { 0x81, 0x1c, 0x54, 0xcd, 0xc3, 0x1f, 0x26, 0xcb, 0xd4, 0x54, 0x71, 0xcb, 0xd4, 0x1c, 0x26, 0xea, 0xf4, 0x39, 0x4a, 0x97, 0x80, 0x23, 0x63, 0xc0, 0xf3, 0x1b, 0x65, 0xc9, 0xc5, 0x00 };
        internal static IEnumerable<byte> L128Masked { get; } = new byte[] { 0x81, 0xfe, 0x00, 0x80, 0x06, 0xa2, 0xa0, 0x74 }.Concat(Forever<byte>(0x66).Take(0x80));
        internal static IEnumerable<byte> L128UMasked { get; } = new byte[] { 0x81, 0x7e, 0x00, 0x80 }.Concat(Forever<byte>(0x66).Take(0x80));
        internal static IEnumerable<byte> L65536Masked { get; } = new byte[] { 0x81, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x06, 0xa2, 0xa0, 0x74 }.Concat(Forever<byte>(0x66).Take(0x010000));
        internal static IEnumerable<byte> L65536UMasked { get; } = new byte[] { 0x81, 0x7f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00 }.Concat(Forever<byte>(0x66).Take(0x010000));
    }

    internal static class LengthAndMask
    {
        internal static ParsedFrame L28Masked { get; } = new ParsedFrame(
                origin: "",
                bitfield: new byte[] { 0x81, 0x9c },
                length: new byte[0],
                mask: new byte[] { 0x06, 0xa2, 0xa0, 0x74 },
                payload: new byte[] { 0x54, 0xcd, 0xc3, 0x1f, 0x26, 0xcb, 0xd4, 0x54, 0x71, 0xcb, 0xd4, 0x1c, 0x26, 0xea, 0xf4, 0x39, 0x4a, 0x97, 0x80, 0x23, 0x63, 0xc0, 0xf3, 0x1b, 0x65, 0xc9, 0xc5, 0x00 });

        internal static ParsedFrame L28UMasked { get; } = new ParsedFrame(
                origin: "",
                bitfield: new byte[] { 0x81, 0x1c },
                length: new byte[0],
                mask: new byte[] { 0x00, 0x00, 0x00, 0x00 },
                payload: new byte[] { 0x54, 0xcd, 0xc3, 0x1f, 0x26, 0xcb, 0xd4, 0x54, 0x71, 0xcb, 0xd4, 0x1c, 0x26, 0xea, 0xf4, 0x39, 0x4a, 0x97, 0x80, 0x23, 0x63, 0xc0, 0xf3, 0x1b, 0x65, 0xc9, 0xc5, 0x00 });

        internal static ParsedFrame L128Masked { get; } = new ParsedFrame(
                origin: "",
                bitfield: new byte[] { 0x81, 0xfe },
                length: new byte[] { 0x00, 0x80 },
                mask: new byte[] { 0x06, 0xa2, 0xa0, 0x74 },
                payload: Forever<byte>(0x66).Take(0x80).ToArray());

        internal static ParsedFrame L128UMasked { get; } = new ParsedFrame(
                origin: "",
                bitfield: new byte[] { 0x81, 0x7e },
                length: new byte[] { 0x00, 0x80 },
                mask: new byte[] { 0x00, 0x00, 0x00, 0x00 },
                payload: Forever<byte>(0x66).Take(0x80).ToArray());

        internal static ParsedFrame L65536Masked { get; } = new ParsedFrame(
                origin: "",
                bitfield: new byte[] { 0x81, 0xff },
                length: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00 },
                mask: new byte[] { 0x06, 0xa2, 0xa0, 0x74 },
                payload: Forever<byte>(0x66).Take(0x010000).ToArray());

        internal static ParsedFrame L65536UMasked { get; } = new ParsedFrame(
                origin: "",
                bitfield: new byte[] { 0x81, 0x7f },
                length: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00 },
                mask: new byte[] { 0x00, 0x00, 0x00, 0x00 },
                payload: Forever<byte>(0x66).Take(0x010000).ToArray());
    }

    public static class SpecExamples
    {
        internal static byte[] SingleFrameUnmaskedTextMessage = new byte[] { 0x81, 0x05, 0x48, 0x65, 0x6c, 0x6c, 0x6f };
        internal static byte[] SingleFrameMaskedTextMessage = new byte[] { 0x81, 0x85, 0x37, 0xfa, 0x21, 0x3d, 0x7f, 0x9f, 0x4d, 0x51, 0x58 };

        internal static ParsedFrame SingleFrameMaskedTextFrame(string origin) => new ParsedFrame(
                origin: origin,
                bitfield: new byte[] { 0x81, 0x85 },
                length: new byte[0],
                mask: new byte[] { 0x37, 0xfa, 0x21, 0x3d },
                payload: new byte[] { 0x7f, 0x9f, 0x4d, 0x51, 0x58 }
        );

        internal static byte[] UnmaskedPing = new byte[] { 0x89, 0x05, 0x48, 0x65, 0x6c, 0x6c, 0x6f };
        internal static byte[] MaskedPong = new byte[] { 0x8a, 0x85, 0x37, 0xfa, 0x21, 0x3d, 0x7f, 0x9f, 0x4d, 0x51, 0x58 };

        internal static byte[] MaskedGoingAwayClose = new byte[] { 0x88, 0x8c, 0x05, 0xbc, 0x3a, 0x6c, 0x06, 0x55, 0x7d, 0x03, 0x6c, 0xd2, 0x5d, 0x4c, 0x44, 0xcb, 0x5b, 0x15 };
        internal static ParsedFrame MaskedGoingAwayCloseFrame(string origin) => new ParsedFrame(
                origin: origin,
                bitfield: new byte[] { 0x88, 0x8c },
                length: new byte[0],
                mask: new byte[] { 0x05, 0xbc, 0x3a, 0x6c },
                payload: new byte[] { 0x06, 0x55, 0x7d, 0x03, 0x6c, 0xd2, 0x5d, 0x4c, 0x44, 0xcb, 0x5b, 0x15 }
        );
    }

    public static class OpenWebsocketRequestData
    {
        public static byte[] BadRequest => ("GET X HTTP/1.0\r\n" + // bad http version
                "X: X\r\n" +
                "\r\n")
                .Select(Convert.ToByte)
                .ToArray();

        public static byte[] WellFormedRequest =>
                ("GET /chat HTTP/1.1\r\n" +
                "Host: 127.1.1.1:80\r\n" +
                "Upgrade: websocket\r\n" +
                "Connection: Upgrade\r\n" +
                "Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==\r\n" +
                "Sec-WebSocket-Version: 13\r\n" +
                "\r\n")
                .Select(Convert.ToByte)
                .ToArray();

        public static string SuccessfulHandshakeResponse = (
                "HTTP/1.1 101 Switching Protocols\r\n" +
                "Upgrade: websocket\r\n" +
                "Connection: Upgrade\r\n" +
                "Sec-WebSocket-Accept: s3pPLMBiTxaQ9kYGzzhZRbK+xOo=\r\n\r\n");
    }
}