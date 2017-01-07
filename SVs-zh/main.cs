﻿using OpcCommander;
using SVs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SVs
{
    public enum MessageStyl { info, succes, error } ;
    public partial class main : Form
    {
        private List<ItemDetails> itemDetailsArray = new List<ItemDetails>(); //cannot be duplicate items (ID)
        private List<string> root = null;
        private List<User> sessions = new List<User>();
        private MyServer server;
        private MyBrowser browser;
        private bool connect = false;
        private bool suppressOutput = false;
        public void SuppressOutput() { suppressOutput = true; }
        public void EnableOutput() { suppressOutput = false; }

        DataTable dt = new DataTable();

        private void main_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "连接状态：" + connect;
            toolStripStatusLabel2.Text = "Timer:" + timer1.Enabled.ToString();

            dt.Columns.Add("item", System.Type.GetType("System.String"));
            dt.Columns.Add("type", System.Type.GetType("System.String"));
            dt.Columns.Add("value", System.Type.GetType("System.String"));            
            dt.Columns.Add("time", System.Type.GetType("System.String"));
            dataGridView1.DataSource = dt;
        }
        private void AddItem(string item,string type,string value)
        {               
            DataRow dr = dt.NewRow();             
            dr["item"] = item;             
            dr["type"] = type;              
            dr["value"] = value;
            dr["time"] = DateTime.Now.ToString("HH:mm:ss:fff");
            dt.Rows.Add(dr);
        }
        public main()
        {
            InitializeComponent();
        }


        private void main_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (server != null)
                server.Stop();
        }

        /*
        public void PrintMessage(string s, MessageStyl styl)
        {
            Color color;
            if (this.suppressOutput == false)
            {
                //set styll
                if (styl == MessageStyl.error)
                    color = Color.Red;
                else if (styl == MessageStyl.succes)
                    color = Color.Green;
                else
                    color = Color.Gray;

                string hour;
                string minute;
                if ((Log.Items.Count + 1) > 50)
                    Log.Items.RemoveAt(0);
                if (DateTime.Now.Hour >= 10)
                    hour = DateTime.Now.Hour.ToString();
                else
                    hour = "0" + DateTime.Now.Hour.ToString();
                if (DateTime.Now.Minute >= 10)
                    minute = DateTime.Now.Minute.ToString();
                else
                    minute = "0" + DateTime.Now.Minute.ToString();
                string time = "[ " + hour + ":" + minute + " ]";
                ListViewItem item = new ListViewItem();
                item.SubItems.Add(time);
                item.SubItems.Add(s);
                item.SubItems[0].BackColor = color;
                item.UseItemStyleForSubItems = false;
                
                Log.Items.Add(item);
                Log.Items[Log.Items.Count - 1].EnsureVisible();
            }
        }
         */

        private void button1_Click_1(object sender, EventArgs e)
        {
            int[] itemHANDLES = new int[] { 0 };
            VarEnum[] arrayType = new VarEnum[] { VarEnum.VT_EMPTY };

            MyVariant[] returnValue = null;
            string[] itemID = new String[] { textBox2.Text.ToString() };
            server.AddItems(itemID, out itemHANDLES, out arrayType);
            textBox3.Text = MyServer.VarEnumToString(arrayType[0]);
            server.ReadItems(itemID, itemHANDLES, arrayType, out returnValue);
            if (returnValue != null)
            {
                textBox4.Text = returnValue[0].GetStringValue();
            }
        }

        private void Menu_Connect_Click(object sender, EventArgs e)
        {
            server = new MyServer(this);
            try
            {
                server.Start();
                //Menu_Connect.Enabled = false;
                //Menu_Disconnect.Enabled = true;
                connect = true;
                toolStripStatusLabel1.Text = "连接状态：" + connect;


                browser = new MyBrowser(server);
                try
                {
                    this.root = browser.GetRoot();
                    //login
                    sessions.Clear();
                    foreach (string item in root)
                    {
                        try
                        {
                            int id = browser.LogInToOpcUser(item);
                            MessageBox.Show("Log in to OPC Ok，连接成功！");
                            //PrintMessage(item + ": Log in to OPC Ok.", MessageStyl.succes);
                            sessions.Add(new User(id, item));
                        }
                        catch (Exception exc)
                        {
                           // PrintMessage(exc.Message, MessageStyl.error);
                            //PrintMessage(item + ": Log in to OPC failed.", MessageStyl.error);
                            MessageBox.Show("Log in to OPC failed", exc.Message);
                        }
                    }
                    
                }
                catch (Exception exc)
                {
                    //PrintMessage(exc.Message, MessageStyl.error);
                    //PrintMessage("Browser: parsing error.", MessageStyl.error);
                    MessageBox.Show("Browser: parsing error.", exc.Message);
                }
            }
            catch (Exception exc)
            {
                //PrintMessage("Server: starting error.", MessageStyl.error);
                //PrintMessage(exc.Message, MessageStyl.error);
                MessageBox.Show("连接远程服务器出现错误：" + exc.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //读取列表中所有items
        private void button3_Click(object sender, EventArgs e)
        {
            int[] itemHANDLES = new int[] { 0 };
            VarEnum[] arrayType = new VarEnum[] { VarEnum.VT_EMPTY };

            MyVariant[] returnValue = null;

            //string[] itemID = new String[] { listBox1.SelectedItem.ToString() };
            string[] itemID = new String[listBox1.Items.Count];
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                itemID[i] = listBox1.Items[i].ToString();
            }

            server.AddItems(itemID, out itemHANDLES, out arrayType);

            //string type = MyServer.VarEnumToString(arrayType[0]);

            server.ReadItems(itemID, itemHANDLES, arrayType, out returnValue);
            if (returnValue != null)
            {
                //string value = returnValue[0].GetStringValue();
                //AddItem(itemID[0], type, value);
                for (int i = 0; i < returnValue.Length-1; i++)
                {
                    string name = itemID[i].ToString();
                    string type = MyServer.VarEnumToString(arrayType[i]);
                    string value = returnValue[i].GetStringValue();
                    AddItem(name, type, value);
                }
                
            }
        }

        //读取单item
        private void button4_Click(object sender, EventArgs e)
        {
            int[] itemHANDLES = new int[] { 0 };
            VarEnum[] arrayType = new VarEnum[] { VarEnum.VT_EMPTY };

            MyVariant[] returnValue = null;

            
            string[] itemID = new String[] { listBox1.SelectedItem.ToString() };

            server.AddItems(itemID, out itemHANDLES, out arrayType);
            string type = MyServer.VarEnumToString(arrayType[0]);

            server.ReadItems(itemID, itemHANDLES, arrayType, out returnValue);
            if (returnValue != null)
            {
                string value = returnValue[0].GetStringValue();
                AddItem(itemID[0], type, value);
                //MessageBox.Show("type:"+type+"  value:"+value);
            }
        }

        private void 读取设定参数ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] List = XMLHelper.ReadParameterFromXML("ParameterList.xml");
            for (int i = 0; i < List.Length; i++)
            {
                listBox1.Items.Add(List[i]);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int[] itemHANDLES = new int[] { 0 };
            VarEnum[] arrayType = new VarEnum[] { VarEnum.VT_EMPTY };

            MyVariant[] returnValue = null;

            //string[] itemID = new String[] { listBox1.SelectedItem.ToString() };
            string[] itemID = new String[dataGridView1.Rows.Count];
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                itemID[i] = dataGridView1.Rows[i].Cells["item"].ToString();
            }

            server.AddItems(itemID, out itemHANDLES, out arrayType);

            //string type = MyServer.VarEnumToString(arrayType[0]);

            server.ReadItems(itemID, itemHANDLES, arrayType, out returnValue);
            if (returnValue != null)
            {
                //string value = returnValue[0].GetStringValue();
                //AddItem(itemID[0], type, value);
                for (int i = 0; i < returnValue.Length - 1; i++)
                {
                    dataGridView1.Rows[i].Cells["value"].Value = returnValue[i].GetStringValue();
                    dataGridView1.Rows[i].Cells["time"].Value = DateTime.Now.ToString("HH:mm:ss:fff");
                }

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            timer1.Start();
            toolStripStatusLabel2.Text = "Timer:" + timer1.Enabled.ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            
            timer1.Stop();
            toolStripStatusLabel2.Text = "Timer:" + timer1.Enabled.ToString();
        }

    }
}