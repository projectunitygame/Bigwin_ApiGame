using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SlotGame._25Lines.Models.Services
{
    public interface ITestService
    {
        bool IsTestAccount(string accountName);
        int[] GetTestData();

        int SetTestData(string data);
    }
    public class TestService : ITestService
    {
        private static readonly string AccountsFile = HostingEnvironment.MapPath("~/App_Data/TestAccounts.txt");
        private static readonly string DataFile = HostingEnvironment.MapPath("~/App_Data/TestData.txt");
        public bool IsTestAccount(string accountName)
        {
            if (!File.Exists(AccountsFile)) return false;
            var allAccount = File.ReadAllText(AccountsFile);
            var accountList = allAccount.Split(',');
            return accountList.Contains(accountName);
        }

        public int[] GetTestData()
        {
            if (!File.Exists(DataFile)) return new int[0];
            var data = File.ReadAllText(DataFile);
            var slotsData = data.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            return slotsData;
        }

        public int SetTestData(string data)
        {
            if (!File.Exists(DataFile)) return -99;
            File.WriteAllText(DataFile, data);
            return 1;
        }
    }
}