using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using static System.Reactive.Linq.Observable;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Collections.Generic;
using System.Threading;
using System.Reactive.Subjects;
using WSr.Deciding;

namespace WSr.Socket
{
    public class TcpConnection : IConnectedSocket
    {
        private readonly TcpClient _socket;

        protected TcpConnection() { }

        internal TcpConnection(TcpClient connectedSocket)
        {
            _socket = connectedSocket;
        }

        public string Address => _socket.Client.RemoteEndPoint.ToString();

        public virtual Stream Stream => _socket.GetStream();

        private Func<IScheduler, byte[], IObservable<int>> CreateReader(int bufferSize)
        {
            return (scheduler, buffer) => Observable
                .FromAsync(() => Stream.ReadAsync(buffer, 0, bufferSize), scheduler);
        }

        private Func<IScheduler, byte[], IObservable<Unit>> CreateWriter()
        {
            return (scheduler, buffer) => FromAsync(() => Stream.WriteAsync(buffer, 0, buffer.Length), scheduler);
        }

        public virtual void Dispose()
        {
            Console.WriteLine($"{Address} disposed.");
            _socket.Dispose();
        }

        public override string ToString()
        {
            return Address;
        }

        public IObservable<Unit> Send(
            IEnumerable<byte> buffer,
            IScheduler scheduler)
        {
            var writer = CreateWriter();

            return writer(scheduler, buffer.ToArray())
                .Do(x => Console.WriteLine($"Wrote {buffer.Count()} on {Address}"));
        }

        public IObservable<IEnumerable<byte>> Receive(byte[] buffer, IScheduler scheduler)
        {
            var reader = CreateReader(buffer.Length);

            return reader(scheduler, buffer)
                .Repeat()
                .TakeWhile(x => x > 0)
                .Do(x => Console.WriteLine($"read {x} bytes from {Address}"))
                .Select(r => buffer.Take(r).ToArray())
                .Catch<byte[], ObjectDisposedException>(e => Observable.Empty<byte[]>());
        }
    }

}