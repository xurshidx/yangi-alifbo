using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using YangiAlifbo.Core;

namespace YangiAlifbo.Office
{
    public enum LicState { Licensed, Trial, Expired, Revoked }

    /// <summary>
    /// Litsenziya holati: %APPDATA%\YangiAlifbo\license.dat (kalit, imzolangan matn, imzo) va state.txt (sinov boshlanishi, oxirgi tekshiruv).
    /// Sinov boshlanishi registrda ham saqlanadi (HKCU\Software\YangiAlifbo\TS) — ikkalasidan eng erta sana olinadi.
    /// Server: https://pos.dellcom.uz (sinov uchun HKCU\Software\YangiAlifbo\Server bilan almashtiriladi).
    /// Tarmoq xatolari o'girishni to'xtatmaydi: sinov va litsenziya mahalliy tekshiriladi.
    /// </summary>
    public class LicenseService
    {
        public const string DefaultServer = "https://pos.dellcom.uz";
        public const string DefaultShop = "https://dellcom.uz/alifbo/sotib-olish.html";
        const int BuyPendingHours = 24;
        public const string Version = "3.2.1";
        const int CheckEveryDays = 7;

        readonly string dir;
        readonly string pubXml;
        readonly string regPath;
        readonly string serverOverride;
        readonly string hwidOverride;
        readonly object gate = new object();

        public LicenseService() : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YangiAlifbo"), null, null, null, null) { }

        /// <summary>Sinovlar uchun: alohida papka, ochiq kalit, server, registr yo'li va kompyuter ID (null — standart).</summary>
        public LicenseService(string dataDir, string pubKeyXml, string server, string registryPath, string hwid)
        {
            dir = dataDir;
            pubXml = pubKeyXml ?? EmbeddedPubKey();
            serverOverride = server;
            regPath = registryPath ?? @"Software\YangiAlifbo";
            hwidOverride = hwid;
        }

        public string MyHwid { get { return hwidOverride ?? Hwid; } }
        public string Server { get { return serverOverride ?? ServerBase; } }

        static string EmbeddedPubKey()
        {
            using (var s = typeof(LicenseService).Assembly.GetManifestResourceStream("YangiAlifbo.Office.alifbo_pub.xml"))
            using (var r = new StreamReader(s)) return r.ReadToEnd().Trim();
        }

        // ---------- kompyuter va sozlamalar ----------
        static string hwidCache;
        public static string Hwid
        {
            get
            {
                if (hwidCache != null) return hwidCache;
                string guid = "";
                foreach (RegistryView v in new[] { RegistryView.Registry64, RegistryView.Registry32 })
                {
                    try
                    {
                        using (var k = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, v).OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                            if (k != null) guid = Convert.ToString(k.GetValue("MachineGuid"));
                    }
                    catch { }
                    if (!string.IsNullOrEmpty(guid)) break;
                }
                if (string.IsNullOrEmpty(guid)) guid = Environment.MachineName + "|" + Environment.UserName;
                return hwidCache = LicenseCore.HwidFrom(guid);
            }
        }

        public static string ServerBase
        {
            get
            {
                try
                {
                    using (var k = Registry.CurrentUser.OpenSubKey(@"Software\YangiAlifbo"))
                    {
                        string s = k == null ? null : k.GetValue("Server") as string;
                        if (!string.IsNullOrEmpty(s)) return s.TrimEnd('/');
                    }
                }
                catch { }
                return DefaultServer;
            }
        }

        // ---------- state.txt ----------
        Dictionary<string, string> ReadState()
        {
            var d = new Dictionary<string, string>();
            try
            {
                foreach (string line in File.ReadAllLines(Path.Combine(dir, "state.txt"), Encoding.UTF8))
                {
                    int i = line.IndexOf('=');
                    if (i > 0) d[line.Substring(0, i)] = line.Substring(i + 1);
                }
            }
            catch { }
            return d;
        }

        void WriteState(Dictionary<string, string> d)
        {
            Directory.CreateDirectory(dir);
            var sb = new StringBuilder();
            foreach (var kv in d) sb.Append(kv.Key).Append('=').Append(kv.Value.Replace("\r", " ").Replace("\n", " ")).Append("\r\n");
            string f = Path.Combine(dir, "state.txt"), t = f + ".tmp";
            File.WriteAllText(t, sb.ToString(), Encoding.UTF8);
            if (File.Exists(f)) File.Delete(f);
            File.Move(t, f);
        }

        static DateTime? ParseUtc(string s)
        {
            DateTime d;
            if (!string.IsNullOrEmpty(s) && DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out d)) return d;
            return null;
        }

