using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace testInterface
{
    public class De : XYSniffer.IDeInterFace
    {
        public bool DataDe(byte[] Indata, int type, out List<byte[]> OutData, out string msg)
        {
            OutData = new List<byte[]>();
            msg = "";

            OutData.Add(Indata);

            return true;
        }
    }
}
