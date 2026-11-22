YANGI ALIFBO v3.1.0 — Word, Excel, PowerPoint qo'shimchasi + Windows'da avto-tuzatish
DELL COMPUTER SERVIZ

O'RNATISH — ikki usuldan biri
A) YangiAlifbo_v3.1.0.msi (tavsiya): ikki marta bosing → «Далее» → «Установить». Dastur «Программы и компоненты»
   ro'yxatida chiqadi, o'sha yerdan o'chiriladi; yangi versiya eskisining ustidan o'rnatiladi. Barcha foydalanuvchilar uchun.
B) YangiAlifbo.Setup.exe: Word, Excel va PowerPoint'ni yoping va ishga tushiring — Windows administrator ruxsatini so'raydi (UAC).
   Faqat joriy foydalanuvchi uchun. Ikkalasini bir vaqtda ishlatmang: Setup.exe bilan o'rnatilgan bo'lsa,
   MSI dan oldin «YangiAlifbo.Setup.exe /uninstall» qiling.
Keyin:
3. Dasturlarni qayta oching — «Надстройки» (Add-ins) varag'ida «Yangi Alifbo» guruhi chiqadi.
4. Soat yonida ko'k «Ö» belgisi paydo bo'ladi — bu avto-tuzatish (kompyuter yonganda o'zi ishga tushadi).

AVTO-TUZATISH (v3)
Istalgan dasturda (Telegram, brauzer, Word, Bloknot...) eski lotinda yozing — so'z bo'sh joy bilan
tugaganda o'zi yangi lotinga almashadi: o'zbek → özbek, bog' → boğ, shahar → şahar, choy → çoy.
- Pause tugmasi yoki belgini ikki marta bosish — yoqish/o'chirish (o'chiq bo'lsa belgi kulrang).
- Noto'g'ri almashsa — darhol Backspace: so'z qaytadi va boshqa tegilmaydi (istisnolarga yoziladi).
- Enter/Tab bilan tugagan so'z faqat shu qatorda oldin bo'sh joy bo'lsa almashadi
  (bitta so'zli login va parollarga tegmaslik uchun). Parol maydonlari, raqamli so'zlar,
  sayt va pochta manzillari (dellcom.uz, ism@mail.uz) o'zgartirilmaydi.
- Ishlamaydigan dasturlar: cmd, PowerShell, VS Code, o'yinlar va h.k. Ro'yxat: belgi → «Tegilmaydigan dasturlar…».
- Istisno so'zlar: belgi → «Istisno so'zlar…» (Bloknotda ochiladi, har qatorda bitta so'z).
- Administrator huquqida ochilgan oynalarda ishlamaydi (Windows cheklovi).
- Dastur faqat hozir yozilayotgan so'zni xotirada ushlaydi; hech narsa diskka yozilmaydi va internetga yuborilmaydi.

TUGMALAR
- Kirill → Lotin      : kirill matnni yangi lotin alifbosiga (Ö, Ğ, Ş, Ç)
- Lotin → Kirill      : eski yoki yangi lotin matnni kirillga
- Eski → Yangi lotin  : O', G', Sh, Ch → Ö, Ğ, Ş, Ç
- Litsenziya          : holat va kalitni faollashtirish
- Haqida

Matn belgilangan bo'lsa — faqat u o'giriladi. Hech narsa belgilanmagan bo'lsa — butun hujjat.
Word'da Ctrl+Z bilan bekor qilsa bo'ladi. Excel'da bekor qilib bo'lmaydi, shuning uchun
butun varaqni o'girishdan oldin tasdiq so'raladi.

MUHIM
Agar Office "«yangi alifbo» qo'shimchasida jiddiy xato... o'chirilsinmi?" deb so'rasa — «Нет / Yo'q».
Agar qo'shimcha chiqmasa: YangiAlifbo.Setup.exe ni qayta ishga tushiring
(u Office o'chirib qo'ygan qo'shimchani qayta yoqadi).
Diagnostika jurnali: %LOCALAPPDATA%\YangiAlifbo\log.txt

LITSENZIYA
7 kun bepul sinov (hamma narsa ishlaydi). Bitta kalit Office qo'shimchasini ham, avto-tuzatishni ham ochadi. Keyin litsenziya kaliti kerak: YA-XXXX-XXXX-XXXX.
Bir martalik to'lov, 1 kompyuter, muddatsiz. Kalitni «Litsenziya» tugmasi orqali kiriting
(birinchi faollashtirishda internet kerak, keyin internetsiz ishlaydi).
Yangi kompyuterga o'tsangiz — sotuvchiga murojaat qiling, u kalitni eski kompyuterdan ajratadi.

O'CHIRISH
MSI bilan o'rnatilgan bo'lsa — «Программы и компоненты» → Yangi Alifbo → «Удалить».
Setup.exe bilan o'rnatilgan bo'lsa — buyruq satrida: YangiAlifbo.Setup.exe /uninstall
Litsenziya va sinov ma'lumoti (%APPDATA%\YangiAlifbo) o'chirilmaydi — qayta o'rnatilganda kalit kerak emas.

TALABLAR
Windows 7/8/10/11, .NET Framework 4.x. Office qo'shimchasi uchun — Microsoft Office 2007 yoki yangiroq (32 yoki 64-bit);
avto-tuzatish Office'siz ham ishlaydi.
