using System;
using System.Collections.Generic;
using YangiAlifbo.Core;

namespace YangiAlifbo.Office
{
    public class WordConverter : IDocumentConverter
    {
        /// <summary>Joyi mos kelmagani uchun o'tkazib yuborilgan so'zlar (oxirgi chaqiruv).</summary>
        public int Skipped;

        public int Convert(dynamic app, Direction d)
        {
            Skipped = 0;
            if ((int)app.Documents.Count == 0) return 0;
            dynamic doc = app.ActiveDocument;
            dynamic sel = app.Selection;
            bool hasSelection = (int)sel.Start != (int)sel.End;

            bool screen = true;
            try { screen = (bool)app.ScreenUpdating; app.ScreenUpdating = false; } catch { }
            bool undo = false;
            try { app.UndoRecord.StartCustomRecord("Yangi Alifbo"); undo = true; } catch { } // Word 2007 da yo'q
            try
            {
                int count = 0;
                if (hasSelection)
                {
                    dynamic r = sel.Range;
                    count += ConvertRange(r, (int)r.Start, (int)r.End, d);
                }
                else
                {
                    foreach (dynamic story in doc.StoryRanges)
                    {
                        dynamic s = story;
                        while (s != null)
                        {
                            count += ConvertRange(s, (int)s.Start, (int)s.End, d);
                            try { s = s.NextStoryRange; } catch { s = null; }
                        }
                    }
                }
                return count;
            }
            finally
            {
                if (undo) { try { app.UndoRecord.EndCustomRecord(); } catch { } }
                try { app.ScreenUpdating = screen; } catch { }
            }
        }

        int ConvertRange(dynamic range, int start, int end, Direction d)
        {
            int count = 0;
            // Paragraflar ro'yxatini oldindan olamiz va oxiridan boshlaymiz:
            // o'zgartirishlar keyingi pozitsiyalarni siljitadi, oldingilarini emas.
            var paras = new List<dynamic>();
            foreach (dynamic p in range.Paragraphs) paras.Add(p);
            for (int i = paras.Count - 1; i >= 0; i--)
            {
                dynamic pr = paras[i].Range;
                int ps = Math.Max((int)pr.Start, start);
                int pe = Math.Min((int)pr.End, end);
                if (pe <= ps) continue;
                var fields = new List<int[]>();
                foreach (dynamic f in pr.Fields)
                {
                    try
                    {
                        dynamic code = f.Code, res = f.Result;
                        fields.Add(new[] { (int)code.Start - 1, (int)res.End + 1, (int)res.Start, (int)res.End });
                    }
                    catch { }
                }
                count += ConvertSpan(pr, ps, pe, TopLevel(fields), fields, d);
            }
            return count;
        }

        /// <summary>Boshqa maydon ichida bo'lmagan maydonlar, boshlanishi bo'yicha kamayish tartibida.</summary>
        static List<int[]> TopLevel(List<int[]> all)
        {
            var top = new List<int[]>();
            foreach (var f in all)
            {
                bool nested = false;
                foreach (var g in all)
                    if (g != f && g[0] <= f[0] && f[1] <= g[1] && !(g[0] == f[0] && g[1] == f[1])) { nested = true; break; }
                if (!nested) top.Add(f);
            }
            top.Sort(delegate(int[] x, int[] y) { return y[0].CompareTo(x[0]); });
            return top;
        }

        /// <summary>
        /// [a,b) oralig'ini maydonlar atrofida bo'laklab o'giradi: maydondan tashqaridagi matn va
        /// maydon natijasi (hyperlink matni) o'giriladi, maydon kodi o'zgarmaydi.
        /// Word 2007 maydonli paragraf matnini pozitsiyalar bilan 1:1 bermaydi, shuning uchun bo'laklash shart.
        /// </summary>
        int ConvertSpan(dynamic baseRange, int a, int b, List<int[]> top, List<int[]> all, Direction d)
        {
            int count = 0;
            int cursor = b;
            foreach (var f in top) // kamayish tartibida
            {
                if (f[1] <= a || f[0] >= cursor) continue;
                if (f[1] < cursor) count += ConvertPlain(baseRange, f[1], cursor, d);
                if (f[2] >= a && f[3] <= b && f[3] > f[2])
                {
                    var inner = new List<int[]>();
                    foreach (var g in all)
                        if (g[0] >= f[2] && g[1] <= f[3]) inner.Add(g);
                    count += ConvertSpan(baseRange, f[2], f[3], TopLevel(inner), inner, d);
                }
                cursor = Math.Min(cursor, f[0]);
            }
            if (cursor > a) count += ConvertPlain(baseRange, a, cursor, d);
            return count;
        }

        /// <summary>Maydonsiz oraliq: matn pozitsiyalar bilan 1:1 mos.</summary>
        int ConvertPlain(dynamic baseRange, int a, int b, Direction d)
        {
            if (b <= a) return 0;
            dynamic r = baseRange.Duplicate;
            r.SetRange(a, b);
            r.TextRetrievalMode.IncludeFieldCodes = false;
            r.TextRetrievalMode.IncludeHiddenText = true;
            string text = (string)r.Text;
            int count = 0;
            foreach (var e in Converter.ExcludeFieldCodes(text, Converter.PlanEdits(text, d)))
            {
                dynamic w = baseRange.Duplicate;
                w.SetRange(a + e.Start, a + e.Start + e.Length);
                w.TextRetrievalMode.IncludeFieldCodes = false;
                w.TextRetrievalMode.IncludeHiddenText = true;
                if ((string)w.Text != text.Substring(e.Start, e.Length)) { Skipped++; continue; }
                w.Text = e.NewText;
                count++;
            }
            return count;
        }
    }
}
