using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

namespace servercore
{
    public interface ISessionEvent
    {
        event EventHandler<TSessionExceptionEventArgs> SessionReceiveException;
        event EventHandler<TSessionExceptionEventArgs> SessionSendException;
        event EventHandler<TSessionEventArgs> DatagramDelimiterError;
        event EventHandler<TSessionEventArgs> DatagramOversizeError;
        event EventHandler<TSessionEventArgs> DatagramAccepted;
        event EventHandler<TSessionEventArgs> DatagramError;
        event EventHandler<TSessionEventArgs> DatagramHandled;
        //2011.11.16增加log输出
        event EventHandler<TSessionEventArgs> DatagramLogout;
    }
}
