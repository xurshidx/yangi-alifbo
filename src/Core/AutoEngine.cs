using System;
using System.Collections.Generic;
using System.Text;

namespace YangiAlifbo.Core
{
    public enum AutoKind { None, Replace, Undo }

    /// <summary>Klaviatura qobig'i bajaradigan ish: Backspaces marta o'chirish, keyin Text ni yozish, keyin tugatuvchi tugmani qayta yuborish.</summary>
    public struct AutoAction
    {
        public AutoKind Kind;
        public int Backspaces;
        public string Text;
        public static readonly AutoAction None = new AutoAction();
    }

    public enum Ender { Space, Enter, Tab }

    /// <summary>
    /// v3: yozayotganda eski lotin → yangi lotin (klaviaturasiz, sof mantiq — test qilinadi).
    /// Qobiq har bir bosilgan harfni Char(), bo'sh joy/Enter/Tab ni End(), Backspace ni Backspace(),
    /// kursor ko'chishi/sichqoncha/oyna almashishi/Ctrl-Alt birikmalarini Reset() bilan beradi.
    /// Bo'lak — oxirgi bo'sh joydan keyin yozilgan matn ("bog',", "o'zbek"). U tugaganda o'giriladi.
    /// </summary>
    public class AutoEngine
    {
        public const int MaxChunk = 60;

        readonly StringBuilder chunk = new StringBuilder();
        bool broken;          // bo'lakning boshi ko'rilmagan (Backspace bilan bo'lakdan chiqildi, juda uzun va h.k.)
        bool sawSpace;        // so'nggi Reset'dan keyin bo'sh joy bo'lganmi (parol/login himoyasi uchun)
        string lastOld, lastNew; // qaytarish uchun
        readonly HashSet<string> exceptions = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>Ichki istisnolar: ch/sh li chet (asosan inglizcha) so'zlar. c (h siz) yoki w li so'zlar yadroda o'zi chetlanadi.</summary>
        public static readonly string[] BuiltinExceptions =
        {
            "chat","chats","check","checked","checkout","change","chrome","chip","chips","chart","charts","channel","charge","chance",
            "champion","cheese","chicken","child","children","choice","chocolate","church","chef","chess","chill","chain","chair",
            "push","share","shared","show","shows","shop","shopping","shops","shopify","flash","fresh","english","cash","shell","ship",
            "shipping","shift","sheet","sheets","short","shorts","shoes","shot","shots","shine","shut","shy","she","shall","should",
            "march","search","match","much","such","touch","teach","rich","branch","tech","each","which","french","coach","catch",
            "fish","dish","smash","crash","brush","bush","rush","finish","polish","publish","english","hash","sushi","shake","shark",
            "shopper","marshal","marshall","bash","shareit","cheat","chase","cherry","chili","chrome","chromecast","cheap","cheers",
            "chips","chicago","michael","richard","charles","chelsea","manchester","sheriff","shield","shock","shower","shrimp"
        };

        static readonly HashSet<string> Builtin = new HashSet<string>(BuiltinExceptions, StringComparer.Ordinal);

        public Action<string> ExceptionAdded; // foydalanuvchi qaytargan so'z (kichik harfda) — qobiq faylga yozadi

        public void AddException(string word)
        {
            if (!string.IsNullOrEmpty(word)) exceptions.Add(word.ToLowerInvariant());
        }

        public bool IsException(string word)
        {
            string w = word.ToLowerInvariant();
            return exceptions.Contains(w) || Builtin.Contains(w);
        }

        public string Chunk { get { return chunk.ToString(); } }

        public void Reset()
        {
            chunk.Length = 0;
            broken = false;
            sawSpace = false;
            lastOld = lastNew = null;
        }

        public void Char(char c)
        {
            lastOld = lastNew = null;
            if (broken) return;
            if (chunk.Length >= MaxChunk) { broken = true; chunk.Length = 0; return; }
            chunk.Append(c);
        }

        /// <summary>Backspace: oxirgi almashtirishdan darhol keyin bo'lsa — qaytaradi va so'zlarni istisnoga qo'shadi.</summary>
        public AutoAction Backspace()
        {
            if (lastOld != null)
            {
                var a = new AutoAction { Kind = AutoKind.Undo, Backspaces = lastNew.Length + 1, Text = lastOld };
                foreach (var t in Tokenizer.Words(lastOld))
                {
                    string nw = Converter.ConvertWord(t.Text, Direction.OldToNew);
                    if (nw != t.Text)
                    {
                        string w = Bare(t.Text).ToLowerInvariant();
                        AddException(w);
                        if (ExceptionAdded != null) ExceptionAdded(w);
                    }
                }
                lastOld = lastNew = null;
                chunk.Length = 0;
                broken = true; // qaytarilgan bo'lak oldidagi matn noma'lum
                return a;
            }
            if (chunk.Length > 0) chunk.Length--;
            else broken = true; // oldingi bo'lak ichiga kirildi
            return AutoAction.None;
        }

        /// <summary>Bo'lak tugadi. Enter/Tab faqat oldin bo'sh joy bo'lgan bo'lsa o'giradi (bitta so'zli login/parolga tegmaslik uchun).</summary>
        public AutoAction End(Ender e)
        {
            string old = chunk.ToString();
            bool wasBroken = broken;
            chunk.Length = 0;
            broken = false;
            lastOld = lastNew = null;
            bool allowed = e == Ender.Space || sawSpace;
            // Enter/Tab dan keyin yangi qator yoki maydon — himoya qayta yoqiladi ("Ism Familiya" Tab "parol" Enter)
            sawSpace = e == Ender.Space;
            if (wasBroken || !allowed || old.Length == 0 || !LooksLikeText(old)) return AutoAction.None;

            string nw = ConvertChunk(old);
            if (nw == old) return AutoAction.None;
            lastOld = old;
            lastNew = nw;
            return new AutoAction { Kind = AutoKind.Replace, Backspaces = old.Length, Text = nw };
        }

        /// <summary>Manzil, pochta, fayl yo'li, raqamli kod yoki yangi/kirill harfli bo'laklarga tegilmaydi.</summary>
        public static bool LooksLikeText(string s)
        {
            bool hasLatin = false;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (char.IsDigit(c) || c == '@' || c == '/' || c == '\\' || c == '_' || c == '#' || c == '=' || c == '<' || c == '>' || c == '&' || c == '%') return false;
                if ((c == '.' || c == ':') && i + 1 < s.Length && char.IsLetter(s[i + 1])) return false; // dellcom.uz, http:x
                if (char.IsLetter(c))
                {
                    if (c > 0x024F) return false; // kirill va boshqa yozuvlar
                    if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) hasLatin = true;
                }
            }
            return hasLatin;
        }

        static string Bare(string w)
        {
            int n = w.Length;
            while (n > 0 && Tokenizer.IsApostrophe(w[n - 1])) n--;
            return w.Substring(0, n);
        }

        string ConvertChunk(string s)
        {
            var tokens = Tokenizer.Words(s);
            var sb = new StringBuilder(s.Length);
            int pos = 0;
            foreach (var t in tokens)
            {
                sb.Append(s, pos, t.Start - pos);
                sb.Append(IsException(Bare(t.Text)) ? t.Text : Converter.ConvertWord(t.Text, Direction.OldToNew));
                pos = t.Start + t.Length;
            }
            sb.Append(s, pos, s.Length - pos);
            return sb.ToString();
        }
    }
}
