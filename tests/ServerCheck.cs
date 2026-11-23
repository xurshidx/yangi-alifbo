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
                // v3.2.1: DNS ishlamagan holat — IP orqali (sertifikat pos.dellcom.uz bo'yicha tekshiriladi)
                int st;
                byte[] bad = Encoding.UTF8.GetBytes("{\"key\":\"YA-AAAA-AAAA-AAAA\",\"hwid\":\"" + L.MyHwid + "\",\"version\":\"sinov\"}");
                string host = new Uri(LicenseService.DefaultServer).Host;
                try
                {
                    string t = IpFallback.Post(LicenseService.DefaultServerIp, 443, host, "/api/alifbo/check", bad, "YangiAlifbo/sinov", 15000, out st);
                    bool okIp = st >= 200 && st < 500 && t.TrimStart().StartsWith("{");
                    Console.WriteLine((okIp ? "ok   " : "FAIL ") + "IP orqali " + LicenseService.DefaultServerIp + " (" + host + "): " + st + " " + t);
                    if (!okIp) fails++;
                }
                catch (Exception e) { fails++; Console.WriteLine("FAIL IP orqali ulanish: " + e.Message); }
                try
                {
                    IpFallback.Post(LicenseService.DefaultServerIp, 443, "example.com", "/api/alifbo/check", bad, "YangiAlifbo/sinov", 15000, out st);
                    fails++; Console.WriteLine("FAIL begona nom (example.com) sertifikati qabul qilindi");
                }
                catch (Exception e) { Console.WriteLine("ok   begona nom rad etildi: " + e.GetType().Name); }
                LicenseService.SimulateDnsFailure = true;
                try
                {
                    try { L.Activate("YA-AAAA-AAAA-AAAA"); Console.WriteLine("FAIL (DNS yo'q) noto'g'ri kalit qabul qilindi"); fails++; }
                    catch (LicenseService.ServerError e) { bool k = e.Message.StartsWith("Kalit topilmadi"); Console.WriteLine((k ? "ok   " : "FAIL ") + "DNS yo'q, IP orqali faollashtirish javobi: " + e.Message); if (!k) fails++; }
                    L.Sync();
                    Console.WriteLine("ok   DNS yo'q, IP orqali sinxron: " + L.State(out d));
                }
                finally { LicenseService.SimulateDnsFailure = false; }
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
