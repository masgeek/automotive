using System;
using System.Security.Cryptography;
using System.Text;

namespace MessagingLib
{
    public class VerificationCode
    {
        /// <summary>
        /// Randomly generate verification codes
        /// for use in SMS and email verifications
        /// </summary>
        /// <param name="min">minimum value</param>
        /// <param name="max">maximum value</param>
        /// <returns>string</returns>
        public static string GenerateCode(int min = 1000, int max = 99999)
        {
            Random rnd = new Random(DateTime.Now.Millisecond); //used current time miliseconds for generation

            int rndNo = rnd.Next(min, max);

            var stringKey = GetUniqueKey(4);

            var code = stringKey + rndNo;

            return code.Substring(0, 6);
        }

        private static string GetUniqueKey(int maxSize = 5)
        {
            var chars = "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b%(chars.Length)]);
            }
            return result.ToString();
        }
    }
}
