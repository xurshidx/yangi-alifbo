using System;
using System.Windows.Forms;
using YangiAlifbo.Core;

namespace YangiAlifbo.Office
{
    public class ExcelConverter : IDocumentConverter
    {
        /// <summary>Butun varaq uchun tasdiq. Testlarda almashtiriladi.</summary>
        public static Func<bool> Confirm = delegate
        {
            return MessageBox.Show("Butun varaq ögirilsinmi?\nExcel'da bu amalni bekor qilib bölmaydi.",
                Connect.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        };

        const int xlCellTypeConstants = 2;
        const int xlTextValues = 2;

        /// <summary>= + - @ bilan boshlangan matn Excel'da formula bo'lib qolmasligi uchun ' prefiksi.</summary>
        public static string SafeText(string s)
        {
            if (s.Length > 0 && "=+-@".IndexOf(s[0]) >= 0) return "'" + s;
            return s;
        }

        public int Convert(dynamic app, Direction d)
        {
            if ((int)app.Workbooks.Count == 0) return 0;
            dynamic sheet = app.ActiveSheet;
            dynamic target;
            double cells;
            try { target = app.Selection; cells = (double)target.CountLarge; }
            catch { return 0; } // katak emas (shakl, diagramma) belgilangan

            if (cells <= 1)
            {
                if (!Confirm()) return -1;
                target = sheet.UsedRange;
            }

            dynamic texts;
            try { texts = target.SpecialCells(xlCellTypeConstants, xlTextValues); }
            catch { return 0; } // matnli katak yo'q

            bool screen = true;
            try { screen = (bool)app.ScreenUpdating; app.ScreenUpdating = false; } catch { }
            try
            {
                int count = 0;
                foreach (dynamic area in texts.Areas)
                {
                    object v = area.Value2;
                    var arr = v as object[,];
                    if (arr == null)
                    {
                        string s = v as string;
                        if (s == null) continue;
                        int n = Converter.PlanEdits(s, d).Count;
                        if (n > 0) { area.Value2 = SafeText(Converter.ConvertText(s, d)); count += n; }
                        continue;
                    }
                    int r0 = arr.GetLowerBound(0), c0 = arr.GetLowerBound(1);
                    for (int r = r0; r <= arr.GetUpperBound(0); r++)
                        for (int c = c0; c <= arr.GetUpperBound(1); c++)
                        {
                            string s = arr[r, c] as string;
                            if (s == null) continue;
                            int n = Converter.PlanEdits(s, d).Count;
                            if (n == 0) continue;
                            // Faqat o'zgargan katak yoziladi: "00123" kabi matnlar songa aylanmasin
                            area.Cells[r - r0 + 1, c - c0 + 1].Value2 = SafeText(Converter.ConvertText(s, d));
                            count += n;
                        }
                }
                return count;
            }
            finally
            {
                try { app.ScreenUpdating = screen; } catch { }
            }
        }
    }
}
