using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XYSniffer
{
    public interface IDeInterFace
    {
        bool DataDe(byte[] Indata,int type, out List<byte[]> OutData, out string msg);
    }
}