        static string Iso(DateTime utc) { return utc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture); }

        /// <summary>Sinov boshlangan sana (UTC): state.txt, registr va berilgan sanalardan eng ertasi; yangisini ikkala joyga yozadi.</summary>
        DateTime TrialStart(DateTime? serverFirstSeen)
        {
            var st = ReadState();
            DateTime best = DateTime.UtcNow;
            DateTime? a = ParseUtc(st.ContainsKey("trialStart") ? st["trialStart"] : null);
            DateTime? b = null;
            try
            {
                using (var k = Registry.CurrentUser.OpenSubKey(regPath)) if (k != null) b = ParseUtc(k.GetValue("TS") as string);
            }
            catch { }
            foreach (var x in new[] { a, b, serverFirstSeen }) if (x.HasValue && x.Value < best) best = x.Value;
            if (!a.HasValue || a.Value != best) { st["trialStart"] = Iso(best); try { WriteState(st); } catch { } }
            try { using (var k = Registry.CurrentUser.CreateSubKey(regPath)) k.SetValue("TS", Iso(best)); } catch { }
            return best;
        }

        // ---------- litsenziya fayli ----------
        string LicFile { get { return Path.Combine(dir, "license.dat"); } }

        /// <summary>license.dat: 1-qator kalit, 2-qator imzolangan matn, 3-qator imzo.</summary>
        bool ReadLicense(out string key, out string payload, out string sig)
        {
            key = payload = sig = "";
            try
            {
                string[] l = File.ReadAllLines(LicFile, Encoding.UTF8);
                if (l.Length < 3) return false;
                key = l[0].Trim(); payload = l[1].Trim(); sig = l[2].Trim();
                return true;
            }
            catch { return false; }
        }

        void WriteLicense(string key, string payload, string sig)
        {
            Directory.CreateDirectory(dir);
            File.WriteAllText(LicFile, key + "\r\n" + payload + "\r\n" + sig + "\r\n", Encoding.UTF8);
        }

        public string LicensedKey
        {
            get { string k, p, s; return ReadLicense(out k, out p, out s) && LicenseCore.IsValidFor(p, s, pubXml, MyHwid) ? k : ""; }
        }

        /// <summary>Hozirgi holat va sinovda qolgan kunlar (internetsiz).</summary>
        public LicState State(out int daysLeft)
        {
            lock (gate)
            {
                daysLeft = 0;
                var st = ReadState();
                if (st.ContainsKey("revoked") && st["revoked"] == "1") return LicState.Revoked;
                if (LicensedKey.Length > 0) return LicState.Licensed;
                daysLeft = LicenseCore.TrialDaysLeft(TrialStart(null), DateTime.UtcNow);
                return daysLeft > 0 ? LicState.Trial : LicState.Expired;
            }
        }

        public bool Allowed
        {
            get { int d; var s = State(out d); return s == LicState.Licensed || s == LicState.Trial; }
        }

        // ---------- server ----------
        public class ServerError : Exception { public ServerError(string m) : base(m) { } }

        public Dictionary<string, object> Post(string path, string json, int timeoutMs)
        {
            try { ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; } catch { } // TLS 1.2 (.NET 4.5+ o'rnatilgan bo'lsa)
            var req = (HttpWebRequest)WebRequest.Create(Server + path);
            req.Method = "POST";
            req.ContentType = "application/json; charset=utf-8";
            req.UserAgent = "YangiAlifbo/" + Version;
            req.Timeout = timeoutMs;
            req.ReadWriteTimeout = timeoutMs;
            byte[] body = Encoding.UTF8.GetBytes(json);
            req.ContentLength = body.Length;
            if (SimulateDnsFailure) return PostViaIp(path, body, timeoutMs, new WebException("sinov: DNS xatosi", WebExceptionStatus.NameResolutionFailure));
            string text;
            try
            {
                using (var s = req.GetRequestStream()) s.Write(body, 0, body.Length);
                using (var resp = (HttpWebResponse)req.GetResponse())
                using (var r = new StreamReader(resp.GetResponseStream(), Encoding.UTF8)) text = r.ReadToEnd();
            }
            catch (WebException e)
            {
                var resp = e.Response as HttpWebResponse;
                if (resp == null && e.Status == WebExceptionStatus.NameResolutionFailure)
                    return PostViaIp(path, body, timeoutMs, e);
                if (resp == null) throw new ServerError("Server bilan boğlanib bölmadi. Internetni tekşiring.");
                using (resp)
                using (var r = new StreamReader(resp.GetResponseStream(), Encoding.UTF8)) text = r.ReadToEnd();
                var err = MiniJson.Parse(text) as Dictionary<string, object>;
                string m = MiniJson.S(err, "error");
                throw new ServerError(m.Length > 0 ? m : "Server xatosi " + (int)resp.StatusCode + ".");
            }
            var d = MiniJson.Parse(text) as Dictionary<string, object>;
            if (d == null) throw new ServerError("Serverdan notöğri javob keldi.");
            return d;
        }

        // ---------- v3.2.1: DNS ishlamasa — IP orqali (hosts fayliga tegmasdan) ----------
        /// <summary>Standart server IP manzili. Registr HKCU\Software\YangiAlifbo\ServerIp bilan almashtiriladi.</summary>
        public const string DefaultServerIp = "37.140.216.54";

        /// <summary>Faqat testlar uchun (ServerCheck): DNS xatosini taqlid qiladi — so'rov darhol IP yo'liga o'tadi.</summary>
        public static bool SimulateDnsFailure;

        public static string ServerIp
        {
            get
            {
                try
                {
                    using (var k = Registry.CurrentUser.OpenSubKey(@"Software\YangiAlifbo"))
                    {
                        string s = k == null ? null : k.GetValue("ServerIp") as string;
                        if (!string.IsNullOrEmpty(s)) return s.Trim();
                    }
                }
                catch { }
                return DefaultServerIp;
            }
        }

        Dictionary<string, object> PostViaIp(string path, byte[] body, int timeoutMs, WebException original)
        {
            Uri u = new Uri(Server);
            string ip = serverOverride == null && u.Host == new Uri(DefaultServer).Host ? ServerIp : null;
            if (ip == null || u.Scheme != "https") throw new ServerError("Server bilan boğlanib bölmadi. Internetni tekşiring.");
            string text;
            int status;
            try
            {
                text = IpFallback.Post(ip, u.Port, u.Host, path, body, "YangiAlifbo/" + Version, timeoutMs, out status);
                Connect.Log("Server: DNS " + u.Host + " ni topmadi, IP " + ip + " orqali ulanildi (" + status + ")");
            }
            catch (Exception e)
            {
                Connect.Log("Server: DNS xatosi (" + original.Message + "), IP " + ip + " ham ishlamadi: " + e.Message);
                throw new ServerError("Server bilan boğlanib bölmadi. Internetni tekşiring.");
            }
            var d = MiniJson.Parse(text) as Dictionary<string, object>;
            if (status >= 400)
            {
                string m = MiniJson.S(d, "error");
                throw new ServerError(m.Length > 0 ? m : "Server xatosi " + status + ".");
            }
            if (d == null) throw new ServerError("Serverdan notöğri javob keldi.");
            return d;
        }

        /// <summary>Faollashtirish (oynadan, sinxron). Muvaffaqiyatda litsenziya saqlanadi, aks holda ServerError.</summary>
        public void Activate(string rawKey)
        {
            string key = LicenseCore.NormKey(rawKey);
            if (key.Length == 0) throw new ServerError("Kalit YA-XXXX-XXXX-XXXX körinişida bölişi kerak.");
            var d = Post("/api/alifbo/activate", MiniJson.Obj("key", key, "hwid", MyHwid, "name", Environment.MachineName, "version", Version), 15000);
            var lic = d.ContainsKey("license") ? d["license"] as Dictionary<string, object> : null;
            string payload = MiniJson.S(lic, "payload"), sig = MiniJson.S(lic, "sig");
            if (!LicenseCore.IsValidFor(payload, sig, pubXml, MyHwid)) throw new ServerError("Server javobidagi litsenziya imzosi tasdiqlanmadi.");
            lock (gate)
            {
                WriteLicense(key, payload, sig);
                var st = ReadState();
                st.Remove("revoked");
                st["lastCheck"] = Iso(DateTime.UtcNow);
                WriteState(st);
            }
            Remember(d);
        }

        // ---------- v3.2.0: onlayn sotib olish (dell-platform v2.12.0, /api/alifbo/claim) ----------

        /// <summary>Sotib olish sahifasi: kompyuter ID va nomi bilan (to'lovdan keyin dastur o'zi faollashadi).</summary>
        public string BuyUrl
        {
            get
            {
                string shop = DefaultShop;
                try { using (var k = Registry.CurrentUser.OpenSubKey(regPath)) { string v = k == null ? null : k.GetValue("Shop") as string; if (!string.IsNullOrEmpty(v)) shop = v; } } catch { }
                return shop + "?hw=" + MyHwid + "&pc=" + Uri.EscapeDataString(Environment.MachineName) + "&v=" + Version;
            }
        }

        /// <summary>«Sotib oliş» bosildi — keyingi 24 soat davomida claim so'raladi.</summary>
        public void MarkBuyPending()
        {
            lock (gate) { var st = ReadState(); st["buyPending"] = Iso(DateTime.UtcNow); try { WriteState(st); } catch { } }
        }

        public bool BuyPending
        {
            get
            {
                var st = ReadState();
                DateTime? t = ParseUtc(st.ContainsKey("buyPending") ? st["buyPending"] : null);
                return t.HasValue && (DateTime.UtcNow - t.Value).TotalHours < BuyPendingHours && t.Value <= DateTime.UtcNow.AddMinutes(5);
            }
        }

        /// <summary>
        /// Server shu kompyuter uchun to'langan buyurtma bormi deb so'raladi. Bo'lsa — imzolangan litsenziya saqlanadi (true).
        /// Tarmoq xatosi ServerError beradi; "pending"/"none" — false.
        /// </summary>
        public bool Claim()
        {
            if (LicensedKey.Length > 0) return true;
            var d = Post("/api/alifbo/claim", MiniJson.Obj("hwid", MyHwid, "version", Version), 10000);
            Remember(d);
            if (MiniJson.S(d, "status") != "paid") return false;
            var lic = d.ContainsKey("license") ? d["license"] as Dictionary<string, object> : null;
            string payload = MiniJson.S(lic, "payload"), sig = MiniJson.S(lic, "sig"), key = MiniJson.S(d, "key");
            if (!LicenseCore.IsValidFor(payload, sig, pubXml, MyHwid)) throw new ServerError("Server javobidagi litsenziya imzosi tasdiqlanmadi.");
            lock (gate)
            {
                WriteLicense(key, payload, sig);
                var st = ReadState();
                st.Remove("revoked");
                st.Remove("buyPending");
                st["lastCheck"] = Iso(DateTime.UtcNow);
                WriteState(st);
            }
            return true;
        }

        /// <summary>Sotib olish ma'lumoti (narx, Telegram) — oxirgi server javobidan.</summary>
        public string Telegram { get { var st = ReadState(); return st.ContainsKey("telegram") ? st["telegram"] : ""; } }
        public string Price { get { var st = ReadState(); return st.ContainsKey("price") ? st["price"] : ""; } }

        void Remember(Dictionary<string, object> d)
        {
            lock (gate)
            {
                var st = ReadState();
                if (d.ContainsKey("telegram")) st["telegram"] = MiniJson.S(d, "telegram");
                if (d.ContainsKey("price")) st["price"] = MiniJson.S(d, "price");
                try { WriteState(st); } catch { }
            }
        }

        /// <summary>
        /// Fon sinxronlash (Office ochilganda): litsenziyasiz bo'lsa — sinov sanasini serverdan oladi;
        /// litsenziyali bo'lsa — haftada bir bekor qilinmaganini tekshiradi. Barcha xatolar yutiladi.
        /// </summary>
        public void SyncInBackground()
        {
            ThreadPool.QueueUserWorkItem(delegate { try { Sync(); } catch (Exception e) { Connect.Log("Litsenziya sinxroni: " + e.Message); } });
        }

        public void Sync()
        {
            string key = LicensedKey;
            if (key.Length == 0)
            {
                var d = Post("/api/alifbo/trial", MiniJson.Obj("hwid", MyHwid, "name", Environment.MachineName, "version", Version), 10000);
                lock (gate) TrialStart(ParseUtc(MiniJson.S(d, "firstSeen")));
                Remember(d);
                return;
            }
            var st = ReadState();
            DateTime? last = ParseUtc(st.ContainsKey("lastCheck") ? st["lastCheck"] : null);
            if (last.HasValue && (DateTime.UtcNow - last.Value).TotalDays < CheckEveryDays && last.Value <= DateTime.UtcNow) return;
            var r = Post("/api/alifbo/check", MiniJson.Obj("key", key, "hwid", MyHwid, "version", Version), 10000);
            string status = MiniJson.S(r, "status");
            lock (gate)
            {
                st = ReadState();
                st["lastCheck"] = Iso(DateTime.UtcNow);
                if (status == "revoked" || status == "moved")
                {
                    // bekor qilingan yoki boshqa kompyuterga o'tkazilgan — litsenziya o'chiriladi
                    try { File.Delete(LicFile); } catch { }
                    if (status == "revoked") st["revoked"] = "1";
                    Connect.Log("Litsenziya holati: " + status);
                }
                WriteState(st);
            }
            Remember(r);
        }
    }
}
