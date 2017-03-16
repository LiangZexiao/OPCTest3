using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;
using System.Configuration;

namespace SVs
{
    public partial class MachineConfig : Form
    {
        //private INIClass inihelper = new INIClass(Application.StartupPath+"\\opcsvc.ini");
        private INIClass inihelper;
        private string FilePath;
        public MachineConfig()
        {
            FilePath = ConfigurationSettings.AppSettings["INIFilePath"];
            inihelper = new INIClass(FilePath);
            
            InitializeComponent();
        }
        private void MachineConfig_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "opcsvc.ini文件路径：" + inihelper.FileName;
            ScanIP();
            GetAllAddressFormINI();
        }
        private void 选择opcsvcini文件路径ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "请选择opcsvc.ini文件";
            dialog.Filter = "配置文件（.ini）|*.ini";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FilePath = dialog.FileName;
                //MessageBox.Show(FilePath);
                inihelper = new INIClass(FilePath);
                toolStripStatusLabel1.Text = "opcsvc.ini文件路径：" + inihelper.FileName;
                GetAllAddressFormINI();

            }
        }
        #region "扫描网络"
        //扫描得本地IP
        public IPAddress GetLocalIP()
        {
            foreach (IPAddress _ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_ip.AddressFamily.ToString() == "InterNetwork")
                {
                    return _ip;
                }
            }
            return IPAddress.None;
        }

        //同时发送225个ping指令
        public void ScanIP()
        {
            for (int i = 1; i < 255; i++)
            {
                Ping myPing;
                myPing = new Ping();
                myPing.PingCompleted += new PingCompletedEventHandler(_myPing_PingCompleted);

                string LocalIP = GetLocalIP().ToString();
                label3.Text = "本机IP:" + LocalIP;

                string pingIP = StringOpr.GetNetSegment(LocalIP) + i.ToString();

                myPing.SendAsync(pingIP, 1000, null);
            }
        }

        //Ping指令异步接受到回应触发
        private void _myPing_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            if (e.Reply.Status == IPStatus.Success)
            {
                listBox2.Items.Add(e.Reply.Address.ToString());
            }
        }
        #endregion

        //添加选中的IP
        private void 添加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItems.Count == 0)
            {
                MessageBox.Show("请先选择要添加的IP地址");
                return;
            }
            for (int i = 0; i < listBox2.SelectedItems.Count; i++)
            {
                string machine_name = "Machine" + i.ToString();                              
                inihelper.WriteKeys("DefaultHosts", machine_name, listBox2.SelectedItems[i].ToString());
            }
            MessageBox.Show("写入成功");
            listBox1.Items.Clear();
            GetAllAddressFormINI();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItems.Count == 0)
            {
                MessageBox.Show("请先选择要添加的IP地址");
                return;
            }
            for (int i = 0; i < listBox2.SelectedItems.Count; i++)
            {
                string machine_name = "Machine" + i.ToString();
                inihelper.WriteKeys("DefaultHosts", machine_name, listBox2.SelectedItems[i].ToString());
            }
            MessageBox.Show("写入成功");
            listBox1.Items.Clear();
            GetAllAddressFormINI();
        }
        //得到ini中所有IP记录
        private void GetAllAddressFormINI()
        {
            listBox1.Items.Clear();
            byte[] result;

            if (inihelper.ExistINIFile())
            {
                result = inihelper.IniReadValues("DefaultHosts", null);
                string value = Encoding.Default.GetString(result);
                string[] list = value.Split('\0');
                foreach (string item in list)
                {
                    if (item != "")
                    {
                        string _value = inihelper.IniReadValue("DefaultHosts", item);
                        listBox1.Items.Add(item + ":" + _value);
                    }
                }
            }
            else
            {
                MessageBox.Show("找不到opcsvc.ini文件！");
            }
        }

        //删除选中的机器
        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count == 0)
            {
                MessageBox.Show("请先选择要删除的机器");
                return;
            }
            for (int i = 0; i < listBox1.SelectedItems.Count; i++)
            {
                string[] machine_name = listBox1.SelectedItems[i].ToString().Split(':');
                inihelper.DeleteKeyInSection("DefaultHosts", machine_name[0]);
            }
            MessageBox.Show("删除成功");
            listBox1.Items.Clear();
            GetAllAddressFormINI();
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count == 0)
            {
                MessageBox.Show("请先选择要删除的机器");
                return;
            }
            for (int i = 0; i < listBox1.SelectedItems.Count; i++)
            {
                string[] machine_name = listBox1.SelectedItems[i].ToString().Split(':');
                inihelper.DeleteKeyInSection("DefaultHosts", machine_name[0]);
            }
            MessageBox.Show("删除成功");
            listBox1.Items.Clear();
            GetAllAddressFormINI();
        }


    }
}
