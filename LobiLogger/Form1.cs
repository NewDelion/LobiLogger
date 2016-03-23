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

namespace LobiLogger
{
    public partial class Form1 : Form
    {
        string url = @"https://web.lobi.co/api/group/";
        string groupid = "ab082650261bfdec5dce6db92ad9f4f8d9e5be0b";




        public Form1()
        {
            InitializeComponent();
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
        public string getChatList(int count)
        {
            WebClient wc = new WebClient();
            string result = wc.DownloadString(url + groupid + "/chats?count=" + count.ToString());
            using (StreamReader read = new StreamReader(wc.OpenRead(url + groupid + "/chats?count=" + count.ToString())))
            {
                result = read.ReadToEnd();
            }
            return format(result);
        }
        public string getChat(string id)
        {
            WebClient wc = new WebClient();
            string result = wc.DownloadString(url + groupid + "/chats/replies?to=" + id + "&error_flavor=json2");
            using (StreamReader read = new StreamReader(wc.OpenRead(url + groupid + "/chats/replies?to=" + id + "&error_flavor=json2")))
            {
                result = read.ReadToEnd();
            }
            return format(result);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string chatlist_json = getChatList(11);
            Chats[] chatlist = JsonConvert.DeserializeObject<Chats[]>(chatlist_json);
            WebClient wc = new WebClient();
            for (int i = 0; i < chatlist.Length; i++)
            {
                if (chatlist[i].assets.Count > 0)
                {
                    for (int j = 0; j < chatlist[i].assets.Count; j++)
                    {
                        string[] tmp = chatlist[i].assets[j].url.Split('/');
                        string filename = tmp[tmp.Length - 1];
                        string url = chatlist[i].assets[j].url;
                        url = url.Replace("http\\:", "http:");
                        url = url.Replace("https\\:", "https:");
                        string tmp2 = url.Substring(0, url.LastIndexOf('_'));
                        string kakutyousi = url.Substring(url.Length - 4);
                        url = tmp2 + "_raw" + kakutyousi;
                        
                        wc.DownloadFile(url, filename);
                    }
                }
                string chats_json = getChat(chatlist[i].id.ToString());
                ReplyRoot reply_root = JsonConvert.DeserializeObject<ReplyRoot>(chats_json);
                for (int j = 0; j < reply_root.chats.Count; j++)
                {
                    if (reply_root.chats[j].assets.Count > 0)
                    {
                        for (int k = 0; k < reply_root.chats[j].assets.Count; k++)
                        {
                            string[] tmp = reply_root.chats[j].assets[k].url.Split('/');
                            string filename = tmp[tmp.Length - 1];
                            string url = reply_root.chats[j].assets[k].url;
                            url = url.Replace("http\\:", "http:");
                            url = url.Replace("https\\:", "https:");
                            string tmp2 = url.Substring(0, url.LastIndexOf('_'));
                            string kakutyousi = url.Substring(url.Length - 4);
                            url = tmp2 + "_raw" + kakutyousi;
                            wc.DownloadFile(url, filename);
                        }
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

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
