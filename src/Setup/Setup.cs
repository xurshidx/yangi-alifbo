using System;
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
        static readonly string[] Files = { "YangiAlifbo.Core.dll", "YangiAlifbo.Office.dll" };

        static string InstallDir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YangiAlifbo"); }
        }

        public static int Main(string[] args)
        {
            try { Console.OutputEncoding = Encoding.UTF8; } catch { }
            string mode = args.Length > 0 ? args[0].ToLowerInvariant() : "/install";
            try
            {
                if (mode == "/uninstall") { Uninstall(); Console.WriteLine("Yangi Alifbo öçirildi."); }
                else { Install(); Console.WriteLine("Yangi Alifbo örnatildi: " + InstallDir); Console.WriteLine("Word, Excel, PowerPoint'ni qayta oçing — «Надстройки» varağida «Yangi Alifbo» guruhi."); }
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
            foreach (string f in Files)
                File.Copy(Path.Combine(src, f), Path.Combine(InstallDir, f), true);

            string dll = Path.Combine(InstallDir, "YangiAlifbo.Office.dll");
            string asmName = AssemblyName.GetAssemblyName(dll).FullName;
            string codeBase = new Uri(dll).AbsoluteUri;

            foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                using (var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view))
                {
                    using (var k = hkcu.CreateSubKey(@"Software\Classes\CLSID\" + Clsid))
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
                    using (var k = hkcu.CreateSubKey(@"Software\Classes\" + ProgId))
                    {
                        k.SetValue("", ClassName);
                        using (var c = k.CreateSubKey("CLSID")) c.SetValue("", Clsid);
                    }
                    foreach (string app in Apps)
                        using (var k = hkcu.CreateSubKey(@"Software\Microsoft\Office\" + app + @"\Addins\" + ProgId))
                        {
                            k.SetValue("FriendlyName", "Yangi Alifbo");
                            k.SetValue("Description", "Kirill / lotin / yangi alifbo ögiruvçi");
                            k.SetValue("LoadBehavior", 3, RegistryValueKind.DWord);
                        }
                }
            }
        }

        static void Uninstall()
        {
            foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
                using (var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view))
                {
                    hkcu.DeleteSubKeyTree(@"Software\Classes\CLSID\" + Clsid, false);
                    hkcu.DeleteSubKeyTree(@"Software\Classes\" + ProgId, false);
                    foreach (string app in Apps)
                        hkcu.DeleteSubKeyTree(@"Software\Microsoft\Office\" + app + @"\Addins\" + ProgId, false);
                }
            // DLL Office ochiq bo'lsa band bo'lishi mumkin — unda keyingi safar o'chiriladi
            foreach (string f in Files)
            {
                try { File.Delete(Path.Combine(InstallDir, f)); } catch { }
            }
        }
    }
}
