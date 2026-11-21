using System;
using System.Collections.Generic;
using System.Text;
using YangiAlifbo.Core;

namespace YangiAlifbo.Tests
{
    public static class CoreTests
    {
        static int fails = 0;
        static int passes = 0;

        static void Eq(string expected, string actual, string name)
        {
            if (expected == actual) { passes++; return; }
            fails++;
            Console.WriteLine("FAIL " + name + ": kutilgan [" + expected + "], olindi [" + actual + "]");
        }

        static string Join(List<TextEdit> edits)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < edits.Count; i++)
            {
                if (i > 0) sb.Append(';');
                sb.Append(edits[i].Start).Append('|').Append(edits[i].Length).Append('|').Append(edits[i].NewText);
            }
            return sb.ToString();
        }

        static void Conv(Direction d, string input, string expected)
        {
            Eq(expected, Converter.ConvertText(input, d), d + ": " + input);
        }

        public static int Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Task 1: tokenizer + fasad
            Eq("3", Tokenizer.Words("Salom, dunyo!  o'zbek").Count.ToString(), "tok_count");
            Eq("o'zbek", Tokenizer.Words("Salom, dunyo!  o'zbek")[2].Text, "tok_apos_inside");
            Eq("15", Tokenizer.Words("Salom, dunyo!  o'zbek")[2].Start.ToString(), "tok_start");
            Eq("1", Tokenizer.Words("'salom'").Count.ToString(), "tok_edge_apos_excluded");
            Eq("salom", Tokenizer.Words("'salom'")[0].Text, "tok_edge_apos_text");
            Eq("0", Tokenizer.Words("").Count.ToString(), "tok_empty");
            Eq("7|4|dunyo;0|5|Salom", Join(Converter.PlanEdits("Салом, дунё!", Direction.CyrToLat)), "plan_desc_order");
            Eq("", Join(Converter.PlanEdits("123 ...", Direction.CyrToLat)), "plan_no_words");

            // Task 2: eski lotin -> yangi lotin
            var o = Direction.OldToNew;
            Conv(o, "O'zbekiston", "Özbekiston");
            Conv(o, "g'isht", "ğişt");
            Conv(o, "Oʻgʻil", "Öğil");
            Conv(o, "o‘g’il", "öğil");
            Conv(o, "o`rta", "örta");
            Conv(o, "oʼq", "öq");
            Conv(o, "G'ALABA", "ĞALABA");
            Conv(o, "shahar", "şahar");
            Conv(o, "Shahar", "Şahar");
            Conv(o, "SHAHAR", "ŞAHAR");
            Conv(o, "choy", "çoy");
            Conv(o, "CHOY", "ÇOY");
            Conv(o, "ma'no", "maʼno");
            Conv(o, "Is'hoq", "Isʼhoq");
            Conv(o, "tong", "tong");
            Conv(o, "Microsoft", "Microsoft");
            Conv(o, "Windows", "Windows");
            Conv(o, "Men o'zbekman, sen-chi?", "Men özbekman, sen-çi?");
            Conv(o, "Özbekiston şahar", "Özbekiston şahar");

            // Task 3: kirill -> yangi lotin
            var k = Direction.CyrToLat;
            Conv(k, "Ўзбекистон", "Özbekiston");
            Conv(k, "шаҳар", "şahar");
            Conv(k, "чой", "çoy");
            Conv(k, "ғишт", "ğişt");
            Conv(k, "қўл", "qöl");
            Conv(k, "хат", "xat");
            Conv(k, "ҳа", "ha");
            Conv(k, "Ер", "Yer");
            Conv(k, "поезд", "poyezd");
            Conv(k, "мева", "meva");
            Conv(k, "съезд", "syezd");
            Conv(k, "цирк", "sirk");
            Conv(k, "лицей", "litsey");
            Conv(k, "маъно", "maʼno");
            Conv(k, "актёр", "aktyor");
            Conv(k, "компьютер", "kompyuter");
            Conv(k, "Юсуф", "Yusuf");
            Conv(k, "ЯНГИ", "YANGI");
            Conv(k, "ШАҲАР", "ŞAHAR");
            Conv(k, "ЁШЛИК", "YOŞLIK");
            Conv(k, "Ёшлик", "Yoşlik");
            Conv(k, "Я", "Ya");
            Conv(k, "Салом, дунё!", "Salom, dunyo!");
            Conv(k, "Windows 10", "Windows 10");
            Conv(k, "щётка", "şyotka");
            Conv(k, "эълон", "eʼlon");

            // Task 4: lotin -> kirill
            var l = Direction.LatToCyr;
            Conv(l, "Özbekiston", "Ўзбекистон");
            Conv(l, "O'zbekiston", "Ўзбекистон");
            Conv(l, "Oʻzbekiston", "Ўзбекистон");
            Conv(l, "şahar", "шаҳар");
            Conv(l, "shahar", "шаҳар");
            Conv(l, "SHAHAR", "ШАҲАР");
            Conv(l, "ŞAHAR", "ШАҲАР");
            Conv(l, "çoy", "чой");
            Conv(l, "choy", "чой");
            Conv(l, "ğişt", "ғишт");
            Conv(l, "g'isht", "ғишт");
            Conv(l, "ekran", "экран");
            Conv(l, "meva", "мева");
            Conv(l, "yer", "ер");
            Conv(l, "poyezd", "поезд");
            Conv(l, "yoşlik", "ёшлик");
            Conv(l, "YOŞLIK", "ЁШЛИК");
            Conv(l, "Yusuf", "Юсуф");
            Conv(l, "maʼno", "маъно");
            Conv(l, "ma'no", "маъно");
            Conv(l, "Is'hoq", "Исҳоқ");
            Conv(l, "Isʼhoq", "Исҳоқ");
            Conv(l, "ketsa", "кетса");
            Conv(l, "litseyda", "лицейда");
            Conv(l, "Kompyuter", "Компьютер");
            Conv(l, "KONSERT", "КОНЦЕРТ");
            Conv(l, "Microsoft", "Microsoft");
            Conv(l, "Salom, dunyo!", "Салом, дунё!");
            Conv(l, "Ўзбек", "Ўзбек");

            Console.WriteLine("OK: " + passes + ", FAIL: " + fails);
            return fails == 0 ? 0 : 1;
        }
    }
}
