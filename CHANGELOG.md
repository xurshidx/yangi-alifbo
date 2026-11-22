# CHANGELOG

## v3.2.0 — 2026-11-23 (onlayn sotib olish, dastur o'zi faollashadi)
- Litsenziya oynasida «Sotib oliş» tugmasi: `https://dellcom.uz/alifbo/sotib-olish.html?hw=<kompyuter ID>&pc=<nom>&v=<versiya>`
  (registr `HKCU\Software\YangiAlifbo\Shop` bilan almashtiriladi); oyna ochiq bo'lsa har 5 s, «Ö» esa 24 soat davomida har daqiqada `POST /api/alifbo/claim` —
  to'lov tasdiqlansa imzolangan litsenziya saqlanadi (kalit kiritish shart emas). Server: dell-platform v2.12.0 (`alifbo_shop.js`)
- Sayt `site/alifbo/`: bosh sahifa, sotib olish (Payme yoki qo'lda rekvizit + «To'ladim», holat va kalit), ommaviy oferta (⟨STIR⟩, ⟨manzil⟩, ⟨telefon⟩ — egasi to'ldiradi)
- Testlar: CoreTests 130, LicenseSmoke 38 (+11 claim), `tests/sayt_e2e.cjs` 5 (Chrome, dell-platform serveri bilan)

## v3.1.0 — 2026-11-23 (MSI o'rnatuvchi)
- `YangiAlifbo_v3.1.0.msi` (WiX 3.14): «Программы и компоненты» da yozuv va belgi, ruscha o'rnatish oynasi, litsenziya matni, avtomatik yangilash (MajorUpgrade), Bosh menyuda yorliq
- MSI barcha foydalanuvchilar uchun: Program Files (x86)\YangiAlifbo, COM + Office qo'shimchasi + avtoyuklash HKLM da (`YangiAlifbo.Setup.exe /msi-register`, ikkala registr ko'rinishi); o'rnatishdan oldin ishlab turgan «Ö» to'xtatiladi, oxirida ishga tushiriladi
- Setup.exe saqlanadi (paketda ikkalasi); yangi `/msi-register`, `/msi-unregister` rejimlari; MSI eski Setup.exe nusxasini (C:\Program Files\YangiAlifbo) olib tashlaydi
- «Ö» har kirishda Office o'chirib qo'ygan qo'shimchani (HKCU Resiliency) o'zi tiklaydi; YangiAlifbo.exe da belgi (installer/YangiAlifbo.ico)
- Sinov (Windows 10, Office 2007): o'rnatish → Word COMAddIn Connect=True → o'chirish (toza, litsenziya saqlanadi) → yangilash (bitta yozuv qoladi); ESET — toza

## v3.0.0 — 2026-11-22 (Windows'da yozayotganda avto-tuzatish)
- Yangi `YangiAlifbo.exe` — treydagi «Ö» belgisi: istalgan dasturda eski lotin (o', g', sh, ch) so'z tugaganda yangi lotinga almashadi
- Pause — yoqish/o'chirish; Backspace — oxirgi almashtirishni qaytarish va so'zni istisnoga qo'shish (`%APPDATA%\YangiAlifbo\avto_istisno.txt`)
- Himoya: Enter/Tab faqat qatorda oldin bo'sh joy bo'lsa (login/parol), Win32 parol maydonlari, raqam/@/manzilli bo'laklar, tegilmaydigan dasturlar (`avto_dasturlar.txt`), chet so'zlar ro'yxati
- Litsenziya Office qo'shimchasi bilan umumiy (bitta kalit); Setup avtoyuklashni (HKCU Run) yozadi va dasturni oddiy huquqda ishga tushiradi
- Tuzatish: so'z oxiridagi o'/g' (tog', bog', o') endi o'giriladi — Office tugmalarida ham
- Testlar: CoreTests 130 (Windows + mono), `tests/avto_sinov.ps1` — Bloknotda haqiqiy klaviatura sinovi 6/6; ESET Smart Security skani — toza

## v2.0.0 — 2026-11-22 (litsenziya)
- 7 kunlik bepul sinov; litsenziya kaliti YA-XXXX-XXXX-XXXX (Boshqaruv markazi, dell-platform v2.11.0 / server 4.32.0); bir martalik, 1 kompyuter, muddatsiz
- Lentada «Litsenziya» tugmasi va oyna: holat, kalit kiritish, narx, Telegram orqali sotib olish
- Imzo RSA-2048/SHA-256 (keys/alifbo_pub.xml — VDS kaliti); sinov sanasi serverda va mahalliy (fayl + registr); haftalik bekor qilinganini tekshirish; internetsiz ishlaydi
- Testlar: CoreTests 104, LicenseSmoke 26 (Windows + test serveri), ServerCheck (haqiqiy pos.dellcom.uz)

## v1.0.1 — 2026-11-22 (Office 2007 da qo'shimcha yuklanmasligi va qulashi tuzatildi)
- IDTExtensibility2: `custom` parametri SAFEARRAY sifatida marshal qilinadi — Excel/PowerPoint 2007 clr.dll ichida qulashi (0xc0000005) tuzatildi
- COM ro'yxati HKLM ga ko'chirildi: .NET shim HKCU dagi managed COM yozuvini o'qimaydi (80070002); Setup administrator ruxsatini so'raydi, fayllar Program Files\YangiAlifbo da
- CodeBase bo'sh joyni kodlamasdan yoziladi (`%20` bilan shim faylni topmaydi)
- Setup Office Resiliency\DisabledItems dagi "yangi alifbo" yozuvlarini tozalaydi
- Office DLL yadroni o'z ichiga oladi (alohida YangiAlifbo.Core.dll yo'q)
- Diagnostika jurnali %LOCALAPPDATA%\YangiAlifbo\log.txt

## v1.0.0 — 2026-11-22
- O'girish yadrosi: Kirill → Yangi lotin, Lotin (eski/yangi) → Kirill, Eski lotin → Yangi lotin
- Word, Excel, PowerPoint (2007+) uchun «Надстройки» varag'ida «Yangi Alifbo» guruhi
- Formatlash so'z darajasida saqlanadi; Word'da hyperlink/maydonlar buzilmaydi; Excel'da formulalar va "00123" matnlari o'zgarmaydi
- Administrator talab qilmaydigan o'rnatuvchi (/install, /uninstall)
