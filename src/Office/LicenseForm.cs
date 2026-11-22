using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace YangiAlifbo.Office
{
    /// <summary>«Litsenziya» oynasi: holat, «Sotib oliş» (v3.2.0 — to'lovdan keyin o'zi faollashadi), kalit kiritish. Matnlar yangi lotin alifbosida.</summary>
    public class LicenseForm : Form
    {
        readonly LicenseService lic;
        readonly Label status = new Label(), buy = new Label(), err = new Label();
        readonly TextBox key = new TextBox();
        readonly Button activate = new Button(), close = new Button(), buyBtn = new Button();
        readonly Label wait = new Label();
        readonly Timer poll = new Timer { Interval = 5000 };
        volatile bool polling;
        readonly LinkLabel tg = new LinkLabel();

        public LicenseForm(LicenseService service, string reason)
        {
            lic = service;
            Text = "Yangi Alifbo — litsenziya";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f);
            ClientSize = new Size(470, 340);

            var head = new Label { Text = reason ?? "", AutoSize = false, Location = new Point(18, 14), Size = new Size(434, 22), Font = new Font(Font, FontStyle.Bold) };
            status.SetBounds(18, 40, 434, 40);
            buy.SetBounds(18, 84, 434, 36);
            buyBtn.Text = "Sotib oliş";
            buyBtn.Font = new Font(Font, FontStyle.Bold);
            buyBtn.SetBounds(18, 122, 160, 32);
            buyBtn.Click += OnBuy;
            wait.SetBounds(188, 130, 264, 20);
            wait.ForeColor = SystemColors.GrayText;
            tg.SetBounds(18, 160, 434, 20);
            tg.LinkClicked += delegate { try { System.Diagnostics.Process.Start(TgUrl(lic.Telegram)); } catch { } };
            var kl = new Label { Text = "Kalitingiz bölsa, kiriting:", Location = new Point(18, 190), AutoSize = true };
            key.SetBounds(18, 212, 300, 26);
            key.CharacterCasing = CharacterCasing.Upper;
            key.Font = new Font("Consolas", 11f);
            key.MaxLength = 24;
            activate.Text = "Faollaştiriş";
            activate.SetBounds(326, 210, 126, 30);
            activate.Click += OnActivate;
            err.SetBounds(18, 244, 434, 44);
            err.ForeColor = Color.FromArgb(170, 40, 40);
            var pc = new Label { Text = "Kompyuter: " + Environment.MachineName + "  ·  ID " + lic.MyHwid.Substring(0, 8), Location = new Point(18, 304), AutoSize = true, ForeColor = SystemColors.GrayText };
            close.Text = "Yopiş";
            close.SetBounds(362, 298, 90, 30);
            close.DialogResult = DialogResult.Cancel;
            CancelButton = close;
            AcceptButton = activate;
            Controls.AddRange(new Control[] { head, status, buy, buyBtn, wait, tg, kl, key, activate, err, pc, close });
            poll.Tick += delegate { TryClaim(); };
            FormClosed += delegate { poll.Stop(); poll.Dispose(); };
            Refill();
            if (lic.BuyPending && lic.LicensedKey.Length == 0) StartWaiting();
        }

        static string TgUrl(string t)
        {
            if (string.IsNullOrEmpty(t)) return "";
            return t.StartsWith("https://") ? t : "https://t.me/" + t.TrimStart('@');
        }

        void Refill()
        {
            int days;
            var s = lic.State(out days);
            switch (s)
            {
                case LicState.Licensed: status.Text = "Faollaştirilgan. Kalit: " + lic.LicensedKey + "\nRahmat! Dastur şu kompyuterda muddatsiz işlaydi."; break;
                case LicState.Trial: status.Text = "Bepul sinov: " + days + " kun qoldi."; break;
                case LicState.Revoked: status.Text = "Litsenziya bekor qilingan. Sotuvçiga murojaat qiling yoki yangi kalit kiriting."; break;
                default: status.Text = "Bepul sinov muddati tugadi. Davom etiş uçun litsenziya kalitini kiriting."; break;
            }
            string price = lic.Price;
            long p;
            string priceText = long.TryParse(price, NumberStyles.Integer, CultureInfo.InvariantCulture, out p) ? p.ToString("#,0", CultureInfo.InvariantCulture).Replace(',', ' ') + " söm" : "";
            buy.Text = (priceText.Length > 0 ? "Narxi: " + priceText + " — bir martalik tölov, 1 kompyuter, muddatsiz." : "Bir martalik tölov, 1 kompyuter, muddatsiz.") +
                       "\nTölovdan keyin dastur özi faollaşadi — kalit kiritiş şart emas.";
            string url = TgUrl(lic.Telegram);
            tg.Visible = url.Length > 0;
            tg.Text = "Savol bölsa — Telegram: " + lic.Telegram;
            bool licensed = s == LicState.Licensed;
            key.Enabled = activate.Enabled = buyBtn.Enabled = !licensed;
            if (licensed) { poll.Stop(); wait.Text = ""; }
        }

        /// <summary>Sotib olish sahifasini ochadi va to'lovni kutadi (har 5 soniyada serverdan so'raydi).</summary>
        void OnBuy(object sender, EventArgs e)
        {
            err.Text = "";
            try { System.Diagnostics.Process.Start(lic.BuyUrl); }
            catch (Exception x) { err.Text = "Brauzer oçilmadi: " + x.Message + "\n" + lic.BuyUrl; }
            lic.MarkBuyPending();
            StartWaiting();
        }

        void StartWaiting()
        {
            wait.Text = "Tölov kutilmoqda… (oynani yopsangiz ham faollaşadi)";
            poll.Start();
        }

        void TryClaim()
        {
            if (polling) return;
            polling = true;
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                bool ok = false;
                try { ok = lic.Claim(); }
                catch (Exception x) { Connect.Log("Claim: " + x.Message); }
                finally { polling = false; }
                if (!ok || IsDisposed) return;
                try
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        poll.Stop();
                        Connect.Log("Faollashtirildi (sotib olish): " + lic.LicensedKey);
                        Refill();
                        MessageBox.Show(this, "Tölov qabul qilindi — litsenziya faollaşdi. Rahmat!", Connect.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DialogResult = DialogResult.OK;
                    });
                }
                catch { }
            });
        }

        void OnActivate(object sender, EventArgs e)
        {
            err.Text = "";
            Cursor = Cursors.WaitCursor;
            activate.Enabled = false;
            try
            {
                lic.Activate(key.Text);
                Connect.Log("Faollashtirildi: " + lic.LicensedKey);
                Refill();
                MessageBox.Show(this, "Litsenziya faollaştirildi. Rahmat!", Connect.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (LicenseService.ServerError x) { err.Text = x.Message; }
            catch (Exception x) { err.Text = "Xatolik: " + x.Message; Connect.Log("Faollashtirish xatosi: " + x); }
            finally { Cursor = Cursors.Default; if (!IsDisposed && lic.LicensedKey.Length == 0) activate.Enabled = true; }
        }
    }
}
