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
