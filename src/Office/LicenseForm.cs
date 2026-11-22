using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace YangiAlifbo.Office
{
    /// <summary>«Litsenziya» oynasi: holat, kalit kiritish, Telegram orqali sotib olish. Matnlar yangi lotin alifbosida.</summary>
    public class LicenseForm : Form
    {
        readonly LicenseService lic;
        readonly Label status = new Label(), buy = new Label(), err = new Label();
        readonly TextBox key = new TextBox();
        readonly Button activate = new Button(), close = new Button();
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
            ClientSize = new Size(470, 300);

            var head = new Label { Text = reason ?? "", AutoSize = false, Location = new Point(18, 14), Size = new Size(434, 22), Font = new Font(Font, FontStyle.Bold) };
            status.SetBounds(18, 40, 434, 40);
            buy.SetBounds(18, 84, 434, 36);
            tg.SetBounds(18, 120, 434, 20);
            tg.LinkClicked += delegate { try { System.Diagnostics.Process.Start(TgUrl(lic.Telegram)); } catch { } };
            var kl = new Label { Text = "Litsenziya kaliti:", Location = new Point(18, 152), AutoSize = true };
            key.SetBounds(18, 174, 300, 26);
            key.CharacterCasing = CharacterCasing.Upper;
            key.Font = new Font("Consolas", 11f);
            key.MaxLength = 24;
            activate.Text = "Faollaştiriş";
            activate.SetBounds(326, 172, 126, 30);
            activate.Click += OnActivate;
            err.SetBounds(18, 206, 434, 44);
            err.ForeColor = Color.FromArgb(170, 40, 40);
            var pc = new Label { Text = "Kompyuter: " + Environment.MachineName + "  ·  ID " + lic.MyHwid.Substring(0, 8), Location = new Point(18, 264), AutoSize = true, ForeColor = SystemColors.GrayText };
            close.Text = "Yopiş";
            close.SetBounds(362, 258, 90, 30);
            close.DialogResult = DialogResult.Cancel;
            CancelButton = close;
            AcceptButton = activate;
            Controls.AddRange(new Control[] { head, status, buy, tg, kl, key, activate, err, pc, close });
            Refill();
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
                       "\nTölovdan keyin sotuvçi sizga kalit yuboradi.";
            string url = TgUrl(lic.Telegram);
            tg.Visible = url.Length > 0;
            tg.Text = "Telegram orqali sotib oliş: " + lic.Telegram;
            bool licensed = s == LicState.Licensed;
            key.Enabled = activate.Enabled = !licensed;
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
