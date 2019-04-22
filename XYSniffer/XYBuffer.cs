using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XYSniffer
{

    public class XYBuffer
    {

      


        public int Id { get; set; }
        public Protocol Type { get; set; }
        public string SourceIP { get; set; }
        public int SourcePort { get; set; }
        public string DestIP { get; set; }
        public int DestPort { get; set; }
        public byte[] Data { get; set; }

        public string HexString { get; set; }
        public string DeHexString { get; set; }
        

        public byte[] DeData { get; set; }
       
    }
}
