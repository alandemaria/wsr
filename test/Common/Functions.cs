using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using Microsoft.Reactive.Testing;

using static WSr.Functions.ListConstruction;

namespace WSr.Tests.Functions
{
    public static class StringEncoding
    {
        public static IEnumerable<byte> BytesFrom(Encoding enc, string str)
        {
            return Forever(str).SelectMany(s => enc.GetBytes(s));
        }

        public static byte[] BytesFrom(Encoding enc, string str, int byteCount) =>
        BytesFrom(enc, str)
            .Take(byteCount)
            .ToArray();

        public static byte[] BytesFromAscii(string str) =>
        BytesFrom(Encoding.ASCII, str)
            .Take(Encoding.ASCII.GetByteCount(str))
            .ToArray();

        
    }

    public static class Debug
    {
        public static string debugElementsEqual<T>(IList<Recorded<Notification<T>>> expected, IList<Recorded<Notification<T>>> actual)
        {
            return $"{Environment.NewLine} expected: {string.Join(", ", expected)} {Environment.NewLine} actual: {string.Join(", ", actual)}";
        }

        public static string Showlist<T>(IEnumerable<T> list) 
        {
            return string.Join(", ", list.Select(x => x.ToString()));
        }
    }

    public static class StreamConstruction
    {
        public static Stream EmptyStream => new MemoryStream(new byte [] { });
    }
}