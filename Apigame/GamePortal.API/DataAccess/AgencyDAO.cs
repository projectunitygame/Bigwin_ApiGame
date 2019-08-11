using GamePortal.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilities.Database;

namespace GamePortal.API.DataAccess
{
    public class AgencyDAO
    {
        public static List<Agency> GetAllAgency()
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            return db.GetList<Agency>($"SELECT * FROM [ag].[Account] WITH (NOLOCK) WHERE IsDelete = 0 AND IsLocked = 0 AND Level = 1 AND Displayable = 1 ORDER BY IndexOrder ASC");
            //SELECT AgencyID as ID, DisplayName, DisplayName as GameName, Phone as Tel, '' as Fb, '' as Telegram, AgencyCode as Information FROM Tbl_Agency where Status = 1 AND IsActive = 1 AND IsLock = 0
            //return db.GetList<Agency>($"SELECT AgencyID as ID, DisplayName, DisplayName as GameName, Phone as Tel, '' as Fb, '' as Telegram, AgencyCode as Information FROM Tbl_Agency with(nolock) where Status = 1 AND IsActive = 1 AND IsLock = 0 ORDER BY ID ASC");
        }

        public static List<Agency> GetAllAgency_v1()
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            //return db.GetList<Agency>($"SELECT * FROM [ag].[Account] WITH (NOLOCK) WHERE IsDelete = 0 AND IsLocked = 0 AND Level = 1 AND Displayable = 1 ORDER BY IndexOrder ASC");
            //SELECT AgencyID as ID, DisplayName, DisplayName as GameName, Phone as Tel, '' as Fb, '' as Telegram, AgencyCode as Information FROM Tbl_Agency where Status = 1 AND IsActive = 1 AND IsLock = 0
            return db.GetList<Agency>($"SELECT AgencyID as ID, DisplayName as Displayname, UWIN_ID as GameName, Phone as Tel, FB as Fb, '' as Telegram, Infomation as Information FROM Tbl_Agency with(nolock) where Status = 1 AND IsActive = 1 AND IsLock = 0 AND Display = 1 ORDER BY ID ASC");
        }
    }
}