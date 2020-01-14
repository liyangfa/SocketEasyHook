using System;
using System.Collections.Generic;
using System.Text;

namespace SocketEasyHook
{
    public struct BufferStruct
    {
        /// <summary>
        /// Socketָ��
        /// </summary>
        public IntPtr sockHander;
        /// <summary>
        /// �������
        /// </summary>
        public byte[] Buffer;
        /// <summary>
        /// �����С
        /// </summary>
        public int BufferSize;
        /// <summary>
        /// �����̬����
        /// </summary>
        public int[] LoginIdent;
        /// <summary>
        /// C[0]/S[1] 
        /// </summary>
        public int ObjectType;
    }
    public class SocketInterFace : MarshalByRefObject
    {
        public delegate void LogArgsHander(BufferStruct ArgsBuffer);
        public static event LogArgsHander LogEvent;
        [Serializable]
        public delegate String SocketHander(byte[] ArgsBuffer);
        public event SocketHander SocketEvent1;


        public void IsInstalled(Int32 InClientPID) { Console.WriteLine("��ȡ��ǰ��Ϸ�߳�PID:{0}.\r\n", InClientPID); }
        /// <summary>
        /// ����Recv��
        /// </summary>
        /// <param name="RecvBuffer"></param>
        /// <param name="LoginIndex"></param>
        /// <param name="LoginIndexEx"></param>
        public void OnRecv(byte[] RecvBuffer, int LoginIndex, int LoginIndexEx)
        {
            BufferStruct BufferArgs = new BufferStruct();
            BufferArgs.ObjectType = 1;
            BufferArgs.Buffer = RecvBuffer;
            BufferArgs.BufferSize = RecvBuffer.Length;
            BufferArgs.LoginIdent = new int[] { LoginIndex, LoginIndexEx };
            Onsendto(new byte[2]);
            OnLog(BufferArgs);
        }
       
        public void OnSend(byte[] RecvBuffer, int LoginIndex, int LoginIndexEx)
        {
            BufferStruct BufferArgs = new BufferStruct();
            BufferArgs.ObjectType = 0;
            BufferArgs.Buffer = RecvBuffer;
            BufferArgs.BufferSize = RecvBuffer.Length;
            BufferArgs.LoginIdent = new int[] { LoginIndex, LoginIndexEx };
            Onsendto(new byte[2]);
            OnLog(BufferArgs);
        }
        public void OnLog(BufferStruct BufferArgs) { 
            if (LogEvent != null) LogEvent(BufferArgs);
        }
        public void Onsendto(byte[] args)
        {
            if (SocketEvent1 != null)
            {
                string gg=SocketEvent1(args);
                OnLog(gg);
            }
        }
        public void OnLog(string BufferArgs) { Console.WriteLine(BufferArgs); }
        public void ReportException(Exception InInfo) { Console.WriteLine("Զ��ִ�г���:\r\n" + InInfo.ToString()); }
    }
}
