using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace GamePortal.API.Models.AnotherLogic
{
    public class PhoneDetector
    {
        private static readonly string[] _arrPrefix
            ={
                "096", "097", "098", "0162", "0163", "0164", "0165", "0166", "0167", "0168", "0169", "090", "093",
                "0120", "0121", "0122", "0126", "0128", "091", "094", "0123", "0124", "0125", "0127", "0129", "0993", "0994",
                "0995", "0996", "0997", "0199", "092", "0186", "0188", "095", "03", "04", "05", "06", "07", "08"
            };

        public static bool IsValidPhone(string phone)
        {
            long number = 0;
            if (!long.TryParse(phone, out number))
                return false;
            if (!Regex.IsMatch(phone, "^(01[2689]|09|03|04|05|06|07|08)[0-9]{8}$"))
                return false;
            if (_arrPrefix.ToList().Exists(x => phone.StartsWith(x)))
                return true;
            return false;
        }
    }
}