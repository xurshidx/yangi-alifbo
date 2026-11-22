using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Win32;

namespace YangiAlifbo.Setup
{
    public static class Program
    {
        const string Clsid = "{894EF783-D4BB-4755-9CFB-7EFC637EAB78}";
        const string ProgId = "YangiAlifbo.Connect";
        const string ClassName = "YangiAlifbo.Office.Connect";
        static readonly string[] Apps = { "Word", "Excel", "PowerPoint" };
        static readonly string[] Files = { "YangiAlifbo.Office.dll", "YangiAlifbo.exe" };
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string RunName = "YangiAlifbo";

        // .NET COM shim managed sinf ma'lumotini faqat HKLM dan o'qiydi (HKCU dagi yozuv 80070002 beradi,
        // 2026-11-22 da Windows 10 19043 + .NET 4.8 da isbotlangan). Shuning uchun COM HKLM ga yoziladi
        // va administrator huquqi kerak. Office "Addins" kalitlari HKCU da qoladi.
        static string InstallDir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "YangiAlifbo"); }
        }

        static string LegacyDir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YangiAlifbo"); }
        }

        static bool IsAdmin()
        {
            var p = new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent());
            return p.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        static readonly RegistryView[] Views = { RegistryView.Registry64, RegistryView.Registry32 };

        /// <summary>v1.0.0 dagi HKCU COM yozuvlari HKLM dagisini to'sadi — o'chiriladi.</summary>
        static void RemoveUserCom()
        {
            foreach (RegistryView view in Views)
                using (var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view))
                {
                    hkcu.DeleteSubKeyTree(@"Software\Classes\CLSID\" + Clsid, false);
                    hkcu.DeleteSubKeyTree(@"Software\Classes\" + ProgId, false);
                }
        }

        public static int Main(string[] args)
        {
            try { Console.OutputEncoding = Encoding.UTF8; } catch { }
            string mode = args.Length > 0 ? args[0].ToLowerInvariant() : "/install";
            if (!IsAdmin())
            {
                Console.WriteLine("Xatolik: Setup'ni administrator nomidan ishga tushiring.");
                return 1;
            }
            try
            {
                if (mode == "/uninstall") { Uninstall(); Console.WriteLine("Yangi Alifbo öçirildi."); }
                else { Install(); Console.WriteLine("Yangi Alifbo örnatildi: " + InstallDir); Console.WriteLine("Word, Excel, PowerPoint'ni qayta oçing — «Надстройки» varağida «Yangi Alifbo» guruhi.");
                    Console.WriteLine("Avto-tuzatiş soat yonidagi «Ö» belgisi orqali boşqariladi (Pause — yoqiş/öçiriş)."); }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Xatolik: " + ex.Message);
                return 1;
            }
        }

        static void Install()
        {
            string src = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.CreateDirectory(InstallDir);
            StopTray();
            foreach (string f in Files)
                File.Copy(Path.Combine(src, f), Path.Combine(InstallDir, f), true);

            string dll = Path.Combine(InstallDir, "YangiAlifbo.Office.dll");
            string asmName = AssemblyName.GetAssemblyName(dll).FullName;
            // regasm uslubi: bo'sh joy kodlanmaydi. "file:///C:/Program%20Files/..." ni .NET shim topa olmaydi (80070002).
            string codeBase = "file:///" + dll.Replace('\\', '/');

            RemoveUserCom();
            ClearDisabledItems();
            foreach (string old in Files)
            {
                try { File.Delete(Path.Combine(LegacyDir, old)); } catch { }
            }
            try { File.Delete(Path.Combine(LegacyDir, "YangiAlifbo.Core.dll")); } catch { }

            foreach (RegistryView view in Views)
            {
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                {
                    using (var k = hklm.CreateSubKey(@"SOFTWARE\Classes\CLSID\" + Clsid))
                    {
                        k.SetValue("", ClassName);
                        using (var ip = k.CreateSubKey("InprocServer32"))
                        {
                            ip.SetValue("", "mscoree.dll");
                            ip.SetValue("ThreadingModel", "Both");
                            ip.SetValue("Class", ClassName);
                            ip.SetValue("Assembly", asmName);
                            ip.SetValue("RuntimeVersion", "v4.0.30319");
                            ip.SetValue("CodeBase", codeBase);
                        }
                        using (var p = k.CreateSubKey("ProgId")) p.SetValue("", ProgId);
                    }
                    using (var k = hklm.CreateSubKey(@"SOFTWARE\Classes\" + ProgId))
                    {
                        k.SetValue("", ClassName);
                        using (var c = k.CreateSubKey("CLSID")) c.SetValue("", Clsid);
                    }
                }
                using (var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view))
                {
                    foreach (string app in Apps)
                        using (var k = hkcu.CreateSubKey(@"Software\Microsoft\Office\" + app + @"\Addins\" + ProgId))
                        {
                            k.SetValue("FriendlyName", "Yangi Alifbo");
                            k.SetValue("Description", "Kirill / lotin / yangi alifbo ögiruvçi");
                            k.SetValue("LoadBehavior", 3, RegistryValueKind.DWord);
                        }
                }
            }

            // v3: Windows'da avto-tuzatish — har kirishda ishga tushadi (HKCU, oddiy huquqda)
            string exe = Path.Combine(InstallDir, "YangiAlifbo.exe");
            using (var run = Registry.CurrentUser.CreateSubKey(RunKey)) run.SetValue(RunName, "\"" + exe + "\"");
            StartTrayUnelevated(exe);
        }

        /// <summary>Ishlab turgan YangiAlifbo.exe ni to'xtatadi (fayl band bo'lmasligi uchun).</summary>
        static void StopTray()
        {
            foreach (var p in Process.GetProcessesByName("YangiAlifbo"))
            {
                try { p.Kill(); p.WaitForExit(5000); } catch { }
                finally { p.Dispose(); }
            }
        }

        /// <summary>Setup administrator huquqida; fon dasturi esa oddiy huquqda ishlashi kerak — explorer orqali ishga tushiriladi.</summary>
        static void StartTrayUnelevated(string exe)
        {
            try { Process.Start("explorer.exe", "\"" + exe + "\""); }
            catch (Exception e) { Console.WriteLine("Fon dasturi ishga tushmadi (kompyuter qayta yoqilganda özi ishga tuşadi): " + e.Message); }
        }

        /// <summary>
        /// Office qulagan qo'shimchani Resiliency\DisabledItems ga yozib qo'yadi va boshqa yuklamaydi.
        /// Faqat bizning ("yangi alifbo") yozuvlarimiz o'chiriladi.
        /// </summary>
        static void ClearDisabledItems()
        {
            foreach (string ver in new[] { "12.0", "14.0", "15.0", "16.0" })
                foreach (string app in Apps)
                    using (var k = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Office\" + ver + @"\" + app + @"\Resiliency\DisabledItems", true))
                    {
                        if (k == null) continue;
                        foreach (string n in k.GetValueNames())
                        {
                            var b = k.GetValue(n) as byte[];
                            if (b != null && Encoding.Unicode.GetString(b).ToLowerInvariant().Contains("yangi alifbo"))
                                k.DeleteValue(n, false);
                        }
                    }
        }

        static void Uninstall()
        {
            StopTray();
            using (var run = Registry.CurrentUser.OpenSubKey(RunKey, true)) if (run != null) run.DeleteValue(RunName, false);
            RemoveUserCom();
            foreach (RegistryView view in Views)
            {
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                {
                    hklm.DeleteSubKeyTree(@"SOFTWARE\Classes\CLSID\" + Clsid, false);
                    hklm.DeleteSubKeyTree(@"SOFTWARE\Classes\" + ProgId, false);
                }
                using (var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view))
                {
                    foreach (string app in Apps)
                        hkcu.DeleteSubKeyTree(@"Software\Microsoft\Office\" + app + @"\Addins\" + ProgId, false);
                }
            }
            // DLL Office ochiq bo'lsa band bo'lishi mumkin — unda keyingi safar o'chiriladi.
            // v1.0.0 dagi eski YangiAlifbo.Core.dll ham tozalanadi.
            foreach (string f in new[] { "YangiAlifbo.Office.dll", "YangiAlifbo.Core.dll", "YangiAlifbo.exe" })
            {
                try { File.Delete(Path.Combine(InstallDir, f)); } catch { }
                try { File.Delete(Path.Combine(LegacyDir, f)); } catch { }
            }
        }
    }
}
