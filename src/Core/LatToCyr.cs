using System.Collections.Generic;
using System.Text;

namespace YangiAlifbo.Core
{
    internal static class LatToCyrRules
    {
        /// <summary>
        /// Istisnolar: lotin so'z boshi -> kirill. '$' bilan tugasa — faqat to'liq so'z.
        /// </summary>
        internal static readonly string[,] Exceptions =
        {
            {"litsey","лицей"}, {"sirk$","цирк"}, {"konsert","концерт"}, {"sentr","центр"},
            {"tsex","цех"}, {"prinsip","принцип"}, {"aksiya","акция"}, {"stansiya","станция"},
            {"kompyuter","компьютер"}, {"oktyabr","октябрь"}, {"sentyabr","сентябрь"},
            {"noyabr","ноябрь"}, {"dekabr","декабрь"}, {"fevral","февраль"}
        };

        static readonly Dictionary<char, char> Map = new Dictionary<char, char>
        {
            {'a','а'},{'b','б'},{'d','д'},{'f','ф'},{'g','г'},{'h','ҳ'},{'i','и'},{'j','ж'},
            {'k','к'},{'l','л'},{'m','м'},{'n','н'},{'o','о'},{'p','п'},{'q','қ'},{'r','р'},
            {'s','с'},{'t','т'},{'u','у'},{'v','в'},{'x','х'},{'y','й'},{'z','з'},
            {'ş','ш'},{'ç','ч'},{'ğ','ғ'},{'ö','ў'}
        };

        const string LatVowels = "aeiouö";

        public static string Word(string w)
        {
            if (LatinUtil.IsForeign(w)) return w;
            bool allUpper = LatinUtil.IsAllUpper(w);
            bool firstUpper = w.Length > 0 && char.IsUpper(w[0]);
            string lower = w.ToLowerInvariant();

            for (int e = 0; e < Exceptions.GetLength(0); e++)
            {
                string key = Exceptions[e, 0];
                bool exact = key.EndsWith("$");
                if (exact) key = key.Substring(0, key.Length - 1);
                if (exact ? lower != key : !lower.StartsWith(key)) continue;
                string cyr = Exceptions[e, 1] + Scan(w.Substring(key.Length), key[key.Length - 1], allUpper);
                if (allUpper) return cyr.ToUpperInvariant();
                if (firstUpper) return char.ToUpperInvariant(cyr[0]) + cyr.Substring(1);
                return cyr;
            }
            return Scan(w, '\0', allUpper);
        }

        static void Put(StringBuilder sb, char cyr, bool upper)
        {
            sb.Append(upper ? char.ToUpperInvariant(cyr) : cyr);
        }

        /// <param name="prevLatin">Oldingi lotin harf (kichik), so'z boshi bo'lsa '\0'.</param>
        static string Scan(string w, char prevLatin, bool allUpper)
        {
            var sb = new StringBuilder(w.Length);
            int n = w.Length;
            char prev = prevLatin;
            for (int i = 0; i < n; i++)
            {
                char c = w[i];
                char lc = char.ToLowerInvariant(c);
                bool up = char.IsUpper(c);
                char n1 = i + 1 < n ? char.ToLowerInvariant(w[i + 1]) : '\0';
                bool aposNext = i + 1 < n && Tokenizer.IsApostrophe(w[i + 1]);

                if (lc == 's' && aposNext && i + 2 < n && char.ToLowerInvariant(w[i + 2]) == 'h')
                {
                    Put(sb, 'с', up); Put(sb, 'ҳ', allUpper);
                    i += 2; prev = 'h'; continue;
                }
                if ((lc == 'o' || lc == 'g') && aposNext)
                {
                    Put(sb, lc == 'o' ? 'ў' : 'ғ', up);
                    i++; prev = lc == 'o' ? 'ö' : 'ğ'; continue;
                }
                if ((lc == 's' || lc == 'c') && n1 == 'h')
                {
                    Put(sb, lc == 's' ? 'ш' : 'ч', up);
                    i++; prev = 'h'; continue;
                }
                if (lc == 'y' && (n1 == 'o' || n1 == 'u' || n1 == 'a' || n1 == 'e'))
                {
                    char r = n1 == 'o' ? 'ё' : n1 == 'u' ? 'ю' : n1 == 'a' ? 'я' : 'е';
                    Put(sb, r, up);
                    i++; prev = n1; continue;
                }
                if (lc == 'e')
                {
                    bool start = prev == '\0' || LatVowels.IndexOf(prev) >= 0;
                    Put(sb, start ? 'э' : 'е', up);
                    prev = lc; continue;
                }
                if (Tokenizer.IsApostrophe(c))
                {
                    Put(sb, 'ъ', allUpper);
                    prev = '\''; continue;
                }
                char m;
                if (Map.TryGetValue(lc, out m)) { Put(sb, m, up); prev = lc; continue; }
                sb.Append(c);
                prev = lc;
            }
            return sb.ToString();
        }
    }
}
