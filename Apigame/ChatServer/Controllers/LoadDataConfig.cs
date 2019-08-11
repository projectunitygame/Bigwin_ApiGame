using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using ChatServer.Helper;
using Newtonsoft.Json;
using Utilities.Log;

namespace ChatServer.Controllers
{
    public static class LoadDataConfig
    {
        private static List<string> ListAdministrators = null;
        private static List<string> ListBadWords = null;
        private static List<ListAccountBlock> ListAccountBlock = null;
        private static List<ObjKeywordReplace> ListKeywordReplace = null;

        public static List<string> LoadListAdmin()
        {
            if (ListAdministrators == null)
            {
                ForceGetListAdmin();
            }
            return ListAdministrators;

        }
        public static List<string> LoadListBadWords()
        {
            if (ListBadWords == null)
            {
                ForceGetListBadWords();
            }
            return ListBadWords;

        }
        public static List<ListAccountBlock> LoadListAccountBlock()
        {
            if (ListAccountBlock == null)
            {
                ForceGetListAccBlock();
            }
            return ListAccountBlock;

        }
        public static List<ObjKeywordReplace> LoadListKeywordReplace()
        {
            if (ListKeywordReplace == null)
            {
                ForceGetListKeywordReplace();
            }
            return ListKeywordReplace;

        }

        public static void ForceGetListAdmin()
        {
            try
            {
                string mappath = HttpContext.Current.Server.MapPath(string.Format("~/App_Data/Chat/Data/Admin.xml"));
                XDocument xmldoc = XDocument.Load(mappath);
                IEnumerable<XElement> q = from xe in xmldoc.Descendants("key") select xe;
                var dt = new DataTable();
                dt.Columns.Add("text");
                foreach (XElement xe in q)
                {
                    DataRow row = dt.NewRow();
                    row[0] = xe.Attribute("text").Value;
                    dt.Rows.Add(row); // Thêm dòng mới vào dtb
                }
                ListAdministrators = dt.AsEnumerable().Select(r => r.Field<string>("text")).ToList();
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        public static void ForceGetListAccBlock()
        {
            try
            {
                string mappath = HttpContext.Current.Server.MapPath(string.Format("~/App_Data/Chat/Data/AccountBlock.xml"));
                XDocument xmldoc = XDocument.Load(mappath);
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
                List<ListAccountBlock> currAccountBlocks = dt.AsEnumerable().Select(m => new ListAccountBlock()
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

                ListAccountBlock = currAccountBlocks;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        public static void ForceGetListKeywordReplace()
        {
            try
            {
                string mappath = HttpContext.Current.Server.MapPath(string.Format("~/App_Data/Chat/Data/KeywordReplace.xml"));
                XDocument xmldoc = XDocument.Load(mappath);
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
                List<ObjKeywordReplace> currKeyword = dt.AsEnumerable().Select(m => new ObjKeywordReplace()
                {
                    text = m.Field<string>("text"),
                    replace = m.Field<string>("replace")
                }).ToList();
                ListKeywordReplace = currKeyword;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        public static void ForceGetListBadWords()
        {
            try
            {
                string mappath = HttpContext.Current.Server.MapPath(string.Format("~/App_Data/Chat/Data/BlackList.xml"));
                XDocument xmldoc = XDocument.Load(mappath);
                IEnumerable<XElement> q = from xe in xmldoc.Descendants("key") select xe;
                var dt = new DataTable();
                dt.Columns.Add("text");
                foreach (XElement xe in q)
                {
                    DataRow row = dt.NewRow();
                    row[0] = xe.Attribute("text").Value;
                    dt.Rows.Add(row); // Thêm dòng mới vào dtb
                }
                ListBadWords = dt.AsEnumerable().Select(r => r.Field<string>("text")).ToList();
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

    }
}