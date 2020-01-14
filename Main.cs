using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using EasyHook;
using System.Runtime.Remoting;
using System.Threading;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.InteropServices;
using msg;
using Utils;
using ProtoBuf;
using MUGame;
using System.IO;
namespace SocketEasyHook
{
    public partial class Main : Form
    {

        targetInterFace Interface;
        public delegate void MyInvoke(BufferStruct ArgsBuffer);
        public bool isjiebao = true;
     
        public Main()
        {
            InitializeComponent();
        }
        const int WM_COPYDATA = 0x004A;
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }
        private void Main_Load(object sender, EventArgs e)
        {

            lstBuffer.GridLines = true;//显示行号
            lstBuffer.FullRowSelect = true;//选择一行
            lstBuffer.View = View.Details;//显示方式
            lstBuffer.Scrollable = true;//显示滚动条
            lstBuffer.MultiSelect = true;//不允许显示多行
            lstBuffer.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lstBuffer.CheckBoxes = true;
            lstBuffer.HideSelection = true;
            lstBuffer.Columns.Add("C/S", 60);
            lstBuffer.Columns.Add("Size", 60);
            lstBuffer.Columns.Add("Time", 80);
            lstBuffer.Columns.Add("Inde", 60);
            lstBuffer.Columns.Add("HEX", 400);
            lstBuffer.Columns.Add("ASCII", 400);
            SocketInterFace.LogEvent+=new SocketInterFace.LogArgsHander(SocketInterFace_LogEvent);
         
        }
        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {

            switch (m.Msg)
            {
                case WM_COPYDATA:
                    COPYDATASTRUCT cdata = new COPYDATASTRUCT();
                    Type mytype = cdata.GetType();
                    cdata = (COPYDATASTRUCT)m.GetLParam(mytype);
                   // this.textBox1.Text = cdata.lpData;
                    Console.WriteLine(cdata.lpData);
                    break;

                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }


        /// <summary>
        /// Socket Event Hander Log
        /// </summary>
        /// <param name="BufferArgs"></param>
        public void SocketInterFace_LogEvent(BufferStruct BufferArgs)
        {
            lock (this)
            {

              //  new Thread((ThreadStart)(delegate ()
               // {
                   
                
                    MyInvoke mi = new MyInvoke(shuchu);
                    //异步调用委托  
                    this.BeginInvoke(mi, new Object[] { BufferArgs });
                    
                    // if (this.checkBox1.Checked) lstBuffer.Items[lstBuffer.Items.Count - 1].EnsureVisible();
               // })).Start();
            }
        }
        private RC4 _rc4;
        public void shuchu(BufferStruct BufferArgs)
        {
            lock (this)
            {
                if (isjiebao)
                {
                    if ((textBox3.Text == "") || (textBox3.Text == BufferArgs.LoginIdent[0].ToString()))
                    {
                        
                        if ((BufferArgs.ObjectType == 0) &&(checkBox3.Checked))
                        {
                            ListViewItem item = new ListViewItem();
                            item.SubItems[0].Text = "send";
                            item.BackColor = Color.DeepSkyBlue;
                            item.SubItems.Add(BufferArgs.BufferSize.ToString());
                            item.SubItems.Add(Convert.ToString(DateTime.Now.ToString("T")));
                            item.SubItems.Add(string.Format("{0}:{1}", BufferArgs.LoginIdent[0].ToString(), BufferArgs.LoginIdent[1].ToString()));
                            item.SubItems.Add(byteToHexStr(BufferArgs.Buffer, BufferArgs.BufferSize));
                            item.SubItems.Add(byteToHexStr(BufferArgs.Buffer, BufferArgs.BufferSize));
                            item.SubItems.Add("-");
                            lstBuffer.Items.Add(item);
                        }
                        if ((BufferArgs.ObjectType == 1) && (checkBox4.Checked))
                        {
                            byte[] dyBuffer;
                            ListViewItem item = new ListViewItem();
                            item.SubItems[0].Text = "recv";
                            item.SubItems.Add(BufferArgs.BufferSize.ToString());
                            item.SubItems.Add(Convert.ToString(DateTime.Now.ToString("T")));
                            item.SubItems.Add(string.Format("{0}:{1}", BufferArgs.LoginIdent[0].ToString(), BufferArgs.LoginIdent[1].ToString()));
                            item.SubItems.Add(byteToHexStr(BufferArgs.Buffer, BufferArgs.BufferSize));                            
                            int port = int.Parse(BufferArgs.LoginIdent[0].ToString());
                            
                            byte[] key;
                            if (port == 11087)
                                {
                                    byte[] Buffer;
                                    byte[] buf = new byte[BufferArgs.BufferSize];
                                    buf = BufferArgs.Buffer;
                                    BinaryReader br;
                                    using (MemoryStream ms = new MemoryStream(buf))
                                    {
                                    
                                        br = new BinaryReader(ms);
                                        int buflen = br.ReadInt32();
                                        if ((BufferArgs.BufferSize - 4) > buflen)
                                        {
                                            Buffer= br.ReadBytes(buflen);
                                            dyBuffer = br.ReadBytes(BufferArgs.BufferSize - 4 - buflen);
                                            Console.WriteLine("有粘包出现了");
                                        }
                                        if ((BufferArgs.BufferSize - 4) == buflen)
                                        {
                                        if (this._rc4 == null)
                                        {
                                            br.ReadBytes(4);
                                            key = br.ReadBytes(buflen);
                                            this._rc4 = new RC4(key);
                                        }
                                        else
                                        {
                                            Buffer = br.ReadBytes(buflen);
                                            this._rc4.Crypt(Buffer, 8, Buffer.Length - 8);
                                            string gg = byteToHexStr(Buffer, Buffer.Length);
                                            Console.WriteLine(gg);
                                        }
                                            
                                        }

                                    
                                    }

                                }
                            item.SubItems.Add(byteToHexStr(BufferArgs.Buffer, BufferArgs.BufferSize));
                            item.SubItems.Add("-");
                            lstBuffer.Items.Add(item);
                        }
                    }
                }
                if (this.checkBox1.Checked) lstBuffer.Items[lstBuffer.Items.Count - 1].EnsureVisible();
            }
        }
        public static byte[] Desc(byte[] RecvBuffer)
        {
            //byte[] DescBuffer = new byte[RecvBuffer[0x14] + 0x14];
            byte[] DescBuffer = new byte[RecvBuffer.Length];
            Array.Copy(RecvBuffer, 0, DescBuffer, 0, DescBuffer.Length);
            Console.WriteLine("Recv:{0}  Desc:{1}", RecvBuffer.Length, DescBuffer.Length);
            for (int i = 0; i < RecvBuffer[0x14]; i++)
            {
                if (i + 0x18 == DescBuffer.Length) break;
                byte edx = RecvBuffer[4];
                byte bl = RecvBuffer[i + 0x18];
                edx ^= (byte)i;
                edx &= 3;
                byte dl = RecvBuffer[edx];
                bl ^= dl;
                DescBuffer[i + 0x18] = bl;
               
            }
            return DescBuffer;
        }

        /// <summary>
        /// 密钥表
        /// </summary>
        static byte[] Keys = strToToHexByte("1FCA29AC031E2DA027B271D48B86F5482F9AB9FC13EEBDF0378201249B5685983F6A494C23BE4D4047529174AB2615E84F3AD99C338EDD90572221C4BBF6A5385F0A69EC435E6DE067F2B114CBC635886FDAF93C532EFD3077C24164DB96C5D87FAA898C63FE8D808792D1B4EB6655288F7A19DC73CE1DD097626104FB36E5789F4AA92C839EAD20A732F1540B0675C8AF1A397C936E3D70B70281A41BD60518BFEAC9CCA33ECDC0C7D211F42BA69568CFBA591CB30E5D10D7A2A1443B7625B8DF8AE96CC3DEED60E77231944B46B508EF5A79BCD3AE7DB0F742C1E45B164558FF2A090CE37E0D00071251346BE6D5A80FFA995CF34E9D5017E2E1847BB665F83F50958A23FC49866708DD620B74511E0F40A5BA736CD93637F8ED925BE4E1CEDF30B5EAC3DC69E607E8FDC2AB54717EAF20C51A134CF996D7D80DF2FBC4012E7F10D54A63BC8946A7C81D224B3491DE4F00E57AB32C19F677B82D529BA4218E1FF0F5AA039CA9A647A83D82EB14B13EEFE005DA530C395617984DB23B8441EEBFD0150AA37CC906E7885DE28BF4D19E8FC0253AF3EC59B6B7786D12DB64614E5FB0356A435CE96687687D422BD4F1FE2FA0459A93CC791657588D727B4481AEFF9055CAE33C09C627489DA2CBB4115ECF8065FA33AC9976F738ADD21B24A10E9F70752A831C2926C728BD026B9431BE6F60855AD38CB9D69718CD32BB04C16E000000000102872496010807780158018872B309");
        /// <summary>
        /// 解密/加密
        /// </summary>
        /// <param name="Code">封包内容</param>
        /// <param name="LoginIndex">动态序列A</param>
        /// <param name="LoginIndexEx">动态序列B</param>
        /// <returns>返回加密/解密封包</returns>
        public static byte[] Desc(byte[] Code, int LoginIndex, int LoginIndexEx)
        {
            byte[] Desc = new byte[Code.Length];
            for (int i = 0; i < Code.Length; i++)
            {
                Desc[i] = Code[i] ^= Keys[LoginIndex];
                Desc[i] ^= Keys[LoginIndexEx + 0x100];
                LoginIndex++;
                if (LoginIndex == 256)
                {
                    LoginIndexEx++;
                    LoginIndex = 0;
                }
            }
            return Desc;
        }
        /// <summary>
        /// Hex转换数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                try
                {
                    returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                }
                catch
                {
                    returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 1), 16);
                }
            return returnBytes;
        }
        public static byte[] strToToHexByte(string hexString, int Leng)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[Leng];
            for (int i = 0; i < returnBytes.Length; i++)
                try
                {
                    returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                }
                catch
                {
                    returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 1), 16);
                }
            return returnBytes;
        }
        public static byte[] ASCIIToString(byte[] bytes, int byteLen)
        {
            for (int i = 0; i < byteLen; i++)
            {
                if (bytes[i] == 0) bytes[i] = 0x20;
            }
            return bytes;
        }

        /// <summary>
        /// 数组转换Hex
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="byteLen"></param>
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes, int byteLen)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < byteLen; i++)
                {
                    returnStr += bytes[i].ToString("X2") +" ";
                }
            }
            return returnStr;
        }

        /// <summary>
        /// Start HOOK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            Process[] UserProList = Process.GetProcessesByName(textBox4.Text);//MEmuHeadless
            if (UserProList.Length == 0)
            {
                MessageBox.Show("未启动游戏!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string ChannelName = null;
            try
            {
                try
                {
                    Config.Register("dashu", "SocketEasyHook.exe", "SocketEasyInject.dll");
                }
                catch (ApplicationException Ext)
                {
                    Console.WriteLine("执行远程注入出错:\r\n{0}", Ext.ToString());
                    MessageBox.Show("请确认当前系统权限为超级管理员!", "初始化", MessageBoxButtons.OK,MessageBoxIcon.Information);
                   // Process.GetCurrentProcess().Kill();
                }
                 RemoteHooking.IpcCreateServer<SocketInterFace>(ref ChannelName, WellKnownObjectMode.SingleCall);
                //  Console.WriteLine(ChannelName);



                if (UserProList[0].Responding)
                {
                    RemoteHooking.Inject(UserProList[0].Id, "SocketEasyInject.dll", "SocketEasyInject.dll", ChannelName);
                    Console.WriteLine(ChannelName);

                }
                else
                {
                    MessageBox.Show("游戏界面无响应,当前游戏关闭!请重新操作", "关闭", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UserProList[0].Kill();
                    return;
                }
              IpcClientChannel channel = new IpcClientChannel();
               // Register the channel with ChannelServices.
               ChannelServices.RegisterChannel(channel, true);
               Interface = (targetInterFace)Activator.GetObject(typeof(targetInterFace), "ipc://ServerChannel/targetInterFace");
                if (Interface == null)
                {
                    Console.WriteLine(Interface.gettime());
                    Console.WriteLine("无法连接到服务器接口。");
                    return;
                }
                else
                {
                    Console.WriteLine(Interface.gettime());
                    Console.WriteLine("连接到服务器接口。");
                }

            }
            catch (Exception ExtInfo)
            {
                Console.WriteLine("执行远程注入出错:\r\n{0}", ExtInfo.ToString());
            }
        }

        private void lstBuffer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
               
                textBox1.Text = "";
                textBox1.Text = lstBuffer.SelectedItems[0].SubItems[4].Text;
                Clipboard.SetText(textBox1.Text);
                textBox2.Text = "";
                byte[] bytes = strToToHexByte(lstBuffer.SelectedItems[0].SubItems[5].Text);
                if (checkBox2.Checked)
                {
                    textBox2.Text = Encoding.UTF8.GetString(ASCIIToString(bytes, bytes.Length), 0, bytes.Length).Replace("\n", ".").Replace("\t", ".").Replace(",", ".").Replace("\\", ".").Replace(" ", ".");
                }else
                {
                    textBox2.Text = Encoding.Default.GetString(ASCIIToString(bytes, bytes.Length), 0, bytes.Length).Replace("\n", ".").Replace("\t", ".").Replace(",", ".").Replace("\\", ".").Replace(" ", ".");

                }
            }
            catch { return; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
        }

        private void 清除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lstBuffer.Items.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "停止截包")
            {
                button2.Text = "开始截包";
                isjiebao = false;
            }else
            {
                button2.Text = "停止截包";
                isjiebao = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] data = strToToHexByte(richTextBox1.Text);
            if (Interface == null)
            {
                
            }else
            {
                BufferStruct gg = new BufferStruct();
                gg.Buffer = data;
                MyInvoke mi = new MyInvoke(fasong);
                //异步调用委托  
                this.BeginInvoke(mi, new Object[] { gg });
                
            }
        }
        public void fasong(BufferStruct BufferArgs)
        {
            Interface.OnSend(BufferArgs.Buffer, 0, 0);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}