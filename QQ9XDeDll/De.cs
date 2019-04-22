using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Concurrent;

namespace QQ9XDeDll
{
    public class De : XYSniffer.IDeInterFace
    {
        string key = "CYANOGEN";

        ConcurrentQueue<List<byte>> queuelist = new ConcurrentQueue<List<byte>>();

        public bool DataDe(byte[] Indata, int type, out List<byte[]> OutData, out string msg)
        {
            OutData = new List<byte[]>(); ;
            msg = "";

            try
            {

                if (Indata.Length < 8)
                {
                    msg = "数据包长度不够";
                    return false;
                }


                List<byte> list = Indata.ToList();

                if (queuelist.Count > 0)
                {
                    List<byte> data;
                    if (queuelist.TryDequeue(out data))
                    {
                        list.InsertRange(0,data);
                    }

                }

            Re:

                List<byte> lengt = list.GetRange(0, 8);

                list.RemoveRange(0, 8);

                string lengtstr = Encoding.Default.GetString(lengt.ToArray());

                int lengtarg = int.Parse(lengtstr);

                if (list.Count >= lengtarg)
                {

                    List<byte> Slist = list.GetRange(0, lengtarg);

                    list.RemoveRange(0, lengtarg);

                    List<byte> ldata = new List<byte>();

                    int i = 0;

                    while (i < Slist.Count)
                    {
                        ldata.Add((byte)(Slist[i] ^ key[i % key.Length]));
                        i++;
                    }


                    byte[] debyte = Decompress(ldata.ToArray());


                    OutData.Add(debyte);


                    if (list.Count > 8)
                        goto Re;

                }
                else
                {
                    list.InsertRange(0, lengt);
                    queuelist.Enqueue(list);
                }

                return true;

            }
            catch (Exception er)
            {
                msg = er.Message;
                return false;
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
