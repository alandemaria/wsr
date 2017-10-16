using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using static WSr.IntegersFromByteConverter;

namespace WSr.Protocol
{
    public static class DefragDataFrames
    {
        public static IObservable<Parse<Fail, Frame>> Defrag(
            this IObservable<Parse<Fail, Frame>> fragmented) =>
            fragmented.WithParser(x => Observable.Create<Parse<Fail, Frame>>(o =>
        {
            OpCode? continuingOn = null;
            var binary = ParsedFrame.Empty;
            var text = TextFrame.Empty;
            
            return x.Subscribe(
                onNext: f =>
                {
                    if (f.IsControlCode())
                    {
                        o.OnNext(new Parse<Fail, Frame>(f));
                    }
                    else
                    {
                        if (f.IsContinuation() && !continuingOn.HasValue)
                            o.OnNext(new Parse<Fail, Frame>(Fail.ProtocolError("not expecting continuation")));
                        if (!f.IsContinuation() && continuingOn.HasValue)
                            o.OnNext(new Parse<Fail, Frame>(Fail.ProtocolError("expecting continuation")));
                        if (f.IsFinal() && !continuingOn.HasValue)
                        {
                            o.OnNext(new Parse<Fail, Frame>(f));
                        }
                        else
                        {
                            if (f.ExpectContinuation())
                                continuingOn = f.GetOpCode();
                            if (continuingOn == OpCode.Text && f is TextFrame t)
                            {
                                text = text.Concat(t);
                            }
                            else if (f is ParsedFrame p)
                            {
                                binary = binary.Concat(p);
                            }
                            if (f.EndsContinuation())
                            {
                                if (continuingOn == OpCode.Text)
                                {
                                    o.OnNext(new Parse<Fail, Frame>(text));
                                    text = TextFrame.Empty;
                                }
                                else
                                {
                                    o.OnNext(new Parse<Fail, Frame>(binary));
                                    binary = ParsedFrame.Empty;
                                }
                                continuingOn = null;
                            }
                        }
                    }
                },
                onError: o.OnError,
                onCompleted: o.OnCompleted
            );
        }));
    }
}