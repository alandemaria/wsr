using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using WSr.Deciding;

namespace WSr.Listening
{
    public static class Functions
    {
        public static IListeningSocket ListenTo(string ip, int port)
        {
            return new TcpSocket(ip, port);
        }

        public static IObservable<IConnectedSocket> AcceptConnections(
            this IListeningSocket server,
            IScheduler scheduler = null)
        {
            if (scheduler == null) scheduler = Scheduler.Default;

            return server.Connect(scheduler).Repeat();
        }

        public static IObservable<IConnectedSocket> Serve(
            string ip, 
            int port,
            IObservable<Unit> eof,
            IScheduler s = null)
        {
            if (s == null) s = Scheduler.Default;

            return Observable.Using(
                resourceFactory: () => new TcpSocket(ip, port),
                observableFactory: l => l.Connect(s).Repeat().TakeUntil(eof));
        }
    }
}