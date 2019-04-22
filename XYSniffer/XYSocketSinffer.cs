using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
namespace XYSniffer
{
    public delegate void XYBufferInHander(XYBuffer data);

    public class XYSocketSinffer
    {
        public Socket sock { get; set; }

        public string ListenIP { get; set; }

        public XYSocketSinffer(string ipaddress)
        {
            this.ListenIP = ipaddress;

        }

        public event XYBufferInHander XYBufferIn;

        public void Start()
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ListenIP), 0);
            sock.Bind(endpoint);            
            byte[] outValue = BitConverter.GetBytes(0);
            byte[] inValue = BitConverter.GetBytes(1);
            sock.IOControl(IOControlCode.ReceiveAll, inValue, outValue);

            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            byte[] data = new byte[40980];
            e.SetBuffer(data, 0, data.Length);  //设置数据包
            e.Completed += new EventHandler<SocketAsyncEventArgs>(e_Completed);
            if (!sock.ReceiveAsync(e))
            {
                eCompleted(e);
            }
        }

        public void Stop()
        {
            if (sock != null)
                sock.Close();
        }


        void e_Completed(object sender, SocketAsyncEventArgs e)
        {
            eCompleted(e);
        }

        void eCompleted(SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                    {

                        
                        IPBufferSwitch(e.Buffer, e.BytesTransferred);
                           

                        byte[] dataLast = new byte[40980];
                        e.SetBuffer(dataLast, 0, dataLast.Length);

                        if (!sock.ReceiveAsync(e))
                            eCompleted(e);

                    }                   
                    break;

            }

        }

        void IPBufferSwitch(byte[] data, int lengt)
        {
            IPHeader iphander = new IPHeader(data, lengt);

            switch (iphander.ProtocolType)
            {
                case Protocol.TCP:
                    {
                        TCPHeader tcphander = new TCPHeader(iphander.Data,iphander.Data.Length);
                       
                        if (iphander.Data.Length >= tcphander.HeaderLength)
                        {

                            int lengtz = iphander.Data.Length - tcphander.HeaderLength;
                            byte[] datax;
                            if (lengtz > 0)
                            {
                                datax = new byte[iphander.Data.Length - tcphander.HeaderLength];

                                Array.Copy(iphander.Data, tcphander.HeaderLength, datax, 0, datax.Length);
                            }
                            else
                            {
                                datax = new byte[0];
                            }

                            XYBuffer buff = new XYBuffer()
                            {
                                Data = datax,
                                DestIP=iphander.DestinationAddress.ToString(),
                                DestPort=tcphander.DestinationPort,
                                SourceIP=iphander.SourceAddress.ToString(),
                                SourcePort=tcphander.SourcePort,
                                Type=Protocol.TCP
                            };
                            Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    if (XYBufferIn != null)
                                        XYBufferIn(buff);
                                }
                                catch
                                {

                                }
                            });
                        }
                    }
                    break;
                case Protocol.UDP:
                    {
                        UDPHeader udphander = new UDPHeader(iphander.Data, iphander.Data.Length);
                        if (iphander.Data.Length >= udphander.HanderLength)
                        {
                            int lengtz=iphander.Data.Length - udphander.HanderLength;
                            byte[] datax;
                            if (lengtz > 0)
                            {
                                datax = new byte[lengtz];
                                Array.Copy(iphander.Data, udphander.HanderLength, datax, 0, datax.Length);
                            }
                            else
                            {
                                datax = new byte[0];
                            }
                            XYBuffer buff = new XYBuffer()
                            {
                                Data = datax,
                                DestIP = iphander.DestinationAddress.ToString(),
                                DestPort = udphander.DestinationPort,
                                SourceIP = iphander.SourceAddress.ToString(),
                                SourcePort = udphander.SourcePort,
                                Type = Protocol.UDP
                            };
                            Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    if (XYBufferIn != null)
                                        XYBufferIn(buff);
                                }
                                catch
                                {

                                }
                            });

                        }
                    }
                    break;
                case Protocol.Unknown:

                    break;
            }

        }


        const string HexValues = "0123456789ABCDEF";
        private static string GetByteArrayHexString(byte[] buf, int startIndex, int size)
        {
            StringBuilder sb = new StringBuilder(size * 5);
            sb.AppendFormat("{0,3:X}: ", 0);
            int j = 1;
            for (int i = startIndex, n = startIndex + size; i < n; i++, j++)
            {
                byte b = buf[i];
                char c = HexValues[(b & 0x0f0) >> 4];
                sb.Append(c);
                c = HexValues[(b & 0x0f)];
                sb.Append(c);
                sb.Append(' ');
                if ((j & 0x0f) == 0)
                {
                    sb.Append(' ');
                    //sb.Append(Encoding.ASCII.GetString(buf,i-15,8));
                    AppendPrintableBytes(sb, buf, i - 15, 8);
                    sb.Append(' ');
                    //sb.Append(Encoding.ASCII.GetString(buf, i - 7, 8));
                    AppendPrintableBytes(sb, buf, i - 7, 8);
                    if (i + 1 != n)
                    {
                        sb.Append('\n');
                        sb.AppendFormat("{0,3:X}: ", i - 1);    //偏移
                    }
                }
                else if ((j & 0x07) == 0)
                {
                    sb.Append(' ');
                }
            }
            int t;
            if ((t = ((j - 1) & 0x0f)) != 0)
            {
                for (int k = 0, kn = 16 - t; k < kn; k++)
                {
                    sb.Append("   ");
                }
                if (t <= 8)
                {
                    sb.Append(' ');
                }

                sb.Append(' ');
                //   sb.Append(Encoding.ASCII.GetString(buf, startIndex + size - t, t>8?8:t));
                AppendPrintableBytes(sb, buf, startIndex + size - t, t > 8 ? 8 : t);
                if (t > 8)
                {
                    sb.Append(' ');
                    //   sb.Append(Encoding.ASCII.GetString(buf, startIndex + size - t + 8, t - 8));
                    AppendPrintableBytes(sb, buf, startIndex + size - t + 8, t - 8);
                }
            }
            return sb.ToString();
        }

        //向sb中添加buf中可打印字符，不可打印字符用'.'代替
        private static void AppendPrintableBytes(StringBuilder sb, byte[] buf, int startIndex, int len)
        {
            for (int i = startIndex, n = startIndex + len; i < n; i++)
            {
                char c = (char)buf[i];
                if (!char.IsControl(c))
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append('.');
                }
            }
        }
    }
}
