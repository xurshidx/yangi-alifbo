using System.Collections.Generic;
using System.Text;

namespace YangiAlifbo.Core
{
    public struct TextEdit
    {
        public int Start;
        public int Length;
        public string NewText;
    }

    public static class Converter
    {
        public const char Tutuq = 'ʼ';

        public static string ConvertWord(string word, Direction d)
        {
            if (string.IsNullOrEmpty(word)) return word;
            switch (d)
            {
                case Direction.CyrToLat: return CyrToLatRules.Word(word);
                case Direction.LatToCyr: return LatToCyrRules.Word(word);
                default: return OldToNewRules.Word(word);
            }
        }

        public static string ConvertText(string text, Direction d)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var tokens = Tokenizer.Words(text);
            var sb = new StringBuilder(text.Length + 16);
            int pos = 0;
            foreach (var t in tokens)
            {
                sb.Append(text, pos, t.Start - pos);
                sb.Append(ConvertWord(t.Text, d));
                pos = t.Start + t.Length;
            }
            sb.Append(text, pos, text.Length - pos);
            return sb.ToString();
        }

        /// <summary>
        /// Word maydon kodlari ichidagi tahrirlarni olib tashlaydi.
        /// Maydon: \u0013 kod \u0014 natija \u0015 (ichma-ich bo'lishi mumkin).
        /// Ochiq maydonlardan birortasi kod qismida bo'lsa — pozitsiya "kod ichida".
        /// </summary>
        public static List<TextEdit> ExcludeFieldCodes(string text, List<TextEdit> edits)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('\u0013') < 0) return edits;
            var inCode = new bool[text.Length];
            var stack = new List<bool>(); // true = kod, false = natija
            int codeDepth = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\u0013') { stack.Add(true); codeDepth++; }
                else if (c == '\u0014' && stack.Count > 0 && stack[stack.Count - 1]) { stack[stack.Count - 1] = false; codeDepth--; }
                else if (c == '\u0015' && stack.Count > 0)
                {
                    if (stack[stack.Count - 1]) codeDepth--;
                    stack.RemoveAt(stack.Count - 1);
                }
                inCode[i] = codeDepth > 0;
            }
            var result = new List<TextEdit>();
            foreach (var e in edits)
                if (!inCode[e.Start]) result.Add(e);
            return result;
        }

        /// <summary>Faqat o'zgargan so'zlar, Start kamayish tartibida (oxiridan boshiga qo'llash uchun).</summary>
        public static List<TextEdit> PlanEdits(string text, Direction d)
        {
            var edits = new List<TextEdit>();
            if (string.IsNullOrEmpty(text)) return edits;
            var tokens = Tokenizer.Words(text);
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                var t = tokens[i];
                string nw = ConvertWord(t.Text, d);
                if (nw == t.Text) continue;
                TextEdit e;
                e.Start = t.Start;
                e.Length = t.Length;
                e.NewText = nw;
                edits.Add(e);
            }
            return edits;
        }
    }
}
