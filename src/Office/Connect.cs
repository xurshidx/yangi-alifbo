using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using YangiAlifbo.Core;

namespace YangiAlifbo.Office
{
    public interface IDocumentConverter
    {
        /// <summary>O'zgargan so'zlar soni; hujjat ochiq bo'lmasa 0.</summary>
        int Convert(dynamic app, Direction d);
    }

    [ComVisible(true)]
    [Guid("894EF783-D4BB-4755-9CFB-7EFC637EAB78")]
    [ProgId("YangiAlifbo.Connect")]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class Connect : IDTExtensibility2, IRibbonExtensibility
    {
        public const string Title = "Yangi Alifbo";
        object app;

        public void OnConnection(object application, int connectMode, object addInInst, ref Array custom) { app = application; }
        public void OnDisconnection(int removeMode, ref Array custom) { app = null; }
        public void OnAddInsUpdate(ref Array custom) { }
        public void OnStartupComplete(ref Array custom) { }
        public void OnBeginShutdown(ref Array custom) { }

        public string GetCustomUI(string ribbonId)
        {
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("YangiAlifbo.Office.Ribbon.xml"))
            using (var r = new StreamReader(s))
                return r.ReadToEnd();
        }

        /// <summary>Hozirgi Office dasturi uchun o'giruvchi (Word/Excel/PowerPoint), boshqasi uchun null.</summary>
        public static IDocumentConverter ForApp(dynamic application)
        {
            string name = (string)application.Name;
            if (name.IndexOf("Word", StringComparison.OrdinalIgnoreCase) >= 0) return new WordConverter();
            if (name.IndexOf("Excel", StringComparison.OrdinalIgnoreCase) >= 0) return new ExcelConverter();
            if (name.IndexOf("PowerPoint", StringComparison.OrdinalIgnoreCase) >= 0) return new PowerPointConverter();
            return null;
        }

        public void OnButton(IRibbonControl control)
        {
            try
            {
                string id = control.Id;
                if (id == "btnAbout")
                {
                    MessageBox.Show(Title + " " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3) +
                        "\nYangi özbek lotin alifbosi (ÖRQ–1184) uçun qöşimça.\n\nDELL COMPUTER SERVIZ",
                        Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Direction d;
                if (id == "btnCyrToLat") d = Direction.CyrToLat;
                else if (id == "btnLatToCyr") d = Direction.LatToCyr;
                else d = Direction.OldToNew;

                var conv = ForApp(app);
                if (conv == null) return;
                int n = conv.Convert(app, d);
                if (n < 0) return; // foydalanuvchi bekor qildi
                MessageBox.Show(n == 0 ? "Ögiriladigan matn topilmadi." : n + " ta söz ögirildi.",
                    Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xatolik: " + ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
