using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;

namespace YangiAlifbo.Office
{
    /// <summary>
    /// v3.2.1: DNS nomni topa olmasa (NameResolutionFailure) — serverga IP orqali to'g'ridan-to'g'ri ulanish.
    /// TLS ulanishida SNI va sertifikat tekshiruvi asl nom (pos.dellcom.uz) bo'yicha qilinadi, ya'ni xavfsizlik
    /// oddiy so'rov bilan bir xil: sertifikat shu nomga tegishli bo'lmasa ulanish rad etiladi.
    /// hosts fayliga tegilmaydi.
    /// </summary>
    public static class IpFallback
    {
        /// <summary>HTTPS POST: ip:port ga ulanadi, TLS va Host — host nomi bilan. Javob: HTTP holat kodi va matn.</summary>
        public static string Post(string ip, int port, string host, string path, byte[] body, string userAgent, int timeoutMs, out int status)
        {
            using (var tcp = new TcpClient())
            {
                var ar = tcp.BeginConnect(ip, port, null, null);
                if (!ar.AsyncWaitHandle.WaitOne(timeoutMs)) throw new IOException("Ulanish vaqti tugadi: " + ip);
                tcp.EndConnect(ar);
                tcp.ReceiveTimeout = timeoutMs;
                tcp.SendTimeout = timeoutMs;
                using (var ssl = new SslStream(tcp.GetStream(), false)) // standart tekshiruv: zanjir + nom
                {
                    ssl.AuthenticateAsClient(host, null, (SslProtocols)3072 /* TLS 1.2 */, true);
                    var head = new StringBuilder();
                    head.Append("POST ").Append(path).Append(" HTTP/1.1\r\n");
                    head.Append("Host: ").Append(host).Append("\r\n");
                    head.Append("User-Agent: ").Append(userAgent).Append("\r\n");
                    head.Append("Content-Type: application/json; charset=utf-8\r\n");
                    head.Append("Content-Length: ").Append(body.Length).Append("\r\n");
                    head.Append("Connection: close\r\n\r\n");
                    byte[] h = Encoding.ASCII.GetBytes(head.ToString());
                    ssl.Write(h, 0, h.Length);
                    ssl.Write(body, 0, body.Length);
                    ssl.Flush();

                    var ms = new MemoryStream();
                    var buf = new byte[8192];
                    int n;
                    while ((n = ssl.Read(buf, 0, buf.Length)) > 0) ms.Write(buf, 0, n);
                    return ParseResponse(ms.ToArray(), out status);
                }
            }
        }

        /// <summary>HTTP/1.1 javobini ajratadi: holat qatori, sarlavhalar, tana (Content-Length yoki chunked).</summary>
        public static string ParseResponse(byte[] raw, out int status)
        {
            int sep = IndexOf(raw, new byte[] { 13, 10, 13, 10 });
            if (sep < 0) throw new IOException("Serverdan to'liq bo'lmagan javob.");
            string headers = Encoding.ASCII.GetString(raw, 0, sep);
            string[] lines = headers.Split(new[] { "\r\n" }, StringSplitOptions.None);
            string[] first = lines[0].Split(' ');
            if (first.Length < 2 || !first[0].StartsWith("HTTP/") || !int.TryParse(first[1], out status))
                throw new IOException("Serverdan noma'lum javob.");
            bool chunked = false;
            int contentLength = -1;
            for (int i = 1; i < lines.Length; i++)
            {
                int c = lines[i].IndexOf(':');
                if (c <= 0) continue;
                string k = lines[i].Substring(0, c).Trim().ToLowerInvariant(), v = lines[i].Substring(c + 1).Trim();
                if (k == "transfer-encoding" && v.ToLowerInvariant().Contains("chunked")) chunked = true;
                if (k == "content-length") int.TryParse(v, out contentLength);
            }
            int start = sep + 4;
            byte[] body;
            if (chunked) body = Dechunk(raw, start);
            else
            {
                int len = raw.Length - start;
                if (contentLength >= 0 && contentLength < len) len = contentLength;
                body = new byte[len];
                Array.Copy(raw, start, body, 0, len);
            }
            return Encoding.UTF8.GetString(body);
        }

        static byte[] Dechunk(byte[] raw, int pos)
        {
            var ms = new MemoryStream();
            while (pos < raw.Length)
            {
                int eol = IndexOf(raw, new byte[] { 13, 10 }, pos);
                if (eol < 0) break;
                string sizeLine = Encoding.ASCII.GetString(raw, pos, eol - pos);
                int semi = sizeLine.IndexOf(';');
                if (semi >= 0) sizeLine = sizeLine.Substring(0, semi);
                int size = Convert.ToInt32(sizeLine.Trim(), 16);
                pos = eol + 2;
                if (size == 0) break;
                if (pos + size > raw.Length) size = raw.Length - pos;
                ms.Write(raw, pos, size);
                pos += size + 2;
            }
            return ms.ToArray();
        }

        static int IndexOf(byte[] a, byte[] p) { return IndexOf(a, p, 0); }

        static int IndexOf(byte[] a, byte[] p, int from)
        {
            for (int i = from; i <= a.Length - p.Length; i++)
            {
                int j = 0;
                while (j < p.Length && a[i + j] == p[j]) j++;
                if (j == p.Length) return i;
            }
            return -1;
        }
    }
}
