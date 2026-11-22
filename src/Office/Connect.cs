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
        static LicenseService lic;
        internal static LicenseService Lic { get { return lic ?? (lic = new LicenseService()); } }

        public Connect() { Log("Connect yaratildi"); }

        /// <summary>Diagnostika jurnali: %LOCALAPPDATA%\YangiAlifbo\log.txt</summary>
        public static void Log(string msg)
        {
            try
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YangiAlifbo");
                Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, "log.txt"),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [" + System.Diagnostics.Process.GetCurrentProcess().ProcessName + "] " + msg + Environment.NewLine);
            }
            catch { }
        }

        public void OnConnection(object application, int connectMode, object addInInst, ref Array custom)
        {
            app = application;
            Log("OnConnection mode=" + connectMode);
            try { Lic.SyncInBackground(); } catch (Exception e) { Log("Litsenziya: " + e.Message); }
        }
        public void OnDisconnection(int removeMode, ref Array custom) { app = null; }
        public void OnAddInsUpdate(ref Array custom) { }
        public void OnStartupComplete(ref Array custom) { }
        public void OnBeginShutdown(ref Array custom) { }

        public string GetCustomUI(string ribbonId)
        {
            try
            {
                using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("YangiAlifbo.Office.Ribbon.xml"))
                using (var r = new StreamReader(s))
                {
                    string xml = r.ReadToEnd();
                    Log("GetCustomUI id=" + ribbonId + " len=" + xml.Length);
                    return xml;
                }
            }
            catch (Exception ex)
            {
                Log("GetCustomUI XATO: " + ex);
                return null;
            }
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
                if (id == "btnLicense")
                {
                    using (var f = new LicenseForm(Lic, null)) f.ShowDialog();
                    return;
                }
                if (!Lic.Allowed)
                {
                    int days;
                    string why = Lic.State(out days) == LicState.Revoked ? "Litsenziya bekor qilingan" : "Bepul sinov muddati tugadi";
                    using (var f = new LicenseForm(Lic, why)) f.ShowDialog();
                    if (!Lic.Allowed) return;
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
                Log("OnButton XATO: " + ex);
                MessageBox.Show("Xatolik: " + ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
