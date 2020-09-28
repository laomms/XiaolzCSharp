using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            if (SqliHelper.CheckDataExsit("授权群号", "GroupID", textBox1.Text) == false)
            {
                SqliHelper.InsertData("授权群号", new string[] { "GroupID", "time" }, new string[] { textBox1.Text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture) });
                SqliHelper.CheckImporlistview(this.listView1, "授权群号", "");
                MessageBox.Show("添加成功.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (SqliHelper.CheckDataExsit("授权群号", "GroupID", textBox1.Text) == true)
            {
                SqliHelper.DeleteData("授权群号", "GroupID", textBox1.Text);
                SqliHelper.CheckImporlistview(this.listView1, "授权群号", "");
                MessageBox.Show("删除成功.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (SqliHelper.CheckDataExsit("高级权限", "QQID", textBox2.Text) == false)
            {
                SqliHelper.InsertData("高级权限", new string[] { "QQID", "time" }, new string[] { textBox2.Text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture) });
                SqliHelper.CheckImporlistview(this.listView2, "授权群号", "");
                MessageBox.Show("添加成功.");
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (SqliHelper.CheckDataExsit("中级权限", "QQID", textBox2.Text) == false)
            {
                SqliHelper.InsertData("中级权限", new string[] { "QQID", "time" }, new string[] { textBox2.Text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture) });
                SqliHelper.CheckImporlistview(this.listView3, "授权群号", "");
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

            SqliHelper.CheckImporlistview(this.listView1, "授权群号", "");
            SqliHelper.CheckImporlistview(this.listView2, "高级权限", "");
            SqliHelper.CheckImporlistview(this.listView3, "中级权限", "");
   
        }

        private void 修改ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Object myValue =Interaction.InputBox("是否要修改群号:"+ slectitem+"?", "修改群号", "");
            Regex regex = new Regex("^[0-9]+$");
            if (Convert.ToString(myValue) != "" && regex.IsMatch(Convert.ToString(myValue)) ==true)
            {
                SqliHelper.UpdateData("授权群号", "GroupID", slectitem, "GroupID='" + Convert.ToString(myValue) + "'");
                SqliHelper.CheckImporlistview(this.listView1, "授权群号", "");
            }
           
        }
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Object myValue = Interaction.InputBox("是否要修改高级权限QQ号:" + slectitem + "?", "高级权限", "");
            Regex regex = new Regex("^[0-9]+$");
            if (Convert.ToString(myValue) != "" && regex.IsMatch(Convert.ToString(myValue)) == true)
            {
                SqliHelper.UpdateData("高级权限", "GroupID", slectitem, "GroupID='" + Convert.ToString(myValue) + "'");
                SqliHelper.CheckImporlistview(this.listView2, "高级权限", "");
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Object myValue = Interaction.InputBox("是否要修改中级权限QQ号:" + slectitem + "?", "中级权限", "");
            Regex regex = new Regex("^[0-9]+$");
            if (Convert.ToString(myValue) != "" && regex.IsMatch(Convert.ToString(myValue)) == true)
            {
                SqliHelper.UpdateData("中级权限", "GroupID", slectitem, "GroupID='" + Convert.ToString(myValue) + "'");
                SqliHelper.CheckImporlistview(this.listView3, "中级权限", "");
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SqliHelper.CheckDataExsit("授权群号", "GroupID", textBox1.Text) == true)
            {
                SqliHelper.DeleteData("授权群号", "GroupID", slectitem);
                SqliHelper.CheckImporlistview(this.listView1, "授权群号", "");
                MessageBox.Show("删除成功.");
            }
        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (SqliHelper.CheckDataExsit("高级权限", "GroupID", textBox1.Text) == true)
            {
                SqliHelper.DeleteData("高级权限", "GroupID", slectitem);
                SqliHelper.CheckImporlistview(this.listView1, "高级权限", "");
                MessageBox.Show("删除成功.");
            }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (SqliHelper.CheckDataExsit("中级权限", "GroupID", textBox1.Text) == true)
            {
                SqliHelper.DeleteData("中级权限", "GroupID", slectitem);
                SqliHelper.CheckImporlistview(this.listView1, "中级权限", "");
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

   
    }
}
