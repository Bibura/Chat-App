using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Chat_Server.Net.IO
{
    internal class PacketBuilder
    {
        MemoryStream _ms;
        CaesarCipher _cipher;

        private ECDiffieHellmanCng serverECDH;
        public byte[] PublicKey { get; private set; }
        private byte[] sharedKey;

        public PacketBuilder()
        {
            _ms = new MemoryStream();
            _cipher = new CaesarCipher();

            serverECDH = new ECDiffieHellmanCng();
            serverECDH.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            serverECDH.HashAlgorithm = CngAlgorithm.Sha256;
            PublicKey = serverECDH.PublicKey.ToByteArray();
        }

        public void GenerateSharedKey(byte[] clientPublicKey)
        {
            using (ECDiffieHellmanCng clientECDH = new ECDiffieHellmanCng(CngKey.Import(clientPublicKey, CngKeyBlobFormat.EccPublicBlob)))
            {
                sharedKey = serverECDH.DeriveKeyMaterial(clientECDH.PublicKey);
            }
        }

        public string EncryptData(string message)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = sharedKey;
                aes.GenerateIV();
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(message);
                        }
                    }
                    byte[] encryptedMessage = ms.ToArray();
                    byte[] result = new byte[aes.IV.Length + encryptedMessage.Length];
                    Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
                    Buffer.BlockCopy(encryptedMessage, 0, result, aes.IV.Length, encryptedMessage.Length);
                    return result.ToString();
                }
            }
        }

        public void WriteOpCode(byte opcode)
        {
            _ms.WriteByte(opcode);
        }

        public void WriteMessage(string msg)
        {
            var msgLenght = msg.Length;
            _ms.Write(BitConverter.GetBytes(msgLenght));

            //ENCRYPTION(SERVER SIDE)
            _ms.Write(Encoding.ASCII.GetBytes(EncryptData(msg)));
        }

        public byte[] GetPacketBytes()
        {
            return _ms.ToArray();
        }
    }
}
