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

            // Task 6 yordamchisi: Word maydon kodlari (\u0013 kod \u0014 natija \u0015) o'zgarmaydi
            string fld = "сайт \u0013 HYPERLINK \"https://dellcom.uz\" \u0014сайт\u0015 ва";
            Eq("45|2|va;39|4|sayt;0|4|sayt", Join(Converter.ExcludeFieldCodes(fld, Converter.PlanEdits(fld, Direction.CyrToLat))), "field_codes_cyr");
            Eq("26|2|ва;20|4|сайт;0|4|сайт", Join(Converter.ExcludeFieldCodes("sayt \u0013 HYPERLINK x \u0014sayt\u0015 va", Converter.PlanEdits("sayt \u0013 HYPERLINK x \u0014sayt\u0015 va", Direction.LatToCyr))), "field_codes_lat");
            Eq("", Join(Converter.ExcludeFieldCodes("\u0013 PAGE \u0013 SET a \u0014\u0015 \u0014", Converter.PlanEdits("\u0013 PAGE \u0013 SET a \u0014\u0015 \u0014", Direction.LatToCyr))), "field_codes_nested");

            // v2.0.0: litsenziya yadrosi (sinov vektori Node crypto bilan imzolangan — server bilan bir xil)
            string pub = "<RSAKeyValue><Modulus>0snBqrTYSvVl682b1fLm7ou88qBv0qwzYkWgZZIP76M9TdDI3Fq19p48O9kRoheu+jABsVRj9hYG4l528LtGwIFLzKV1QflwuKymMONsg59grrNMM8aGBdte24vcsBJlNgoWY2dDLt3PVrmKDgj1OnUSldRfb/DQyIueM0AosEvLnV5adrqFJW6cNoOJUspdfLvJEk665FHkzLPjwIpcpE948Y5F3VLpn2JybUZsTt6nc9nmH3XgKulVP3EZcOhEjmssunA+gtodtRkMBHvk/YebMo0CTtWLUv8p3EP6uZGCaK4mzvA/FMs0P46hB/tGLkbKluQEYGbk+Z7q5l+W6Q==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            string pay = "YA1|YA-ABCD-EFGH-JKMN|d1bfaf4aff653cb27984b7d978e51a7d|perpetual|2026-11-22T09:30:00.000Z";
            string sig = "AWpl3ppMYOQz7xObjrHz2FEgsLw+Iyk6IHUPp8hnu91AHrntwrb/h7ZNNWfD1sVjLbmSf5NyCBQXntqyB5DmrpO9wMG+2UDvYobQ3ipS5TBqHwZ2cIl5wnKSBRndhMfggfJeBjf/pRYgv0vy116bH1qOuTFWYhrl8VgLz/4y0fSp0QaVdyuicdaGTE9CyuNMVVcRJMkfBrfcP6DKy1OYs3MPuHlxggkj0a1A39uWzjzCCBtPwlCZXbabyY26KVaOH9B+OV4zsX08PxRnYYCJKFG49kGbGYj1j4tnzEkd7EWP6WSpSkVewDHaPCKNq79gCK6PR/8n4rwIVpDQVHF6CQ==";
            Eq("d1bfaf4aff653cb27984b7d978e51a7d", LicenseCore.HwidFrom("3F2504E0-4F89-11D3-9A0C-0305E82C3301"), "hwid_from_guid");
            Eq("True", LicenseCore.Verify(pay, sig, pub).ToString(), "lic_verify_ok");
            Eq("False", LicenseCore.Verify(pay.Replace("ABCD", "ABCE"), sig, pub).ToString(), "lic_verify_tampered");
            Eq("False", LicenseCore.Verify(pay, "AAAA" + sig.Substring(4), pub).ToString(), "lic_verify_badsig");
            Eq("False", LicenseCore.Verify(pay, "bu base64 emas!", pub).ToString(), "lic_verify_garbage");
            string lk, lh;
            Eq("True", LicenseCore.ParsePayload(pay, out lk, out lh).ToString(), "lic_parse");
            Eq("YA-ABCD-EFGH-JKMN|d1bfaf4aff653cb27984b7d978e51a7d", lk + "|" + lh, "lic_parse_values");
            Eq("False", LicenseCore.ParsePayload("YA2|a|b|perpetual|x", out lk, out lh).ToString(), "lic_parse_version");
            Eq("True", LicenseCore.IsValidFor(pay, sig, pub, "d1bfaf4aff653cb27984b7d978e51a7d").ToString(), "lic_valid_for_pc");
            Eq("False", LicenseCore.IsValidFor(pay, sig, pub, "00000000000000000000000000000000").ToString(), "lic_other_pc");
            var t0 = new DateTime(2026, 11, 22, 10, 0, 0, DateTimeKind.Utc);
            Eq("7", LicenseCore.TrialDaysLeft(t0, t0).ToString(), "trial_7");
            Eq("1", LicenseCore.TrialDaysLeft(t0, t0.AddDays(6.5)).ToString(), "trial_1");
            Eq("0", LicenseCore.TrialDaysLeft(t0, t0.AddDays(7.1)).ToString(), "trial_0");
            Eq("7", LicenseCore.TrialDaysLeft(t0, t0.AddDays(-3)).ToString(), "trial_clock_back");
            Eq("YA-ABCD-EFGH-JKMN", LicenseCore.NormKey(" ya abcd-efgh jkmn "), "normkey");
            Eq("", LicenseCore.NormKey("YA-ABCD-EFGH-JKM0"), "normkey_bad");
            var js = MiniJson.Parse("{\"status\":\"active\",\"price\":15000,\"ok\":true,\"n\":null,\"license\":{\"payload\":\"a|b\\u00f6\",\"sig\":\"x\"},\"arr\":[1,\"s\"],\"msg\":\"Kalit \\\"topilmadi\\\"\\n\"}") as System.Collections.Generic.Dictionary<string, object>;
            Eq("active|15000|True||a|b\u00f6|2|Kalit \"topilmadi\"\n", js["status"] + "|" + js["price"] + "|" + js["ok"] + "|" + js["n"] + "|" + ((System.Collections.Generic.Dictionary<string, object>)js["license"])["payload"] + "|" + ((System.Collections.Generic.List<object>)js["arr"]).Count + "|" + js["msg"], "json_parse");
            Eq("{\"key\":\"a\\\"b\",\"n\":\"\\u00f6\\n\"}", MiniJson.Obj("key", "a\"b", "n", "\u00f6\n"), "json_obj");
            Eq("null", (MiniJson.Parse("{bad") == null ? "null" : "x"), "json_bad");

            Console.WriteLine("OK: " + passes + ", FAIL: " + fails);
            return fails == 0 ? 0 : 1;
        }
    }
}
