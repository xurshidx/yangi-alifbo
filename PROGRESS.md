# Yangi Alifbo — PROGRESS

Topshiriq: `docs/spec-v1.md` · Reja: `docs/plans/2026-11-22-v1-office-qoshimcha.md` · Branch: `v1-office`

| # | Bosqich | Holat | Tekshiruv |
|---|---|---|---|
| 1 | Tokenizer, fasad, test-runner | bajarildi | CoreTests (mono) |
| 2 | Eski lotin → Yangi lotin | bajarildi | CoreTests (mono) |
| 3 | Kirill → Yangi lotin | bajarildi | CoreTests (mono) |
| 4 | Lotin → Kirill | bajarildi | CoreTests (mono): 82/82 |
| 5 | COM qobiq, lenta, o'rnatuvchi | kod yozildi | mono kompilyatsiya OK; Windows tekshiruvi kutilmoqda |
| 6 | Word | kod yozildi | maydon kodlari testi (mono) OK; Windows smoke kutilmoqda |
| 7 | Excel | kod yozildi | Windows smoke kutilmoqda |
| 8 | PowerPoint | kod yozildi | Windows smoke kutilmoqda |
| 9 | Paket va yakuniy tekshiruv | — | Windows |

## Qarorlar
- 2026-11-22: "Is'hoq" → "Isʼhoq" (foydalanuvchi tasdiqladi)
- 2026-11-22: Word'da maydon kodlari (hyperlink va h.k.) IncludeFieldCodes bilan o'qiladi va o'zgartirilmaydi
- 2026-11-22: Excel'da faqat o'zgargan kataklar yoziladi; = + - @ bilan boshlangan matnga ' prefiksi
