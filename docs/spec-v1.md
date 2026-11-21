# Yangi Alifbo (DELL COMPUTER SERVIZ) — Texnik topshiriq, v1

Sana: 2026-11-22 · Holat: tasdiqlangan dizayn (foydalanuvchi: "1-aytganingdek qilamiz")

## 1. Maqsad

O'zbek tili uchun yangi lotin alifbosi (O'RQ–1184, 21.10.2026; 28 harf + tutuq belgisi)
bilan ishlash dasturi. Andoza: Microsoft Store'dagi "Yangi Alifbo" (Fazo Creative Group,
ID 9NB22N371BDX) — faqat ishlash mantiqi olinadi, kod ko'chirilmaydi.
Bizning asosiy qo'shimcha: Word, Excel, PowerPoint ichida «Надстройки» (Add-ins)
varag'ida o'girish tugmalari.

## 2. Bosqichlar

| Versiya | Tarkib |
|---|---|
| **v1 (shu hujjat)** | O'girish yadrosi + Office qo'shimchasi (Word/Excel/PowerPoint) + per-user o'rnatuvchi |
| v2 | Litsenziya: Boshqaruv markazi (markaz.dellcom.uz) API, apparat ID, oflayn muddat |
| v3 | Fon dasturi: yozayotganda avto-almashtirish, tez yozish oynasi (Ctrl+Alt+Space), yoqish/o'chirish (Ctrl+Alt+Y), o'yin/terminalda o'chish, fayl va papka o'girish |

## 3. Alifbo qoidalari

Yangi harflar: G'→Ğ, O'→Ö, Sh→Ş, Ch→Ç. Qolgan harflar o'zgarmaydi. "ng" o'zgarmaydi.
Tutuq belgisi har doim **ʼ (U+02BC)** ko'rinishida chiqariladi.
Natija hech qachon eski lotinda chiqmaydi.

### 3.1 Eski lotin → Yangi lotin
- o/g + apostrof varianti (`'` U+0027, `` ` `` U+0060, `ʻ` U+02BB, `ʼ` U+02BC, `‘` U+2018, `’` U+2019) → ö/ğ
- sh → ş, ch → ç (katta-kichik saqlanadi: SH/Sh → Ş, sh → ş)
- s + apostrof + h (Is'hoq) → `sʼh` (Isʼhoq) — ajratuvchi saqlanadi
- boshqa apostroflar (maʼno) → ʼ

### 3.2 Kirill → Yangi lotin
а a, б b, в v, г g, д d, ж j, з z, и i, й y, к k, л l, м m, н n, о o, п p, р r, с s,
т t, у u, ф f, х x, ш ş, ч ç, щ ş, ў ö, қ q, ғ ğ, ҳ h, ы i, э e, ъ ʼ, ь (tushadi),
ё yo, ю yu, я ya.
- ъ dan keyin е/ё/ю/я kelsa ъ tushadi (съезд → syezd), aks holda ʼ (маъно → maʼno)
- е → **ye**: so'z boshida, unli, ъ yoki ь dan keyin; aks holda **e**
- ц → **ts**: unlidan keyin; aks holda **s**
- Ikki harfli natija (Yo, Yu, Ya, Ye, Ts): so'zning barcha harflari katta bo'lsa (≥2 harf) → YO/YU/YA/YE/TS, aks holda Yo...
- Kirill matn ichidagi lotin harflar (Windows) o'zgarmaydi

### 3.3 Lotin (eski ham, yangi ham) → Kirill
- Kirishda eski shakllar ham qabul qilinadi (o', g', sh, ch), keyin yangi: ö ў, ğ ғ, ş ш, ç ч
- yo ё, yu ю, ya я, ye е; q қ, h ҳ, x х, j ж, y й
- e → **э**: so'z boshida yoki unlidan keyin; aks holda е
- ts → **тс** (ketsa → кетса); ц faqat istisnolar lug'ati orqali
- apostrof (tutuq) → ъ; s'h → сҳ
- Istisnolar lug'ati (so'z boshi bo'yicha, katta-kichik e'tiborsiz): litsey→лицей, sirk→цирк,
  konsert→концерт, sentr→центр, tsex→цех, prinsip→принцип, aksiya→акция,
  stansiya→станция, kompyuter→компьютер, oktyabr→октябрь, sentyabr→сентябрь,
  noyabr→ноябрь, dekabr→декабрь, fevral→февраль

### 3.4 So'z (token) ta'rifi
Harflar ketma-ketligi; apostrof varianti faqat ikki harf orasida bo'lsa so'z ichida hisoblanadi.
Lotin→Kirill va Eski→Yangi yo'nalishida **c** (h siz) yoki **w** harfi bor so'z chet so'z
hisoblanib o'tkazib yuboriladi (Microsoft, Windows).

## 4. Office qo'shimchasi

- Texnologiya: C# shared COM add-in (IDTExtensibility2 + IRibbonExtensibility), late binding
  (`dynamic`), PIA/VSTO yo'q → Office **2007, 2010, 2013, 2016, 2019, 2021, 365**, 32 va 64-bit
- Lenta: `idMso="TabAddIns"` («Надстройки»), guruh «Yangi Alifbo», tugmalar:
  «Kirill → Lotin», «Lotin → Kirill», «Eski → Yangi lotin», «Haqida»
- Interfeys matnlari yangi lotin alifbosida
- Ish doirasi: belgilangan matn; hech narsa belgilanmagan bo'lsa — butun hujjat
  (Word: barcha story'lar — asosiy matn, kolontitullar, izohlar, matn maydonlari;
  Excel: faol varaqning UsedRange; PowerPoint: barcha slaydlar, guruh va jadvallar ichi)
- Formatlash: so'z darajasida saqlanadi (so'z ichidagi aralash formatda so'zning birinchi
  harfi formati olinadi). Excel'da katak formati saqlanadi, katak ichidagi aralash format yo'qoladi
- Excel'da formula kataklari o'zgartirilmaydi
- Bekor qilish: Word'da Ctrl+Z (2010+ da bitta qadam sifatida); Excel'da bekor qilib bo'lmaydi →
  butun varaqni o'girishdan oldin tasdiq so'raladi
- Natija xabari: "N ta söz ögirildi."; hujjat ochiq bo'lmasa yoki himoyalangan bo'lsa — xabar, qulamaydi

## 5. O'rnatish

`YangiAlifbo.Setup.exe` (administrator kerak emas): fayllarni `%LOCALAPPDATA%\YangiAlifbo\`
ga ko'chiradi, COM'ni HKCU da (32 va 64 ko'rinishda) ro'yxatdan o'tkazadi,
Word/Excel/PowerPoint `Addins\YangiAlifbo.Connect` kalitlarini yozadi (LoadBehavior=3).
`/uninstall` hammasini o'chiradi. Talab: .NET Framework 4.x (Windows 10 da bor).

## 6. Ochiq savollar
- ~~Is'hoq tipidagi so'zlar~~ — HAL QILINDI (2026-11-22): **Isʼhoq** (ajratuvchi tutuq saqlanadi)
- Istisnolar lug'atini kengaytirish (foydalanuvchi qo'shishi mumkin bo'lsinmi — v3)
