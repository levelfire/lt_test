using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;

namespace Clent02
{
    public partial class Form1 : Form
    {
        const int portNo = 8011;
        //const int portNo = 500;
        const int ClientCount = 1;
        TcpClient client;
        TcpClient[] clientlist = new TcpClient[ClientCount];
        byte[] data;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (btnSign.Text == "SignIn")
            {
                try
                {
                    //for (int i = 0; i < ClientCount; ++i)
                    //{
                    //    clientlist[i] = new TcpClient();
                    //    clientlist[i].Connect("192.168.1.50", portNo);
                    //    data = new byte[clientlist[i].ReceiveBufferSize];

                    //    //SendMessage(txtNick.Text);

                    //    clientlist[i].GetStream().BeginRead(data, 0,
                    //        System.Convert.ToInt32(clientlist[i].ReceiveBufferSize),
                    //        ReceiveMessage, null);
                    //}
                    client = new TcpClient();
                    client.Connect("192.168.1.132", portNo);
                    data = new byte[client.ReceiveBufferSize];

                    //SendMessage(txtNick.Text);

                    client.GetStream().BeginRead(data, 0,
                        System.Convert.ToInt32(client.ReceiveBufferSize),
                        ReceiveMessage, null);

                    btnSign.Text = "SignOut";
                    btnSend.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("button1_Click error : " + ex.ToString());
                }

            }
            else 
            {
                Disconnect();
                btnSign.Text = "SignIn";
                btnSend.Enabled = false;
            }
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage(txtMessage.Text);
            txtMessage.Clear();
        }

