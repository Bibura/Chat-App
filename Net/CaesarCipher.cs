using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat_App.Net
{
    internal class CaesarCipher
    {
        public string Encrypt(string text, int shift)
        {
            char[] buffer = text.ToCharArray();
            for (int i = 0; i < buffer.Length; i++)
            {
                char letter = buffer[i];
                // Зміщуємо тільки літери
                if (char.IsLetter(letter))
                {
                    // Обробляємо заголовні і малі літери окремо
                    char letterOffset = char.IsUpper(letter) ? 'A' : 'a';
                    letter = (char)(((letter + shift - letterOffset) % 26) + letterOffset);
                    buffer[i] = letter;
                }
            }
            return new string(buffer);
        }
        public string Decrypt(string text, int shift)
        {
            // Для дешифрування зміщення повинно бути негативним
            return Encrypt(text, 26 - shift);
        }
    }
}
