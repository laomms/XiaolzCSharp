using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace XiaolzCSharp
{
    public partial class Form1 : Form
    {
        private string slectitem ;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "") return;
            if (SqliHelper.CheckDataExsit("授权群号", "GroupID", textBox1.Text) == false)
            {
                SqliHelper.InsertData("授权群号", new string[] { "GroupID", "time" }, new string[] { textBox1.Text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture) });
                SqliHelper.CheckImporlistview(this.listView1, "授权群号", "");
                MessageBox.Show("添加成功.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "") return;
            if (SqliHelper.CheckDataExsit("授权群号", "GroupID", textBox1.Text) == true)
            {
                SqliHelper.DeleteData("授权群号", "GroupID", "QQID like'" + textBox1.Text + "'");
                SqliHelper.CheckImporlistview(this.listView1, "授权群号", "");
                MessageBox.Show("删除成功.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "") return;
            if (SqliHelper.CheckDataExsit("高级权限", "QQID", textBox2.Text) == false)
            {
                SqliHelper.InsertData("高级权限", new string[] { "QQID", "time" }, new string[] { textBox2.Text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture) });
                SqliHelper.CheckImporlistview(this.listView2, "高级权限", "");
                MessageBox.Show("添加成功.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "") return;
            if (SqliHelper.CheckDataExsit("中级权限", "QQID", textBox2.Text) == false)
            {
                SqliHelper.InsertData("中级权限", new string[] { "QQID", "time" }, new string[] { textBox2.Text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture) });
                SqliHelper.CheckImporlistview(this.listView3, "中级权限", "");
                MessageBox.Show("添加成功.");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.listView1.Items.Clear();
            this.listView1.GridLines = true;
            this.listView1.View = View.Details;
            this.listView1.FullRowSelect = true;
            this.listView1.Columns.Add("ID", 30, HorizontalAlignment.Center);
            this.listView1.Columns.Add("群号", listView1.Width - 30-5, HorizontalAlignment.Center);
            

            this.listView1.Items.Clear();
            this.listView2.GridLines = true;
            this.listView2.View = View.Details;
            this.listView2.FullRowSelect = true;
            this.listView2.Columns.Add("ID", 30, HorizontalAlignment.Center);
            this.listView2.Columns.Add("高级权限QQ号", listView2.Width - 30-5, HorizontalAlignment.Center);

            this.listView3.Items.Clear();
            this.listView3.GridLines = true;
            this.listView3.View = View.Details;
            this.listView3.FullRowSelect = true;
            this.listView3.Columns.Add("ID", 30, HorizontalAlignment.Center);
            this.listView3.Columns.Add("中级权限QQ号", listView3.Width - 30-5, HorizontalAlignment.Center);

            this.listView4.Items.Clear();
            this.listView4.GridLines = true;
            this.listView4.View = View.Details;
            this.listView4.FullRowSelect = true;
            this.listView4.Columns.Add("ID", 30, HorizontalAlignment.Center);
            this.listView4.Columns.Add("群号", 60, HorizontalAlignment.Center);
            this.listView4.Columns.Add("QQ号", 60, HorizontalAlignment.Center);
            this.listView4.Columns.Add("MessageReq", 60, HorizontalAlignment.Center);
            this.listView4.Columns.Add("MessageRandom", 80, HorizontalAlignment.Center);
            this.listView4.Columns.Add("时间", 150, HorizontalAlignment.Center);
            this.listView4.Columns.Add("消息", 250, HorizontalAlignment.Left);

            SqliHelper.CheckImporlistview(this.listView1, "授权群号", "");
            SqliHelper.CheckImporlistview(this.listView2, "高级权限", "");
            SqliHelper.CheckImporlistview(this.listView3, "中级权限", "");

            List<List<string>> MasterInfo = SqliHelper.ReadData("主人信息", new string[] { "FeedbackGroup", "MasterQQ", }, "", "FeedbackGroup like '%%'");
            if (MasterInfo.Count > 0)
            {
                textBox4.Text = MasterInfo[0][0];
                textBox5.Text = MasterInfo[0][1];
            }
            new Thread(() =>
            {
                while (true)
                {
                    List<string> status = CpuMemoryCapacity.GetUsage();
                    try
                    {
                        label21.Invoke((MethodInvoker)(() => label21.Text = string.Join(" ", status)));
                    }
                    catch { }                    
                    Thread.Sleep(2000);
                }
                   
            }).Start();
            
        }

        private void 修改ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Object myValue =Interaction.InputBox("是否要修改群号:"+ slectitem+"?", "修改群号", "");
            Regex regex = new Regex("^[0-9]+$");
            if (Convert.ToString(myValue) != "" && regex.IsMatch(Convert.ToString(myValue)) ==true)
            {
                SqliHelper.UpdateData("授权群号", new string[] { "GroupID like'" + slectitem + "'" }, "GroupID='" + Convert.ToString(myValue) + "'");
                SqliHelper.CheckImporlistview(this.listView1, "授权群号", "");
            }
           
        }
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Object myValue = Interaction.InputBox("是否要修改高级权限QQ号:" + slectitem + "?", "高级权限", "");
            Regex regex = new Regex("^[0-9]+$");
            if (Convert.ToString(myValue) != "" && regex.IsMatch(Convert.ToString(myValue)) == true)
            {
                SqliHelper.UpdateData("高级权限", new string[] { "QQID like'" + slectitem + "'" }, "QQID='" + Convert.ToString(myValue) + "'");
                SqliHelper.CheckImporlistview(this.listView2, "高级权限", "");
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Object myValue = Interaction.InputBox("是否要修改中级权限QQ号:" + slectitem + "?", "中级权限", "");
            Regex regex = new Regex("^[0-9]+$");
            if (Convert.ToString(myValue) != "" && regex.IsMatch(Convert.ToString(myValue)) == true)
            {
                SqliHelper.UpdateData("中级权限", new string[] { "QQID like'" + slectitem + "'" }, "QQID='" + Convert.ToString(myValue) + "'");
                SqliHelper.CheckImporlistview(this.listView2, "中级权限", "");
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SqliHelper.CheckDataExsit("授权群号", "GroupID", textBox1.Text) == true)
            {
                SqliHelper.DeleteData("授权群号", "GroupID", "GroupID like'" + slectitem + "'");
                SqliHelper.CheckImporlistview(this.listView1, "授权群号", "");
                MessageBox.Show("删除成功.");
            }
        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (SqliHelper.CheckDataExsit("高级权限", "QQID", slectitem) == true)
            {
                SqliHelper.DeleteData("高级权限", "QQID", "QQID like'" + slectitem + "'");
                SqliHelper.CheckImporlistview(this.listView2, "高级权限", "");
                MessageBox.Show("删除成功.");
            }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (SqliHelper.CheckDataExsit("中级权限", "QQID", slectitem) == true)
            {
                SqliHelper.DeleteData("中级权限", "QQID", "QQID like'" + slectitem + "'");
                SqliHelper.CheckImporlistview(this.listView3, "中级权限", "");
                MessageBox.Show("删除成功.");
            }
        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    slectitem = listView1.SelectedItems[0].SubItems[1].Text;
                }
            }
            catch { }
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (listView2.SelectedItems.Count > 0)
                {
                    slectitem = listView2.SelectedItems[0].SubItems[1].Text;
                }
            }
            catch { }
        }

        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (listView3.SelectedItems.Count > 0)
                {
                    slectitem = listView3.SelectedItems[0].SubItems[1].Text;
                }
            }
            catch { }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string actualdata = string.Empty;
            char[] entereddata = textBox1.Text.ToCharArray();
            foreach (char aChar in entereddata.AsEnumerable())
            {
                if (Char.IsDigit(aChar))
                {
                    actualdata = actualdata + aChar;
                }
                else
                {                   
                    actualdata.Replace(aChar, ' ');
                    actualdata.Trim();
                }
            }
            textBox1.Text = actualdata;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            string actualdata = string.Empty;
            char[] entereddata = textBox2.Text.ToCharArray();
            foreach (char aChar in entereddata.AsEnumerable())
            {
                if (Char.IsDigit(aChar))
                {
                    actualdata = actualdata + aChar;
                }
                else
                {
                    actualdata.Replace(aChar, ' ');
                    actualdata.Trim();
                }
            }
            textBox2.Text = actualdata;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
                API.MsgRecod = true;
            else
                API.MsgRecod = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (SqliHelper.ClearTable("消息记录") == true)
                MessageBox.Show("已清空记录.");
            listView4.Items.Clear();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (textBox3.Text == "") return;
            SqliHelper.CheckImporlistview(listView4, "消息记录", " where QQID like '" + textBox3.Text + "' " );          

        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView4.SelectedItems.Count > 0)
                {
                    foreach (ListViewItem item in listView4.SelectedItems)
                    {
                        bool sucess = API.Undo_GroupEvent(PInvoke.plugin_key, API.MyQQ, long.Parse(item.SubItems[1].Text), long.Parse(item.SubItems[4].Text), int.Parse(item.SubItems[3].Text));
                        if (sucess)
                            MessageBox.Show("已撤回该消息.");
                    }
                }
            }
            catch(Exception ex)
            { Console.WriteLine(ex.Message); }
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView4.SelectedItems.Count > 0)
                {
                    foreach (ListViewItem item in listView4.SelectedItems)
                    {
                        SqliHelper.DeleteData("消息记录", "ID", "ID like'" + item.SubItems[0].Text + "'");
                        listView4.Items.Remove(item);
                    }
                    MessageBox.Show("删除成功.");
                }
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }           
        }

        private void listView4_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (listView4.SelectedItems.Count > 0)
                {
                                
                }
            }
            catch { }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (textBox4.Text == "" || textBox5.Text == "")
                return;
            if (SqliHelper.CheckDataExsit("主人信息", "FeedbackGroup", textBox4.Text) == false)
            {
                if (SqliHelper.CheckDataExsit("主人信息", "MasterQQ", textBox5.Text) == false)
                {
                    SqliHelper.ClearTable("主人信息");
                    SqliHelper.InsertData("主人信息", new string[] { "FeedbackGroup", "MasterQQ" }, new string[] { textBox4.Text, textBox5.Text });                   
                    MessageBox.Show("添加成功.");
                }
                else
                {
                    SqliHelper.UpdateData("主人信息", new string[] { "MasterQQ like'" + textBox5.Text + "'" }, "FeedbackGroup='" + textBox4.Text + "'");
                    MessageBox.Show("修改成功.");
                }
            }
            else
            {
                if (SqliHelper.CheckDataExsit("主人信息", "MasterQQ", textBox5.Text) == false)
                {
                    SqliHelper.UpdateData("主人信息", new string[] { "FeedbackGroup like'%" + textBox4.Text + "%''" }, "MasterQQ='" + textBox5.Text + "'");
                    MessageBox.Show("修改成功.");
                }
            }
            PInvoke.FeedbackGroup = long.Parse(textBox4.Text);
            PInvoke.MasterQQ = textBox5.Text;
            if (SqliHelper.CheckDataExsit("授权群号", "GroupID", textBox4.Text) == false)
            {
                SqliHelper.InsertData("授权群号", new string[] { "GroupID", "time" }, new string[] { textBox4.Text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture) });
                SqliHelper.CheckImporlistview(this.listView1, "授权群号", "");
            }
            if (SqliHelper.CheckDataExsit("高级权限", "QQID", textBox5.Text) == false)
            {
                SqliHelper.InsertData("高级权限", new string[] { "QQID", "time" }, new string[] { textBox5.Text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture) });
                SqliHelper.CheckImporlistview(this.listView2, "高级权限", "");
            }
            if (SqliHelper.CheckDataExsit("中级权限", "QQID", textBox5.Text) == false)
            {
                SqliHelper.InsertData("中级权限", new string[] { "QQID", "time" }, new string[] { textBox2.Text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture) });
                SqliHelper.CheckImporlistview(this.listView3, "中级权限", "");
            }
        }
    }
}
