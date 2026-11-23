# Yangi Alifbo — PROGRESS

Topshiriq: `docs/spec-v1.md` · Reja: `docs/plans/2026-11-22-v1-office-qoshimcha.md` · Branch: `v1-office`

| # | Bosqich | Holat | Tekshiruv |
|---|---|---|---|
| 1 | Tokenizer, fasad, test-runner | bajarildi | CoreTests (mono) |
| 2 | Eski lotin → Yangi lotin | bajarildi | CoreTests (mono) |
| 3 | Kirill → Yangi lotin | bajarildi | CoreTests (mono) |
| 4 | Lotin → Kirill | bajarildi | CoreTests (mono): 82/82 |
| 5 | COM qobiq, lenta, o'rnatuvchi | bajarildi | Office 2007: Word/Excel/PowerPoint Connect=True (oddiy huquqda) |
| 6 | Word | bajarildi | OfficeSmoke word 13/13 (Word 2007) |
| 7 | Excel | bajarildi | OfficeSmoke excel 11/11 (Excel 2007) |
| 8 | PowerPoint | bajarildi | OfficeSmoke ppt 8/8 (PowerPoint 2007) |
| 9 | Paket va yakuniy tekshiruv | bajarildi | build.cmd smoke: 85+13+11+8, FAIL 0; qo'lda checklist — foydalanuvchida |

## Qarorlar
- 2026-11-22: "Is'hoq" → "Isʼhoq" (foydalanuvchi tasdiqladi)
- 2026-11-22: Word'da maydon kodlari (hyperlink va h.k.) IncludeFieldCodes bilan o'qiladi va o'zgartirilmaydi
- 2026-11-22: Excel'da faqat o'zgargan kataklar yoziladi; = + - @ bilan boshlangan matnga ' prefiksi
- 2026-11-22: Word 2007 maydonli paragraf matnini pozitsiyalar bilan 1:1 bermaydi → paragraf maydonlar atrofida bo'laklanadi (Code.Start-1 … Result.End+1), hyperlink matni ham o'giriladi
- 2026-11-22: PowerPoint 2007 jadval katagi shaklining Type xossasi NotImplemented → xossalar xavfsiz o'qiladi
- 2026-11-22: Administrator huquqidagi jarayonlar HKCU COM ro'yxatini e'tiborsiz qoldiradi → Office oddiy huquqda ochilishi kerak (README)

## Tekshiruv natijasi (2026-11-22, Windows 10 19043, Office 2007 32-bit, .NET 4.8)
- CoreTests 85/85, OfficeSmoke word 13/13, excel 11/11, ppt 8/8
- O'rnatilgandan keyin oddiy huquqda: Word, Excel, PowerPoint — COMAddIns Connect=True, LoadBehavior=3

## v2.0.0 — litsenziya (2026-11-22)
| Qism | Holat | Tekshiruv |
|---|---|---|
| Server 4.32.0 `alifbo.js` + Markaz v22 (dell-platform v2.11.0) | VDS da ✅ (09:33) | test_v2110_server 11, Markaz e2e 7; health 4.32.0-alifbo |
| Qo'shimcha: LicenseCore, MiniJson | ✅ | CoreTests 104/104 (mono + Windows) |
| Qo'shimcha: LicenseService, LicenseForm, «Litsenziya» tugmasi | ✅ | LicenseSmoke 26/26 (Windows), ServerCheck — haqiqiy server OK |
| O'rnatildi (Office 2007) | ✅ | Word: 5 ta tugma (egasi ko'rdi), fonda sinov sanasi serverdan |
| Haqiqiy kalit bilan faollashtirish (Markaz → Word) | ⏳ egasi bilan | |

## v3.0.0 — Windows'da avto-tuzatish (22.11.2026)
Topshiriq: `docs/spec-v3-avto.md` (egasi tasdiqladi: yozayotganda avto, bitta kalit, Pause, nomi «Yangi Alifbo»).

| # | Bosqich | Holat | Tekshiruv |
|---|---|---|---|
| 1 | AutoEngine (yadro) + Tokenizer tuzatishi (oxirgi o'/g') | bajarildi | CoreTests 130/130 (mono va Windows) |
| 2 | YangiAlifbo.exe: hook, SendInput, Pause, istisnolar, litsenziya | bajarildi | avto_sinov.ps1 — Bloknot 6/6 (Windows 10 19043) |
| 3 | Setup: exe, HKCU Run, oddiy huquqda ishga tushirish | bajarildi | kompilyatsiya; o'rnatish — egasida |
| 4 | Antivirus | bajarildi | ESET Smart Security (ecls) — 0 tahdid |
| 5 | Qo'lda: Telegram Desktop, Chrome, Word 2007, o'rnatish/o'chirish | ⏳ egasida | docs/test-checklist-v3.md |

