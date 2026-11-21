# v1 qo'lda tekshiruv ro'yxati (Windows)

Sana: ____ · Office versiyasi/bitligi: ____ · Tekshiruvchi: ____

## Avtomatik
- [ ] `build.cmd smoke` → CoreTests, OfficeSmoke word/excel/ppt — hammasi `FAIL: 0`

## O'rnatish
- [ ] `out\YangiAlifbo.Setup.exe` → "örnatildi" xabari
- [ ] Word, Excel, PowerPoint: «Надстройки» varag'ida «Yangi Alifbo» guruhi, 4 tugma
- [ ] «Haqida» oynasi versiya 1.0.0 bilan chiqadi

## Word
- [ ] Kirill hujjat, hech narsa belgilanmagan → Kirill → Lotin: butun hujjat, kolontitul, jadval, matn maydoni
- [ ] Bitta paragraf belgilangan → faqat u o'giriladi
- [ ] Qalin/kursiv/rang so'zlarda saqlanadi
- [ ] Hyperlink ishlaydi va manzili o'zgarmagan
- [ ] Ctrl+Z bitta qadamda qaytaradi (Word 2010+)
- [ ] Lotin → Kirill, Eski → Yangi lotin — xuddi shunday
- [ ] Hujjat ochiq emas → "Ögiriladigan matn topilmadi." (qulamaydi)

## Excel
- [ ] Bitta katak belgilangan → tasdiq oynasi; "Yo'q" → hech narsa o'zgarmaydi
- [ ] "Ha" → butun varaq; formulalar, sonlar, "00123" matnlari o'zgarmaydi
- [ ] Bir nechta katak belgilangan → tasdiqsiz faqat ular

## PowerPoint
- [ ] Hech narsa belgilanmagan → barcha slaydlar, guruhlar, jadvallar
- [ ] Shakl belgilangan → faqat u; matn belgilangan → faqat matn

## O'chirish
- [ ] `YangiAlifbo.Setup.exe /uninstall` → Office'ni qayta ochganda guruh yo'q
