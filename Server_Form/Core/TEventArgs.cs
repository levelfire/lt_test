using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

namespace servercore
{
    //实现传递字符串参数的事件
    public class OnMessageEventArgs : EventArgs
    {
        private string message;
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
            }
        }
    }

    public class TExceptionEventArgs : EventArgs
    {
        private string m_exceptionMessage;

        public TExceptionEventArgs(Exception exception)
        {
            m_exceptionMessage = exception.Message;
        }

        public TExceptionEventArgs(string exceptionMessage)
        {
            m_exceptionMessage = exceptionMessage;
        }

        public string ExceptionMessage
        {
            get { return m_exceptionMessage; }
        }
    }

    public class TSessionEventArgs : EventArgs
    {
        TSessionCoreInfo m_sessionBaseInfo;

        public TSessionEventArgs(TSessionCoreInfo sessionCoreInfo)
        {
            m_sessionBaseInfo = sessionCoreInfo;
        }

        public TSessionCoreInfo SessionBaseInfo
        {
            get { return m_sessionBaseInfo; }
        }
    }

    public class TSessionExceptionEventArgs : TSessionEventArgs
    {
        private string m_exceptionMessage;

        public TSessionExceptionEventArgs(Exception exception, TSessionCoreInfo sessionCoreInfo)
            : base(sessionCoreInfo)
        {
            m_exceptionMessage = exception.Message;
        }

        public string ExceptionMessage
        {
            get { return m_exceptionMessage; }
        }
    }
}
