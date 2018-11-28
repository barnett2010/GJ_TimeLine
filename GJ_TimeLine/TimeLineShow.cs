﻿
using GJ_TimeLine.Common;
using GJ_TimeLine.TimeLine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GJ_TimeLine
{
    public partial class TimeLineShow : Form
    {
        public TimelineConfig tc { get; set; }
        public TimelineConfig tctemp { get; set; }
        private int nowact { get; set; }
    
        private Thread thread;
        private TimeLineThread tlt;

        //窗口拖动

        private bool click = false;//鼠标是否点击
        private int xposition = 0;//面板相对屏幕的X轴坐标
        private int yposition = 0;//面板相对屏幕的Y轴坐标


        public TimeLineShow()
        {
            InitializeComponent();
        }
        public TimeLineShow(TimelineConfig tconfig)
        {
            InitializeComponent();
            tc = tconfig;
            this.nowact = 0;

            //透明
            this.BackColor = Color.SeaShell;
            this.TransparencyKey = Color.SeaShell;
            this.AllowTransparency = true;
        }

        private void TimeLineShow_Load(object sender, EventArgs e)
        {
            this.listView1.Clear();
            this.listView1.View = View.Details;
            this.listView1.Columns.Add("动作", 120, HorizontalAlignment.Left); //一步添加
            this.listView1.Columns.Add("时间", 60, HorizontalAlignment.Left); //一步添加
            if (tc == null)
            {
                MessageBox.Show("无法加载");
            }
            if (tc.Items.Count != 0)
            {
                this.listView1.BeginUpdate();   //数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度

                foreach (var item in tc.Items)
                {
                    ListViewItem lvi = new ListViewItem();
                    
                    lvi.Text = item.value;
                    lvi.SubItems.Add(convert2time((int)item.time));
                    this.listView1.Items.Add((lvi));

                }
                this.listView1.EndUpdate();  //结束数据处理，UI界面一次性绘制。
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dr = MessageBox.Show("是否退出?", "提示:", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (dr == DialogResult.OK)   //如果单击“是”按钮
            {
                stopThread();
                e.Cancel = false;                 //关闭窗体
            }
            else if (dr == DialogResult.Cancel)
            {
                e.Cancel = true;                  //不执行操作
            }
        }

        public void stopThread()
        {
            if (tlt != null)
            {
                tlt.isStart = false;
                thread.Abort();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            tctemp = this.tc;
            tlt = new TimeLineThread(this.tc);
            tlt.isStart = true;
            tlt.upact = new TimeLineThread.UpActDelegate(RefreshAct);
            tlt.uptime = new TimeLineThread.UpTimeDelegate(RefreshTime);
            thread = new Thread(new ThreadStart(tlt.ThreadFun));
            thread.Start();

        }


        private void button2_Click(object sender, EventArgs e)
        {
            stopThread();
            selectItem(0);
            time_label.Text = "";
            nowact = 0;
            TimeLineShow_Load(sender, e);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public void ReflushListView(int nowtime) {

          
            this.listView1.Clear();
            this.listView1.View = View.Details;
            this.listView1.Columns.Add("动作", 120, HorizontalAlignment.Left); //一步添加
            this.listView1.Columns.Add("时间", 60, HorizontalAlignment.Left); //一步添加
            if (tctemp.Items.Count == 0) { return; }
            this.listView1.BeginUpdate();   //数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度
            foreach (var item in tctemp.Items)
            {
                if (item.time >= nowtime)
                {

                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = item.value;
                    if (item.value.Split(']')[0] == "[H")
                    {
                        lvi.ForeColor = ColorTranslator.FromHtml("#33FF00");
                    }
                    else if (item.value.Split(']')[0] == "[D")
                    {
                        lvi.ForeColor = ColorTranslator.FromHtml("#3366CC");
                    }
                    else if (item.value.Split(']')[0] == "[T")
                    {
                        lvi.ForeColor = ColorTranslator.FromHtml("#CC0000");
                    }
                    else {
                        lvi.ForeColor = ColorTranslator.FromHtml("#FF6600");
                    }
                    lvi.SubItems.Add(convert2time((int)item.time-nowtime));
                    this.listView1.Items.Add((lvi));
                }

            }
            this.listView1.EndUpdate();  //结束数据处理，UI界面一次性绘制。
            this.listView1.Items[0].BackColor = System.Drawing.ColorTranslator.FromHtml("#99CCFF");

        }
        public void selectItem(int nowact) {
            this.listView1.Select();
            foreach (ListViewItem item in this.listView1.Items) {
                item.BackColor = Color.Empty;
            }

            //this.listView1.Items[nowact+1].BackColor = Color.Red;
            //if (nowact + 1 > this.listView1.Items.Count)
            //{
            //    this.listView1.EnsureVisible(nowact);//滚动到指定的行位置
            //}
            //else {
            //    this.listView1.EnsureVisible(nowact + 1);//滚动到指定的行位置
            //}

          
            
        }

        private string convert2time(int duration) {
            int sec = (duration % 60);
            int min = (duration / 60);
            return string.Format("{0:00}:{1:00}", min,sec);
        }

        private int convert2unixtime(string time)
        {
            int min = Convert.ToInt32( time.Split(':')[0]);
            int sec = Convert.ToInt32(time.Split(':')[1]);

            int all = min * 60 + sec;
            return all;
        }

        private void RefreshTime(int nowtime) {

            if (this.time_label.InvokeRequired)
            {
                TimeLineThread mtlt = new TimeLineThread(tc);
                mtlt.uptime = new TimeLineThread.UpTimeDelegate(RefreshTime);
                this.Invoke(mtlt.uptime, new object[] { nowtime });
            }
            else {
                time_label.Text = convert2time(nowtime);
                ReflushListView(nowtime);

            }
        }
        private void RefreshAct(int nowact) {
            if (this.listView1.InvokeRequired)
            {
                TimeLineThread mtlt = new TimeLineThread(tc);
                mtlt.upact = new TimeLineThread.UpActDelegate(RefreshAct);
                this.Invoke(mtlt.upact, new object[] { nowact });
            }
            else
            {
                selectItem(nowact);
                this.nowact = nowact;
            }
        }

        private void TimeLineShow_FormClosed(object sender, FormClosedEventArgs e)
        {
            stopThread();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            stopThread();
            this.Close();
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            click = true;
            xposition = e.X;
            yposition = e.Y;
        }

        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (click)
                {
                    this.SetBounds((MousePosition.X - xposition), (MousePosition.Y - yposition - this.label1.Height), this.Size.Width, this.Size.Height);
                }
            }
   
        }

        private void label1_MouseUp(object sender, MouseEventArgs e)
        {
            click = true;
            xposition = e.X;
            yposition = e.Y;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void TimeLineShow_MouseDown(object sender, MouseEventArgs e)
        {
            click = true;
            xposition = e.X;
            yposition = e.Y;
        }

        private void TimeLineShow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (click)
                {
                    this.SetBounds((MousePosition.X - xposition), (MousePosition.Y - yposition - this.label1.Height), this.Size.Width, this.Size.Height);
                }
            }
        }

        private void TimeLineShow_MouseUp(object sender, MouseEventArgs e)
        {
            click = true;
            xposition = e.X;
            yposition = e.Y;
        }
    }



}
