using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WSr.Messaging;
using WSr.Protocol;
using WSr.Socket;

using static WSr.Tests.Functions.Debug;
using static WSr.Tests.Functions.FrameCreator;
using static WSr.Deciding.Functions;
using WSr.Deciding;

namespace WSr.Tests
{
    [TestClass]
    public class ObservableExtensionsShould : ReactiveTest
    {
        private static string Origin => "o";

        private static IEnumerable<byte> TestBuffer => Enumerable.Repeat((byte)0x00, 100);
        public static Mock<IConnectedSocket> MockSocket(
            IList writeTo,
            string address)
        {
            var socket = new Mock<IConnectedSocket>();
            socket.Setup(x => x.Address).Returns(address);
            socket.Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<IScheduler>()))
                .Returns(Observable.Return(Unit.Default))
                .Callback<byte[], IScheduler>((b, s) => writeTo.Add(b));

            return socket;
        }

        public static IMessage withOrigin(string origin)
        {
            var mock = new Mock<IMessage>();
            mock.Setup(x => x.Origin).Returns(origin);

            return mock.Object;
        }

        private Dictionary<string, string> WithRequestKey(string key)
        {
            return new Dictionary<string, string>()
            {
                ["Sec-WebSocket-Key"] = key
            };
        }

        [TestMethod]
        public void EchoProcessSendsSuccessfulOpenHandshake()
        {
            var run = new TestScheduler();

            var actualWrites = new List<byte[]>();
            var socket = MockSocket(actualWrites, Origin);

            var command = new IOCommand(withOrigin(Origin), CommandName.SuccessfulOpeningHandshake, TestBuffer);
            var commands = run.CreateColdObservable(
                OnNext(10, command)
            );

            var expected = run.CreateHotObservable(
                OnNext(110, new ProcessResult(run.Now, command))
            );

            var actual = run.Start(
                create: () => commands.Process(socket.Object, run),
                created: 0,
                subscribed: 100,
                disposed: 1000
            );

            ReactiveAssert.AreElementsEqual(
               expected: expected.Messages,
               actual: actual.Messages,
               message: debugElementsEqual(expected.Messages, actual.Messages));

            Assert.IsTrue(actualWrites.Count() == 1);
        }

        [Ignore]
        [TestMethod]
        public void EchoProcessSendsUnsuccessfulOpenHandshake()
        {
            // var run = new TestScheduler();

            // var actualWrites = new List<byte[]>();
            // var socket = MockSocket(actualWrites, Origin);

            // var messages = run.CreateColdObservable(
            //     OnNext(10, new BadUpgradeRequest(Origin, UpgradeFail.MalformedHeaderLine))
            // );

            // var expected = run.CreateHotObservable(
            //     OnNext(110, new ProcessResult(run.Now, Origin, ResultType.UnSuccessfulOpeningHandshake)),
            //     OnNext(111, new ProcessResult(run.Now, Origin, ResultType.CloseSocket))
            // );

            // var actual = run.Start(
            //     create: () => messages.EchoProcess(socket.Object, run),
            //     created: 0,
            //     subscribed: 100,
            //     disposed: 1000
            // );

            // ReactiveAssert.AreElementsEqual(
            //    expected: expected.Messages,
            //    actual: actual.Messages,
            //    message: debugElementsEqual(expected.Messages, actual.Messages));

            // Assert.IsTrue(actualWrites.Count() == 1);
        }

        [Ignore]
        [TestMethod]
        public void EchoProcessResendsTextMessageToSocket()
        {
            // var run = new TestScheduler();

            // var actualWrites = new List<byte[]>();
            // var socket = MockSocket(actualWrites, Origin);

            // var messages = run.CreateColdObservable(
            //     OnNext(10, new TextMessage(Origin, OpCode.Text, Create("test")))
            // );

            // var expected = run.CreateHotObservable(
            //     OnNext(110, new ProcessResult(run.Now, Origin, ResultType.TextMessageSent))
            // );

            // var actual = run.Start(
            //     create: () => messages.EchoProcess(socket.Object, run),
            //     created: 0,
            //     subscribed: 100,
            //     disposed: 1000
            // );

            // ReactiveAssert.AreElementsEqual(
            //    expected: expected.Messages,
            //    actual: actual.Messages,
            //    message: debugElementsEqual(expected.Messages, actual.Messages));

            // Assert.AreEqual(1, actualWrites.Count());
        }

        [Ignore]
        [TestMethod]
        public void EchoProcessPerformsCloseHandshakeAndSignalsSocketClose()
        {
            // var run = new TestScheduler();

            // var origin = "test";
            // var actualWrites = new List<byte[]>();
            // var socket = MockSocket(actualWrites, origin);

            // var messages = run.CreateColdObservable(
            //     OnNext(10, new Close(Origin, Create(1000, "")))
            // );

            // var expected = run.CreateHotObservable(
            //     OnNext(110, new ProcessResult(new DateTimeOffset(110, TimeSpan.FromSeconds(0)), origin, ResultType.CloseHandshakeFinished)),
            //     OnNext(111, new ProcessResult(new DateTimeOffset(110, TimeSpan.FromSeconds(0)), origin, ResultType.CloseSocket))
            // );

            // var actual = run.Start(
            //     create: () => messages.EchoProcess(socket.Object, run),
            //     created: 0,
            //     subscribed: 100,
            //     disposed: 1000
            // );

            // ReactiveAssert.AreElementsEqual(
            //    expected: expected.Messages,
            //    actual: actual.Messages,
            //    message: debugElementsEqual(expected.Messages, actual.Messages));

            // Assert.IsTrue(actualWrites.Single().SequenceEqual(NormalClose));
        }
    }
}