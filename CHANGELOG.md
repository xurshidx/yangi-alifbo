# CHANGELOG

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
