# Yangi Alifbo v2 — litsenziya (Boshqaruv markazi orqali). Dizayn

Sana: 2026-11-22 · Holat: **egasi tasdig'ini kutmoqda**

## 1. Egasi qarorlari (22.11.2026)
- Faollashtirish: **litsenziya kaliti** `YA-XXXX-XXXX-XXXX`, Markaz beradi
- Narx modeli: **bir martalik to'lov, 1 kompyuter**, muddatsiz
- Litsenziyasiz: **7 kun bepul sinov** (to'liq ishlaydi), keyin o'girish tugmalari faollashtirish oynasini ochadi
- Server ishi: **shu chatda** (`xurshidx/dell-platform`, tarmoq `feat/7-crm-vlapp`)

## 2. Qismlar va versiyalar
| Qism | Versiya | Joyi |
|---|---|---|
| Server moduli `Server_v4/alifbo.js` | platforma v2.11.0, server 4.32.0 | dell-platform |
| Markaz bo'limi «Yangi Alifbo» | Markaz v22 (v21 o'zgarmaydi) | dell-platform |
| Office qo'shimchasi: litsenziya tekshiruvi va oyna | Yangi Alifbo v2.0.0 | yangi-alifbo |

Ish tartibi: server + testlar → Markaz v22 → qo'shimcha → Windows'da (Office 2007) sinov → VDS (egasi ruxsati bilan) → haqiqiy faollashtirish sinovi.

## 3. Server (`alifbo.js`)
Ma'lumot `DATA_DIR/alifbo.json` (PostgreSQL ko'zgusi bilan, boshqa modullar kabi):
```
keys:   { <KEY>: { key, buyer:{name,phone}, price, note, createdAt, by,
                   status:'active'|'revoked', machine:{hwid,name,activatedAt,lastSeen}|null,
                   history:[{at,event,hwid,by}] } }
trials: { <hwid>: { firstSeen, lastSeen, name } }
```

Ochiq API (sessiyasiz, IP bo'yicha so'rov chegarasi bilan):
| So'rov | Vazifa |
|---|---|
| `POST /api/alifbo/trial {hwid,name,version}` | sinov boshlangan sanani qaytaradi (birinchi marta yoziladi). Dasturni qayta o'rnatish sinovni yangilamaydi |
| `POST /api/alifbo/activate {key,hwid,name,version}` | kalit faol va bog'lanmagan (yoki shu kompyuter) → bog'laydi, **imzolangan litsenziya** qaytaradi. Boshqa kompyuterga bog'langan → 409 "Bu kalit boshqa kompyuterda faollashtirilgan" |
| `POST /api/alifbo/check {key,hwid}` | `active` / `revoked` / `moved` (haftada bir) |

Admin API (`/api/admin/alifbo/*`, faqat markaz.dellcom.uz, admin seansi):
ro'yxat va statistika, kalit yaratish (bir nechta, xaridor ismi/telefoni, narx, izoh),
**kompyuterni ajratish** (mijoz yangi kompyuterga o'tsa), bekor qilish/tiklash, tarix.

Imzo: **RSA-2048 + SHA-256**. Maxfiy kalit serverda (`DATA_DIR/alifbo_sign.pem`, 0600, birinchi ishga tushishda yaratiladi), ochiq kalit qo'shimchaga joylanadi.
Ed25519 emas: .NET 4.0 (Office 2007 / Windows 7) da yo'q.
Litsenziya: `{v:1,key,hwid,type:'perpetual',issuedAt}` + imzo (base64).

## 4. Markaz v22 — «Yangi Alifbo» bo'limi
- Kartalar: sotilgan kalitlar soni, faollashtirilgan, jami tushum (so'm), faol sinovlar
- Jadval: kalit, xaridor, telefon, narx, sana, holat, kompyuter nomi, oxirgi ko'rilgan; qidiruv
- Tugmalar: «Kalit yaratish», «Nusxalash», «Kompyuterni ajratish», «Bekor qilish/Tiklash», «Tarix»
- Markaz o'zbekcha (9c qarori)

## 5. Qo'shimcha (Yangi Alifbo v2.0.0)
- **Kompyuter ID (hwid)**: `SHA-256(MachineGuid)` (`HKLM\SOFTWARE\Microsoft\Cryptography`), 32 belgi. Windows qayta o'rnatilsa o'zgaradi — unda Markazda «Kompyuterni ajratish»
- Litsenziya fayli `%APPDATA%\YangiAlifbo\license.dat`: imzo va hwid har tugma bosilganda tekshiriladi (internetsiz ham)
- **Sinov**: birinchi ishga tushishda server sanasi olinadi. Internet bo'lmasa — mahalliy sana, internet chiqqach server sanasi ustun. 7 kundan keyin o'girish tugmalari faollashtirish oynasini ochadi
- **Bekor qilinganini tekshirish**: haftada bir `check` (internet bo'lsa). Internetsiz — litsenziya ishlayveradi
- Lentaga 5-tugma **«Litsenziya»**: holat ("Sinov: 5 kun qoldi" / "Faollashtirilgan") + faollashtirish oynasi (kalit maydoni, «Faollashtirish», sotib olish uchun aloqa)
- Server manzili: `https://pos.dellcom.uz/api/alifbo/*` (VDS da tekshiriladi). Windows 7 + .NET 4.x uchun TLS 1.2 yoqiladi
- Xatolar o'zbekcha; tarmoq xatosi o'girishni to'xtatmaydi (sinov yoki litsenziya mahalliy tekshiriladi)

## 6. Himoya darajasi (ochiq aytilgan)
Bu halol foydalanuvchilarni tartibga soladigan himoya. Tajribali dasturchidan to'liq himoya emas:
.NET kodini dekompilyatsiya qilib tekshiruvni o'chirish mumkin. Kalitni boshqa kompyuterda ishlatish, sinovni qayta o'rnatib uzaytirish va soxta litsenziya yasash
server va imzo bilan yopiladi. Kuchliroq himoya (obfuskatsiya) — keyin, kerak bo'lsa.

## 7. Ochiq savollar (egasiga)
1. Narx: bir martalik litsenziya necha so'm? (Markazda kalit yaratishda o'zgartirish mumkin, bu — standart qiymat)
2. Faollashtirish oynasida sotib olish uchun qaysi aloqa: telefon / Telegram?
3. Yangi Alifbo kodi uchun alohida yopiq GitHub repo (`xurshidx/yangi-alifbo`) ochilsinmi? Hozir u faqat kompyuteringizda (`C:\Projects\YangiAlifbo`)
