using System.Text;

namespace YangiAlifbo.Core
{
    internal static class LatinUtil
    {
        /// <summary>c (h siz) yoki w bo'lsa — chet so'z (Microsoft, Windows).</summary>
        public static bool IsForeign(string w)
        {
            for (int i = 0; i < w.Length; i++)
            {
                char c = char.ToLowerInvariant(w[i]);
                if (c == 'w') return true;
                if (c == 'c' && (i + 1 >= w.Length || char.ToLowerInvariant(w[i + 1]) != 'h')) return true;
            }
            return false;
        }

        /// <summary>Barcha harflar katta va harflar soni >= 2.</summary>
        public static bool IsAllUpper(string w)
        {
            int letters = 0;
            foreach (char c in w)
            {
                if (!char.IsLetter(c)) continue;
                if (char.IsLower(c)) return false;
                letters++;
            }
            return letters >= 2;
        }
    }

    internal static class OldToNewRules
    {
        public static string Word(string w)
        {
            if (LatinUtil.IsForeign(w)) return w;
            var sb = new StringBuilder(w.Length);
            int n = w.Length;
            for (int i = 0; i < n; i++)
            {
                char c = w[i];
                char lc = char.ToLowerInvariant(c);
                bool upper = char.IsUpper(c);
                bool nextApos = i + 1 < n && Tokenizer.IsApostrophe(w[i + 1]);

                if (lc == 's' && nextApos && i + 2 < n && char.ToLowerInvariant(w[i + 2]) == 'h')
                {
                    sb.Append(c).Append(Converter.Tutuq);
                    i++;
                    continue;
                }
                if ((lc == 'o' || lc == 'g') && nextApos)
                {
                    char r = lc == 'o' ? 'ö' : 'ğ';
                    sb.Append(upper ? char.ToUpperInvariant(r) : r);
                    i++;
                    continue;
                }
                if ((lc == 's' || lc == 'c') && i + 1 < n && char.ToLowerInvariant(w[i + 1]) == 'h')
                {
                    char r = lc == 's' ? 'ş' : 'ç';
                    sb.Append(upper ? char.ToUpperInvariant(r) : r);
                    i++;
                    continue;
                }
                if (Tokenizer.IsApostrophe(c))
                {
                    sb.Append(Converter.Tutuq);
                    continue;
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
