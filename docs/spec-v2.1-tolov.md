# Yangi Alifbo — onlayn sotish va avtomatik faollashtirish (dizayn)

Holat: **tasdiqlandi** 23.11.2026 · amalga oshirildi: dell-platform v2.12.0, yangi-alifbo v3.2.0, `site/alifbo/`
Versiyalar: dell-platform **v2.12.0** (server 4.33.0, Markaz v23) · yangi-alifbo **v3.2.0** · dellcom.uz/alifbo sahifasi
Egasi qarorlari: Payme Business (ro'yxatdan o'tgan, biznes tasdiqlangan, kassa hali yo'q) · yangi alohida Telegram bot · sahifa dellcom.uz/alifbo (taxmin, egasi javob bermagan).

## 1. Xaridor nimani ko'radi
1. Dasturda «Litsenziya» → **«Sotib oliş»** tugmasi (yoki saytdan to'g'ridan-to'g'ri) → brauzerda
   `https://dellcom.uz/alifbo/sotib-olish?hw=<kompyuter ID>` ochiladi.
2. Sahifa: narx (15 000 so'm), ism va telefon (ixtiyoriy, chek va aloqa uchun), oferta bilan rozilik → **«Payme orqali to'lash»**.
3. Payme to'lov sahifasi (karta, SMS kod) → to'lovdan keyin saytga qaytadi: **«To'lov qabul qilindi. Kalitingiz: YA-XXXX-XXXX-XXXX»**.
4. **Dastur o'zi faollashadi** (oyna ochiq bo'lsa 5 soniyada, aks holda «Ö» bir daqiqa ichida). Kalitni kiritish shart emas.
   Saytdan dasturisiz sotib olinsa — kalit sahifada ko'rsatiladi, xaridor uni dasturga o'zi kiritadi.

## 2. Ikki rejim (Markaz sozlamasida almashtiriladi)
- **Qo'lda (hozir, kassa ochilguncha):** sahifa to'lov rekvizitlarini (Markazda o'zingiz yozgan matn) va buyurtma raqamini ko'rsatadi,
  xaridor «To'ladim» bosadi → Telegram botga xabar: buyurtma, ism, telefon, summa va **[✅ To'landi] [❌ Bekor]** tugmalari.
  ✅ bosilganda kalit yaratiladi, xaridor kompyuteriga bog'lanadi, dastur o'zi faollashadi.
- **Payme (kassa ochilgach):** to'liq avtomat. Telegram faqat xabar beradi: «💰 Yangi Alifbo sotildi: 15 000 so'm, YA-…».

## 3. Server (dell-platform, `Server_v4/alifbo.js` kengaytiriladi, yangi `alifbo_pay.js`)
**Buyurtma** (`alifbo.json` → `orders`): raqam (7 xonali, Payme'da «Buyurtma raqami» rekviziti), hwid (bo'lishi mumkin), ism, telefon,
summa, holat `new | paid | cancelled`, usul `payme | manual`, kalit, maxfiy `token` (holat sahifasi uchun), vaqtlar.

Ochiq API (CORS faqat `https://dellcom.uz`):
- `POST /api/alifbo/order` — buyurtma yaratish (IP bo'yicha soatiga 20 ta chegara) → raqam, token, Payme havolasi yoki rekvizit matni.
- `GET /api/alifbo/order/<raqam>?t=<token>` — holat; to'langan bo'lsa kalit.
- `POST /api/alifbo/order/<raqam>/paid` — qo'lda rejimda «To'ladim» (faqat xabar yuboradi, hech narsani tasdiqlamaydi).
- `POST /api/alifbo/claim {hwid}` — dastur so'raydi: shu kompyuter uchun to'langan buyurtma bo'lsa → imzolangan litsenziya.

**Payme Merchant API** — `POST /api/alifbo/payme` (Endpoint URL). JSON-RPC: CheckPerformTransaction, CreateTransaction,
PerformTransaction, CancelTransaction, CheckTransaction, GetStatement (+ SetFiscalData).
- Kirish: `Authorization: Basic Paycom:<kalit>`; faqat Payme IP manzillari (185.234.113.1–15) — Nginx'da ham.
- Summa tiyinda (15 000 so'm = 1 500 000). Noto'g'ri summa/buyurtma/holat — Payme hujjatidagi xato kodlari.
- **Kalit faqat PerformTransaction'da** yaratiladi (pul yechilgandan keyin). Bekor qilinsa (CancelTransaction, holat −2) — kalit bekor qilinadi.
- 12 soatda tugallanmagan tranzaksiya — bekor (Payme qoidasi). Bitta buyurtmaga bitta faol tranzaksiya.
- **Fiskal chek:** CheckPerformTransaction javobida `detail` — mahsulot nomi, **MXIK (IKPU) kodi** va o'lchov birligi kodi (sizdan kerak, 8-band).
- Kalitlar (test va haqiqiy) Markazda kiritiladi, serverda shifrlangan holda saqlanadi; chatga yozilmaydi.

**Telegram bot** (`alifbo_tg.js`): token Markazda kiritiladi → webhook `/api/alifbo/tg/<maxfiy>`. Markazda «Botni ulash» kodi chiqadi,
siz botga `/start <kod>` yozasiz — faqat shu chat(lar) xabar oladi va tugmalarni bosa oladi.

## 4. Markaz v23 («Dasturlar → Yangi Alifbo»)
- Yangi **«Buyurtmalar»** ro'yxati: raqam, sana, xaridor, summa, usul, holat, kalit; qo'lda «To'landi / Bekor qilish» tugmalari (Telegram ishlamasa ham).
- Sozlamalar: rejim (Qo'lda / Payme), qo'lda rejim rekvizit matni, Payme kassa ID, test/haqiqiy kalit, test rejimi belgisi, Telegram bot tokeni.
- Statistika: sotuvlar va tushum Payme/qo'lda bo'yicha.

## 5. Sayt — `dellcom.uz/alifbo` (statik, VDS `/var/www/dellcom-site/alifbo/`)
- `index.html` — dastur haqida (Office qo'shimchasi + avto-tuzatish), skrinshotlar, narx, yuklab olish (.msi), «Sotib olish».
- `sotib-olish.html` — buyurtma formasi va holat; `oferta.html` — ommaviy oferta (qoralama — siz tekshirasiz); aloqa.
- Mavjud sayt uslubida, o'zbekcha (yangi lotin); sitemap'ga qo'shiladi.

## 6. Dastur — yangi-alifbo v3.2.0
- Litsenziya oynasida **«Sotib oliş»** tugmasi (Telegram havolasi o'rniga) → sahifani kompyuter ID bilan ochadi.
- Oyna ochiq turganda har 5 soniyada `claim`; tugma bosilgandan keyin «Ö» 24 soat davomida har daqiqada `claim` → litsenziya kelsa saqlanadi,
  «Litsenziya faollaştirildi. Rahmat!» xabari. Sinov tugagan oynada ham shu tugma.

## 7. Xavfsizlik va tekshiruv
- Kalit **faqat** Payme tasdig'i (PerformTransaction) yoki siz (Telegram/Markaz) tasdiqlaganda beriladi; saytdagi «muvaffaqiyatli» sahifa hech narsa bermaydi.
- Bir buyurtma — bitta kalit (takroriy so'rovlar xavfsiz). Hamma hodisa audit jurnaliga.
- Testlar: Payme'ning sandbox ssenariylari (noto'g'ri kalit, summa, buyurtma, takroriy Create/Perform, bekor qilish, GetStatement) — avtomatik test sifatida;
  Telegram — soxta Telegram serveri bilan; Markaz e2e; dastur — `claim` bilan LicenseSmoke.
- Tartib: avval sinov serverida → VDS'ga o'rnatishdan oldin sizdan ruxsat (zaxira bilan, oldingidek). Payme test kassasi ochilgach — test.paycom.uz bilan haqiqiy sinov.

## 8. Sizdan kerak bo'ladi
1. **Payme kassa** (qo'llab-quvvatlash javobidan keyin): Endpoint URL `https://pos.dellcom.uz/api/alifbo/payme`, rekvizit nomi `order_id`.
2. **MXIK (IKPU) kodi** — dasturiy ta'minot litsenziyasi uchun (tasnif.soliq.uz), o'lchov birligi kodi. Bilmasangiz, birga topamiz.
3. **Yangi Telegram bot**: @BotFather → `/newbot` → tokenni Markaz sozlamasiga (chatga emas).
4. Qo'lda rejim uchun to'lov rekvizitlari matni (Markaz sozlamasiga).
5. Oferta matnini o'qib tasdiqlash.

## 9. Bosqichlar
1. Server: buyurtmalar + claim + qo'lda rejim + Telegram bot → testlar
2. Server: Payme Merchant API → sandbox ssenariy testlari
3. Markaz v23
4. Dastur v3.2.0
5. Sayt sahifalari + oferta
6. VDS'ga o'rnatish (ruxsat bilan) → qo'lda rejim haqiqiy sinov → Payme kassa ochilgach test.paycom.uz sinovi → haqiqiy rejim
