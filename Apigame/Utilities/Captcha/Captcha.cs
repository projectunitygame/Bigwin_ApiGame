using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Utilities.Encryption;

namespace Utilities
{
    public class Captcha
    {
        public string Data
        {
            get;
            private set;
        }

        public string Token
        {
            get;
            private set;
        }

        private string _text;
        public Captcha(Color line, Color text, Color background)
        {
            CaptchaImage captcha = new CaptchaImage( line,  text,  background);
            _text = captcha.Text.ToUpper();
            Data = ImageGenarate(captcha);
            Encrypt();
        }

        private void Encrypt()
        {
            var now = DateTime.Now;
            var time = now.Ticks;

            Token = string.Format("{0}_{1}_{2}", time, Security.MD5Encrypt(_text + time.ToString()), _text);


            Token = System.Web.HttpUtility.UrlEncode(Security.TripleDESEncrypt(Security.MD5Encrypt(Environment.MachineName), Token.ToUpper()));

            var cache = MemoryCache.Default;

            cache.AddOrGetExisting(_text, true,  new CacheItemPolicy { AbsoluteExpiration = now.AddSeconds(60) });
        }

        public static int Verify(string plainText, string token)
        {
            var cache = MemoryCache.Default;

            if (cache.Get(plainText.ToUpper()) == null)
                return -1;

            cache.Remove(plainText.ToUpper());

            string decryptToken = Security.TripleDESDecrypt(Security.MD5Encrypt(Environment.MachineName), System.Web.HttpUtility.UrlDecode(token).Replace(" ", "+"));

            string [] splData = decryptToken.Split('_');

            long time = long.Parse(splData[0]);
            string text = splData[2];

            if(TimeSpan.FromTicks(DateTime.Now.Ticks - time).TotalSeconds > 60)
                return -1;//Experied captcha

            if (text.ToUpper() != plainText.ToUpper())
                return -2;

            return 1;
        }

        private string ImageGenarate(CaptchaImage captcha)
        {
            var codec = GetEncoderInfo("image/jpeg");
            // set image quality
            var eps = new EncoderParameters();
            eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)95);
            var ms = new MemoryStream();
            captcha.RenderImage().Save(ms, codec, eps);

            byte[] bitmapBytes = ms.GetBuffer();
            string result = Convert.ToBase64String(bitmapBytes, Base64FormattingOptions.InsertLineBreaks);

            ms.Close();
            ms.Dispose();
            GC.Collect();
            return result;
        }

        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            var myEncoders =
                ImageCodecInfo.GetImageEncoders();

            foreach (var myEncoder in myEncoders)
                if (myEncoder.MimeType == mimeType)
                    return myEncoder;
            return null;
        }
    }
}