### Qarorlar
- 2026-11-22: sozlamalar JSON emas, matn fayllari (`avto_istisno.txt`, `avto_dasturlar.txt`) + registr `AvtoEnabled`
- 2026-11-22: Enter/Tab himoyasi har qator/maydonda qayta yoqiladi («Ism Familiya» Tab «parol» Enter sinovida topilgan xato)
- 2026-11-22: boshqa dasturlarning sun'iy tugmalari (KeePass avto-kiritish va h.k.) qayta ishlanmaydi; o'zimiz yuborganlar `dwExtraInfo` belgisi bilan ajratiladi.
  Sinov rejimi `HKCU\Software\YangiAlifbo\AvtoTestInjected=1` (faqat avto_sinov.ps1 uchun, oxirida o'chiriladi)
- 2026-11-22: PowerShell `SendKeys` journal playback ishlatadi — LL hook uni ko'rmaydi; sinov `SendInput` + VK kodlar bilan

## v3.1.0 — MSI o'rnatuvchi (23.11.2026)
Egasi so'rovi: .exe dan tashqari .msi; paketda ikkalasi qoladi.

| # | Bosqich | Holat | Tekshiruv |
|---|---|---|---|
| 1 | WiX 3.14 (C:\Projects\tools\wix314) | bajarildi | candle 3.14.1.8722 |
| 2 | Setup.exe /msi-register, /msi-unregister; tray DisabledItems | bajarildi | kompilyatsiya, CoreTests 130 |
| 3 | installer/Product.wxs, build.cmd msi/release | bajarildi | light ICE (38/43/57/61 asoslangan holda o'chirilgan) |
| 4 | O'rnatish / o'chirish / yangilash | bajarildi | msiexec /qn: exit 0; COM ikkala ko'rinishda; Word Connect=True; o'chirishdan keyin registr toza; yangilashdan keyin 1 mahsulot |
| 5 | ESET | bajarildi | msi + zip — 0 tahdid |

### Qarorlar
- 2026-11-23: 32-bit MSI (x86) — Windows 7..11 va 32/64-bit Office; 64-bit registrga yozish Setup.exe custom action orqali
- 2026-11-23: MSI HKCU ga yozmaydi (custom action SYSTEM nomidan); avtoyuklash va Office qo'shimchasi HKLM — barcha foydalanuvchilar
- 2026-11-23: o'rnatish oynasi ruscha (WiX'da o'zbekcha yo'q), litsenziya matni o'zbekcha (installer/License.rtf)
- Egasining kompyuterida v3.1.0 MSI bilan o'rnatilgan holda qoldi (23.11.2026 00:23)

## v3.2.0 — onlayn sotib olish (23.11.2026)
Topshiriq: `docs/spec-v2.1-tolov.md` (egasi tasdiqladi). Server tomoni — dell-platform v2.12.0.

| # | Bosqich | Holat | Tekshiruv |
|---|---|---|---|
| 1 | LicenseService: BuyUrl, MarkBuyPending/BuyPending, Claim | bajarildi | LicenseSmoke 38/38 (mono + testserver) |
| 2 | LicenseForm «Sotib oliş» + 5 s kutish; «Ö» 24 soat har daqiqada | bajarildi | kompilyatsiya; Windows'da — egasida |
| 3 | Sayt site/alifbo (index, sotib-olish, oferta) | bajarildi | sayt_e2e 5/5 (qo'lda va Payme rejimi, Chrome) |
| 4 | VDS: server v2.12.0, nginx /api/alifbo/, sahifalar, MSI | bajarildi 23.11.2026 01:22 | dellcom.uz/alifbo 200, /api/alifbo/info 200, MSI sha256 038306ff…; oferta — STIR/manzil/telefon kutilmoqda |
| 5 | Payme kassa | ⏳ Payme yordami | «Добавить кассу» tugmasi yo'q |

### Qarorlar
- 2026-11-23: sahifa API'ni o'z domenidan chaqiradi (dellcom.uz nginx → `/api/alifbo/`), CORS ochilmaydi
- 2026-11-23: buyurtmada kompyuter ID bo'lsa kalit to'lov paytida shu kompyuterga bog'lanadi; dastur `claim` bilan oladi

## v3.2.1 — DNS ishlamasa IP orqali ulanish (24.11.2026)
Sabab: egasining kompyuterida kalitni faollashtirishda «Server bilan bog'lanib bo'lmadi». Tekshiruv: dellcom.uz ning ikkinchi nom serveri
`revers.hostmaster.uz` zonani sinxron bermayapti — `pos`/`markaz` uchun NXDOMAIN, `dellcom.uz` → 91.213.99.91; `ns2.hostmaster.uz` to'g'ri (37.140.216.54).
Router (192.168.1.1) NXDOMAIN ni keshlagan. Asosiy tuzatish — hostmaster.uz panelida (egasida).

| # | Bosqich | Holat | Tekshiruv |
|---|---|---|---|
| 1 | `IpFallback` (TcpClient + SslStream, SNI/sertifikat — pos.dellcom.uz), `Post` NameResolutionFailure da unga o'tadi | bajarildi | ServerCheck: IP orqali 200, begona nom rad etildi (AuthenticationException), DNS taqlidida activate/sync — OK |
| 2 | Regressiya | bajarildi | CoreTests 130/130, LicenseSmoke 38/38 (testserver), ServerCheck OK (Windows 10) |
| 3 | Paket | bajarildi | build.cmd release — YangiAlifbo_v3.2.1.msi/.zip; ESET ecls — 0 tahdid |
| 4 | O'rnatish egasining kompyuterida, hosts qatorisiz sinov | ⏳ egasida | |

### Qarorlar
- 2026-11-24: hosts fayliga yozilmaydi (antivirus HostsFileHijack deb bloklaydi, admin kerak, IP o'zgarsa qotib qoladi) — dastur ichida zaxira yo'l
- Zaxira faqat standart server (pos.dellcom.uz, Server registr almashtirilmagan) uchun; IP `HKCU\Software\YangiAlifbo\ServerIp` bilan almashtiriladi
