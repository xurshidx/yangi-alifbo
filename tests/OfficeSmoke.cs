using System;
using System.Text;
using YangiAlifbo.Core;
using YangiAlifbo.Office;

// Haqiqiy Word/Excel/PowerPoint bilan avtomatik tekshiruv.
// Foydalanish: OfficeSmoke.exe word | excel | ppt     (exit 0 = OK)
namespace YangiAlifbo.Tests
{
    public static class OfficeSmoke
    {
        static int fails = 0, passes = 0;

        static void Eq(object expected, object actual, string name)
        {
            string e = Convert.ToString(expected), a = Convert.ToString(actual);
            if (e == a) { passes++; Console.WriteLine("ok   " + name); return; }
            fails++;
            Console.WriteLine("FAIL " + name + ": kutilgan [" + e + "], olindi [" + a + "]");
        }

        static dynamic Create(string progId)
        {
            return Activator.CreateInstance(Type.GetTypeFromProgID(progId));
        }

        public static int Main(string[] args)
        {
            try { Console.OutputEncoding = Encoding.UTF8; } catch { }
            string which = args.Length > 0 ? args[0].ToLowerInvariant() : "";
            try
            {
                if (which == "word") Word();
                else if (which == "excel") Excel();
                else if (which == "ppt") PowerPoint();
                else { Console.WriteLine("Foydalanish: OfficeSmoke.exe word|excel|ppt"); return 2; }
            }
            catch (Exception ex)
            {
                fails++;
                Console.WriteLine("FAIL istisno: " + ex);
            }
            Console.WriteLine("OK: " + passes + ", FAIL: " + fails);
            return fails == 0 ? 0 : 1;
        }

        static void Word()
        {
            dynamic app = Create("Word.Application");
            try
            {
                app.Visible = false;
                var conv = new WordConverter();
                Eq(0, conv.Convert(app, Direction.CyrToLat), "word_no_document");

                dynamic doc = app.Documents.Add();
                doc.Content.Text = "Ўзбекистон пойтахти Тошкент.\rБизнинг сайт ва дўкон";
                doc.Range(11, 19).Bold = 1;
                int p2 = (int)doc.Paragraphs.Item(2).Range.Start;
                doc.Hyperlinks.Add(doc.Range(p2 + 8, p2 + 12), "https://dellcom.uz"); // "сайт"
                doc.Sections.Item(1).Headers.Item(1).Range.Text = "Шаҳар";
                app.Selection.HomeKey(6); // wdStory — hech narsa belgilanmagan

                int n = conv.Convert(app, Direction.CyrToLat);
                Eq("Özbekiston poytaxti Toşkent.", ((string)doc.Paragraphs.Item(1).Range.Text).TrimEnd('\r'), "word_text");
                Eq(-1, doc.Range(11, 12).Bold, "word_bold_first");
                Eq(-1, doc.Range(18, 19).Bold, "word_bold_last");
                Eq(0, doc.Range(20, 21).Bold, "word_not_bold_after");
                Eq("Bizning sayt va dökon", ((string)doc.Paragraphs.Item(2).Range.Text).TrimEnd('\r'), "word_text_after_hyperlink");
                Eq(1, doc.Hyperlinks.Count, "word_hyperlink_kept");
                Eq(true, ((string)doc.Hyperlinks.Item(1).Address).Contains("dellcom.uz"), "word_hyperlink_address");
                Eq("Şahar", ((string)doc.Sections.Item(1).Headers.Item(1).Range.Text).TrimEnd('\r'), "word_header");
                Eq(0, conv.Skipped, "word_skipped");
                Eq(8, n, "word_count");

                // Belgilangan qism: faqat 1-paragraf, LatToCyr
                doc.Paragraphs.Item(1).Range.Select();
                conv.Convert(app, Direction.LatToCyr);
                Eq("Ўзбекистон пойтахти Тошкент.", ((string)doc.Paragraphs.Item(1).Range.Text).TrimEnd('\r'), "word_selection_converted");
                Eq("Bizning sayt va dökon", ((string)doc.Paragraphs.Item(2).Range.Text).TrimEnd('\r'), "word_outside_selection_untouched");
                doc.Close(0);
            }
            finally { app.Quit(0); }
        }

