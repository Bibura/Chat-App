using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Chat_App.Net.IO
{
    class PacketBuilder
    {
        MemoryStream _ms;
        CaesarCipher _cipher;
        private ECDiffieHellmanCng clientECDH;
        public byte[] _publicKey { get; private set; }
        private byte[] _sharedKey;

        public PacketBuilder() { 
            _ms = new MemoryStream();
            _cipher = new CaesarCipher();
            clientECDH = new ECDiffieHellmanCng();
            clientECDH.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            clientECDH.HashAlgorithm = CngAlgorithm.Sha256;
            _publicKey = clientECDH.PublicKey.ExportSubjectPublicKeyInfo();
        }

        public void GenerateSharedKey(byte[] publicKey)
        {
            using(ECDiffieHellmanCng serverECDH = new ECDiffieHellmanCng(CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob)))
            {
                _sharedKey = clientECDH.DeriveKeyMaterial(serverECDH.PublicKey);
            }
        }

        public string EncryptData(string message)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = _sharedKey;
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

            //ENCRYPTION (CLIENT SIDE)
            _ms.Write(Encoding.ASCII.GetBytes(EncryptData(msg)));
        }

        public byte[] GetPacketBytes()
        {
            return _ms.ToArray();
        }
    }
}
