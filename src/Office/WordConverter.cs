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
            // Paragraflar ro'yxatini oldindan olamiz: o'zgartirishlar paragraf chegaralarini siljitadi,
            // shuning uchun oxirgisidan boshlaymiz.
            var paras = new List<dynamic>();
            foreach (dynamic p in range.Paragraphs) paras.Add(p);
            for (int i = paras.Count - 1; i >= 0; i--)
            {
                dynamic pr = paras[i].Range;
                int ps = Math.Max((int)pr.Start, start);
                int pe = Math.Min((int)pr.End, end);
                if (pe <= ps) continue;
                dynamic r = pr.Duplicate;
                r.SetRange(ps, pe);
                r.TextRetrievalMode.IncludeFieldCodes = true;
                r.TextRetrievalMode.IncludeHiddenText = true;
                string text = (string)r.Text;
                var edits = Converter.ExcludeFieldCodes(text, Converter.PlanEdits(text, d));
                foreach (var e in edits)
                {
                    dynamic w = pr.Duplicate;
                    w.SetRange(ps + e.Start, ps + e.Start + e.Length);
                    w.TextRetrievalMode.IncludeFieldCodes = true;
                    w.TextRetrievalMode.IncludeHiddenText = true;
                    if ((string)w.Text != text.Substring(e.Start, e.Length)) { Skipped++; continue; }
                    w.Text = e.NewText;
                    count++;
                }
            }
            return count;
        }
    }
}