        public void SendMessage(string message)
        {
                try
                {
                    string[] szComm = message.Split(' ');
                    NetworkStream ns = client.GetStream();
                    //byte[] dataI = System.BitConverter.GetBytes(99);
                    //byte[] dataS = System.Text.Encoding.ASCII.GetBytes(message);

                    //byte[] nCon = new byte[dataI.Length + dataS.Length];
                    //dataI.CopyTo(nCon, 0);
                    //dataS.CopyTo(nCon, dataI.Length);

                    ////ns.Write(dataS, 0,dataS.Length);
                    //ns.Write(nCon, 0, nCon.Length);
                    //ns.Write(nCon, 0, nCon.Length);
                    ////byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                    ////ns.Write(data, 0, data.Length);
                    //ns.Flush();
                    //for (int i = 0; i <= 100; ++i)
                    //{

                        if (szComm[0] == "singleenter")
                        {
                            //玄关进管卡
                            byte[] group = { 0x00 };
                            byte[] type = { 0x01 };

                            byte[] nCon = new byte[1 + 1 + 4 + 1 + 1 + 4 * 3];

                            byte[] nLenth = System.BitConverter.GetBytes(4 + 1 + 1 + 4 * 3);

                            byte[] data1 = System.BitConverter.GetBytes(1);
                            byte[] data2 = System.BitConverter.GetBytes(1);
                            byte[] data3 = System.BitConverter.GetBytes(1);

                            byte[] head = System.Text.Encoding.ASCII.GetBytes("<");
                            byte[] end = System.Text.Encoding.ASCII.GetBytes(">");

                            head.CopyTo(nCon, 0);
                            nLenth.CopyTo(nCon, 1);
                            group.CopyTo(nCon, 5);
                            type.CopyTo(nCon, 6);
                            data1.CopyTo(nCon, 7);
                            data2.CopyTo(nCon, 11);
                            data3.CopyTo(nCon, 15);
                            end.CopyTo(nCon, 19);

                            ns.Write(nCon, 0, nCon.Length);
                        }
                        else if (szComm[0] == "login")
                        {
                            //登陆
                            byte[] group = { 0x00 };
                            byte[] type = { 0x00 };

                            byte[] head = System.Text.Encoding.ASCII.GetBytes("<");
                            byte[] end = System.Text.Encoding.ASCII.GetBytes(">");

                            //byte[] name = System.Text.Encoding.ASCII.GetBytes("test01");
                            //byte[] password = System.Text.Encoding.ASCII.GetBytes("123");

                            byte[] name = System.Text.Encoding.ASCII.GetBytes(szComm[1]);
                            byte[] password = System.Text.Encoding.ASCII.GetBytes(szComm[2]);

                            byte[] data1 = System.BitConverter.GetBytes(name.Length);
                            byte[] data2 = System.BitConverter.GetBytes(password.Length);

                            byte[] nCon = new byte[1 + 1 + 4 + 1 + 1 + 8 + name.Length + password.Length];

                            byte[] nLenth = System.BitConverter.GetBytes(4 + 1 + 1 + +4 + 4 + name.Length + password.Length);

                            head.CopyTo(nCon, 0);
                            nLenth.CopyTo(nCon, 1);
                            group.CopyTo(nCon, 5);
                            type.CopyTo(nCon, 6);
                            data1.CopyTo(nCon, 7);
                            data2.CopyTo(nCon, 11);
                            name.CopyTo(nCon, 15);
                            password.CopyTo(nCon, 15 + name.Length);
                            end.CopyTo(nCon, 15 + name.Length + password.Length);

                            ns.Write(nCon, 0, nCon.Length);
                        }
                        //else if (message == "2")
                        //{
                        //    //登陆
                        //    byte[] group = { 0x00 };
                        //    byte[] type = { 0x00 };

                        //    byte[] head = System.Text.Encoding.ASCII.GetBytes("<");
                        //    byte[] end = System.Text.Encoding.ASCII.GetBytes(">");

                        //    byte[] name = System.Text.Encoding.ASCII.GetBytes("test02");
                        //    byte[] password = System.Text.Encoding.ASCII.GetBytes("123");

                        //    byte[] data1 = System.BitConverter.GetBytes(name.Length);
                        //    byte[] data2 = System.BitConverter.GetBytes(password.Length);

                        //    byte[] nCon = new byte[1 + 1 + 4 + 1 + 1 + 8 + name.Length + password.Length];

                        //    byte[] nLenth = System.BitConverter.GetBytes(4 + 1 + 1 + +4 + 4 + name.Length + password.Length);

                        //    head.CopyTo(nCon, 0);
                        //    nLenth.CopyTo(nCon, 1);
                        //    group.CopyTo(nCon, 5);
                        //    type.CopyTo(nCon, 6);
                        //    data1.CopyTo(nCon, 7);
                        //    data2.CopyTo(nCon, 11);
                        //    name.CopyTo(nCon, 15);
                        //    password.CopyTo(nCon, 15 + name.Length);
                        //    end.CopyTo(nCon, 15 + name.Length + password.Length);

                        //    ns.Write(nCon, 0, nCon.Length);
                        //}
                        //else if (message == "3")
                        else if (szComm[0] == "create")
                        {
                            //创建房间
                            byte[] group = { 0x00 };
                            byte[] type = { 0x07 };

                            byte[] head = System.Text.Encoding.ASCII.GetBytes("<");
                            byte[] end = System.Text.Encoding.ASCII.GetBytes(">");

                            //byte[] name = System.Text.Encoding.UTF8.GetBytes("房间");
                            //byte[] password = System.Text.Encoding.ASCII.GetBytes("");

                            byte[] name = System.Text.Encoding.UTF8.GetBytes("房间");
                            byte[] password = System.Text.Encoding.ASCII.GetBytes("");

                            byte[] data1 = System.BitConverter.GetBytes(name.Length);
                            byte[] data2 = System.BitConverter.GetBytes(password.Length);

                            byte[] nID = System.BitConverter.GetBytes(System.Convert.ToInt32(szComm[1]));
                            byte[] Count = System.BitConverter.GetBytes(System.Convert.ToInt32(szComm[2]));

                            byte[] nCon = new byte[16+4+4 + name.Length + password.Length];
                            byte[] nLenth = System.BitConverter.GetBytes(nCon.Length-2);

                            head.CopyTo(nCon, 0);
                            nLenth.CopyTo(nCon, 1);
                            group.CopyTo(nCon, 5);
                            type.CopyTo(nCon, 6);
                            nID.CopyTo(nCon, 7);
                            Count.CopyTo(nCon, 11);
                            data1.CopyTo(nCon, 15);
                            data2.CopyTo(nCon, 19);
                            name.CopyTo(nCon, 23);
                            if (password.Length > 0)
                            {
                                password.CopyTo(nCon, 23 + name.Length);
                            } 
                            
                            end.CopyTo(nCon, 23 + name.Length + password.Length);

                            ns.Write(nCon, 0, nCon.Length);
                        }
                        else if (message == "4")
                        {
                            //加入房间
                            byte[] group = { 0x00 };
                            byte[] type = { 0x08 };

                            byte[] head = System.Text.Encoding.ASCII.GetBytes("<");
                            byte[] end = System.Text.Encoding.ASCII.GetBytes(">");

                            byte[] name = System.Text.Encoding.UTF8.GetBytes("房间");
                            byte[] password = System.Text.Encoding.ASCII.GetBytes("");

                            byte[] nID = System.BitConverter.GetBytes(1);
                            byte[] nIndex = System.BitConverter.GetBytes(1);

                            byte[] nCon = new byte[4 + 4 + 4 + 4];
                            byte[] nLenth = System.BitConverter.GetBytes(nCon.Length - 2);


                            head.CopyTo(nCon, 0);
                            nLenth.CopyTo(nCon, 1);
                            group.CopyTo(nCon, 5);
                            type.CopyTo(nCon, 6);
                            nID.CopyTo(nCon, 7);
                            nIndex.CopyTo(nCon, 11);

                            end.CopyTo(nCon, 15);

                            ns.Write(nCon, 0, nCon.Length);
                        }
                        else if (message == "5")
                        {
                            //加入房间
                            byte[] group = { 0x00 };
                            byte[] type = { 0x06 };

                            byte[] head = System.Text.Encoding.ASCII.GetBytes("<");
                            byte[] end = System.Text.Encoding.ASCII.GetBytes(">");

                            byte[] nID = System.BitConverter.GetBytes(2);

                            byte[] nCon = new byte[4 + 4 + 4];
                            byte[] nLenth = System.BitConverter.GetBytes(nCon.Length - 2);


                            head.CopyTo(nCon, 0);
                            nLenth.CopyTo(nCon, 1);
                            group.CopyTo(nCon, 5);
                            type.CopyTo(nCon, 6);
                            nID.CopyTo(nCon, 7);

                            end.CopyTo(nCon, 11);

                            ns.Write(nCon, 0, nCon.Length);
                        }
                        else if (szComm[0] == "ready")
                        {
                            //加入房间
                            byte[] group = { 0x00 };
                            byte[] type = { 14 };

                            byte[] head = System.Text.Encoding.ASCII.GetBytes("<");
                            byte[] end = System.Text.Encoding.ASCII.GetBytes(">");

                            byte[] nID = System.BitConverter.GetBytes(System.Convert.ToInt32(szComm[1]));
                            byte[] nIndex = System.BitConverter.GetBytes(System.Convert.ToInt32(szComm[2]));

                            byte[] nCon = new byte[4 + 4 + 4 + 4];
                            byte[] nLenth = System.BitConverter.GetBytes(nCon.Length - 2);


                            head.CopyTo(nCon, 0);
                            nLenth.CopyTo(nCon, 1);
                            group.CopyTo(nCon, 5);
                            type.CopyTo(nCon, 6);
                            nID.CopyTo(nCon, 7);
                            nIndex.CopyTo(nCon, 11);
                            end.CopyTo(nCon, 15);

                            ns.Write(nCon, 0, nCon.Length);
                        }
                        else if (szComm[0] == "join")
                        {
                            //加入房间
                            byte[] group = { 0x00 };
                            byte[] type = { 8 };

                            byte[] head = System.Text.Encoding.ASCII.GetBytes("<");
                            byte[] end = System.Text.Encoding.ASCII.GetBytes(">");

                            byte[] nID = System.BitConverter.GetBytes(System.Convert.ToInt32(szComm[1]));
                            byte[] nIndex = System.BitConverter.GetBytes(System.Convert.ToInt32(szComm[2]));

                            byte[] nCon = new byte[4 + 4 + 4 + 4];
                            byte[] nLenth = System.BitConverter.GetBytes(nCon.Length - 2);


                            head.CopyTo(nCon, 0);
                            nLenth.CopyTo(nCon, 1);
                            group.CopyTo(nCon, 5);
                            type.CopyTo(nCon, 6);
                            nID.CopyTo(nCon, 7);
                            nIndex.CopyTo(nCon, 11);
                            end.CopyTo(nCon, 15);

                            ns.Write(nCon, 0, nCon.Length);
                        }

                        ns.Flush();
                        //ns.Flush();
                    //}
                }
                catch (Exception ex)
                {
                    MessageBox.Show("SendMessage error :" + ex.ToString());
                }
        }

