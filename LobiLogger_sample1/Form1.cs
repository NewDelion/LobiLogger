using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace LobiLogger_sample1
{
    public partial class Form1 : Form
    {
        string url = @"https://web.lobi.co/api/group/";
        string groupid = "";
        string category = "";
        int load = 3;

        Object lockObj = new Object();

        List<ThreadListItem> CheckThreadList = new List<ThreadListItem>();
        List<long> removeRequestList = new List<long>();
        List<long> addRequestList = new List<long>();
        long lastThread = 0;

        public Form1()
        {
            InitializeComponent();
            textBox2.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.timer1.Stop();
            this.UpdateTick();
            timer1.Start();
        }

        public async void UpdateTick()
        {
            Chats[] ThreadList = getThreadList(this.load).Reverse().ToArray();
            await Task.Run(() =>
            {
                for (int i = 0; i < ThreadList.Length; i++)
                {
                    if (lastThread < ThreadList[i].id)
                    {
                        lock (this.lockObj)
                        {
                            this.CheckThreadList.Add(new ThreadListItem(ThreadList[i].id));
                        }
                        lastThread = ThreadList[i].id;
                        if (ThreadList[i].assets.Count > 0)
                        {
                            WebClient wc = new WebClient();
                            for (int j = 0; j < ThreadList[i].assets.Count; j++)
                            {
                                string url = ThreadList[i].assets[j].url;
                                string[] tmp = url.Split('/');
                                string filename = tmp[tmp.Length - 1];
                                url = url.Replace("http\\:", "http:");
                                url = url.Replace("https\\:", "https:");
                                string tmp2 = url.Substring(0, url.LastIndexOf('_'));
                                string kakutyousi = url.Substring(url.Length - 4);
                                url = tmp2 + "_raw" + kakutyousi;
                                wc.DownloadFile(url, textBox2.Text + "\\" + filename);
                            }
                        }
                        if (ThreadList[i].image != null && ((string)ThreadList[i].image).IndexOf("?hash=") == -1)
                        {
                            WebClient wc = new WebClient();
                            string url = (string)ThreadList[i].image;
                            string[] tmp = url.Split('/');
                            string filename = tmp[tmp.Length - 1];
                            url = url.Replace("http\\:", "http:");
                            url = url.Replace("https\\:", "https:");
                            string tmp2 = url.Substring(0, url.LastIndexOf('_'));
                            string kakutyousi = url.Substring(url.Length - 4);
                            url = tmp2 + "_raw" + kakutyousi;
                            wc.DownloadFile(url, textBox2.Text + "\\" + filename);

                        }
                    }
                }
                //削除リクエスト
                if (this.removeRequestList.Count > 0)
                {
                    lock (this.lockObj)
                    {
                        removeCheckItem(this.removeRequestList[0]);
                    }
                    this.removeRequestList.RemoveAt(0);
                }
                //追加リクエスト
                if (this.addRequestList.Count > 0)
                {
                    lock (this.lockObj)
                    {
                        bool flg = true;
                        for (int i = 0; i < this.CheckThreadList.Count; i++)
                        {
                            if (this.CheckThreadList[i].ThreadId == this.addRequestList[0])
                            {
                                flg = false;
                                break;
                            }
                        }
                        if (flg) this.CheckThreadList.Add(new ThreadListItem(this.addRequestList[0]));
                    }
                    this.addRequestList.RemoveAt(0);
                }
            });
            //リストボックス更新
            List<string> update = new List<string>();
            int select = listBox1.SelectedIndex;
            lock (lockObj)
            {
                for (int i = 0; i < this.CheckThreadList.Count; i++)
                {
                    update.Add(this.CheckThreadList[i].ThreadId.ToString());
                }
            }
            listBox1.Items.Clear();
            listBox1.Items.AddRange(update.ToArray());
            if (select < listBox1.Items.Count)
                listBox1.SelectedIndex = select;

            await Task.Run(() =>
            {
                for (int i = 0; i < this.CheckThreadList.Count; i++)
                {
                    Reply[] ChatList = getReplyList(this.CheckThreadList[i].ThreadId);
                    if (ChatList == null) continue;
                    ChatList = ChatList.Reverse().ToArray();
                    for (int j = 0; j < ChatList.Length; j++)
                    {
                        if (this.CheckThreadList[i].lastChat >= ChatList[j].id)
                            continue;
                        this.CheckThreadList[i].lastChat = ChatList[j].id;
                        if (ChatList[j].assets.Count > 0)
                        {
                            WebClient wc = new WebClient();
                            for (int k = 0; k < ChatList[j].assets.Count; k++)
                            {
                                string[] tmp = ChatList[j].assets[k].url.Split('/');
                                string filename = tmp[tmp.Length - 1];
                                string url = ChatList[j].assets[k].url;
                                url = url.Replace("http\\:", "http:");
                                url = url.Replace("https\\:", "https:");
                                string tmp2 = url.Substring(0, url.LastIndexOf('_'));
                                string kakutyousi = url.Substring(url.Length - 4);
                                url = tmp2 + "_raw" + kakutyousi;
                                wc.DownloadFile(url, textBox2.Text + "\\" + filename);
                            }
                        }
                        if (ChatList[j].image != null && ((string)ChatList[j].image).IndexOf("?hash=") == -1)
                        {
                            WebClient wc = new WebClient();
                            string url = (string)ChatList[j].image;
                            string[] tmp = url.Split('/');
                            string filename = tmp[tmp.Length - 1];
                            url = url.Replace("http\\:", "http:");
                            url = url.Replace("https\\:", "https:");
                            string tmp2 = url.Substring(0, url.LastIndexOf('_'));
                            string kakutyousi = url.Substring(url.Length - 4);
                            url = tmp2 + "_raw" + kakutyousi;
                            wc.DownloadFile(url, textBox2.Text + "\\" + filename);
                        }
                    }
                }
            });
        }

        public string format(string json)
        {
            string result = json;
            result = result.Replace("\\n", "%n");
            result = result.Replace("/", @"\/");
            result = result.Replace("http:", "http\\\\:");
            result = result.Replace("https:", "https\\\\:");
            return result;
        }
        public void removeCheckItem(long id)
        {
            
            for (int i = 0; i < this.CheckThreadList.Count; i++)
            {
                if (this.CheckThreadList[i].ThreadId == id)
                {
                    this.CheckThreadList.RemoveAt(i);
                    return;
                }
            }
        }

        public Chats[] getThreadList(int count)
        {
            WebClient client = new WebClient();
            string result = client.DownloadString(url + groupid + "/chats?count=" + count.ToString());
            using (StreamReader read = new StreamReader(client.OpenRead(url + groupid + "/chats?count=" + count.ToString())))
            {
                result = read.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<Chats[]>(format(result));
        }
        public Reply[] getReplyList(long id)
        {
            WebClient wc = new WebClient();
            string result = "";
            try
            {
                result = wc.DownloadString(url + groupid + "/chats/replies?to=" + id.ToString() + "&error_flavor=json2");
            }
            catch (WebException ex)
            {
                for (int i = 0; i < this.CheckThreadList.Count; i++)
                {
                    if (this.CheckThreadList[i].ThreadId == id)
                    {
                        this.CheckThreadList.RemoveAt(i);
                        return null;
                    }
                }
            }
            using (StreamReader read = new StreamReader(wc.OpenRead(url + groupid + "/chats/replies?to=" + id.ToString() + "&error_flavor=json2")))
            {
                result = read.ReadToEnd();
            }
            ReplyRoot reply_root = JsonConvert.DeserializeObject<ReplyRoot>(format(result));
            return reply_root.chats.ToArray();
        }

        class ThreadListItem
        {
            public long ThreadId;
            public long lastChat;
            public ThreadListItem(long id)
            {
                this.ThreadId = id;
                this.lastChat = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.textBox1.Text == "")
            {
                MessageBox.Show("グループIDを入力してください");
                return;
            }
            this.groupid = this.textBox1.Text;
            WebClient wc = new WebClient();
            this.textBox1.ReadOnly = true;
            this.button1.Enabled = false;
            this.button2.Enabled = false;
            this.numericUpDown1.Enabled = false;
            try
            {
                wc.DownloadString("https://web.lobi.co/group/" + groupid);
            }
            catch (Exception ex)
            {
                MessageBox.Show("接続中にエラーが発生しました。");
                this.textBox1.ReadOnly = false;
                this.button1.Enabled = true;
                this.button2.Enabled = true;
                this.numericUpDown1.Enabled = true;
                return;
            }
            this.listBox1.Enabled = true;
            this.textBox3.Enabled = true;
            this.button4.Enabled = true;
            this.button5.Enabled = true;
            this.button6.Enabled = true;
            this.button7.Enabled = true;
            button1.Text = "Running";
            this.load = (int)this.numericUpDown1.Value;
            timer1.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(textBox2.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("エラーが発生しました");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBox2.Text = this.folderBrowserDialog1.SelectedPath;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (textBox3.Text == "")
            {
                MessageBox.Show("スレッドIDを入力してください");
                return;
            }
            this.addRequestList.Add(long.Parse(textBox3.Text));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex >= 0)
            {
                this.removeRequestList.Add(long.Parse((string)this.listBox1.Items[this.listBox1.SelectedIndex]));
                return;
            }
            MessageBox.Show("削除するアイテムを選択してください");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex > 0)
            {
                string url = "https://web.lobi.co/group/";
                url += groupid + "/chat/";
                url += (string)this.listBox1.Items[this.listBox1.SelectedIndex];
                System.Diagnostics.Process.Start(url);
            }
            else
            {
                MessageBox.Show("スレッドを選択してください");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string url = "https://web.lobi.co/group/";
            url += groupid;
            System.Diagnostics.Process.Start(url);
        }
    }

    public class User
    {
        public string icon { get; set; }
        public string cover { get; set; }
        public string uid { get; set; }
        public string name { get; set; }
        public string @default { get; set; }
        public string description { get; set; }
    }

    public class Reply
    {
        public string booed { get; set; }
        public string liked { get; set; }
        public string is_group_bookmarked { get; set; }
        public string is_me_bookmarked { get; set; }
        public string likes_count { get; set; }
        public string bookmarks_count { get; set; }
        public string boos_count { get; set; }
        public string created_date { get; set; }
        public List<object> urls { get; set; }
        public string message { get; set; }
        public object image { get; set; }
        public List<Assets> assets { get; set; }
        public string reply_to { get; set; }
        public User user { get; set; }
        public string type { get; set; }
        public long id { get; set; }
    }

    public class Assets
    {
        public string id { get; set; }
        public string type { get; set; }
        public string url { get; set; }
    }

    public class Replies
    {
        public List<Reply> replies { get; set; }
        public string count { get; set; }
    }
    public class ReplyRoot
    {
        public List<Reply> chats { get; set; }
        public Reply to { get; set; }
        public Game game { get; set; }
    }
    public class Info
    {
        public string alias_name { get; set; }
    }
    public class Game
    {
        public Info info { get; set; }
        public string bookmarks_count { get; set; }
        public string gamelists_count { get; set; }
        public string icon { get; set; }
        public object app { get; set; }
        public string uid { get; set; }
        public string name { get; set; }
        public string groups_count { get; set; }
        public string cover { get; set; }
        public string genres { get; set; }
        public string members_count { get; set; }
        public object official_site_uri { get; set; }
        public string appstore_uri { get; set; }
        public string comments_count { get; set; }
        public string playstore_uri { get; set; }
    }

    public class Chats
    {
        public string booed { get; set; }
        public string boos_count { get; set; }
        public string liked { get; set; }
        public List<object> urls { get; set; }
        public Replies replies { get; set; }
        public User user { get; set; }
        public long id { get; set; }
        public string is_me_bookmarked { get; set; }
        public string bookmarks_count { get; set; }
        public string likes_count { get; set; }
        public string created_date { get; set; }
        public string is_group_bookmarked { get; set; }
        public object image { get; set; }
        public string message { get; set; }
        public List<Assets> assets { get; set; }
        public object reply_to { get; set; }
        public string type { get; set; }
    }
}
