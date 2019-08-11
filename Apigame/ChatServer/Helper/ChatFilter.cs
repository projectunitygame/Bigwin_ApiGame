using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;
using Utilities.Cache;
using Utilities.Log;

namespace ChatServer.Helper
{
    public static class ChatFilter
    {
        private static readonly object lockLoadBadWords = new object();
        private static readonly object lockLoadBadLinks = new object();
        private static readonly object lockLoadBanUsers = new object();

        private static readonly string RegexAcceptChars = @"[^aáàảãạăắằẳẵặâấầẩẫậđeéèẻẽẹêếềểễệiíìỉĩịoóòỏõọôốồổỗộơớờởỡợuúùủũụưứừửữựyýỳỷỹỵAÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬĐEÉÈẺẼẸÊẾỀỂỄỆIÍÌỈĨỊOÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢUÚÙỦŨỤƯỨỪỬỮỰYÝỲỶỸỴ\w\s\d]+";
        private static List<string> BadWords = new List<string>();
        private static List<string> BadLinks = new List<string>();
        private static List<string> BanUsers = new List<string>();
        private static List<ObjKeywordReplace> KeywordReplace = new List<ObjKeywordReplace>();
        private static List<ListAccountBlock> ListAccountBlock = new List<ListAccountBlock>();
        private static ConcurrentDictionary<string, string> ReplaceVNs = new ConcurrentDictionary<string, string>();

        private static readonly string BANUSERS_FILE = HostingEnvironment.MapPath("~/App_Data/Chat/Data/banusers.txt");
        private static readonly string BADLINKS_FILE = HostingEnvironment.MapPath("~/App_Data/Chat/Data/badlinks.txt");
        private static readonly string BADWORDS_FILE = HostingEnvironment.MapPath("~/App_Data/Chat/Data/badwords.txt");


        //Đọc từ file xml 
        private static readonly string BLACKLIST_FILE = HostingEnvironment.MapPath("~/App_Data/Chat/Data/BlackList.xml");
        private static readonly string KEYWORDREPLACE_FILE = HostingEnvironment.MapPath("~/App_Data/Chat/Data/KeywordReplace.xml");
        private static readonly string ACCOUNTBLOCK_FILE = HostingEnvironment.MapPath("~/App_Data/Chat/Data/AccountBlock.xml");

