using PTCN.CrossPlatform.Minigame.LuckyDice.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;
using Utilities.Log;

namespace PTCN.CrossPlatform.Minigame.LuckyDice.Models.Chat
{
    public class ObjKeywordReplace
    {
        public string text { get; set; }
        public string replace { get; set; }
    }

    public class ChatFilter
    {
        static List<BannedUser> _banned;// = new List<BannedUser>();
        static object _lockBanned;// = new object();

        private static readonly object lockLoadBadWords = new object();
        private static readonly string RegexAcceptChars = @"[^aáàảãạăắằẳẵặâấầẩẫậđeéèẻẽẹêếềểễệiíìỉĩịoóòỏõọôốồổỗộơớờởỡợuúùủũụưứừửữựyýỳỷỹỵAÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬĐEÉÈẺẼẸÊẾỀỂỄỆIÍÌỈĨỊOÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢUÚÙỦŨỤƯỨỪỬỮỰYÝỲỶỸỴ\w\s\d]+";
        private static readonly string BLACKLIST_FILE = HostingEnvironment.MapPath("~/App_Data/Chat/Data/BlackList.xml");
        private static readonly string KEYWORDREPLACE_FILE = HostingEnvironment.MapPath("~/App_Data/Chat/Data/KeywordReplace.xml");
        private static List<string> BadWords = new List<string>();
        private static List<ObjKeywordReplace> KeywordReplace = new List<ObjKeywordReplace>();

        public static void Init()
        {
            _lockBanned = new object();

            _banned = Lddb.Instance.GetBannedUser();

            if (_banned == null)
                _banned = new List<BannedUser>();

            if (BadWords == null || BadWords.Count < 1)
            {
                if (Monitor.TryEnter(lockLoadBadWords, 60000))
                {
                    try
                    {
                        if (File.Exists(BLACKLIST_FILE))
                        {
                            LoadBlackList();
                        }
                    }
                    catch (Exception e)
                    {
                        NLogManager.PublishException(e);
                    }
                    finally
                    {
                        Monitor.Exit(lockLoadBadWords);
                    }
                }
            }

            if (KeywordReplace == null || KeywordReplace.Count < 1)
            {
                try
                {
                    NLogManager.LogMessage(string.Format("Load file keyword replace: {0}", KEYWORDREPLACE_FILE));
                    if (File.Exists(KEYWORDREPLACE_FILE))
                    {
                        LoadKeywordReplace();
                    }
                }
                catch (Exception e)
                {
                    NLogManager.PublishException(e);
                }
            }

        }

        public static string RemoveBadWords(string input, out bool Flag)
        {
            LoadBlackList();
            Flag = false;
            int bwLength = BadWords.Count;
            for (int i = 0; i < bwLength; i++)
            {
                try
                {
                    string bw = BadWords[i];
                    if (bw.StartsWith("regex::", StringComparison.OrdinalIgnoreCase))
                    {
                        bw = bw.Substring(7);
                        Regex regx = new Regex(bw, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        if (regx.IsMatch(input))
                            Flag = true;
                        input = regx.Replace(input, "***");
                    }
                    else
                    {
                        int countLength = input.Length;
                        input = input.Replace(bw, "***", StringComparison.OrdinalIgnoreCase);
                        if (input.Length != countLength)
                            Flag = true;
                    }
                }
                catch (Exception ex)
                {
                    NLogManager.PublishException(ex);
                }
            }

            return input;
        }

        private static void LoadKeywordReplace()
        {
            try
            {
                XDocument xmldoc = XDocument.Load(KEYWORDREPLACE_FILE);
                IEnumerable<XElement> q = from xe in xmldoc.Descendants("key") select xe;
                var dt = new DataTable();
                dt.Columns.Add("text");
                dt.Columns.Add("replace");
                foreach (XElement xe in q)
                {
                    DataRow row = dt.NewRow();
                    row[0] = xe.Attribute("text").Value;
                    row[1] = xe.Attribute("replace").Value;
                    dt.Rows.Add(row); // Thêm dòng mới vào dtb
                }
                List<ObjKeywordReplace> Data = dt.AsEnumerable().Select(m => new ObjKeywordReplace()
                {
                    text = m.Field<string>("text"),
                    replace = m.Field<string>("replace"),
                }).ToList();
                KeywordReplace = Data;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        private static void LoadBlackList()
        {
            XDocument xmldoc = XDocument.Load(BLACKLIST_FILE);
            IEnumerable<XElement> q = from xe in xmldoc.Descendants("key") select xe;
            var dt = new DataTable();
            dt.Columns.Add("text");
            foreach (XElement xe in q)
            {
                DataRow row = dt.NewRow();
                row[0] = xe.Attribute("text").Value;
                dt.Rows.Add(row); // Thêm dòng mới vào dtb
            }
            BadWords = dt.AsEnumerable().Select(r => r.Field<string>("text")).ToList();
        }

        public static bool CheckBanned(string username)
        {
            if(Monitor.TryEnter(_lockBanned, 5000))
            {
                try
                {
                    username = username.ToLower();
                    return (_banned.Exists(x => x.AccountName == username));
                }
                finally
                {
                    Monitor.Exit(_lockBanned);
                }
            }
            return false;
        }

        public static bool BanUser(string username)
        {
            if (Monitor.TryEnter(_lockBanned, 5000))
            {
                try
                {
                    username = username.ToLower();
                    var user = _banned.FirstOrDefault(x => x.AccountName == username);
                    if (user == null)
                    {
                        _banned.Add(new BannedUser { AccountName = username, Locked = true });
                    }
                    Lddb.Instance.BanUser(username);

                    return true;
                }
                finally
                {
                    Monitor.Exit(_lockBanned);
                }
            }

            return false;
        }

        public static bool UnbanUser(string username)
        {
            if (Monitor.TryEnter(_lockBanned, 5000))
            {
                try
                {
                    username = username.ToLower();
                    _banned = _banned.Where(x => x.AccountName != username).ToList();
                    Lddb.Instance.UnbanUser(username);
                    return true;
                }
                finally
                {
                    Monitor.Exit(_lockBanned);
                }
            }

            return false;
        }
    }

    public class BannedUser
    {
        public string AccountName { get; set; }
        public bool Locked { get; set; }
    }

    public static class StringExt
    {
        public static string Replace(this string s, string oldValue, string newValue, StringComparison comparisonType)
        {
            if (s == null)
                return null;

            if (String.IsNullOrEmpty(oldValue))
                return s;

            StringBuilder result = new StringBuilder(Math.Min(4096, s.Length));
            int pos = 0;

            while (true)
            {
                int i = s.IndexOf(oldValue, pos, comparisonType);
                if (i < 0)
                    break;

                result.Append(s, pos, i - pos);
                result.Append(newValue);

                pos = i + oldValue.Length;
            }
            result.Append(s, pos, s.Length - pos);

            return result.ToString();
        }
    }
}