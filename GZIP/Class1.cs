using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HtmlData
{
    public class Datas : XYSniffer.IDeInterFace
    {

        public bool DataDe(byte[] Indata, int type, out List<byte[]> OutData, out string msg)
        {
            switch (type)
            {
                case 1:
                    {
                        return InDataM(Indata, out OutData, out msg);
                    }
                case 2:
                    {
                        return InDataM(Indata, out OutData, out msg);
                    }

                default:
                    {
                        OutData = new List<byte[]>();
                        msg = "错误的TYPE";
                        return false;
                    }

            }

        }


        private bool InDataM(byte[] data, out List<byte[]> OutData, out string msg)
        {
            msg = "";
            OutData = new List<byte[]>();

            int postion = -1;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0x0D && data[i + 1] == 0x0A
                    && data[i + 2] == 0x0D && data[i + 3] == 0x0A)
                {

                    postion = i + 4;
                    break;
                }
            }

            if (postion != -1)
            {
                OutData.Add(data.ToList().GetRange(postion, data.Length - postion).ToArray());
                return true;
            }
            else
            {

                return false;
            }
        }

    }
}