        static void Excel()
        {
            dynamic app = Create("Excel.Application");
            try
            {
                app.Visible = false;
                app.DisplayAlerts = false;
                ExcelConverter.Confirm = delegate { return true; };
                var conv = new ExcelConverter();
                Eq(0, conv.Convert(app, Direction.CyrToLat), "excel_no_workbook");

                dynamic wb = app.Workbooks.Add();
                dynamic ws = wb.Worksheets.Item(1);
                ws.Range["A1"].Value2 = "Шаҳар";
                ws.Range["A2"].Formula = "=A1&\"!\"";
                ws.Range["B1"].Value2 = "Тошкент";
                ws.Range["B1"].Interior.Color = 255;
                ws.Range["C1"].Value2 = 123;
                ws.Range["D1"].Value2 = "'00123";
                ws.Range["E1"].Value2 = "'- Салом";
                ws.Range["A1"].Select();

                conv.Convert(app, Direction.CyrToLat);
                Eq("Şahar", ws.Range["A1"].Value2, "excel_text");
                Eq(true, ws.Range["A2"].HasFormula, "excel_formula_kept");
                Eq("=A1&\"!\"", ws.Range["A2"].Formula, "excel_formula_text");
                Eq("Toşkent", ws.Range["B1"].Value2, "excel_text_b1");
                Eq(255, ws.Range["B1"].Interior.Color, "excel_format_kept");
                Eq(123, ws.Range["C1"].Value2, "excel_number_kept");
                Eq("00123", ws.Range["D1"].Value2, "excel_text_number_kept");
                Eq("- Salom", ws.Range["E1"].Value2, "excel_dash_text_not_formula");

                ExcelConverter.Confirm = delegate { return false; };
                Eq(-1, conv.Convert(app, Direction.LatToCyr), "excel_cancel");
                Eq("Şahar", ws.Range["A1"].Value2, "excel_cancel_untouched");
                wb.Close(false);
            }
            finally { app.Quit(); }
        }

        static void PowerPoint()
        {
            dynamic app = Create("PowerPoint.Application");
            try
            {
                var conv = new PowerPointConverter();
                Eq(0, conv.Convert(app, Direction.CyrToLat), "ppt_no_presentation");

                dynamic pres = app.Presentations.Add(0); // oynasiz
                dynamic slide = pres.Slides.Add(1, 11);  // ppLayoutTitleOnly
                dynamic title = slide.Shapes.Item(1);
                title.TextFrame.TextRange.Text = "Янги алифбо";
                title.TextFrame.TextRange.Characters(6, 6).Font.Bold = -1;
                dynamic s1 = slide.Shapes.AddTextbox(1, 10, 100, 100, 50);
                s1.TextFrame.TextRange.Text = "чой";
                dynamic s2 = slide.Shapes.AddTextbox(1, 10, 200, 100, 50);
                s2.TextFrame.TextRange.Text = "қўл";
                dynamic grp = slide.Shapes.Range(new object[] { (string)s1.Name, (string)s2.Name }).Group();
                dynamic tbl = slide.Shapes.AddTable(2, 1);
                tbl.Table.Cell(1, 1).Shape.TextFrame.TextRange.Text = "ғишт";
                tbl.Table.Cell(2, 1).Shape.TextFrame.TextRange.Text = "шаҳар";

                conv.Convert(app, Direction.CyrToLat);
                Eq("Yangi alifbo", title.TextFrame.TextRange.Text, "ppt_title");
                Eq(-1, title.TextFrame.TextRange.Characters(7, 6).Font.Bold, "ppt_bold_kept");
                Eq(0, title.TextFrame.TextRange.Characters(1, 5).Font.Bold, "ppt_not_bold");
                Eq("çoy", grp.GroupItems.Item(1).TextFrame.TextRange.Text, "ppt_group_1");
                Eq("qöl", grp.GroupItems.Item(2).TextFrame.TextRange.Text, "ppt_group_2");
                Eq("ğişt", tbl.Table.Cell(1, 1).Shape.TextFrame.TextRange.Text, "ppt_table_1");
                Eq("şahar", tbl.Table.Cell(2, 1).Shape.TextFrame.TextRange.Text, "ppt_table_2");
                pres.Close();
            }
            finally { app.Quit(); }
        }
    }
}
