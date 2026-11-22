using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using YangiAlifbo.Office;

namespace YangiAlifbo.Tray
{
    /// <summary>
    /// YangiAlifbo.exe — treydagi fon dasturi (v3): yozayotganda eski lotin → yangi lotin.
    /// Sozlamalar: HKCU\Software\YangiAlifbo\AvtoEnabled; %APPDATA%\YangiAlifbo\avto_istisno.txt (so'zlar),
    /// avto_dasturlar.txt (tegilmaydigan dasturlar). Litsenziya Office qo'shimchasi bilan umumiy.
    /// </summary>
    internal sealed class TrayApp : ApplicationContext
    {
        public const string Title = "Yangi Alifbo";
        const string RegPath = @"Software\YangiAlifbo";

        static readonly string[] DefaultApps =
        {
            "cmd", "powershell", "pwsh", "WindowsTerminal", "conhost", "mintty", "putty", "kitty", "Code", "devenv",
            "notepad++", "sublime_text", "idea64", "pycharm64", "rider64", "webstorm64", "studio64", "KeePass", "KeePassXC",
            "mstsc", "vmconnect", "VirtualBox", "AnyDesk", "TeamViewer", "steam", "csgo", "cs2", "dota2", "GTA5", "javaw"
        };

        readonly string dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YangiAlifbo");
        readonly LicenseService lic = new LicenseService();
        readonly KeyboardFixer fixer = new KeyboardFixer();
        readonly NotifyIcon tray = new NotifyIcon();
        readonly ToolStripMenuItem miEnabled, miLicense;
        readonly System.Windows.Forms.Timer licTimer = new System.Windows.Forms.Timer();
        readonly SynchronizationContext ui;
        Icon iconOn, iconOff;
        volatile bool enabled, allowed;
        HashSet<string> apps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        FileSystemWatcher watcher;
        bool expiredShown;

        string WordsFile { get { return Path.Combine(dataDir, "avto_istisno.txt"); } }
        string AppsFile { get { return Path.Combine(dataDir, "avto_dasturlar.txt"); } }