        public void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                //foreach (TcpClient c in clientlist)
                //{
                //    int bytesRead;
                //    bytesRead = c.GetStream().EndRead(ar);

                //    if (bytesRead < 1)
                //    {
                //        continue;
                //    }
                //    else
                //    {
                //        object[] para = { System.Text.Encoding.ASCII.GetString(data, 0, bytesRead) };
                //        this.Invoke(new delUpdateHistory(UpdateHistory), para);

                //        c.GetStream().BeginRead(data, 0,
                //            System.Convert.ToInt32(c.ReceiveBufferSize),
                //            ReceiveMessage, null);
                //    }
                //}

                int bytesRead;
                bytesRead = client.GetStream().EndRead(ar);

                if (bytesRead < 1)
                {
                    //continue;
                }
                else
                {
                    object[] para = { System.Text.Encoding.ASCII.GetString(data, 0, bytesRead) };
                    this.Invoke(new delUpdateHistory(UpdateHistory), para);

                    client.GetStream().BeginRead(data, 0,
                        System.Convert.ToInt32(client.ReceiveBufferSize),
                        ReceiveMessage, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(/*"ReceiveMessage erroe :" + */ex.ToString());
            }
            //try
            //{
            //    //if (0 == client.Available)
            //    //{
            //    //    return;
            //    //}


            //    int bytesRead;
            //    bytesRead = client.GetStream().EndRead(ar);

            //    if (bytesRead < 1)
            //    {
            //        return;
            //    }
            //    else
            //    {
            //        object[] para = { System.Text.Encoding.ASCII.GetString(data, 0, bytesRead) };
            //        this.Invoke(new delUpdateHistory(UpdateHistory), para);
            //    }

            //    client.GetStream().BeginRead(data, 0,
            //            System.Convert.ToInt32(client.ReceiveBufferSize),
            //            ReceiveMessage, null);

            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("ReceiveMessage erroe :" + ex.ToString());
            //}
        }

        public delegate void delUpdateHistory(string str);
        public void UpdateHistory(string str)
        {
            rtxtMsg.AppendText(str);
        }

        public void Disconnect()
        {
            try
            {
                client.GetStream().Close();
                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Disconnect Error : " + ex.ToString());
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Closing(object sender, EventArgs e)
        {
            Disconnect();
        }
        
    }
}
