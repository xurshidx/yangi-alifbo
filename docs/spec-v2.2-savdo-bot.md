# Yangi Alifbo — savdo boti (dizayn)

Holat: **tasdiqlandi** 23.11.2026 · amalga oshirildi: dell-platform **v2.13.0** (server 4.34.0, Markaz v24)
Egasi qarorlari: bitta bot (siz va mijozlar); menyu + bot ichida sotish + AI javob + savol sizga o'tadi; o'zbek va rus tili.

## 1. Mijoz nimani ko'radi
1. `/start` → **til tanlash** (🇺🇿 O'zbekcha / 🇷🇺 Русский) → salom va pastda doimiy menyu:
   `💰 Narx va sotib olish` · `✨ Imkoniyatlar` · `⬇️ Yuklab olish` · `❓ Savollar` · `💬 Operator`
2. **Imkoniyatlar / Savollar / Yuklab olish** — tayyor javoblar (saytdagi matnlar asosida), `.msi` havolasi, o'rnatish yo'riqnomasi.
3. **Sotib olish** (bot ichida):
   - qo'lda rejim: buyurtma raqami + rekvizitlar + `[✅ To'ladim]` → sizga hozirgidek tasdiq tugmalari keladi →
     ✅ bosilganda **kalit mijozning chatiga yuboriladi** (faollashtirish yo'riqnomasi bilan);
   - Payme rejimi: `[💳 Payme orqali to'lash]` tugmasi → to'lov o'tishi bilan kalit chatga keladi.
   - Dasturdan kelgan bo'lsa (`t.me/<bot>?start=b_<kompyuter ID>`) — kalit shu kompyuterga bog'lanadi va dastur o'zi faollashadi.
4. **Erkin savol** (masalan «Windows 7 da ishlaydimi?») → **AI javob beradi** — faqat Yangi Alifbo haqida, qisqa, mijoz tilida.
   AI bilmasa yoki mijoz `💬 Operator` ni bossa → savol sizga o'tadi: «Mijoz javobingizni kutyapti».

## 2. Siz nimani ko'rasiz (o'sha bot)
- Hozirgi xabarlar (buyurtma, «To'ladim» + ✅/❌, Payme sotuvi) o'zgarmaydi.
- **Mijoz savoli**: `💬 Aziz (@aziz, 🇺🇿): Windows XP da ishlaydimi?` — shu xabarga Telegramda **«Ответить / Javob berish»** (reply) qilib yozsangiz,
  javobingiz bot nomidan mijozga boradi. AI javob bergan savollar sizga kelmaydi (Markazda sozlama bilan «hammasi menga kelsin» qilsa bo'ladi).
- `/mijozlar` — oxirgi 10 ta suhbat.

## 3. AI
- Serverdagi OpenAI kaliti (POS'dagi AI yordamchi bilan bir xil); savol + qisqa mahsulot ma'lumotnomasi (narx, imkoniyatlar, talablar, o'rnatish, litsenziya qoidalari).
- Faqat dastur haqida javob beradi; boshqa mavzu, narxni o'zgartirish, va'da berish — yo'q; ishonchi bo'lmasa — operatorga.
- Cheklov: bitta mijozga kuniga 20 ta AI javob, oyiga jami 2 000 ta (Markazda o'zgartiriladi); AI o'chirilsa — hamma savol sizga.

## 4. Markaz v24 («Sotuv sozlamalari» → «Telegram bot»)
- Mijoz menyusi yoqilgan/o'chirilgan, AI yoqilgan/o'chirilgan, oylik AI chegarasi, «hamma savol menga kelsin».
- Statistika: mijozlar soni (uz/ru), bot orqali sotuvlar, AI javoblari (oy), operatorga o'tgan savollar.
- Bot ulanganda nomi, tavsifi va buyruqlari (/start, /til, /sotib_olish) o'zi o'rnatiladi.

## 5. Ma'lumot va xavfsizlik
- `alifbo.json → shop.bot.customers` {chat: {til, ism, @username, birinchi/oxirgi xabar}}; suhbat matni saqlanmaydi
  (faqat sizga yuborilgan savol ↔ mijoz bog'lanishi, oxirgi 500 ta — javob berish uchun).
- Egasi chatlari (`/start <kod>` bilan ulangan) mijoz menyusini ko'rmaydi; mijoz egasi tugmalarini bosa olmaydi (hozirgidek tekshiriladi).
- Spam: bitta mijozdan daqiqasiga 10 ta xabar; ortig'i javobsiz.

## 6. Tekshiruv
- Soxta Telegram + soxta OpenAI bilan server testlari: til, menyu, sotib olish (qo'lda va Payme), kalit chatga, deep-link bilan kompyuterga bog'lanish,
  AI javobi, operatorga o'tish va reply orqali javob, chegaralar, egasi/mijoz ajratilishi. Markaz e2e. Keyin VDS (ruxsatingiz bilan) va haqiqiy bot bilan sinov.
