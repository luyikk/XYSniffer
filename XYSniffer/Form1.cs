using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace XYSniffer
{
    public partial class Form1 : DevComponents.DotNetBar.Office2007Form
    {
        XYSocketSinffer sinffer;

        int AllCount;
        int Count;

        Encoding Encode = Encoding.UTF8;
        public Protocol BuffType { get; set; }
        public bool AllIP { get; set; }
        public List<string> Ipaddress { get; set; }
        public bool AllPort { get; set; }
        public List<int> Port { get; set; }

        public string BindIp { get; set; }

        ListViewColumnSorter Sorter = new ListViewColumnSorter();

        ConcurrentQueue<XYBuffer> queue = new ConcurrentQueue<XYBuffer>();


        IDeInterFace DeInterFace;

        System.Threading.EventWaitHandle wait = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);

        public Form1()
        {
            InitializeComponent();

          

            BuffType = Protocol.ALL;
            AllIP = true;
            AllPort = true;
            this.listViewEx1.ListViewItemSorter = Sorter;

            System.Threading.ThreadPool.RegisterWaitForSingleObject(wait, new System.Threading.WaitOrTimerCallback(ShowItem), null, 20, true);
        }

        bool isStop = false;


        void ShowItem(object o, bool b)
        {
            try
            {

                if (!isStop)
                {
                    XYBuffer buff;

                    if (queue.TryDequeue(out buff))
                    {

                        this.listViewEx1.BeginInvoke(new EventHandler((a, c) =>
                            {
                                ListViewItem item = new ListViewItem((buff.Id).ToString());


                                item.Tag = buff;
                                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, buff.Type.ToString()));
                                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, buff.SourceIP.ToString()));
                                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, buff.SourcePort.ToString()));
                                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, buff.DestIP.ToString()));
                                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, buff.DestPort.ToString()));
                                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, buff.Data.Length.ToString()));
                                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, buff.HexString));
                                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, buff.DeData == null ? "无效" : buff.DeHexString));
                                this.listViewEx1.Items.Add(item);

                            }));

                    }

                    this.label1.BeginInvoke(new EventHandler((a, c) =>
                    {
                        this.label1.Text = "共读取" + AllCount + " 筛选" + Count;
                    }));
                }
                else
                {
                    XYBuffer buff;
                    while (queue.TryDequeue(out buff))
                    {

                    }

                }
               

            }
            finally
            {
                System.Threading.ThreadPool.RegisterWaitForSingleObject(wait, new System.Threading.WaitOrTimerCallback(ShowItem), null, 20, true);
            }

        }



      

        private void Form1_Load(object sender, EventArgs e)
        {
            string hostName = Dns.GetHostName();
            var hostAddreses = Dns.GetHostAddresses(hostName);
            this.comboBoxEx1.Items.AddRange(hostAddreses);

            foreach (var p in hostAddreses)
            {
                if (p.IsIPv6LinkLocal == false && p.IsIPv6Multicast == false && p.IsIPv6SiteLocal == false && p.IsIPv6Teredo == false)
                {
                    this.comboBoxEx1.SelectedItem = p;
                    break;
                }

            }
        }

       

        private void buttonX4_Click(object sender, EventArgs e)
        {
           
            if (this.comboBoxEx1.SelectedItem != null)
            {
                sinffer = new XYSocketSinffer(this.comboBoxEx1.SelectedItem.ToString());
                BindIp = this.comboBoxEx1.SelectedItem.ToString();
                sinffer.XYBufferIn += new XYBufferInHander(sinffer_XYBufferIn);
                sinffer.Start();
                isStop = false;
                buttonX4.Enabled = false;
                buttonX5.Enabled = true;
            }
            else
            {
                MessageBox.Show("请选择IP地址");
            }
        }

        private void buttonX5_Click(object sender, EventArgs e)
        {
            sinffer.Stop();
            sinffer.XYBufferIn -= new XYBufferInHander(sinffer_XYBufferIn);
            sinffer = null;

            isStop = true;
            buttonX4.Enabled = true;
            buttonX5.Enabled = false;
        }

        private void buttonX6_Click(object sender, EventArgs e)
        {
            Count = 0;
            AllCount = 0;
            this.listViewEx1.Items.Clear();

            XYBuffer buff;
            while (queue.TryDequeue(out buff))
            {

            }
        }



        void sinffer_XYBufferIn(XYBuffer data)
        {
            if (!isStop)
            {
                AllCount = Interlocked.Increment(ref AllCount);

                switch (BuffType)
                {

                    case Protocol.TCP:
                        {
                            if (data.Type != Protocol.TCP)
                                return;
                        }
                        break;
                    case Protocol.UDP:
                        {
                            if (data.Type != Protocol.UDP)
                                return;
                        }
                        break;
                }

                if (!AllIP)
                {
                    //if (!data.SourceIP.Equals(Ipaddress, StringComparison.Ordinal) && !data.DestIP.Equals(Ipaddress, StringComparison.Ordinal))
                    //{
                    //    return;
                    //}

                    if(Ipaddress.Find(p=>p.Equals(data.SourceIP,StringComparison.Ordinal)||p.Equals(data.DestIP,StringComparison.Ordinal))==null)
                        return;

                }

                if (!AllPort)
                {
                    //if (data.DestPort != Port && data.SourcePort != Port)
                    //    return;

                    bool IsReturn = true;

                    foreach (int pt in Port)
                    {
                        if (data.DestPort == pt || data.SourcePort == pt)
                        {
                            IsReturn = false;
                            break;
                        }

                    }


                    if (IsReturn)
                        return;
                }


                data.Id = Count;
                Count = Interlocked.Increment(ref Count);

                data.HexString = GetHexString(data.Data).ToString().Replace("\r\n", "").Replace('\r', ' ').Replace('\n', ' ');


                data=DeCode(data);


                InsertList(data);
            }

        }

        public XYBuffer DeCode(XYBuffer data)
        {
            if (DeInterFace != null && data.Data.Length != 0 && data.DeData==null)
            {
                string msg;
                List<byte[]> datalist = new List<byte[]>();

                int type = 2;
                if (data.DestIP != BindIp)
                    type = 1;

                if (DeInterFace.DataDe(data.Data, type, out datalist, out msg))
                {
                    List<byte> tmpdata = new List<byte>();

                    foreach (var b in datalist)
                    {
                        foreach (var c in b)
                        {
                            tmpdata.Add(c);

                        }

                    }

                    data.DeData = tmpdata.ToArray();

                    data.DeHexString = GetHexString(data.DeData).ToString().Replace("\r\n", "").Replace('\r', ' ').Replace('\n', ' ');

                }
                else
                {
                    data.DeData = null;
                    this.richTextBox5.BeginInvoke(new EventHandler((a, b) =>
                    {
                        this.richTextBox5.AppendText((Count - 1) + "--> Error:" + msg + "\r\n");

                    }));
                }
            }

            return data;
        }

        void InsertList(XYBuffer buff)
        {
            queue.Enqueue(buff);
           
        }

        private void listViewEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewEx1.SelectedItems.Count > 0)
            {
                ListViewItem item = listViewEx1.SelectedItems[0];

                XYBuffer buff = item.Tag as XYBuffer;

                if (buff != null)
                {
                    this.richTextBox1.Text = ByteArrayToHexString(buff.Data);
                    this.richTextBox2.Text = GetHexString(buff.Data).ToString();

                    if (buff.DeData != null)
                    {
                        this.richTextBox3.Text = ByteArrayToHexString(buff.DeData);
                        this.richTextBox4.Text = GetHexString(buff.DeData);

                    }

                }

            }
        }

        private void buttonX3_Click(object sender, EventArgs e)
        {
            SETWin win = new SETWin(this.BuffType, AllIP, Ipaddress, AllPort, Port,Encode);
            win.ShowDialog();

            if (win.IsOk)
            {
                AllIP = win.AllIP;
                Ipaddress = win.Ipaddress;
                BuffType = win.BuffType;
                Port = win.Port;
                AllPort = win.AllPort;
                Encode = win.Encode;
            }

        }

        public static string ByteArrayToHexString(byte[] Bytes)
        {
            StringBuilder Result = new StringBuilder();
            string HexAlphabet = "0123456789ABCDEF";
            foreach (byte B in Bytes)
            {               
                Result.Append(HexAlphabet[(int)(B >> 4)]);
                Result.Append(HexAlphabet[(int)(B & 0xF)]);
                Result.Append(" ");
            }
            return Result.ToString();

        }

        public string GetHexString(byte[] Data)
        {
            StringBuilder sb = new StringBuilder();

            List<byte> utf = new List<byte>();

            for (int i = 0; i < Data.Length; i++)
            {

                if (Data[i] > 127)
                {
                    utf.Add(Data[i]);


                    if (i >= Data.Length - 1)
                    {
                        if (utf.Count > 0)
                        {
                            sb.Append(Encode.GetString(utf.ToArray()));
                            utf.Clear();
                        }
                        break;
                    }
                }
                else
                {
                    if (utf.Count > 0)
                    {
                        sb.Append(Encode.GetString(utf.ToArray()));
                        utf.Clear();
                    }


                    char c = (char)Data[i];
                    if (!char.IsControl(c) || c == '\r' || c == '\n')
                    {
                        sb.Append(c);
                    }
                    else if (c == '\0')
                    {
                        sb.Append('.');
                    }
                    else
                    {
                        sb.Append(' ');
                    }

                }
            }

            return sb.ToString();

            //StringBuilder sb = new StringBuilder();

            //for (int i = 0; i < Data.Length; i++)
            //{
            //    if (Data[i] > 127 && Data.Length - i > 2)
            //    {

            //        List<byte> utf = new List<byte>();
            //        utf.Add(Data[i]);
            //        i++;
            //        utf.Add(Data[i]);
            //        i++;

            //    Re:
            //        if (Data[i] > 127 && Data.Length - i > 2)
            //        {
            //            utf.Add(Data[i]);
            //            i++;
            //            goto Re;
            //        }
                    

                   

            //        string ut = Encode.GetString(utf.ToArray());

            //        sb.Append(ut);
            //    }
            //    else
            //    {
            //        char c = (char)Data[i];
            //        if (!char.IsControl(c) || c == '\r' || c == '\n')
            //        {
            //            sb.Append(c);
            //        }
            //        else if (c == '\0')
            //        {
            //            sb.Append('.');
            //        }
            //        else
            //        {
            //            sb.Append(' ');
            //        }
            //    }




            //}

            //return sb.ToString();

        }

        private void listViewEx1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == 0)
            {
                Sorter.Type = SortType.Number;

            }
            else
                Sorter.Type = SortType.String;

            Sorter.SortColumn = e.Column;
            
            if (Sorter.Order == SortOrder.Ascending)
            {
                Sorter.Order = SortOrder.Descending;
            }
            else
            {
                Sorter.Order = SortOrder.Ascending;
            }

            this.listViewEx1.Sort();
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                List<XYBuffer> item = new List<XYBuffer>();

                foreach (ListViewItem p in listViewEx1.Items)
                {
                    XYBuffer c = p.Tag as XYBuffer;

                    if (c != null)
                    {
                        c.HexString = null;
                        c.DeHexString = null;
                        item.Add(c);
                    }

                }

                File.WriteAllBytes(saveFileDialog1.FileName, Compress(SerializeObject(item)));

            }

        }


        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                byte[] data = Decompress(File.ReadAllBytes(openFileDialog1.FileName));

                List<XYBuffer> item = DeserializeObject<List<XYBuffer>>(data);

                foreach (var p in item)
                {
                    if(p.Data!=null)
                        p.HexString =  GetHexString(p.Data).ToString().Replace("\r\n", "").Replace('\r', ' ').Replace('\n', ' ');
                    if(p.DeData!=null)
                         p.DeHexString = GetHexString(p.DeData).ToString().Replace("\r\n", "").Replace('\r', ' ').Replace('\n', ' ');


                    XYBuffer x = DeCode(p);
                    InsertList(x);
                }

            }
        }



        /// <summary>
        /// 把字节反序列化成相应的对象
        /// </summary>
        /// <param name="pBytes">字节流</param>
        /// <returns>object</returns>
        public static T DeserializeObject<T>(byte[] pBytes)
        {

            string xml = Encoding.UTF8.GetString(pBytes);
            Object result = new object();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (Stream stream = new MemoryStream(Encoding.Unicode.GetBytes(xml)))
            {
                XmlReader xmlReader = XmlReader.Create(stream);

                result = (T)xmlSerializer.Deserialize(xmlReader);

                xmlReader.Close();

            }
            return (T)result;

        }


        /// <summary>
        /// 把对象序列化并返回相应的字节
        /// </summary>
        /// <param name="pObj">需要序列化的对象</param>
        /// <returns>byte[]</returns>
        public static byte[] SerializeObject(object pObj)
        {

            StringBuilder sBuilder = new StringBuilder();

            XmlSerializer xmlSerializer = new XmlSerializer(pObj.GetType());
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Encoding = Encoding.Unicode;
            XmlWriter xmlWriter = XmlWriter.Create(sBuilder, xmlWriterSettings);
            xmlSerializer.Serialize(xmlWriter, pObj);
            xmlWriter.Close();

            return Encoding.UTF8.GetBytes(sBuilder.ToString());
        }


        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] bytes)
        {
            byte[] buffer2;
            using (MemoryStream stream = new MemoryStream(bytes, false))
            {
                using (DeflateStream stream2 = new DeflateStream(stream, CompressionMode.Decompress, false))
                {
                    using (MemoryStream stream3 = new MemoryStream())
                    {
                        int num;
                        byte[] buffer = new byte[bytes.Length];
                        while ((num = stream2.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            stream3.Write(buffer, 0, num);
                        }
                        stream3.Close();
                        buffer2 = stream3.ToArray();
                    }
                }
            }
            return buffer2;
        }

        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (DeflateStream stream2 = new DeflateStream(stream, CompressionMode.Compress, false))
                {
                    stream2.Write(bytes, 0, bytes.Length);
                }
                stream.Close();
                return stream.ToArray();
            }
        }

        private void buttonX7_Click(object sender, EventArgs x)
        {
            try
            {
                openFileDialog2.InitialDirectory = Application.StartupPath;

                if (openFileDialog2.ShowDialog() == DialogResult.OK)
                {
                    string dllfile = openFileDialog2.FileName;

                    Assembly e = Assembly.LoadFile(dllfile);

                    Type[] type = e.GetTypes();

                    foreach (Type ty in type)
                    {
                        Type p = ty.GetInterface("IDeInterFace");

                        if (p != null)
                        {
                            object obj = Activator.CreateInstance(ty);

                            DeInterFace = obj as IDeInterFace;

                            if (DeInterFace != null)
                            {
                                this.richTextBox5.AppendText("成功更换了数据包解密接口:" + dllfile);
                            } 


                            return;
                        }

                    }

                    MessageBox.Show("此DLL未找到接口类,请确认此DLL中有类继承了IDeInterFace接口");

                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString());

            }
        }

        private void buttonX8_Click(object sender, EventArgs e)
        {
            this.richTextBox5.Text = "";
        }

       
    }


    class ListViewColumnSorter : IComparer
    {
        private int ColumnToSort;// 指定按照哪个列排序      
        private SortOrder OrderOfSort;// 指定排序的方式               
        private CaseInsensitiveComparer ObjectCompare;// 声明CaseInsensitiveComparer类对象，
        public ListViewColumnSorter()// 构造函数
        {
            ColumnToSort = 0;// 默认按第一列排序            
            OrderOfSort = SortOrder.Ascending;// 排序方式为不排序            
            ObjectCompare = new CaseInsensitiveComparer();// 初始化CaseInsensitiveComparer类对象
            Type = SortType.String;
        }
        // 重写IComparer接口.        
        // <returns>比较的结果.如果相等返回0，如果x大于y返回1，如果x小于y返回-1</returns>
        public int Compare(object x, object y)
        {
            int compareResult;
            ListViewItem listviewX, listviewY;
            // 将比较对象转换为ListViewItem对象
            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;
            // 比较

            if (Type == SortType.String)
            {
                compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
                // 根据上面的比较结果返回正确的比较结果
                if (OrderOfSort == SortOrder.Ascending)
                {   // 因为是正序排序，所以直接返回结果
                    return compareResult;
                }
                else if (OrderOfSort == SortOrder.Descending)
                {  // 如果是反序排序，所以要取负值再返回
                    return (-compareResult);
                }
                else
                {
                    // 如果相等返回0
                    return 0;
                }
            }
            else if (Type == SortType.Number)
            {
                int A = int.Parse(listviewX.SubItems[ColumnToSort].Text);
                int B = int.Parse(listviewY.SubItems[ColumnToSort].Text);

                int res = 0;

                if (A > B)
                    res = 1;
                else if (B > A)
                    res = -1;
                if (OrderOfSort == SortOrder.Ascending)
                {   // 因为是正序排序，所以直接返回结果
                    return res;
                }
                else if (OrderOfSort == SortOrder.Descending)
                {  // 如果是反序排序，所以要取负值再返回
                    return (-res);
                }
                else
                    return 0;

            }
            else if (Type == SortType.DateTime)
            {
                DateTime A = DateTime.Parse(listviewX.SubItems[ColumnToSort].Text);
                DateTime B = DateTime.Parse(listviewX.SubItems[ColumnToSort].Text);

                int res = 0;

                if (A > B)
                    res = 1;
                else if (B > A)
                    res = -1;
                if (OrderOfSort == SortOrder.Ascending)
                {   // 因为是正序排序，所以直接返回结果
                    return res;
                }
                else if (OrderOfSort == SortOrder.Descending)
                {  // 如果是反序排序，所以要取负值再返回
                    return (-res);
                }
                else
                    return 0;

            }
            else
            {
                return 0;
            }
        }
        /// 获取或设置按照哪一列排序.        
        public int SortColumn
        {
            set
            {
                ColumnToSort = value;
            }
            get
            {
                return ColumnToSort;
            }
        }
        /// 获取或设置排序方式.    
        public SortOrder Order
        {
            set
            {
                OrderOfSort = value;
            }
            get
            {
                return OrderOfSort;
            }
        }


        public SortType Type { get; set; }
        
    }

    public enum SortType
    {
        Number=0,
        DateTime=1,
        String=2
    }

}
