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

            Console.WriteLine("OK: " + passes + ", FAIL: " + fails);
            return fails == 0 ? 0 : 1;
        }
    }
}
