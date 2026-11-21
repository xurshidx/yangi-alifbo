using System.Collections.Generic;
using System.Text;

namespace YangiAlifbo.Core
{
    internal static class CyrToLatRules
    {
        static readonly Dictionary<char, string> Map = new Dictionary<char, string>
        {
            {'а',"a"},{'б',"b"},{'в',"v"},{'г',"g"},{'д',"d"},{'ж',"j"},{'з',"z"},{'и',"i"},
            {'й',"y"},{'к',"k"},{'л',"l"},{'м',"m"},{'н',"n"},{'о',"o"},{'п',"p"},{'р',"r"},
            {'с',"s"},{'т',"t"},{'у',"u"},{'ф',"f"},{'х',"x"},{'ш',"ş"},{'ч',"ç"},{'щ',"ş"},
            {'ў',"ö"},{'қ',"q"},{'ғ',"ğ"},{'ҳ',"h"},{'ы',"i"},{'э',"e"},
            {'ё',"yo"},{'ю',"yu"},{'я',"ya"}
        };

        const string Vowels = "аеёиоуэюяў";

        static bool IsVowel(char c) { return Vowels.IndexOf(char.ToLowerInvariant(c)) >= 0; }

        public static string Word(string w)
        {
            bool allUpper = LatinUtil.IsAllUpper(w);
            var sb = new StringBuilder(w.Length + 4);
            for (int i = 0; i < w.Length; i++)
            {
                char c = w[i];
                char lc = char.ToLowerInvariant(c);
                char prev = i > 0 ? char.ToLowerInvariant(w[i - 1]) : '\0';
                string r;

                if (lc == 'е')
                    r = (i == 0 || IsVowel(prev) || prev == 'ъ' || prev == 'ь') ? "ye" : "e";
                else if (lc == 'ц')
                    r = (i > 0 && IsVowel(prev)) ? "ts" : "s";
                else if (lc == 'ь')
                    continue;
                else if (lc == 'ъ')
                {
                    char next = i + 1 < w.Length ? char.ToLowerInvariant(w[i + 1]) : '\0';
                    if ("еёюя".IndexOf(next) >= 0) continue;
                    sb.Append(Converter.Tutuq);
                    continue;
                }
                else if (!Map.TryGetValue(lc, out r))
                {
                    sb.Append(c); // kirill bo'lmagan harf o'zgarmaydi
                    continue;
                }

                if (char.IsUpper(c))
                {
                    if (r.Length == 1 || allUpper) r = r.ToUpperInvariant();
                    else r = char.ToUpperInvariant(r[0]) + r.Substring(1);
                }
                sb.Append(r);
            }
            return sb.ToString();
        }
    }
}
