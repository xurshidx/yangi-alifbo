using System.Collections.Generic;

namespace YangiAlifbo.Core
{
    public struct Token
    {
        public int Start;
        public int Length;
        public string Text;
    }

    public static class Tokenizer
    {
        /// <summary>Apostrof/tutuq variantlari: ' ` ʻ ʼ ‘ ’</summary>
        public static bool IsApostrophe(char c)
        {
            return c == '\'' || c == '`' || c == 'ʻ' || c == 'ʼ' || c == '‘' || c == '’';
        }

        static bool IsOG(char c)
        {
            return c == 'o' || c == 'O' || c == 'g' || c == 'G';
        }

        /// <summary>
        /// Matnni so'zlarga ajratadi: harflar ketma-ketligi; apostrof faqat
        /// ikki harf orasida bo'lsa so'z ichida hisoblanadi.
        /// </summary>
        public static List<Token> Words(string text)
        {
            var result = new List<Token>();
            if (string.IsNullOrEmpty(text)) return result;
            int n = text.Length;
            int i = 0;
            while (i < n)
            {
                if (!char.IsLetter(text[i])) { i++; continue; }
                int start = i;
                i++;
                while (i < n)
                {
                    if (char.IsLetter(text[i])) { i++; continue; }
                    if (IsApostrophe(text[i]) && i + 1 < n && char.IsLetter(text[i + 1])) { i += 2; continue; }
                    // v3.0.0: so'z oxiridagi o' / g' (tog', bog', o') — apostrof so'zga kiradi
                    if (IsApostrophe(text[i]) && IsOG(text[i - 1]) && (i + 1 >= n || !char.IsLetter(text[i + 1]))) { i++; }
                    break;
                }
                Token t;
                t.Start = start;
                t.Length = i - start;
                t.Text = text.Substring(start, i - start);
                result.Add(t);
            }
            return result;
        }
    }
}
