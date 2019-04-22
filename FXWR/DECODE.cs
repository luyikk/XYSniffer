using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Concurrent;

namespace FXWR
{
    public class DECODE:XYSniffer.IDeInterFace
    {
        object lock1 = new object();
        object lock2 = new object();

        public List<byte> Data1 = new List<byte>();
        public List<byte> Data2 = new List<byte>();

        public bool DataDe(byte[] Indata,int type, out List<byte[]> OutData, out string msg)
        {
            switch (type)
            {
                case 1:
                    {
                        return Type1(Indata, out OutData, out msg);
                    }                   
                case 2:
                    {
                       return  Type2(Indata, out OutData, out msg);
                    }
                   
                default:
                    {
                        OutData = new List<byte[]>();
                        msg = "错误的TYPE";
                        return false;
                    }

            }
         
        }

        private bool Type1(byte[] Indata, out List<byte[]> OutData, out string msg)
        {
            lock (lock1)
            {
                OutData = new List<byte[]>();
                msg = "";

                try
                {

                    Data1.InsertRange(Data1.Count,Indata);

                    byte[] xData = Data1.ToArray();
                    string html = Encoding.UTF8.GetString(xData);

                    string lengt = "Content-Length:";
                    int p = 0;
                    if ((p = html.IndexOf(lengt)) > 0)
                    {
                        p += lengt.Length;
                        int t = html.IndexOf("\r\n", p);

                        string Strlengt = html.Substring(p, t - p).Trim();

                    
                        if (int.TryParse(Strlengt, out Tlength1))
                        {

                            string htmlHand = html.Substring(0, html.IndexOf("\r\n\r\n"));

                            byte[] HandByte = Encoding.UTF8.GetBytes(htmlHand);

                            List<byte> buff = Data1;

                            if ((buff.Count - HandByte.Length) < Tlength1)
                            {
                                return true;
                            }
                            
                            List<byte> subbuff = buff.GetRange(buff.Count - Tlength1, buff.Count - (buff.Count - Tlength1));                          
                            string check = Encoding.UTF8.GetString(subbuff.ToArray());
                            subbuff.RemoveRange(0, 3);
                            OutData.Add(Decompress(subbuff.ToArray()));
                            Data1.Clear();

                            return true;

                        }
                        else
                        {
                            msg = "无法转换长度到INT";
                            return false;
                        }
                    }
                    else
                    {
                        if (Data1.Count > Tlength1)
                        {
                            string htmlHand = Encoding.UTF8.GetString(Data1.ToArray());

                            htmlHand = htmlHand.Substring(0, html.IndexOf("\r\n\r\n"));

                            byte[] HandByte = Encoding.UTF8.GetBytes(htmlHand);

                            List<byte> buff = Data1;

                            if ((buff.Count - HandByte.Length) < Tlength1)
                            {
                                return true;
                            }

                            List<byte> subbuff = buff.GetRange(buff.Count - Tlength1, buff.Count - (buff.Count - Tlength1));                          
                            string check = Encoding.UTF8.GetString(subbuff.ToArray());
                            subbuff.RemoveRange(0, 3);
                            OutData.Add(Decompress(subbuff.ToArray()));
                            Data1.Clear();

                            return true;



                        }

                        return true;
                    }
                }
                catch (Exception er)
                {
                    msg = er.Message;
                    Data1.Clear();
                    return false;
                }
            }

        }
        private static int Tlength1;
        private static int Tlength2;

        private bool Type2(byte[] Indata, out List<byte[]> OutData, out string msg)
        {
            lock (lock2)
            {
                OutData = new List<byte[]>();
                msg = "";

                try
                {

                    Data2.InsertRange(Data2.Count, Indata);

                    byte[] xData = Data2.ToArray();
                    string html = Encoding.UTF8.GetString(xData);

                    string lengt = "Content-Length:";
                    int p = 0;
                    if ((p = html.IndexOf(lengt)) > 0)
                    {
                        p += lengt.Length;
                        int t = html.IndexOf("\r\n", p);

                        string Strlengt = html.Substring(p, t - p).Trim();


                        if (int.TryParse(Strlengt, out Tlength2))
                        {

                            string htmlHand = html.Substring(0, html.IndexOf("\r\n\r\n"));

                            byte[] HandByte = Encoding.UTF8.GetBytes(htmlHand);

                            List<byte> buff = Data2;

                            if ((buff.Count - HandByte.Length) < Tlength2)
                            {
                                return true;
                            }

                            List<byte> subbuff = buff.GetRange(buff.Count - Tlength2, buff.Count - (buff.Count - Tlength2));                           
                            string check = Encoding.UTF8.GetString(subbuff.ToArray());                         
                            OutData.Add(Decompress(subbuff.ToArray()));
                            Data2.Clear();

                            return true;

                        }
                        else
                        {
                            msg = "无法转换长度到INT";
                            return false;
                        }
                    }
                    else
                    {
                        if (Data2.Count > Tlength2)
                        {
                            string htmlHand = Encoding.UTF8.GetString(Data2.ToArray());

                            htmlHand = htmlHand.Substring(0, html.IndexOf("\r\n\r\n"));

                            byte[] HandByte = Encoding.UTF8.GetBytes(htmlHand);

                            List<byte> buff = Data2;

                            if ((buff.Count - HandByte.Length) < Tlength2)
                            {
                                return true;
                            }

                            List<byte> subbuff = buff.GetRange(buff.Count - Tlength2, buff.Count - (buff.Count - Tlength2));                          
                            string check = Encoding.UTF8.GetString(subbuff.ToArray());
                            OutData.Add(Decompress(subbuff.ToArray()));
                            Data2.Clear();
                            return true;
                        }

                        return true;
                    }
                }
                catch (Exception er)
                {
                    msg = er.Message;
                    Data2.Clear();
                    return false;
                }

            }


        }


        public static byte[] Decompress(byte[] bytes)
        {
            MemoryStream stream1 = new MemoryStream(bytes);

            MemoryStream stream2 = new MemoryStream();

            zlib.ZOutputStream outZStream = new zlib.ZOutputStream(stream2);

            try
            {
                CopyStream(stream1, outZStream);

            }
            finally
            {
                outZStream.Close();
                stream1.Close();
                stream2.Close();

            }

            return stream2.ToArray();
        }
        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);

            }

            output.Flush();
        }

    }
}
