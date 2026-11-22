using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;
using YangiAlifbo.Core;
using YangiAlifbo.Office;

// Litsenziya oqimi haqiqiy Windows CSP va HTTP bilan: LicenseSmoke.exe http://127.0.0.1:18787
// Haqiqiy registr va %APPDATA% ga tegmaydi: vaqtinchalik papka va HKCU\Software\YangiAlifbo_Sinov (oxirida o'chiriladi).
namespace YangiAlifbo.Tests
{
    public static class LicenseSmoke
    {
        static int fails = 0, passes = 0;
        static void Eq(object expected, object actual, string name)
        {
            string e = Convert.ToString(expected), a = Convert.ToString(actual);
            if (e == a) { passes++; Console.WriteLine("ok   " + name); return; }
            fails++; Console.WriteLine("FAIL " + name + ": kutilgan [" + e + "], olindi [" + a + "]");
        }
        static string Throws(Action f) { try { f(); return "(xatosiz)"; } catch (LicenseService.ServerError e) { return e.Message; } }

        const string Reg = @"Software\YangiAlifbo_Sinov";
        const string PC1 = "11111111111111111111111111111111", PC2 = "22222222222222222222222222222222";

        public static int Main(string[] args)
        {
            try { Console.OutputEncoding = Encoding.UTF8; } catch { }
            string server = args.Length > 0 ? args[0] : "http://127.0.0.1:18787";
            string tmp = Path.Combine(Path.GetTempPath(), "ya_sinov_" + Guid.NewGuid().ToString("N").Substring(0, 8));
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(Reg, false);
                // ochiq kalit serverdan (haqiqiy o'rnatishda — qo'shimcha ichida)
                string pub = Get(server + "/api/alifbo/pubkey", "xml");
                var L = new LicenseService(Path.Combine(tmp, "pc1"), pub, server, Reg, PC1);

                int d;
                Eq("Trial|7", L.State(out d) + "|" + d, "yangi o'rnatish: sinov 7 kun (internetsiz)");
                Eq(true, L.Allowed, "sinovda o'girish ruxsat");
                L.Sync();
                Eq("15000", L.Price, "serverdan narx");
                // sinovni tugatish: state.txt va registrda boshlanish 8 kun oldin
                string past = DateTime.UtcNow.AddDays(-8).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                File.WriteAllText(Path.Combine(tmp, "pc1", "state.txt"), "trialStart=" + past + "\r\nprice=15000\r\n");
                using (var k = Registry.CurrentUser.CreateSubKey(Reg)) k.SetValue("TS", past);
                Eq("Expired|0", L.State(out d) + "|" + d, "8 kun o'tgach: sinov tugadi");
                Eq(false, L.Allowed, "sinov tugagach o'girish yo'q");
                // registrdagi sana state.txt dan ustun (faylni o'chirib sinovni yangilab bo'lmaydi)
                File.Delete(Path.Combine(tmp, "pc1", "state.txt"));
                Eq("Expired", L.State(out d), "state.txt o'chirilsa ham registrdan: tugagan");

                Eq("Kalit YA-XXXX-XXXX-XXXX körinişida bölişi kerak.", Throws(() => L.Activate("123")), "noto'g'ri formatdagi kalit");
                Eq("Kalit topilmadi. Kalitni tekshirib, qayta kiriting.", Throws(() => L.Activate("YA-AAAA-AAAA-AAAA")), "mavjud bo'lmagan kalit");

                string key = Create(server);
                Eq("(xatosiz)", Throws(() => L.Activate(key.ToLowerInvariant().Replace("-", " "))), "faollashtirish (kichik harf, bo'sh joy)");
                Eq("Licensed|" + key, L.State(out d) + "|" + L.LicensedKey, "litsenziyali");
                Eq(true, L.Allowed, "litsenziya bilan o'girish ruxsat");
                Eq(true, File.Exists(Path.Combine(tmp, "pc1", "license.dat")), "license.dat yozildi");

                var L2 = new LicenseService(Path.Combine(tmp, "pc2"), pub, server, Reg + "2", PC2);
                Eq(true, Throws(() => L2.Activate(key)).StartsWith("Bu kalit boshqa kompyuterda faollashtirilgan"), "ikkinchi kompyuterda — rad");

                // license.dat ni boshqa kompyuterga ko'chirish ishlamaydi
                Directory.CreateDirectory(Path.Combine(tmp, "pc2"));
                File.Copy(Path.Combine(tmp, "pc1", "license.dat"), Path.Combine(tmp, "pc2", "license.dat"), true);
                Eq("", L2.LicensedKey, "license.dat boshqa kompyuterga ko'chirilsa — yaroqsiz");
                // imzolangan matnni o'zgartirish
                string lf = Path.Combine(tmp, "pc1", "license.dat"), orig = File.ReadAllText(lf);
                File.WriteAllText(lf, orig.Replace(PC1, PC2));
                Eq("", L.LicensedKey, "license.dat o'zgartirilsa — yaroqsiz");
                File.WriteAllText(lf, orig);
                Eq(key, L.LicensedKey, "asl fayl qaytsa — yana yaroqli");

                // bekor qilish → haftalik tekshiruv
                Admin(server, "revoke", key);
                L.Sync(); Eq("Licensed", L.State(out d), "tekshiruv vaqti kelmagan (oxirgi < 7 kun) — o'zgarmaydi");
                string sf = Path.Combine(tmp, "pc1", "state.txt");
                File.WriteAllText(sf, File.ReadAllText(sf).Replace("lastCheck=", "x=") + "lastCheck=" + DateTime.UtcNow.AddDays(-8).ToString("yyyy-MM-ddTHH:mm:ss.fffZ") + "\r\n");
                L.Sync(); Eq("Revoked", L.State(out d), "8 kundan keyingi tekshiruv: bekor qilingan");
                Eq(false, L.Allowed, "bekor qilingach o'girish yo'q");
                Admin(server, "restore", key);
                Eq("(xatosiz)", Throws(() => L.Activate(key)), "tiklangach qayta faollashtirish");
                Eq("Licensed", L.State(out d), "yana litsenziyali");

                // kompyuterdan ajratish → yangi kompyuter
                Admin(server, "detach", key);
                Eq("(xatosiz)", Throws(() => L2.Activate(key)), "ajratilgach ikkinchi kompyuterda faollashadi");
                File.WriteAllText(sf, File.ReadAllText(sf).Replace("lastCheck=", "x=") + "lastCheck=2000-01-01T00:00:00.000Z\r\n");
                L.Sync(); Eq("Expired", L.State(out d), "eski kompyuter: litsenziya olib tashlandi (sinov tugagan)");

                // server ishlamasa — mahalliy holat
                var off = new LicenseService(Path.Combine(tmp, "pc2"), pub, "http://127.0.0.1:1", Reg + "2", PC2);
                Eq("Licensed", off.State(out d), "internetsiz: litsenziya ishlaydi");
                var off3 = new LicenseService(Path.Combine(tmp, "pc3"), pub, "http://127.0.0.1:1", Reg + "3", "33333333333333333333333333333333");
                Eq("Server bilan boğlanib bölmadi. Internetni tekşiring.", Throws(() => off3.Sync()), "server yo'q — tushunarli xato");
                Eq("Trial", off3.State(out d), "server yo'q — sinov mahalliy hisoblanadi");

                // v3.2.0: onlayn sotib olish — «Sotib oliş» → buyurtma → tasdiq → claim (dastur o'zi faollashadi)
                const string PC4 = "44444444444444444444444444444444";
                var L4 = new LicenseService(Path.Combine(tmp, "pc4"), pub, server, Reg + "4", PC4);
                Eq(true, L4.BuyUrl.StartsWith("https://dellcom.uz/alifbo/sotib-olish.html?hw=" + PC4 + "&pc="), "sotib olish havolasi kompyuter ID bilan");
                Eq(false, L4.BuyPending, "tugma bosilmagan — kutish yo'q");
                L4.MarkBuyPending();
                Eq(true, L4.BuyPending, "tugma bosildi — 24 soat kutiladi");
                Eq(false, L4.Claim(), "buyurtma yo'q — claim false");
                Http(server + "/api/admin/alifbo/shop", "{\"manualText\":\"Karta 8600 sinov\"}");
                var od = MiniJson.Parse(Http(server + "/api/alifbo/order", "{\"agree\":true,\"hwid\":\"" + PC4 + "\",\"pc\":\"SINOV-PC\"}")) as Dictionary<string, object>;
                string no = MiniJson.S(od, "no");
                Eq(false, L4.Claim(), "to'lanmagan buyurtma — claim false (pending)");
                Eq("Trial", L4.State(out d), "hali sinovda");
                Http(server + "/api/admin/alifbo/confirm", MiniJson.Obj("no", no));
                Eq(true, L4.Claim(), "to'lov tasdiqlandi — claim litsenziyani saqladi");
                Eq("Licensed", L4.State(out d), "dastur o'zi faollashdi (kalit kiritilmadi)");
                Eq(false, L4.BuyPending, "faollashgach kutish to'xtadi");
                Eq(true, L4.LicensedKey.StartsWith("YA-"), "kalit saqlandi: " + L4.LicensedKey);
                var L5 = new LicenseService(Path.Combine(tmp, "pc5"), pub, server, Reg + "5", "55555555555555555555555555555555");
                Eq(false, L5.Claim(), "boshqa kompyuter shu buyurtmani ololmaydi");
                Eq(true, Throws(() => L5.Activate(L4.LicensedKey)).StartsWith("Bu kalit boshqa kompyuterda"), "kalit ham boshqa kompyuterda ishlamaydi");
            }
            catch (Exception e) { fails++; Console.WriteLine("FAIL istisno: " + e); }
            finally
            {
                try { foreach (string r in new[] { Reg, Reg + "2", Reg + "3", Reg + "4", Reg + "5" }) Registry.CurrentUser.DeleteSubKeyTree(r, false); } catch { }
                try { Directory.Delete(tmp, true); } catch { }
            }
            Console.WriteLine("OK: " + passes + ", FAIL: " + fails);
            return fails == 0 ? 0 : 1;
        }

        static string Http(string url, string json)
        {
            var c = new System.Net.WebClient();
            c.Encoding = Encoding.UTF8;
            c.Headers["Content-Type"] = "application/json";
            return json == null ? c.DownloadString(url) : c.UploadString(url, json);
        }
        static string Get(string url, string field) { return MiniJson.S(MiniJson.Parse(Http(url, null)) as Dictionary<string, object>, field); }
        static string Create(string server)
        {
            var d = MiniJson.Parse(Http(server + "/api/admin/alifbo/create", "{\"count\":1,\"buyerName\":\"Sinov\"}")) as Dictionary<string, object>;
            return MiniJson.S(((List<object>)d["keys"])[0] as Dictionary<string, object>, "key");
        }
        static void Admin(string server, string act, string key) { Http(server + "/api/admin/alifbo/" + act, MiniJson.Obj("key", key)); }
    }
}
