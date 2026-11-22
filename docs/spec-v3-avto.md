# Yangi Alifbo v3 — Windows'da yozayotganda avto-tuzatish (dizayn)

Holat: **tasdiqlandi** 22.11.2026 · Egasi qarorlari: yozayotganda avto, bitta kalit hammasini ochadi, v3 v2.1 (to'lov) dan oldin; yoqish/o'chirish — **Pause**; nomi — «Yangi Alifbo».

**Amalga oshirishda aniqlashtirilgan:** sozlamalar JSON emas, oddiy matn fayllari — `avto_istisno.txt`, `avto_dasturlar.txt` (Bloknotda tahrirlanadi) va registr `AvtoEnabled`;
Enter/Tab faqat oldin bo'sh joy yozilgan bo'lsa o'giradi (bitta so'zli login/parolga tegmaslik uchun); raqam, @, /, `dellcom.uz` kabi bo'laklarga tegilmaydi;
yadrodagi xato tuzatildi — so'z oxiridagi `g'`/`o'` (`tog'`, `bog'`) endi o'giriladi (Office qo'shimchasida ham).

## 1. Foydalanuvchi nimani ko'radi
- Kompyuter yonganda soat yonida (trey) **«Yangi Alifbo»** belgisi chiqadi.
- Istalgan dasturda (Telegram, brauzer, Word, 1C, bloknot) eski lotinda yozadi:
  `bog'` + bo'sh joy → `boğ`, `o'zbek` → `özbek`, `shahar` → `şahar`, `choy` → `çoy`.
- Almashtirish **so'z tugaganda** bo'ladi (bo'sh joy, vergul, nuqta, Enter, Tab).
- Katta-kichik harf saqlanadi: `O'ZBEK` → `ÖZBEK`, `Shahar` → `Şahar`.
- **Noto'g'ri almashsa** — darhol **Backspace** bosiladi: so'z eski holiga qaytadi va
  shaxsiy istisnolar ro'yxatiga qo'shiladi (keyingi safar tegmaydi). Punto Switcher'dagidek.
- **Pause** tugmasi (yoki treydagi menyu) — yoqish/o'chirish. O'chiq bo'lsa belgi kulrang.

## 2. Qoidalar (mavjud yadrodan, `OldToNew.cs`)
- `o'`→ö, `g'`→ğ, `sh`→ş, `ch`→ç, apostrof → tutuq; `s'h` (Is'hoq) — ikki harf qoladi.
- Chet so'zlar tegilmaydi: `c` (h siz) yoki `w` bor so'zlar (Microsoft, Windows) — mavjud qoida.
- Qo'shimcha istisnolar lug'ati (inglizcha `share`, `push`, `English` kabi) + shaxsiy ro'yxat.
- Kirill harflarida yozilganda hech narsa qilinmaydi (bu v3 da yo'q).

## 3. Qayerda ishlamaydi (atayin)
- Parol maydonlari, buyruq oynalari (cmd, PowerShell, Terminal), dasturlash muharrirlari
  (VS Code, Visual Studio, Notepad++), o'yinlar — standart **istisno dasturlar ro'yxati**, sozlamada o'zgartiriladi.
- Administrator huquqida ochilgan oynalar (Windows cheklovi; fon dasturi oddiy huquqda ishlaydi).

## 4. Texnik
- Alohida dastur `YangiAlifbo.exe` (WinForms, .NET 4.0 — Windows 7..11, qo'shimcha bilan bir xil).
- Klaviatura: `WH_KEYBOARD_LL` + `ToUnicodeEx` (joriy tartibga qarab harf), xotirada **faqat joriy so'z**
  (≤ 40 belgi). Hech narsa diskka yozilmaydi va internetga yuborilmaydi.
- Almashtirish: so'z tugatuvchi tugma ushlanadi → `SendInput` bilan N × Backspace + yangi so'z (Unicode) + tugatuvchi belgi.
- Sichqoncha bosilsa yoki ↑↓←→ / Home / End bosilsa — bufer tozalanadi (kursor joyi o'zgargan).
- Avtoyuklash: `HKCU\...\Run` (administrator huquqi kerak emas). O'rnatuvchi (`Setup.cs`) kengaytiriladi.
- Sozlamalar: `%APPDATA%\YangiAlifbo\avto.json` (yoqilgan, istisno dasturlar, shaxsiy istisno so'zlar).

## 5. Litsenziya
- Qo'shimcha bilan **umumiy**: `%APPDATA%\YangiAlifbo\license.dat` va sinov muddati. Bitta kalit ikkalasini ochadi.
- Sinov tugasa va kalit yo'q bo'lsa — avto-tuzatish to'xtaydi, treydan «Litsenziya» oynasi ochiladi
  (qo'shimchadagi oyna bilan bir xil). Server tomonida o'zgarish **yo'q**.

## 6. Xavf — antivirus (ochiq aytilgan)
- Klaviatura ilgagi + `SendInput` — keylogger'larga o'xshash xatti-harakat. Imzolanmagan `.exe`ni
  ba'zi antiviruslar (ESET ham) shubhali deb belgilashi mumkin. Rejamiz:
  1) ESET bilan sinab ko'ramiz; 2) chiqsa — ESET'ga «false positive» arizasi;
  3) uzoq muddatda — kod imzolash sertifikati (oldin keyinga qoldirilgan).

## 7. Versiyalar va tekshiruv
- Kod: `xurshidx/yangi-alifbo`, versiya **v3.0.0** (v2.0.0 o'zgarmaydi, alohida paket).
- Testlar: yadro testlari + yangi «bufer/so'z tugashi/Backspace qaytarish» testlari (klaviaturasiz, mantiq darajasida);
  keyin sizning kompyuteringizda: Bloknot, Telegram Desktop, Chrome, Word 2007 — qo'lda checklist.
- v2.0.0 dagi haqiqiy faollashtirish sinovi (Markaz kaliti) — parallel, sizda.

## 8. Ochiq savollar
1. Yoqish/o'chirish tugmasi **Pause** bo'lsinmi yoki boshqa (masalan Ctrl+Alt+Y)?
2. Dastur nomi treyda va «Programmalar»da: **«Yangi Alifbo»** (qo'shimcha bilan bitta mahsulot) — maqulmi?
