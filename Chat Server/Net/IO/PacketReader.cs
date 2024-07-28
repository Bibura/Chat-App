using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Chat_Server.Net.IO
{
    internal class PacketReader : BinaryReader
    {
        private NetworkStream _ns;
        CaesarCipher _cipher;

        private byte[] sharedKey;
        public PacketReader(NetworkStream ns, byte[] sharedKey) : base(ns)
        {
            _ns = ns;
            _cipher = new CaesarCipher();
            this.sharedKey = sharedKey;
        }

        public string DecryptData(byte[] encryptedData)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = sharedKey;
                byte[] iv = new byte[aes.BlockSize / 8];
                byte[] cipherText = new byte[encryptedData.Length - iv.Length];

                Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(encryptedData, iv.Length, cipherText, 0, cipherText.Length);

                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(cipherText))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }

        public string readMessage()
        {
            byte[] msgBuffer;
            var lenght = ReadInt32();
            msgBuffer = new byte[lenght];
            _ns.Read(msgBuffer, 0, lenght);

            char[] msg = Encoding.ASCII.GetString(msgBuffer).ToCharArray();


            //DECRYPTION (CLIENT SIDE)
            string message = DecryptData(Encoding.UTF8.GetBytes(msg));
            return message;
        }
    }
}
