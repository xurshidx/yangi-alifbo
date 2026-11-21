using System;
using YangiAlifbo.Core;

namespace YangiAlifbo.Office
{
    public class PowerPointConverter : IDocumentConverter
    {
        const int ppSelectionShapes = 2;
        const int ppSelectionText = 3;
        const int msoGroup = 6;

        public int Skipped;

        public int Convert(dynamic app, Direction d)
        {
            Skipped = 0;
            if ((int)app.Presentations.Count == 0) return 0;

            dynamic sel = null;
            int selType = 0;
            try { sel = app.ActiveWindow.Selection; selType = (int)sel.Type; } catch { } // oyna yo'q

            if (selType == ppSelectionText)
            {
                dynamic tr = sel.TextRange;
                if ((int)tr.Length > 0) return ConvertTextRange(tr, d);
                selType = ppSelectionShapes;
            }
            int count = 0;
            if (selType == ppSelectionShapes)
            {
                foreach (dynamic sh in sel.ShapeRange) count += ConvertShape(sh, d);
                return count;
            }
            dynamic pres;
            try { pres = app.ActivePresentation; } catch { pres = app.Presentations.Item(1); } // oynasiz taqdimot
            foreach (dynamic slide in pres.Slides)
                foreach (dynamic sh in slide.Shapes)
                    count += ConvertShape(sh, d);
            return count;
        }

        static bool True(object o) { return System.Convert.ToInt32(o) != 0; }

        int ConvertShape(dynamic sh, Direction d)
        {
            int count = 0;
            if ((int)sh.Type == msoGroup)
            {
                foreach (dynamic item in sh.GroupItems) count += ConvertShape(item, d);
                return count;
            }
            if (True(sh.HasTable))
            {
                dynamic t = sh.Table;
                for (int r = 1; r <= (int)t.Rows.Count; r++)
                    for (int c = 1; c <= (int)t.Columns.Count; c++)
                        count += ConvertShape(t.Cell(r, c).Shape, d);
                return count;
            }
            if (True(sh.HasTextFrame) && True(sh.TextFrame.HasText))
                count += ConvertTextRange(sh.TextFrame.TextRange, d);
            return count;
        }

        int ConvertTextRange(dynamic tr, Direction d)
        {
            string text = (string)tr.Text;
            int count = 0;
            foreach (var e in Converter.PlanEdits(text, d))
            {
                dynamic ch = tr.Characters(e.Start + 1, e.Length);
                if ((string)ch.Text != text.Substring(e.Start, e.Length)) { Skipped++; continue; }
                ch.Text = e.NewText;
                count++;
            }
            return count;
        }
    }
}