        public TrayApp()
        {
            ui = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
            Directory.CreateDirectory(dataDir);
            iconOn = MakeIcon(Color.FromArgb(0, 120, 212));
            iconOff = MakeIcon(Color.FromArgb(150, 150, 150));
            enabled = ReadEnabled();
            EnsureFiles();
            LoadLists();

            var menu = new ContextMenuStrip();
            miEnabled = new ToolStripMenuItem("Avto-tuzatiş yoqilgan  (Pause)", null, delegate { Toggle(); });
            miLicense = new ToolStripMenuItem("Litsenziya…", null, delegate { ShowLicense(null); });
            menu.Items.Add(miEnabled);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Istisno sözlar…", null, delegate { OpenInNotepad(WordsFile); });
            menu.Items.Add("Tegilmaydigan dasturlar…", null, delegate { OpenInNotepad(AppsFile); });
            menu.Items.Add(miLicense);
            menu.Items.Add("Haqida", null, delegate { About(); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Çiqiş", null, delegate { ExitThread(); });
            tray.ContextMenuStrip = menu;
            tray.DoubleClick += delegate { Toggle(); };

            fixer.CanWork = () => enabled && allowed;
            fixer.AcceptInjected = ReadFlag("AvtoTestInjected");
            fixer.TogglePressed = () => ui.Post(delegate { Toggle(); }, null);
            fixer.IsExcludedApp = p => apps.Contains(p);
            fixer.Engine.ExceptionAdded = w => ui.Post(delegate { AppendWord(w); }, null);

            RefreshLicense();
            licTimer.Interval = 60 * 1000;
            licTimer.Tick += delegate { RefreshLicense(); };
            licTimer.Start();
            try { lic.SyncInBackground(); } catch { }

            fixer.Start();
            UpdateUi();
            tray.Visible = true;
            WatchFiles();
            Connect.Log("Avto: ishga tushdi (v" + LicenseService.Version + ")" + (fixer.AcceptInjected ? " [sinov rejimi]" : "") + " enabled=" + enabled + " allowed=" + allowed);

            if (!ReadFlag("AvtoWelcomeShown"))
            {
                tray.ShowBalloonTip(8000, Title, "Avto-tuzatiş yoqildi: o'zbek → özbek, shahar → şahar.\nPause — yoqiş/öçiriş. Notöğri almaşsa — darhol Backspace.", ToolTipIcon.Info);
                WriteFlag("AvtoWelcomeShown", true);
            }
        }

        protected override void ExitThreadCore()
        {
            fixer.Dispose();
            licTimer.Stop();
            if (watcher != null) watcher.Dispose();
            tray.Visible = false;
            tray.Dispose();
            base.ExitThreadCore();
        }

        // ---------- holat ----------
        void Toggle()
        {
            enabled = !enabled;
            fixer.Engine.Reset();
            WriteFlag("AvtoEnabled", enabled);
            UpdateUi();
            tray.ShowBalloonTip(2000, Title, enabled ? "Avto-tuzatiş yoqildi" : "Avto-tuzatiş öçirildi", ToolTipIcon.None);
        }

        void RefreshLicense()
        {
            int days;
            var s = lic.State(out days);
            allowed = s == LicState.Licensed || s == LicState.Trial;
            miLicense.Text = s == LicState.Licensed ? "Litsenziya: faollaştirilgan" :
                             s == LicState.Trial ? "Litsenziya: sinov, " + days + " kun qoldi…" : "Litsenziya: kalit kerak…";
            if (!allowed && !expiredShown)
            {
                expiredShown = true;
                tray.ShowBalloonTip(10000, Title, "Bepul sinov muddati tugadi — avto-tuzatiş töxtadi. Litsenziya uçun belgini bosing.", ToolTipIcon.Warning);
                tray.BalloonTipClicked += OnExpiredBalloon;
            }
            if (allowed) expiredShown = false;
            UpdateUi();
        }

        void OnExpiredBalloon(object s, EventArgs e) { tray.BalloonTipClicked -= OnExpiredBalloon; ShowLicense("Sinov muddati tugadi"); }

        void UpdateUi()
        {
            if (miEnabled == null) return;
            miEnabled.Checked = enabled;
            bool on = enabled && allowed;
            tray.Icon = on ? iconOn : iconOff;
            string t = Title + " — " + (!allowed ? "litsenziya kerak" : enabled ? "yoqilgan" : "öçirilgan");
            tray.Text = t.Length > 63 ? t.Substring(0, 63) : t;
        }

        void ShowLicense(string reason)
        {
            using (var f = new LicenseForm(lic, reason ?? "Yangi Alifbo"))
            {
                f.ShowInTaskbar = true;
                f.TopMost = true;
                f.ShowDialog();
            }
            RefreshLicense();
        }

        void About()
        {
            MessageBox.Show("Yangi Alifbo v" + LicenseService.Version + "\nDELL COMPUTER SERVIZ\n\n" +
                "Yozayotganingizda eski lotin yangi lotinga almaşadi: o' → ö, g' → ğ, sh → ş, ch → ç.\n" +
                "Söz böş joy, Enter yoki Tab bilan tugaganda almaşadi.\n" +
                "Notöğri almaşsa — darhol Backspace: söz qaytadi va istisnolarga qöşiladi.\n" +
                "Pause — yoqiş/öçiriş.\n\n" +
                "Dastur faqat hozir yozilayotgan sözni xotirada uşlaydi; hec narsa diskka yozilmaydi va internetga yuborilmaydi.",
                Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ---------- fayllar ----------
        void EnsureFiles()
        {
            if (!File.Exists(WordsFile))
                File.WriteAllText(WordsFile, "# Avto-tuzatiş tegmaydigan sözlar — har qatorda bitta (kiçik harfda).\r\n# Backspace bilan qaytarilgan sözlar şu yerga özi qöşiladi.\r\n", Encoding.UTF8);
            if (!File.Exists(AppsFile))
                File.WriteAllText(AppsFile, "# Avto-tuzatiş işlamaydigan dasturlar — jarayon nomi, .exe siz (Vazifalar dispetçeri → Tafsilotlar).\r\n" +
                    string.Join("\r\n", DefaultApps) + "\r\n", Encoding.UTF8);
        }

        static IEnumerable<string> Lines(string file)
        {
            string[] all;
            try { all = File.ReadAllLines(file, Encoding.UTF8); } catch { yield break; }
            foreach (string l in all)
            {
                string t = l.Trim();
                if (t.Length > 0 && t[0] != '#') yield return t;
            }
        }

        void LoadLists()
        {
            var a = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string l in Lines(AppsFile)) a.Add(l.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? l.Substring(0, l.Length - 4) : l);
            apps = a;
            foreach (string l in Lines(WordsFile)) fixer.Engine.AddException(l);
        }

        void AppendWord(string w)
        {
            try
            {
                foreach (string l in Lines(WordsFile)) if (string.Equals(l, w, StringComparison.OrdinalIgnoreCase)) return;
                File.AppendAllText(WordsFile, w + "\r\n", Encoding.UTF8);
            }
            catch (Exception e) { Connect.Log("Avto: istisno yozilmadi: " + e.Message); }
        }

        void WatchFiles()
        {
            try
            {
                watcher = new FileSystemWatcher(dataDir, "avto_*.txt") { NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName };
                watcher.Changed += delegate { ui.Post(delegate { LoadLists(); }, null); };
                watcher.Created += delegate { ui.Post(delegate { LoadLists(); }, null); };
                watcher.EnableRaisingEvents = true;
            }
            catch { }
        }

        static void OpenInNotepad(string file)
        {
            try { Process.Start("notepad.exe", "\"" + file + "\""); } catch { }
        }

        // ---------- registr ----------
        bool ReadEnabled()
        {
            try { using (var k = Registry.CurrentUser.OpenSubKey(RegPath)) { object v = k == null ? null : k.GetValue("AvtoEnabled"); return v == null || Convert.ToInt32(v) != 0; } }
            catch { return true; }
        }

        static bool ReadFlag(string name)
        {
            try { using (var k = Registry.CurrentUser.OpenSubKey(RegPath)) { object v = k == null ? null : k.GetValue(name); return v != null && Convert.ToInt32(v) != 0; } }
            catch { return false; }
        }

        static void WriteFlag(string name, bool v)
        {
            try { using (var k = Registry.CurrentUser.CreateSubKey(RegPath)) k.SetValue(name, v ? 1 : 0, RegistryValueKind.DWord); } catch { }
        }

        // ---------- belgi ----------
        static Icon MakeIcon(Color bg)
        {
            using (var bmp = new Bitmap(32, 32))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    using (var path = new GraphicsPath())
                    {
                        path.AddArc(1, 1, 10, 10, 180, 90); path.AddArc(21, 1, 10, 10, 270, 90);
                        path.AddArc(21, 21, 10, 10, 0, 90); path.AddArc(1, 21, 10, 10, 90, 90);
                        path.CloseFigure();
                        using (var b = new SolidBrush(bg)) g.FillPath(b, path);
                    }
                    using (var f = new Font("Segoe UI", 17f, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        g.DrawString("Ö", f, Brushes.White, new RectangleF(0, 1, 32, 32), sf);
                }
                IntPtr h = bmp.GetHicon();
                var icon = (Icon)Icon.FromHandle(h).Clone();
                Native.DestroyIcon(h);
                return icon;
            }
        }
    }

    internal static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            bool created;
            using (var mutex = new Mutex(true, @"Local\YangiAlifbo.Avto", out created))
            {
                if (!created) return 0; // allaqachon ishlayapti
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += (s, e) => Connect.Log("Avto xatosi: " + e.Exception);
                try { Application.Run(new TrayApp()); }
                catch (Exception e)
                {
                    Connect.Log("Avto ishga tuşmadi: " + e);
                    MessageBox.Show("Yangi Alifbo ishga tuşmadi: " + e.Message, TrayApp.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 1;
                }
                return 0;
            }
        }
    }
}
