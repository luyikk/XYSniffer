using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.IO.Compression;
namespace XYSniffer
{
    public partial class SETWin : DevComponents.DotNetBar.Office2007Form
    {
        public SETWin(Protocol type,bool allIp,List<string> ipaddress,bool allPort,List<int> port,Encoding encode)
        {
            InitializeComponent();

            Port = new List<int>();
            Ipaddress = new List<string>();
            switch (type)
            {
                case Protocol.ALL:
                    {
                        radioButton1.Checked = true;
                    }
                    break;
                case Protocol.TCP:
                    {
                        radioButton2.Checked = true;
                    }
                    break;
                case Protocol.UDP:
                    {
                        radioButton3.Checked = true;
                    }
                    break;

            }

            if (allIp)
            {
                this.radioButton4.Checked = true;
            }
            else
            {
                this.radioButton5.Checked = true;
                this.integerInput1.Enabled = true;
                //this.ipAddressInput1.Value = ipaddress;

                foreach (string ip in ipaddress)
                    this.listViewEx1.Items.Add(new ListViewItem() { Text = ip });
            }

            if (allPort)
            {
                this.radioButton6.Checked = true;

            }
            else
            {
                this.radioButton7.Checked = true;
                this.integerInput1.Enabled = true;
              //  this.integerInput1.Value = port;

                foreach (int pt in port)
                    this.listViewEx2.Items.Add(new ListViewItem() { Text = pt.ToString(),Tag=pt });
            }

            this.comboBoxEx1.Items.Add(Encoding.ASCII);
            this.comboBoxEx1.Items.Add(Encoding.Default);
            this.comboBoxEx1.Items.Add(Encoding.UTF8);
            this.comboBoxEx1.Items.Add(Encoding.UTF7);
            this.comboBoxEx1.Items.Add(Encoding.UTF32);
            this.comboBoxEx1.Items.Add(Encoding.Unicode);
            this.comboBoxEx1.SelectedItem = encode;

            this.contextMenuBar1.SetContextMenuEx(this.listViewEx1, buttonItem1);
            this.contextMenuBar2.SetContextMenuEx(this.listViewEx2, buttonItem4);

           
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked)
            {
                this.buttonX3.Enabled = true;
                this.ipAddressInput1.Enabled = true;
            }
            else
            {
                this.ipAddressInput1.Enabled = false;
                this.buttonX3.Enabled = false;
            }
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton7.Checked)
            {
                this.buttonX4.Enabled = true;
                this.integerInput1.Enabled = true;
            }
            else
            {
                this.buttonX4.Enabled = false;
                this.integerInput1.Enabled = false;
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                this.buttonX3.Enabled = false;
                this.ipAddressInput1.Enabled = false;
            }
            else
            {
                this.buttonX3.Enabled = true;
                this.ipAddressInput1.Enabled = true;
            }
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton6.Checked)
            {
                this.buttonX4.Enabled = false;
                this.integerInput1.Enabled = false;
            }
            else
            {
                this.buttonX4.Enabled = true;
                this.integerInput1.Enabled = true;
            }
        }


        public bool IsOk { get; set; }


        public Protocol BuffType { get; set; }
        public bool AllIP { get; set; }
        public List<string> Ipaddress { get; set; }
        public bool AllPort { get; set; }
        public List<int> Port { get; set; }
        public Encoding Encode { get; set; }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                BuffType = Protocol.ALL;
            if (radioButton2.Checked)
                BuffType = Protocol.TCP;
            if (radioButton3.Checked)
                BuffType = Protocol.UDP;
                       
             AllIP = radioButton4.Checked;



             if (!radioButton4.Checked)
             {
                 //Ipaddress = this.ipAddressInput1.Value;

                 //if (string.IsNullOrEmpty(Ipaddress))
                 //{
                 //    MessageBox.Show("请输入IP地址");
                 //    return;
                 //}

                 Ipaddress.Clear();

                 foreach (ListViewItem p in listViewEx1.Items)
                 {
                     Ipaddress.Add(p.Text);
                 }


             }


             AllPort = radioButton6.Checked;

             if (!radioButton6.Checked)
             {
              //   Port = this.integerInput1.Value;

                 Port.Clear();
                 foreach (ListViewItem p in listViewEx2.Items)
                 {
                     Port.Add((int)p.Tag);
                 }
             }
             if (this.comboBoxEx1.SelectedItem != null)
             {
                 Encode = this.comboBoxEx1.SelectedItem as Encoding;
             }
             else
             {
                 Encode = Encoding.GetEncoding(this.comboBoxEx1.Text);
             }



            IsOk = true;
            this.Close();
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            IsOk = false;
            this.Close();
        }

        private void buttonX3_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.ipAddressInput1.Value))
                this.listViewEx1.Items.Add(new ListViewItem { Text = this.ipAddressInput1.Value });
        }

        private void buttonX4_Click(object sender, EventArgs e)
        {
            this.listViewEx2.Items.Add(new ListViewItem { Text = this.integerInput1.Value.ToString(), Tag = this.integerInput1.Value });
        }

        private void buttonItem2_Click(object sender, EventArgs e)
        {
            if (this.listViewEx1.SelectedItems.Count > 0)
            {
                foreach (ListViewItem p in this.listViewEx1.SelectedItems)
                    this.listViewEx1.Items.Remove(p);

            }
        }

        private void buttonItem5_Click(object sender, EventArgs e)
        {
            if (this.listViewEx2.SelectedItems.Count > 0)
            {
                foreach (ListViewItem p in this.listViewEx2.SelectedItems)
                    this.listViewEx2.Items.Remove(p);

            }
        }

        private void buttonItem3_Click(object sender, EventArgs e)
        {
            this.listViewEx1.Items.Clear();
        }

        private void buttonItem6_Click(object sender, EventArgs e)
        {
            this.listViewEx2.Items.Clear();
        }

        private void buttonX5_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (radioButton1.Checked)
                    BuffType = Protocol.ALL;
                if (radioButton2.Checked)
                    BuffType = Protocol.TCP;
                if (radioButton3.Checked)
                    BuffType = Protocol.UDP;

                AllIP = radioButton4.Checked;



                if (!radioButton4.Checked)
                {

                    Ipaddress.Clear();

                    foreach (ListViewItem p in listViewEx1.Items)
                    {
                        Ipaddress.Add(p.Text);
                    }


                }


                AllPort = radioButton6.Checked;

                if (!radioButton6.Checked)
                {

                    Port.Clear();
                    foreach (ListViewItem p in listViewEx2.Items)
                    {
                        Port.Add((int)p.Tag);
                    }
                }
                if (this.comboBoxEx1.SelectedItem != null)
                {
                    Encode = this.comboBoxEx1.SelectedItem as Encoding;
                }
                else
                {
                    Encode = Encoding.GetEncoding(this.comboBoxEx1.Text);
                }

                IsOk = true;


                Configsave tmp = new Configsave()
                {
                    BuffType = BuffType,
                    AllIP = AllIP,
                    AllPort = AllPort,                 
                    Ipaddress = Ipaddress,
                    Port = Port
                };


                File.WriteAllBytes(saveFileDialog1.FileName, SerializeObject(tmp));


            }
        }

        
        public class Configsave
        {
            public Protocol BuffType { get; set; }
            public bool AllIP { get; set; }
            public List<string> Ipaddress { get; set; }
            public bool AllPort { get; set; }
            public List<int> Port { get; set; }
           
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


        private void buttonX6_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Configsave data = DeserializeObject<Configsave>(File.ReadAllBytes(openFileDialog1.FileName));

                Port.Clear();
                Ipaddress.Clear();

                switch (data.BuffType)
                {
                    case Protocol.ALL:
                        {
                            radioButton1.Checked = true;
                        }
                        break;
                    case Protocol.TCP:
                        {
                            radioButton2.Checked = true;
                        }
                        break;
                    case Protocol.UDP:
                        {
                            radioButton3.Checked = true;
                        }
                        break;

                }


                if (data.AllIP)
                {
                    this.radioButton4.Checked = true;
                }
                else
                {
                    this.radioButton5.Checked = true;
                    this.integerInput1.Enabled = true;
                    //this.ipAddressInput1.Value = ipaddress;

                    foreach (string ip in data.Ipaddress)
                        this.listViewEx1.Items.Add(new ListViewItem() { Text = ip });
                }

                if (data.AllPort)
                {
                    this.radioButton6.Checked = true;

                }
                else
                {
                    this.radioButton7.Checked = true;
                    this.integerInput1.Enabled = true;
                    //  this.integerInput1.Value = port;

                    foreach (int pt in data.Port)
                        this.listViewEx2.Items.Add(new ListViewItem() { Text = pt.ToString(), Tag = pt });
                }


            }
        }

    }



}
