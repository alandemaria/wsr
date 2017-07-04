using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WSr.Interfaces;
using static WSr.Factories.Fns;

namespace WSr.Tests.Factories
{
    internal struct TestSocket : ISocket
    {
        public TestSocket(string address)
        {
            Address = address;
        }
        public string Address { get; }

        public Func<IScheduler, byte[], IObservable<int>> CreateReader(int bufferSize)
        {
            throw new NotImplementedException();
        }

        public Func<IScheduler, byte[], IObservable<Unit>> CreateWriter()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            return;
        }
    }



    [TestClass]
    public class Factories : ReactiveTest
    {
        private ISocket WithAddress(string s) => new TestSocket(s);

        [TestMethod]
        public void GenerateClientObservable()
        {
            var run = new TestScheduler();

            var socket = run.CreateHotObservable(
                OnNext(0, WithAddress("0")),
                OnNext(100, WithAddress("1")),
                OnNext(200, WithAddress("2")),
                OnNext(300, WithAddress("3"))
            );

            var expected = run.CreateHotObservable(
                OnNext(200, "2")
            );

            var server = new Mock<IServer>();
            server
                .Setup(x => x.Serve(It.IsAny<IScheduler>()))
                .Returns<IScheduler>(s => socket.Take(1, s));

            var actual = run.Start(
                create: () => Observable
                    .Using(
                        () => server.Object,
                        s => s.AcceptConnections(run)
                                .Select(x => x.Address)),
                created: 50,
                subscribed: 150,
                disposed: 250
            );

            ReactiveAssert.AreElementsEqual(
                expected: expected.Messages,
                actual: actual.Messages,
                message: $"{Environment.NewLine} expected: {string.Join(", ", expected.Messages)} {Environment.NewLine} actual: {string.Join(", ", actual.Messages)}");

            server.Verify(x => x.Dispose(), Times.Exactly(1));
        }

        private IEnumerable<byte> Nulls() { while (true) yield return (byte)'\0'; }


        [TestMethod]
        [DataRow(10, 20)]
        [DataRow(20, 10)]
        public void ObservableFromReadOnStream(
            int availiable,
            int bufferSize)
        {
            var run = new TestScheduler();
            var expected = run.CreateHotObservable(
                OnNext(71, Math.Min(availiable, bufferSize)),
                OnCompleted<int>(71)
            );

            var bytes = Enumerable.Repeat((byte)'c', availiable).ToArray();
            var incoming = new MemoryStream(bytes);
            var sut = incoming.CreateReader(bufferSize);

            var buffer = new byte[bufferSize];
            var actual = run.Start(
                create: () => sut(run, buffer),
                created: 50,
                subscribed: 70,
                disposed: 110
            );

            Assert.IsTrue(buffer.SequenceEqual(bytes.Concat(Nulls()).Take(bufferSize)),
                $"Expected: {Encoding.ASCII.GetString(bytes.Concat(Nulls()).Take(bufferSize).ToArray())} Actual: {Encoding.ASCII.GetString(buffer)}");

            ReactiveAssert.AreElementsEqual(
               expected: expected.Messages,
               actual: actual.Messages,
               message: $"{Environment.NewLine} expected: {string.Join(", ", expected.Messages)} {Environment.NewLine} actual: {string.Join(", ", actual.Messages)}");
        }

        private string debugElementsEqual<T>(IList<Recorded<Notification<T>>> expected, IList<Recorded<Notification<T>>> actual)
        {
            return $"{Environment.NewLine} expected: {string.Join(", ", expected)} {Environment.NewLine} actual: {string.Join(", ", actual)}";
        }

        private IEnumerable<byte> BytesFrom(Encoding enc, string str)
        {
            IEnumerable<string> Forever(string s) { while (true) yield return s; }

            return Forever(str).SelectMany(s => enc.GetBytes(s));
        }

        private byte[] BytesFrom(Encoding enc, string str, int byteCount) =>
        BytesFrom(enc, str)
            .Take(byteCount)
            .ToArray();

        public byte[] BytesFromAscii(string str) =>
        BytesFrom(Encoding.ASCII, str)
            .Take(Encoding.ASCII.GetByteCount(str))
            .ToArray();

        private byte[] From42Ascii(int count) => BytesFrom(Encoding.ASCII, "42", count);

        [TestMethod]
        public void CreateIncomingBytesObservable()
        {
            var bufferSize = 20;
            var writesSize = 50;

            var run = new TestScheduler();
            var socket = new Mock<ISocket>();

            var reads = run.CreateHotObservable(
                OnNext(10, 20),
                OnNext(11, 20),
                OnNext(12, 10),
                OnNext(20, 20),
                OnNext(21, 20),
                OnNext(22, 10)
            );

            var seq = From42Ascii(writesSize);
            var times = 0;
            socket.Setup(s => s.CreateReader(It.Is<int>(a => a == bufferSize)))
                .Returns((sch, buf) =>
                {
                    seq
                        .Skip((times % 3) * bufferSize)
                        .Take(bufferSize)
                        .ToArray()
                        .CopyTo(buf, 0);

                    return reads.Take(1, sch);
                });

            Func<IEnumerable<byte>, string> show = s => Encoding.ASCII.GetString(s.ToArray());

            var expected = run.CreateColdObservable(
                OnNext(10, show(seq.Skip(bufferSize * 0).Take(bufferSize))),
                OnNext(11, show(seq.Skip(bufferSize * 1).Take(bufferSize))),
                OnNext(12, show(seq.Skip(bufferSize * 2).Take(bufferSize))),
                OnNext(20, show(seq.Skip(bufferSize * 0).Take(bufferSize))),
                OnNext(21, show(seq.Skip(bufferSize * 1).Take(bufferSize))),
                OnNext(22, show(seq.Skip(bufferSize * 2).Take(bufferSize)))
            );

            var sut = socket.Object;

            var actual = run.Start(
                create: () => sut.ReadToEnd(bufferSize, run).Select(show),
                created: 0,
                subscribed: 0,
                disposed: 100
            );

            ReactiveAssert.AreElementsEqual(
                expected: expected.Messages,
                actual: actual.Messages,
                message: debugElementsEqual(expected.Messages, actual.Messages)
            );

            socket.Verify(x => x.Dispose(), Times.Exactly(1));
        }
    }
}
