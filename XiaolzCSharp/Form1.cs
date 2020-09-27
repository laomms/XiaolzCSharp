using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XiaolzCSharp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (SqliHelper.CheckDataExsit("授权群号", "GroupID", textBox1.Text) == false)
            {
                SqliHelper.InsertData("授权群号", new string[] { "GroupID", "time" }, new string[] { textBox1.Text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture) });
                MessageBox.Show("添加成功.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (SqliHelper.CheckDataExsit("授权群号", "GroupID", textBox1.Text) == true)
            {
                SqliHelper.DeleteData("授权群号", "GroupID", textBox1.Text);
                MessageBox.Show("删除成功.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (SqliHelper.CheckDataExsit("高级权限", "QQID", textBox2.Text) == false)
            {
                SqliHelper.InsertData("高级权限", new string[] { "QQID", "time" }, new string[] { textBox2.Text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture) });
                MessageBox.Show("添加成功.");
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (SqliHelper.CheckDataExsit("中级权限", "QQID", textBox2.Text) == false)
            {
                SqliHelper.InsertData("中级权限", new string[] { "QQID", "time" }, new string[] { textBox2.Text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture) });
                MessageBox.Show("添加成功.");
            }
        }
    }
}
