using System;
using System.IO;
using System.Text;
using Microsoft.Win32;
using YangiAlifbo.Office;

// Haqiqiy server (https://pos.dellcom.uz) bilan tekshiruv: TLS, sinov, noto'g'ri kalit xabari.
// O'rnatilgan (resursdagi) ochiq kalit ishlatiladi. Vaqtinchalik papka va HKCU\Software\YangiAlifbo_Sinov (oxirida o'chiriladi).
// Ixtiyoriy: ServerCheck.exe YA-XXXX-XXXX-XXXX — haqiqiy kalitni shu kompyuterda faollashtirish ham tekshiriladi.
namespace YangiAlifbo.Tests
{
    public static class ServerCheck
    {
        public static int Main(string[] args)
        {
            try { Console.OutputEncoding = Encoding.UTF8; } catch { }
            string tmp = Path.Combine(Path.GetTempPath(), "ya_server_" + Guid.NewGuid().ToString("N").Substring(0, 8));
            const string reg = @"Software\YangiAlifbo_Sinov";
            int fails = 0;
            try
            {
                var L = new LicenseService(tmp, null, null, reg, null);
                Console.WriteLine("Server: " + L.Server + "  kompyuter ID: " + L.MyHwid);
                L.Sync();
                int d;
                Console.WriteLine("ok   sinov: " + L.State(out d) + ", " + d + " kun; narx " + L.Price + "; telegram '" + L.Telegram + "'");
                try { L.Activate("YA-AAAA-AAAA-AAAA"); Console.WriteLine("FAIL noto'g'ri kalit qabul qilindi"); fails++; }
                catch (LicenseService.ServerError e) { Console.WriteLine((e.Message.StartsWith("Kalit topilmadi") ? "ok   " : "FAIL ") + "noto'g'ri kalit: " + e.Message); if (!e.Message.StartsWith("Kalit topilmadi")) fails++; }
                if (args.Length > 0)
                {
                    L.Activate(args[0]);
                    Console.WriteLine((L.State(out d) == LicState.Licensed ? "ok   " : "FAIL ") + "haqiqiy kalit faollashtirildi: " + L.LicensedKey);
                }
            }
            catch (Exception e) { fails++; Console.WriteLine("FAIL " + e.Message); }
            finally
            {
                try { Registry.CurrentUser.DeleteSubKeyTree(reg, false); } catch { }
                try { Directory.Delete(tmp, true); } catch { }
            }
            Console.WriteLine(fails == 0 ? "NATIJA: OK" : "NATIJA: " + fails + " xato");
            return fails == 0 ? 0 : 1;
        }
    }
}
