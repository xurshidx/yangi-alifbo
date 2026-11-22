using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace YangiAlifbo.Core
{
    /// <summary>
    /// Litsenziyaning toza (diskka va tarmoqqa tegmaydigan) qismi. Server: dell-platform Server_v4/alifbo.js.
    /// Imzolangan matn: "YA1|KALIT|HWID|perpetual|issuedAt" (UTF-8), imzo RSA-2048 + SHA-256, base64.
    /// </summary>
    public static class LicenseCore
    {
        public const int TrialDays = 7;
        static readonly Regex KeyRe = new Regex("^YA-[A-HJ-NP-Z2-9]{4}-[A-HJ-NP-Z2-9]{4}-[A-HJ-NP-Z2-9]{4}$");

        /// <summary>Kompyuter ID: SHA-256(MachineGuid, kichik harf) hex ning 32 belgisi.</summary>
        public static string HwidFrom(string machineGuid)
        {
            using (var sha = SHA256.Create())
            {
                byte[] h = sha.ComputeHash(Encoding.UTF8.GetBytes((machineGuid ?? "").Trim().ToLowerInvariant()));
                var sb = new StringBuilder();
                for (int i = 0; i < 16; i++) sb.Append(h[i].ToString("x2"));
                return sb.ToString();
            }
        }

        /// <summary>" ya abcd-efgh jkmn " → "YA-ABCD-EFGH-JKMN"; yaroqsiz bo'lsa "".</summary>
        public static string NormKey(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in (s ?? "").ToUpperInvariant())
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')) sb.Append(c);
            string r = sb.ToString();
            if (r.StartsWith("YA")) r = r.Substring(2);
            if (r.Length != 12) return "";
            string k = "YA-" + r.Substring(0, 4) + "-" + r.Substring(4, 4) + "-" + r.Substring(8, 4);
            return KeyRe.IsMatch(k) ? k : "";
        }

        public static bool Verify(string payload, string sigBase64, string pubXml)
        {
            try
            {
                byte[] sig = Convert.FromBase64String(sigBase64 ?? "");
                byte[] data = Encoding.UTF8.GetBytes(payload ?? "");
                byte[] hash;
                using (var sha = SHA256.Create()) hash = sha.ComputeHash(data);
                // PROV_RSA_AES (24) — Windows'ning eski CSP lari SHA-256 ni bilmaydi
                var csp = new CspParameters(24);
                csp.Flags = CspProviderFlags.UseMachineKeyStore;
                using (var rsa = new RSACryptoServiceProvider(csp))
                {
                    rsa.PersistKeyInCsp = false;
                    rsa.FromXmlString(pubXml);
                    return rsa.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA256"), sig);
                }
            }
            catch (FormatException) { return false; }
            catch (CryptographicException) { return false; }
        }

        public static bool ParsePayload(string payload, out string key, out string hwid)
        {
            key = ""; hwid = "";
            string[] p = (payload ?? "").Split('|');
            if (p.Length != 5 || p[0] != "YA1" || p[3] != "perpetual") return false;
            key = p[1]; hwid = p[2];
            return key.Length > 0 && hwid.Length == 32;
        }

        /// <summary>Imzo to'g'ri va litsenziya shu kompyuter uchun berilgan.</summary>
        public static bool IsValidFor(string payload, string sig, string pubXml, string hwid)
        {
            string k, h;
            return ParsePayload(payload, out k, out h) && h == hwid && Verify(payload, sig, pubXml);
        }

        /// <summary>Server bilan bir xil: ceil((boshlangan + 7 kun − hozir) / 1 kun), 0..7. Soat orqaga surilsa 7 dan oshmaydi.</summary>
        public static int TrialDaysLeft(DateTime firstSeenUtc, DateTime nowUtc)
        {
            double left = (firstSeenUtc.AddDays(TrialDays) - nowUtc).TotalDays;
            int d = (int)Math.Ceiling(left);
            return Math.Max(0, Math.Min(TrialDays, d));
        }
    }
}
