using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
using System.Windows.Forms;

//using System.Data.OleDb;
//using Server_Form.Protorol;
//using System.IO;
using servercore;
using Server_Form.Properties;

namespace Server_Form
{
    public partial class Form1 : Form
    {
        TSocketServerBase<TTestSession, TTestAccessDatabase> m_socketServer;

        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        private void SocketServerDemo_Load(object sender, EventArgs e)
        {
            cb_maxDatagramSize.SelectedIndex = 1;
            ////初始化数据库连接
            //Common.Connection.Host = "localhost";
            //Common.Connection.Uid = "root";
            //Common.Connection.Pwd = "a";
            //Common.Connection.DB = "diff";

            ////Common.Connection.Host = Resources.Config.Host;
            ////Common.Connection.Uid = Resources.Config.UID;
            ////Common.Connection.Pwd = Resources.Config.PWD;
            ////Common.Connection.DB = Resources.Config.DB;

            //Common.DBHelper.UpdateConnectionString();
            //先连接数据库，再读取数据
            Define.LoadFile();

            //Team.InitRobot();
        }

        private void SocketServerDemo_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_socketServer != null)
            {
                m_socketServer.Dispose();  // 关闭服务器进程
            }
        }

        private void AttachServerEvent()
        {
            //OnAddInfo用来显示table中的输出事件
            m_socketServer.ShowMessageAddInfo += this.OnAddInfo;

            m_socketServer.ServerStarted += this.SocketServer_Started;
            m_socketServer.ServerClosed += this.SocketServer_Stoped;
            m_socketServer.ServerListenPaused += this.SocketServer_Paused;
            m_socketServer.ServerListenResumed += this.SocketServer_Resumed;
            m_socketServer.ServerException += this.SocketServer_Exception;

            m_socketServer.SessionRejected += this.SocketServer_SessionRejected;
            m_socketServer.SessionConnected += this.SocketServer_SessionConnected;
            m_socketServer.SessionDisconnected += this.SocketServer_SessionDisconnected;
            m_socketServer.SessionReceiveException += this.SocketServer_SessionReceiveException;
            m_socketServer.SessionSendException += this.SocketServer_SessionSendException;

            m_socketServer.DatagramDelimiterError += this.SocketServer_DatagramDelimiterError;
            m_socketServer.DatagramOversizeError += this.SocketServer_DatagramOversizeError;
            m_socketServer.DatagramAccepted += this.SocketServer_DatagramReceived;
            m_socketServer.DatagramError += this.SocketServer_DatagramrError;
            m_socketServer.DatagramHandled += this.SocketServer_DatagramHandled;
            m_socketServer.DatagramLogout += this.SocketServer_DatagramLogout;

            if (ck_UseDatabase.Checked)
            {
                m_socketServer.DatabaseOpenException += this.SocketServer_DatabaseOpenException;
                m_socketServer.DatabaseCloseException += this.SocketServer_DatabaseCloseException;
                m_socketServer.DatabaseException += this.SocketServer_DatabaseException;
            }

            m_socketServer.ShowDebugMessage += this.SocketServer_ShowDebugMessage;
        }

        private void bn_Start_Click(object sender, EventArgs e)
        {

            string connStr = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source = DemoAccessDatabase.mdb;";

            if (ck_UseDatabase.Checked)
            {
                m_socketServer = new TSocketServerBase<TTestSession, TTestAccessDatabase>(1024, 32 * 1024, 64 * 1024, connStr);
            }
            else
            {
                m_socketServer = new TSocketServerBase<TTestSession, TTestAccessDatabase>();
            }

            m_socketServer.MaxDatagramSize = 1024 * int.Parse(cb_maxDatagramSize.Text);

            this.AttachServerEvent();  // 附加服务器全部事件
            m_socketServer.Start();
        }

        private void bn_Stop_Click(object sender, EventArgs e)
        {
            m_socketServer.Stop();
            m_socketServer.Dispose();
        }

        private void bn_Pause_Click(object sender, EventArgs e)
        {
            m_socketServer.PauseListen();
        }

        private void bn_Resume_Click(object sender, EventArgs e)
        {
            m_socketServer.ResumeListen();
        }

        private void OnAddInfo(object sender, OnMessageEventArgs e)
        {
            this.AddInfo(e.Message);
        }

        private void SocketServer_Started(object sender, EventArgs e)
        {
            this.AddInfo("Server started at: " + DateTime.Now.ToString());
        }

        private void SocketServer_Stoped(object sender, EventArgs e)
        {
            this.AddInfo("Server stoped at: " + DateTime.Now.ToString());
        }

        private void SocketServer_Paused(object sender, EventArgs e)
        {
            this.AddInfo("Server paused at: " + DateTime.Now.ToString());
        }

        private void SocketServer_Resumed(object sender, EventArgs e)
        {
            this.AddInfo("Server resumed at: " + DateTime.Now.ToString());
        }

        private void SocketServer_Exception(object sender, TExceptionEventArgs e)
        {
            this.tb_ServerExceptionCount.Text = m_socketServer.ServerExceptionCount.ToString();
            this.AddInfo("Server exception: " + e.ExceptionMessage);
        }

        private void SocketServer_SessionRejected(object sender, EventArgs e)
        {
            this.AddInfo("Session connect rejected");
        }

        private void SocketServer_SessionTimeout(object sender, TSessionEventArgs e)
        {
            this.AddInfo("Session timeout: ip " + e.SessionBaseInfo.IP);
        }

        private void SocketServer_SessionConnected(object sender, TSessionEventArgs e)
        {
            this.tb_SessionCount.Text = m_socketServer.SessionCount.ToString();
            this.AddInfo("Session connected: ip " + e.SessionBaseInfo.IP);
        }

        private void SocketServer_SessionDisconnected(object sender, TSessionEventArgs e)
        {
            this.tb_SessionCount.Text = m_socketServer.SessionCount.ToString();
            this.AddInfo("Session disconnected: ip " + e.SessionBaseInfo.IP);
        }

        private void SocketServer_SessionReceiveException(object sender, TSessionEventArgs e)
        {
            this.tb_SessionCount.Text = m_socketServer.SessionCount.ToString();
            this.tb_ClientExceptionCount.Text = m_socketServer.SessionExceptionCount.ToString();
            this.AddInfo("Session receive exception: ip " + e.SessionBaseInfo.IP);
        }

        private void SocketServer_SessionSendException(object sender, TSessionEventArgs e)
        {
            this.tb_SessionCount.Text = m_socketServer.SessionCount.ToString();
            this.tb_ClientExceptionCount.Text = m_socketServer.SessionExceptionCount.ToString();
            this.AddInfo("Session send exception: ip " + e.SessionBaseInfo.IP);
        }

        private void SocketServer_SocketReceiveException(object sender, TSessionExceptionEventArgs e)
        {
            this.tb_SessionCount.Text = m_socketServer.SessionCount.ToString();
            this.tb_ClientExceptionCount.Text = m_socketServer.SessionExceptionCount.ToString();
            this.AddInfo("client socket receive exception: ip: " + e.SessionBaseInfo.IP + " exception message: " + e.ExceptionMessage);
        }

        private void SocketServer_SocketSendException(object sender, TSessionExceptionEventArgs e)
        {
            this.tb_SessionCount.Text = m_socketServer.SessionCount.ToString();
            this.tb_ClientExceptionCount.Text = m_socketServer.SessionExceptionCount.ToString();
            this.AddInfo("client socket send exception: ip: " + e.SessionBaseInfo.IP + " exception message: " + e.ExceptionMessage);
        }

        private void SocketServer_DatagramDelimiterError(object sender, TSessionEventArgs e)
        {
            this.tb_DatagramCount.Text = m_socketServer.ReceivedDatagramCount.ToString();
            this.tb_DatagramQueueCount.Text = m_socketServer.DatagramQueueLength.ToString();
            this.tb_ErrorDatagramCount.Text = m_socketServer.ErrorDatagramCount.ToString();

            this.AddInfo("datagram delimiter error. ip: " + e.SessionBaseInfo.IP);
        }

        private void SocketServer_DatagramOversizeError(object sender, TSessionEventArgs e)
        {
            this.tb_DatagramCount.Text = m_socketServer.ReceivedDatagramCount.ToString();
            this.tb_DatagramQueueCount.Text = m_socketServer.DatagramQueueLength.ToString();
            this.tb_ErrorDatagramCount.Text = m_socketServer.ErrorDatagramCount.ToString();

            this.AddInfo("datagram oversize error. ip: " + e.SessionBaseInfo.IP);
        }

        private void SocketServer_DatagramReceived(object sender, TSessionEventArgs e)
        {
            this.tb_DatagramCount.Text = m_socketServer.ReceivedDatagramCount.ToString();
            this.tb_DatagramQueueCount.Text = m_socketServer.DatagramQueueLength.ToString();
            this.AddInfo("datagram received. ip: " + e.SessionBaseInfo.IP);
        }

        private void SocketServer_DatagramrError(object sender, TSessionEventArgs e)
        {
            this.tb_DatagramCount.Text = m_socketServer.ReceivedDatagramCount.ToString();
            this.tb_DatagramQueueCount.Text = m_socketServer.DatagramQueueLength.ToString();
            this.tb_ErrorDatagramCount.Text = m_socketServer.ErrorDatagramCount.ToString();

            this.AddInfo("datagram error. ip: " + e.SessionBaseInfo.IP);
        }

        private void SocketServer_DatagramHandled(object sender, TSessionEventArgs e)
        {
            this.tb_DatagramCount.Text = m_socketServer.ReceivedDatagramCount.ToString();
            this.tb_DatagramQueueCount.Text = m_socketServer.DatagramQueueLength.ToString();
            this.AddInfo("datagram handled. ip: " + e.SessionBaseInfo.IP);
        }

        //2011.11.16 增加log输出
        private void SocketServer_DatagramLogout(object sender, TSessionEventArgs e)
        {
            this.AddInfo("datagram logout: " + e.SessionBaseInfo.Logout);
        }

        private void SocketServer_DatabaseOpenException(object sender, TExceptionEventArgs e)
        {
            this.tb_DatabaseExceptionCount.Text = m_socketServer.DatabaseExceptionCount.ToString();
            this.AddInfo("open database exception: " + e.ExceptionMessage);
        }

        private void SocketServer_DatabaseCloseException(object sender, TExceptionEventArgs e)
        {
            this.tb_DatabaseExceptionCount.Text = m_socketServer.DatabaseExceptionCount.ToString();
            this.AddInfo("close database exception: " + e.ExceptionMessage);
        }

        private void SocketServer_DatabaseException(object sender, TExceptionEventArgs e)
        {
            this.tb_DatabaseExceptionCount.Text = m_socketServer.DatabaseExceptionCount.ToString();
            this.AddInfo("operate database exception: " + e.ExceptionMessage);
        }

        private void SocketServer_ShowDebugMessage(object sender, TExceptionEventArgs e)
        {
            this.AddInfo("debug message: " + e.ExceptionMessage);
        }

        private void AddInfo(string message)
        {
            if (lb_ServerInfo.Items.Count > 1000)
            {
                lb_ServerInfo.Items.Clear();
            }

            lb_ServerInfo.Items.Add(message);
            lb_ServerInfo.SelectedIndex = lb_ServerInfo.Items.Count - 1;
            lb_ServerInfo.Focus();
        }

        public void MessageAddInfo(string message)
        {
            this.AddInfo(message);
        }
    }


}
