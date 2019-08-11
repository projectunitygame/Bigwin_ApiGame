using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Utilities.Encryption;
using Utilities.Log;

namespace OTP
{
    public class OTPApp
    {
        private static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static bool ValidateOTP(string secret, string otp)
        {
            long counter = GetCurrentCounter();
            NLogManager.LogMessage("OTP key 1: " + secret);
            string key = Security.MD5Encrypt(secret);
            NLogManager.LogMessage("OTP key: " + key);
            key = key.Replace('0', '2');
            key = key.Replace('1', '2');
            key = key.Replace('8', '2');
            key = key.Replace('9', '2');

            for (long i = counter - 1; i <= counter; i++)
                if (GeneratePassword(key, i) == otp)
                    return true;

            return false;
        }

        public static string GetTimeOTP(string secret)
        {
            long counter = GetCurrentCounter();
            string key = Security.MD5Encrypt(secret);
            key = key.Replace('0', '2');
            key = key.Replace('1', '2');
            key = key.Replace('8', '2');
            key = key.Replace('9', '2');
            return GeneratePassword(key, counter);
        }

        private static string GeneratePassword(string secret, long iterationNumber, int digits = 6)
        {
            byte[] counter = BitConverter.GetBytes(iterationNumber);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(counter);

            byte[] key = Base32.ToByteArray(secret);

            HMACSHA1 hmac = new HMACSHA1(key, true);

            byte[] hash = hmac.ComputeHash(counter);

            int offset = hash[hash.Length - 1] & 0xf;

            int binary =
                ((hash[offset] & 0x7f) << 24)
                | ((hash[offset + 1] & 0xff) << 16)
                | ((hash[offset + 2] & 0xff) << 8)
                | (hash[offset + 3] & 0xff);

            int password = binary % (int)Math.Pow(10, digits); // 6 digits

            return password.ToString(new string('0', digits));
        }


        public static long GetCurrentCounter()
        {
            return GetCurrentCounter(DateTime.UtcNow, UNIX_EPOCH, 120);
        }

        private static long GetCurrentCounter(DateTime now, DateTime epoch, int timeStep)
        {
            return (long)(Math.Floor(Math.Round((now - epoch).TotalMilliseconds / 1000.0) / timeStep));
        }
    }


    internal static class Base32
    {
        private static readonly char[] DIGITS;
        private static readonly int MASK;
        private static readonly int SHIFT;
        private static Dictionary<char, int> CHAR_MAP = new Dictionary<char, int>();
        private const string SEPARATOR = "-";

        static Base32()
        {
            DIGITS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();
            MASK = DIGITS.Length - 1;
            SHIFT = numberOfTrailingZeros(DIGITS.Length);
            for (int i = 0; i < DIGITS.Length; i++) CHAR_MAP[DIGITS[i]] = i;
        }

        private static int numberOfTrailingZeros(int i)
        {
            // HD, Figure 5-14
            int y;
            if (i == 0) return 32;
            int n = 31;
            y = i << 16; if (y != 0) { n = n - 16; i = y; }
            y = i << 8; if (y != 0) { n = n - 8; i = y; }
            y = i << 4; if (y != 0) { n = n - 4; i = y; }
            y = i << 2; if (y != 0) { n = n - 2; i = y; }
            return n - (int)((uint)(i << 1) >> 31);
        }

        /// <summary>
        /// Convert a given base32 string into an array of bytes
        /// </summary>
        internal static byte[] ToByteArray(string input)
        {
            input = input.TrimEnd('=');
            int numBytes = input.Length * 5 / 8;
            byte[] result = new byte[numBytes];

            byte curByte = 0, bitsRemaining = 8;
            int mask = 0;
            int arrayIndex = 0;

            foreach (char c in input)
            {
                int ascii = CharToInt(c);

                if (bitsRemaining > 5)
                {
                    mask = ascii << (bitsRemaining - 5);
                    curByte = (byte)(curByte | mask);
                    bitsRemaining -= 5;
                }
                else
                {
                    mask = ascii >> (5 - bitsRemaining);
                    curByte = (byte)(curByte | mask);
                    result[arrayIndex++] = curByte;
                    curByte = (byte)(ascii << (3 + bitsRemaining));
                    bitsRemaining += 3;
                }
            }

            if (arrayIndex != numBytes)
            {
                result[arrayIndex] = curByte;
            }

            return result;
        }
        public static string Encode(string str, bool padOutput = false)
        {
            byte[] data = Encoding.ASCII.GetBytes(str);
            if (data.Length == 0)
            {
                return "";
            }

            // SHIFT is the number of bits per output character, so the length of the
            // output is the length of the input multiplied by 8/SHIFT, rounded up.
            if (data.Length >= (1 << 28))
            {
                // The computation below will fail, so don't do it.
                throw new ArgumentOutOfRangeException("data");
            }

            int outputLength = (data.Length * 8 + SHIFT - 1) / SHIFT;
            StringBuilder result = new StringBuilder(outputLength);

            int buffer = data[0];
            int next = 1;
            int bitsLeft = 8;
            while (bitsLeft > 0 || next < data.Length)
            {
                if (bitsLeft < SHIFT)
                {
                    if (next < data.Length)
                    {
                        buffer <<= 8;
                        buffer |= (data[next++] & 0xff);
                        bitsLeft += 8;
                    }
                    else
                    {
                        int pad = SHIFT - bitsLeft;
                        buffer <<= pad;
                        bitsLeft += pad;
                    }
                }
                int index = MASK & (buffer >> (bitsLeft - SHIFT));
                bitsLeft -= SHIFT;
                result.Append(DIGITS[index]);
            }
            if (padOutput)
            {
                int padding = 8 - (result.Length % 8);
                if (padding > 0) result.Append(new string('=', padding == 8 ? 0 : padding));
            }
            return result.ToString();
        }

        // Helper - convert a base32 character into an int
        private static int CharToInt(char c)
        {
            int ascii = c;

            // upper case letters
            if (ascii < 91 && ascii > 64)
            {
                return ascii - 65;
            }

            // lower case letters
            if (ascii < 123 && ascii > 96)
            {
                return ascii - 97;
            }

            // digits 2 through 7
            if (ascii < 56 && ascii > 49)
            {
                return ascii - 24;
            }

            throw new ArgumentException(string.Format("Invalid base32 character {0}", c));
        }
    }
}
