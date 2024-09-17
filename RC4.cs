using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AnonChat
{
    class RC4
    {
        private byte[] S = new byte[256];
        private int i = 0;
        private int j = 0;

        public RC4(List<byte> key)
        {
            for (int k = 0; k < 256; k++)
            {
                S[k] = (byte)k;
            }

            int j = 0;
            for (int k = 0; k < 256; k++)
            {
                j = (j + S[k] + key[k % key.Count]) % 256;
                Swap(k, j);
            }
        }

        private void Swap(int a, int b)
        {
            byte temp = S[a];
            S[a] = S[b];
            S[b] = temp;
        }

        public byte Generate()
        {
            i = (i + 1) % 256;
            j = (j + S[i]) % 256;
            Swap(i, j);
            int t = (S[i] + S[j]) % 256;
            return S[t];
        }

        public static string Crypt(byte[] data, string key)
        {
            RC4 rc4 = new RC4(Encoding.UTF8.GetBytes(key).ToList());
            byte[] cipherData = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                cipherData[i] = (byte)(data[i] ^ rc4.Generate());
            }
            return Encoding.UTF8.GetString(cipherData);
        }
        public static byte[] CryptBytes(byte[] data, string key)
        {
            RC4 rc4 = new RC4(Encoding.UTF8.GetBytes(key).ToList());
            byte[] cipherData = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                cipherData[i] = (byte)(data[i] ^ rc4.Generate());
            }
            return cipherData;
        }
        public static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }

        public static string getPass(string user1, string user2)
        {
            var sortedUsers = new List<string> { user1, user2 };
            sortedUsers.Sort();

            var combined = string.Join("", sortedUsers);
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(combined);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