        static ChatFilter()
        {
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

            if (BadWords == null || BadWords.Count < 1)
            {
                if (Monitor.TryEnter(lockLoadBadWords, 60000))
                {
                    try
                    {
                        NLogManager.LogMessage(string.Format("Load file bad word: {0}", BLACKLIST_FILE));
                        if (File.Exists(BLACKLIST_FILE))
                        {
                            //string[] allText = File.ReadAllLines(BADWORDS_FILE);
                            //BadWords = new List<string>(allText);
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

            if (BadLinks == null || BadLinks.Count < 1)
            {
                if (Monitor.TryEnter(lockLoadBadLinks, 60000))
                {
                    try
                    {
                        NLogManager.LogMessage(string.Format("Load file bad links: {0}", BADLINKS_FILE));
                        if (File.Exists(BADLINKS_FILE))
                        {
                            string[] allText = File.ReadAllLines(BADLINKS_FILE);

                            BadLinks = new List<string>(allText);
                        }
                    }
                    catch (Exception e)
                    {
                        NLogManager.PublishException(e);
                    }
                    finally
                    {
                        Monitor.Exit(lockLoadBadLinks);
                    }
                }
            }

            if (ListAccountBlock == null || ListAccountBlock.Count < 1)
            {
                try
                {
                    NLogManager.LogMessage(string.Format("Load file Account Block: {0}", ACCOUNTBLOCK_FILE));
                    if (File.Exists(ACCOUNTBLOCK_FILE))
                    {
                        LoadAccountBlock();
                    }
                }
                catch (Exception e)
                {
                    NLogManager.PublishException(e);
                }
            }
            //if (BanUsers == null || BanUsers.Count < 1)
            //{
            //    if (Monitor.TryEnter(lockLoadBanUsers, 60000))
            //    {
            //        try
            //        {
            //            NLogManager.LogMessage(string.Format("Load file banned user: {0}", BANUSERS_FILE));
            //            if (File.Exists(BANUSERS_FILE))
            //            {
            //                string[] allText = File.ReadAllLines(BANUSERS_FILE);

            //                BanUsers.AddRange(allText);
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            NLogManager.PublishException(e);
            //        }
            //        finally
            //        {
            //            Monitor.Exit(lockLoadBanUsers);
            //        }
            //    }
            //}

        }
        public static string ReplaceKeyword(string input)
        {
            LoadKeywordReplace();
            if (KeywordReplace.Count > 0)
            {
                foreach (var item in KeywordReplace)
                {
                    input = input.Replace(item.text, item.replace);
                }
            }
            return input;
        }
        public static string CutOff(string input, string pattern = " ")
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                input = input.Replace(pattern[i].ToString(), "");
            }

            return input;
        }

        public static string ReplaceVN(string input)
        {
            if (ReplaceVNs == null || ReplaceVNs.Count < 1)
            {
                if (Monitor.TryEnter(ReplaceVNs, 60000))
                {
                    try
                    {
                        ReplaceVNs.TryAdd("[@ÅÄäẢÃÁÀẠảãáàạÂĂẨẪẤẦẬẩẫấầậẲẴẮẰẶẳẵắằặ]+", "a");
                        ReplaceVNs.TryAdd("[ß]+", "b");
                        ReplaceVNs.TryAdd("[Ç€]+", "c");
                        ReplaceVNs.TryAdd("[ËẺẼÉÈẸẻẽéèẹÊỂỄẾỀỆêểễếềệ]+", "e");
                        ReplaceVNs.TryAdd("[ÏιỈĨÍÌỊỉĩíìị]+", "i");
                        ReplaceVNs.TryAdd("[ØÖöΘ☻❂ỎÕÓÒỌỏõóòọÔỔỖỐỒỘôổỗốồộƠỞỠỚỜỢơởỡớờợ0]+", "o");
                        ReplaceVNs.TryAdd("[Šš]+", "s");
                        ReplaceVNs.TryAdd("[τ]+", "t");
                        ReplaceVNs.TryAdd("[ÜỦŨÙỤÚủũúùụỬỮỨỪỰửữứừự]+", "u");
                        ReplaceVNs.TryAdd("[•,;:]+", ".");
                    }
                    finally
                    {
                        Monitor.Exit(ReplaceVNs);
                    }
                }
            }

            foreach (string key in ReplaceVNs.Keys)
            {
                try
                {
                    Regex regx = new Regex(key, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    input = regx.Replace(input, ReplaceVNs[key]);

                    //NLogManager.LogMessage(key + " : " + input);
                }
                catch (Exception ex)
                {
                    NLogManager.PublishException(ex);
                }
            }

            return input;
        }

        public static string RemoveUnAcceptChars(string input)
        {
            Regex regx = new Regex(RegexAcceptChars, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string output = regx.Replace(input, "*");

            return output;
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
                        //input = input.Replace(bw, "***", StringComparison.OrdinalIgnoreCase);
                        input = input.Replace(bw, "***");
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


        public static string RemoveBadLinks(string input, out bool Flag)
        {
            input = CutOff(input, " '`~");
            input = ReplaceVN(input);

            Flag = false;
            int bwLength = BadLinks.Count;
            for (int i = 0; i < bwLength; i++)
            {
                try
                {
                    string bl = BadLinks[i];
                    if (bl.StartsWith("regex::", StringComparison.OrdinalIgnoreCase))
                    {
                        bl = bl.Substring(7);
                        Regex regx = new Regex(bl, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        if (regx.IsMatch(input))
                            Flag = true;
                        input = regx.Replace(input, "*");
                    }
                    else
                    {
                        int countLength = input.Length;
                        //input = input.Replace(bl, "*", StringComparison.OrdinalIgnoreCase);
                        input = input.Replace(bl, "***");
                        if (input.Length != countLength)
                        {
                            Flag = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    NLogManager.PublishException(ex);
                }
            }

            return input;
        }

        public static bool CheckBanUsers(string username)
        {
            LoadAccountBlock();
            if (ListAccountBlock.Count > 0)
            {
                foreach (var item in ListAccountBlock)
                {
                    if (item.name == username)
                    {
                        if (DateTime.Now >= DateTime.Parse(item.endtimeblock))
                        {
                            DeleteAccountBlock(item.key);
                            return false;
                        }
                        return true;
                    }
                }
            }
            return false;
            //Bỏ Return đọc từ file txt cũ - huandh
            //return BanUsers.Contains(username);
        }
        public static string ReturnCheckBanUsers(string username)
        {
            string message = "";
            LoadAccountBlock();
            if (ListAccountBlock.Count > 0)
            {
                foreach (var item in ListAccountBlock)
                {
                    if (item.name == username)
                    {
                        message = string.Format("Tài khoản đang bị Block, lý do: {0} - Thời hạn đến: {1}", item.namereasonblock, item.endtimeblock);
                    }
                }
            }
            return message;
        }
        public static bool BanUser(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            if (Monitor.TryEnter(lockLoadBanUsers, 60000))
            {
                try
                {
                    if (CheckBanUsers(username))
                        return true;

                    File.AppendAllText(BANUSERS_FILE, Environment.NewLine + username);

                    BanUsers.Add(username);
                    NLogManager.LogMessage(string.Format("Admins has been banned user: username={0}", username));

                    return true;
                }
                finally
                {
                    Monitor.Exit(lockLoadBanUsers);
                }
            }
            return false;
        }

        public static bool AddBadLink(string link)
        {
            if (Monitor.TryEnter(lockLoadBadLinks, 60000))
            {
                try
                {
                    File.AppendAllText(BADLINKS_FILE, Environment.NewLine + link);

                    BadLinks.Add(link);
                    NLogManager.LogMessage(string.Format("Admins has been added bad link: link={0}", link));

                    return true;
                }
                finally
                {
                    Monitor.Exit(lockLoadBadLinks);
                }
            }

            return false;
        }

        public static bool AddBadWord(string word)
        {
            if (Monitor.TryEnter(lockLoadBadWords, 60000))
            {
                try
                {
                    File.AppendAllText(BADWORDS_FILE, Environment.NewLine + word);

                    BadWords.Add(word);
                    NLogManager.LogMessage(string.Format("Admins has been added bad word: word={0}", word));

                    return true;
                }
                finally
                {
                    Monitor.Exit(lockLoadBadWords);
                }
            }

            return false;
        }

        private static void LoadBlackList()
        {
            string key = "BLACKLIST_FILE";
            if (CacheHandler.Get(key) == null)
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
                CacheHandler.Add(key, BadWords);
            }
            else
            {
                BadWords = (List<string>)CacheHandler.Get(key);
            }
        }
        private static void LoadKeywordReplace()
        {
            try
            {
                string key = "KEYWORDREPLACE_FILE";
                if (CacheHandler.Get(key) == null)
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
                    CacheHandler.Add(key, KeywordReplace);
                }
                else
                {
                    KeywordReplace = (List<ObjKeywordReplace>)CacheHandler.Get(key);
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

        }
        private static void LoadAccountBlock()
        {
            try
            {
                string key = "ACCOUNTBLOCK_FILE";
                if (CacheHandler.Get(key) == null)
                {
                    XDocument xmldoc = XDocument.Load(ACCOUNTBLOCK_FILE);
                    IEnumerable<XElement> q = from xe in xmldoc.Descendants("key") select xe;
                    var dt = new DataTable();
                    dt.Columns.Add("key");
                    dt.Columns.Add("name");
                    dt.Columns.Add("accountid");
                    dt.Columns.Add("reasonblock");
                    dt.Columns.Add("namereasonblock");
                    dt.Columns.Add("typeblock");
                    dt.Columns.Add("endtimeblock");
                    dt.Columns.Add("createDate");
                    foreach (XElement xe in q)
                    {
                        DataRow row = dt.NewRow();
                        row[0] = xe.Attribute("key").Value;
                        row[1] = xe.Attribute("name").Value;
                        row[2] = xe.Attribute("accountid").Value;
                        row[3] = xe.Attribute("reasonblock").Value;
                        row[4] = xe.Attribute("namereasonblock").Value;
                        row[5] = xe.Attribute("typeblock").Value;
                        row[6] = xe.Attribute("endtimeblock").Value;
                        row[7] = xe.Attribute("createDate").Value;
                        dt.Rows.Add(row); // Thêm dòng mới vào dtb
                    }
                    List<ListAccountBlock> Data = ListAccountBlock = dt.AsEnumerable().Select(m => new ListAccountBlock()
                    {
                        key = m.Field<string>("key"),
                        name = m.Field<string>("name"),
                        accountid = m.Field<string>("accountid"),
                        reasonblock = m.Field<string>("reasonblock"),
                        namereasonblock = m.Field<string>("namereasonblock"),
                        typeblock = m.Field<string>("typeblock"),
                        endtimeblock = m.Field<string>("endtimeblock"),
                        createDate = m.Field<string>("createDate")
                    }).ToList();
                    CacheHandler.Add(key, Data);
                }
                else
                {
                    ListAccountBlock = (List<ListAccountBlock>)CacheHandler.Get(key);
                }
            }
            catch (Exception ex)
            {
                NLogManager.LogMessage(">> Ex LoadAccountBlock:" + ex.Message);
            }

        }
        private static void DeleteAccountBlock(string key)
        {
            try
            {
                XDocument xmldoc = XDocument.Load(ACCOUNTBLOCK_FILE);
                XElement xmlelement = xmldoc.Element("AccountBlock").Elements("key").Single(x => (string)x.Attribute("key") == key);
                xmlelement.Remove();
                xmldoc.Save(ACCOUNTBLOCK_FILE);
                CacheHandler.Remove("ACCOUNTBLOCK_FILE");
                LoadAccountBlock();
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

        }

    }
    public class ObjKeywordReplace
    {
        public string text { get; set; }
        public string replace { get; set; }
    }
    public class ListAccountBlock
    {
        public string key { get; set; }
        public string name { get; set; }
        public string accountid { get; set; }
        public string reasonblock { get; set; }
        public string namereasonblock { get; set; }
        public string typeblock { get; set; }
        public string endtimeblock { get; set; }
        public string createDate { get; set; }
    }
}