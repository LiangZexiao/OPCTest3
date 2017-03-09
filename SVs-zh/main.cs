using OpcCommander;
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
        #region "声明变量"
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
        #endregion

        private void main_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "OPC服务器连接状态：" + connect;
            toolStripStatusLabel2.Text = "循环读数Timer状态:" + timer1.Enabled.ToString();

            dt.Columns.Add("item", System.Type.GetType("System.String"));
            dt.Columns.Add("type", System.Type.GetType("System.String"));
            dt.Columns.Add("value", System.Type.GetType("System.String"));            
            dt.Columns.Add("time", System.Type.GetType("System.String"));
            dataGridView1.DataSource = dt;
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
        /*******************************/
        private void AddItem(string item,string type,string value)
        {               
            DataRow dr = dt.NewRow();             
            dr["item"] = item;             
            dr["type"] = type;              
            dr["value"] = value;
            dr["time"] = DateTime.Now.ToString("HH:mm:ss:fff");
            dt.Rows.Add(dr);
        }
      
        //单Item查询
        private void button1_Click_1(object sender, EventArgs e)
        {
            string[] itemID = new String[] { textBox2.Text.ToString() };
            string type = "";
            string value = "";
            OPCOprHelper.GetItem(this.server, itemID, out type, out value);
            textBox3.Text = type;
            textBox4.Text = value;
        }

        private void Menu_Connect_Click(object sender, EventArgs e)
        {
            server = new MyServer(this);
            try
            {
                server.Start();
                connect = true;
                toolStripStatusLabel1.Text = "连接状态：" + connect;
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;

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
                            sessions.Add(new User(id, item));
                        }
                        catch (Exception exc)
                        {
                           MessageBox.Show("Log in to OPC failed", exc.Message);
                        }
                    }
                }
                catch (Exception exc)
                {
                   MessageBox.Show("Browser: parsing error.", exc.Message);
                }
            }
            catch (Exception exc)
            {
               MessageBox.Show("连接远程服务器出现错误：" + exc.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
       
        //读取列表中所有items       
        private void button3_Click(object sender, EventArgs e)
        {
            if(listBox1.Items.Count == 0)
            {
                MessageBox.Show("请先读取设定参数");
            }
            else
            {
                string[] itemID = new String[listBox1.Items.Count];
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    itemID[i] = listBox1.Items[i].ToString();
                }
                string[] type;
                string[] value;
                OPCOprHelper.GetItems(this.server, itemID, out type, out value);
                for (int i = 0; i < itemID.Length; i++)
                {
                    AddItem(itemID[i], type[i], value[i]);
                } 
            }
      
        }
        
        //读取单item
        private void button4_Click(object sender, EventArgs e)
        {
            if (flag == false )
            {
                MessageBox.Show("请先点击“读取设定参数”并选中列表中所要查询的参数");
            }
            else
            {
                if (listBox1.SelectedItems.Count == 0)
                {
                    MessageBox.Show("请选取参数列表中所要查询的参数");
                }
                else
                {
                    string[] itemID = new string[listBox1.SelectedItems.Count];
                    for (int i = 0; i < listBox1.SelectedItems.Count; i++)
                    {
                        itemID[i] = listBox1.SelectedItems[i].ToString();
                    }
                    string[] type;
                    string[] value;
                    OPCOprHelper.GetItems(this.server, itemID, out type, out value);
                    for (int i = 0; i < itemID.Length; i++)
                    {
                        AddItem(itemID[i], type[i], value[i]);
                    }       
                }                
            }           
        }
        bool flag = false;
        private void 读取设定参数ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            string[] List = XMLHelper.ReadParameterFromXML("ParameterList.xml");
            for (int i = 0; i < List.Length; i++)
            {
                listBox1.Items.Add(List[i]);   
            }
            flag = true;        
        }
      
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.server == null)
            {
                MessageBox.Show("请先连接OPC服务器");
                timer1.Stop();
                button2.Text = "开始循环读数";
                toolStripStatusLabel2.Text = "Timer:" + timer1.Enabled.ToString();
            }
            else if(dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("请先添加items");
                timer1.Stop();
                button2.Text = "开始循环读数";
                toolStripStatusLabel2.Text = "Timer:" + timer1.Enabled.ToString();
            }
            else
            {
                string[] itemID = new String[dataGridView1.Rows.Count];
                for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                {
                    itemID[i] = dataGridView1.Rows[i].Cells["item"].Value.ToString();
                    toolStripStatusLabel2.Text = "Timer:" + timer1.Enabled.ToString();
                }
                string[] type;
                string[] value;
                OPCOprHelper.GetItems(this.server, itemID, out type, out value);
                for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                {
                    dataGridView1.Rows[i].Cells["type"].Value = type[i];
                    dataGridView1.Rows[i].Cells["value"].Value = value[i];
                    dataGridView1.Rows[i].Cells["time"].Value = DateTime.Now.ToString("HH:mm:ss:fff");
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(timer1.Enabled == false)
            {
                timer1.Start();
                button2.Text = "停止循环读数";
                toolStripStatusLabel2.Text = "Timer:" + timer1.Enabled.ToString();
            }
            else
            {
                timer1.Stop();
                button2.Text = "开始循环读数";
                toolStripStatusLabel2.Text = "Timer:" + timer1.Enabled.ToString();
            }

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval =Convert.ToInt32( numericUpDown1.Value);
            toolStripStatusLabel3.Text = "时间间隔：" + Convert.ToInt32(numericUpDown1.Value) + "（ms）";
        }


        private void 添加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (flag == false)
            {
                MessageBox.Show("请先点击“读取设定参数”并选中列表中所要查询的参数");
            }
            else
            {
                if (listBox1.SelectedItems.Count == 0)
                {
                    MessageBox.Show("请选取参数列表中所要查询的参数");
                }
                else
                {
                    string[] itemID = new string[listBox1.SelectedItems.Count];
                    for (int i = 0; i < listBox1.SelectedItems.Count; i++)
                    {
                        itemID[i] = listBox1.SelectedItems[i].ToString();
                    }
                    string[] type;
                    string[] value;
                    OPCOprHelper.GetItems(this.server, itemID, out type, out value);
                    for (int i = 0; i < itemID.Length; i++)
                    {
                        AddItem(itemID[i], type[i], value[i]);
                    }
                }
            }

        }

    }
}
