using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AMFDataOut
{
    public class AMFOut : XYSniffer.IDeInterFace
    {
        object lockIns = new object();
        object lockOuts = new object();

        public List<byte> DataInBuff = new List<byte>();
        public List<byte> DataOutBuff = new List<byte>();

        public bool DataDe(byte[] Indata, int type, out List<byte[]> OutData, out string msg)
        {
            switch (type)
            {
                case 1:
                    {
                        return InDataM(lockIns, DataInBuff,Indata, out OutData, out msg);
                    }
                case 2:
                    {
                        return InDataM(lockOuts, DataOutBuff, Indata, out OutData, out msg);
                    }

                default:
                    {
                        OutData = new List<byte[]>();
                        msg = "错误的TYPE";
                        return false;
                    }

            }

        }


        private bool InDataM(object lockIn,List<byte> DataIn, byte[] data, out List<byte[]> OutData, out string msg)
        {
            OutData = new List<byte[]>();
            msg = "";

            lock (lockIn)
            {
                DataIn.AddRange(data);

                string htmlHand = Encoding.Default.GetString(DataIn.ToArray());

                string handLengt = "Content-Length: ";

                int p = 0;

                if ((p = htmlHand.IndexOf(handLengt)) >= 0)
                {
                    p += handLengt.Length;

                    int t = htmlHand.IndexOf('\r', p);

                    string lengt = htmlHand.Substring(p, t - p);

                    int numlengt;

                    if (int.TryParse(lengt, out numlengt))
                    {                        

                        string endHand = "\r\n\r\n";

                        if ((p=htmlHand.IndexOf(endHand)) >= 0)
                        {
                            p += endHand.Length;

                            htmlHand = htmlHand.Substring(0, p);

                            int htmlHandLength = Encoding.Default.GetBytes(htmlHand).Length;

                            if (DataIn.Count >= htmlHandLength + numlengt)
                            {
                                List<byte> theData = DataIn.GetRange(0, htmlHandLength + numlengt);

                                List<byte> PostData = theData.GetRange(htmlHandLength, theData.Count - htmlHandLength);

                                OutData.Add(PostData.ToArray());

                                DataIn.RemoveRange(0, htmlHandLength + numlengt);

                                return true;

                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        DataIn.Clear();
                        return false;
                    }
                }
                else
                {
                    DataIn.Clear();

                    return false;
                }




            }
        }


      
    }
}
