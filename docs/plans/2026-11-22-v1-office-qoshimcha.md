# Yangi Alifbo v1 — O'girish yadrosi va Office qo'shimchasi: Amalga oshirish rejasi

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Word, Excel, PowerPoint (2007+) «Надстройки» varag'ida Kirill→Lotin, Lotin→Kirill, Eski→Yangi lotin tugmalarini beradigan, formatlashni saqlaydigan qo'shimcha va uning yadrosi.

**Architecture:** Toza C# yadro (`YangiAlifbo.Core.dll`) so'z darajasida o'giradi va matn uchun "tahrirlar ro'yxati"ni (offset, uzunlik, yangi matn) quradi. Office qatlami (`YangiAlifbo.Office.dll`) late-binding (`dynamic`) bilan hujjat bo'laklarini o'qiydi, yadrodan tahrirlarni olib, oxiridan boshiga qo'llaydi. `YangiAlifbo.Setup.exe` HKCU'da COM va Office kalitlarini yozadi.

**Tech Stack:** C# 5, .NET Framework 4.0 (csc.exe `v4.0.30319`), COM interop, Ribbon XML (2006/01 schema). Tashqi paket yo'q.

**Spec:** `docs/spec-v1.md`

## Global Constraints

- Kompilyator: `C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe` → faqat **C# 5** (string interpolation, `?.`, `=>` a'zolar YO'Q)
- Har bir csc chaqiruvi `/codepage:65001` bilan; manba fayllar UTF-8
- Target: .NET Framework 4.0; havolalar faqat: `System.dll, System.Core.dll, Microsoft.CSharp.dll, System.Windows.Forms.dll, System.Xml.dll`
- Office PIA, VSTO, extensibility.dll, office.dll ishlatilmaydi — COM interfeyslar o'zimizda `[ComImport]` bilan
- Office 2007–365, 32/64-bit; DLL'lar `/platform:anycpu`
- COM CLSID: `894EF783-D4BB-4755-9CFB-7EFC637EAB78`, ProgId: `YangiAlifbo.Connect`
- O'rnatish faqat HKCU, administrator talab qilinmaydi
- Tutuq chiqishi doim `\u02BC`; interfeys matnlari yangi lotinda
- Har bir tugallangan vazifadan keyin `PROGRESS.md` yangilanadi; versiyalar ustidan yozilmaydi

## Review Focus

1. Word paragrafida maydon (hyperlink, sahifa raqami) bo'lsa `Range.Text` offsetlari siljiydi → har bir tahrirdan oldin `doc.Range(a,b).Text == eski so'z` tekshiruvi, mos kelmasa o'tkazib yuborish (Task 6, hyperlinkli smoke test).
2. Apostrofning 6 xil varianti (`' ` ʻ ʼ ‘ ’`) eski lotinda → hammasi ö/ğ ga (Task 2 testlari).
3. Katta harfli so'zlar: ЁШЛИК→YOŞLIK, Ёшлик→Yoşlik, SHAHAR→ŞAHAR (Task 2, 3 testlari).
4. Excel formula katagi o'zgarmasligi kerak (Task 7 smoke testi).
5. Hujjat ochiq emas / himoyalangan / bo'sh belgi → qulamaslik, o'zbekcha xabar (Task 5: barcha callback'lar try/catch; Task 6–8 smoke'da hujjatsiz chaqiruv 0 qaytaradi).

---

## Fayl tuzilmasi

```
src/Core/Direction.cs        enum Direction { CyrToLat, LatToCyr, OldToNew }
src/Core/Tokenizer.cs        so'zlarga ajratish
src/Core/OldToNew.cs         3.1 qoidalari
src/Core/CyrToLat.cs         3.2 qoidalari
src/Core/LatToCyr.cs         3.3 qoidalari + istisnolar
src/Core/Converter.cs        fasad: ConvertWord, ConvertText, PlanEdits
tests/CoreTests.cs           mini test-runner (exit code 0 = OK)
src/Office/ComInterfaces.cs  IDTExtensibility2, IRibbonExtensibility, IRibbonControl
src/Office/Connect.cs        COM klass, lenta callback'lari
src/Office/Ribbon.xml        embedded resource
src/Office/WordConverter.cs, ExcelConverter.cs, PowerPointConverter.cs
src/Setup/Setup.cs           /install, /uninstall
tests/OfficeSmoke.cs         haqiqiy Word/Excel/PowerPoint bilan avtomatik tekshiruv
build.cmd                    hammasini yig'ish → out\
PROGRESS.md, CHANGELOG.md
```

---

### Task 1: Tokenizer, fasad va test-runner

**Files:** Create `src/Core/Direction.cs`, `src/Core/Tokenizer.cs`, `src/Core/Converter.cs`, `tests/CoreTests.cs`, `build.cmd`, `PROGRESS.md`, `CHANGELOG.md`

**Interfaces — Produces:**
- `namespace YangiAlifbo.Core`
- `public struct Token { public int Start; public int Length; public string Text; }`
- `public static class Tokenizer { public static List<Token> Words(string text); public static bool IsApostrophe(char c); }`
- `public struct TextEdit { public int Start; public int Length; public string NewText; }`
- `public static class Converter { string ConvertWord(string w, Direction d); string ConvertText(string t, Direction d); List<TextEdit> PlanEdits(string t, Direction d); }` — `PlanEdits` faqat o'zgargan so'zlarni, **Start kamayish tartibida** qaytaradi
- Test-runner: `static void Eq(string expected, string actual, string name)`; xatolar soni >0 bo'lsa `Environment.Exit(1)`

- [ ] **Step 1: Failing testlar**
```csharp
Eq("3", Tokenizer.Words("Salom, dunyo!  o'zbek").Count.ToString(), "tok_count");
Eq("o'zbek", Tokenizer.Words("Salom, dunyo!  o'zbek")[2].Text, "tok_apos_inside");
Eq("1", Tokenizer.Words("'salom'").Count.ToString(), "tok_edge_apos_excluded");
Eq("salom", Tokenizer.Words("'salom'")[0].Text, "tok_edge_apos_text");
// PlanEdits (Task 3 dan keyin yashil bo'ladi):
var e = Converter.PlanEdits("Салом, дунё!", Direction.CyrToLat);
Eq("7|4|dunyo;0|5|Salom", Join(e), "plan_desc_order");
```
- [ ] **Step 2:** `build.cmd test` → kompilyatsiya xatosi (klasslar yo'q)
- [ ] **Step 3:** Tokenizer: token = `char.IsLetter` ketma-ketligi; apostrof varianti oldi va keyini harf bo'lsa token ichida. `ConvertWord` Direction bo'yicha `OldToNewRules.Word / CyrToLatRules.Word / LatToCyrRules.Word` ga yo'naltiradi (hozircha o'zini qaytaradigan stub). `ConvertText` matnni token bo'yicha qayta yig'adi, token bo'lmagan qismlar o'zgarmaydi (lekin OldToNew'da yakka apostroflar ham `\u02BC` ga — faqat token ichidagilar).
- [ ] **Step 4:** `build.cmd` — `test` argumenti bilan `out\CoreTests.exe` ni yig'ib ishga tushiradi; csc yo'li Global Constraints'dagi. Natija: `tok_*` OK, `plan_*` FAIL (kutilgan)
- [ ] **Step 5:** PROGRESS.md (v1 bosqich jadvali), commit `feat(core): tokenizer va fasad`

### Task 2: Eski lotin → Yangi lotin

**Files:** Create `src/Core/OldToNew.cs`; Modify `tests/CoreTests.cs`
**Interfaces — Produces:** `internal static class OldToNewRules { public static string Word(string w); }`

- [ ] **Step 1: Failing testlar** (`Converter.ConvertText(x, Direction.OldToNew)`)
```
"O'zbekiston"→"Özbekiston"   "g'isht"→"ğişt"      "Oʻgʻil"→"Öğil"
"o‘g’il"→"öğil"              "o`rta"→"örta"       "oʼq"→"öq"
"shahar"→"şahar"  "Shahar"→"Şahar"  "SHAHAR"→"ŞAHAR"
"choy"→"çoy"  "CHOY"→"ÇOY"   "ma'no"→"ma\u02BCno"  "Is'hoq"→"Is\u02BChoq"
"tong"→"tong"  "Microsoft"→"Microsoft"  "Windows"→"Windows"
"Men o'zbekman, sen-chi?"→"Men özbekman, sen-çi?"
```
- [ ] **Step 2:** test → FAIL
- [ ] **Step 3:** Chap-o'ngga skaner: chet so'z tekshiruvi (c h'siz yoki w) → o'zgarishsiz; `s`+apostrof+`h` → `s\u02BCh`; o/g + apostrof → ö/ğ (katta-kichik o/g bo'yicha); s/c + h → ş/ç (katta-kichik birinchi harf bo'yicha); qolgan apostrof → `\u02BC`
- [ ] **Step 4:** test → PASS
- [ ] **Step 5:** commit `feat(core): eski lotin -> yangi lotin`

### Task 3: Kirill → Yangi lotin

**Files:** Create `src/Core/CyrToLat.cs`; Modify `tests/CoreTests.cs`
**Interfaces — Produces:** `internal static class CyrToLatRules { public static string Word(string w); }`

- [ ] **Step 1: Failing testlar** (`Direction.CyrToLat`)
```
"Ўзбекистон"→"Özbekiston"  "шаҳар"→"şahar"  "чой"→"çoy"  "ғишт"→"ğişt"  "қўл"→"qöl"
"хат"→"xat"  "ҳа"→"ha"  "Ер"→"Yer"  "поезд"→"poyezd"  "мева"→"meva"  "съезд"→"syezd"
"цирк"→"sirk"  "лицей"→"litsey"  "маъно"→"ma\u02BCno"  "актёр"→"aktyor"
"компьютер"→"kompyuter"  "Юсуф"→"Yusuf"  "ЯНГИ"→"YANGI"  "ШАҲАР"→"ŞAHAR"
"ЁШЛИК"→"YOŞLIK"  "Ёшлик"→"Yoşlik"  "Салом, дунё!"→"Salom, dunyo!"  "Windows 10"→"Windows 10"
```
plus Task 1 dagi `plan_desc_order` endi PASS.
- [ ] **Step 2:** test → FAIL
- [ ] **Step 3:** Jadval `Dictionary<char,string>` (kichik harflar, spec 3.2); е va ц kontekst qoidalari oldingi kirill harfga qarab (unlilar: а е ё и о у э ю я ў); katta harf: bitta harfli natija `ToUpper`, ikki harfli — so'z to'liq katta (≥2 harf) bo'lsa to'liq, aks holda birinchisi katta
- [ ] **Step 4:** test → PASS (barcha Task 1–3)
- [ ] **Step 5:** commit `feat(core): kirill -> yangi lotin`

### Task 4: Lotin → Kirill

**Files:** Create `src/Core/LatToCyr.cs`; Modify `tests/CoreTests.cs`
**Interfaces — Produces:** `internal static class LatToCyrRules { public static string Word(string w); internal static readonly string[,] Exceptions; }` (spec 3.3 dagi 14 juftlik)

- [ ] **Step 1: Failing testlar** (`Direction.LatToCyr`)
```
"Özbekiston"→"Ўзбекистон"  "O'zbekiston"→"Ўзбекистон"  "Oʻzbekiston"→"Ўзбекистон"
"şahar"→"шаҳар"  "shahar"→"шаҳар"  "SHAHAR"→"ШАҲАР"  "çoy"→"чой"  "choy"→"чой"
"ğişt"→"ғишт"  "g'isht"→"ғишт"  "ekran"→"экран"  "meva"→"мева"  "yer"→"ер"
"poyezd"→"поезд"  "yoşlik"→"ёшлик"  "Yusuf"→"Юсуф"  "maʼno"→"маъно"  "ma'no"→"маъно"
"Is'hoq"→"Исҳоқ"  "ketsa"→"кетса"  "litseyda"→"лицейда"  "Kompyuter"→"Компьютер"
"Microsoft"→"Microsoft"
```
- [ ] **Step 2:** test → FAIL
- [ ] **Step 3:** Avval istisno (so'z boshi, case-insensitive; qoldiq qism umumiy qoida bilan; natija katta-kichigi kirishning birinchi harfi/to'liq katta bo'yicha). Keyin chet so'z tekshiruvi, keyin eski shakllarni (o', g', sh, ch, s'h) va digraf yo/yu/ya/ye ni chapdan uzunroq moslik bo'yicha o'qish; e-qoidasi oldingi lotin harfga qarab
- [ ] **Step 4:** test → PASS
- [ ] **Step 5:** commit `feat(core): lotin -> kirill`

### Task 5: COM qobiq, lenta va o'rnatuvchi

**Files:** Create `src/Office/ComInterfaces.cs`, `src/Office/Connect.cs`, `src/Office/Ribbon.xml`, `src/Setup/Setup.cs`; Modify `build.cmd`

**Interfaces:**
- Consumes: `Converter`, `Direction`
- Produces: `public interface IDocumentConverter { int Convert(dynamic app, Direction d); }` — o'zgargan so'zlar sonini qaytaradi; hujjat yo'q bo'lsa 0. `Connect` app nomiga qarab (`app.Name` ichida "Word"/"Excel"/"PowerPoint") tanlaydi
- COM GUID'lar: IDTExtensibility2 `B65AD801-ABAF-11D0-BB8B-00A0C90F2744`, IRibbonExtensibility `000C0396-0000-0000-C000-000000000046`, IRibbonControl `000C0395-0000-0000-C000-000000000046` (IDispatch)
- Lenta tugma id'lari: `btnCyrToLat`, `btnLatToCyr`, `btnOldToNew`, `btnAbout`; callback `OnButton(IRibbonControl c)`

- [ ] **Step 1:** Ribbon.xml: `customUI xmlns="http://schemas.microsoft.com/office/2006/01/customui"`, `tab idMso="TabAddIns"`, `group id="grpYangiAlifbo" label="Yangi Alifbo"`, 4 ta `size="large"` tugma (yorliqlar spec 4)
- [ ] **Step 2:** Connect: `[ComVisible(true), Guid(...), ProgId("YangiAlifbo.Connect"), ClassInterface(None)]`; `OnConnection` da `app` saqlanadi; `GetCustomUI` resursdan XML; `OnButton` try/catch → xato bo'lsa `MessageBox` "Xatolik: …"; natija "N ta söz ögirildi." / N=0 bo'lsa "Ögiriladigan matn topilmadi."; Haqida: versiya va "DELL COMPUTER SERVIZ"
- [ ] **Step 3:** Setup.cs: `/install` → `%LOCALAPPDATA%\YangiAlifbo\` ga DLL'larni ko'chiradi; `Registry32` va `Registry64` ko'rinishlarida `HKCU\Software\Classes\CLSID\{GUID}\InprocServer32` (Default=`mscoree.dll`, `ThreadingModel=Both`, `Class=YangiAlifbo.Office.Connect`, `Assembly`=to'liq nom, `RuntimeVersion=v4.0.30319`, `CodeBase=file:///…`), `ProgId`, `HKCU\Software\Classes\YangiAlifbo.Connect\CLSID`; `HKCU\Software\Microsoft\Office\{Word,Excel,PowerPoint}\Addins\YangiAlifbo.Connect` (`FriendlyName="Yangi Alifbo"`, `LoadBehavior=3` DWORD). `/uninstall` — hammasini o'chiradi. Konsolga o'zbekcha natija
- [ ] **Step 4: Tekshiruv (Windows):** `build.cmd` → `out\YangiAlifbo.Setup.exe /install` → Word, Excel, PowerPoint ochilganda «Надстройки» varag'ida 4 tugma; «Haqida» oynasi chiqadi; hujjatsiz «Kirill → Lotin» qulamaydi. `/uninstall` → tugmalar yo'qoladi
- [ ] **Step 5:** commit `feat(office): COM qobiq, lenta, o'rnatuvchi`

### Task 6: Word

**Files:** Create `src/Office/WordConverter.cs`, `tests/OfficeSmoke.cs`; Modify `build.cmd`
**Interfaces — Produces:** `public class WordConverter : IDocumentConverter`; smoke: `OfficeSmoke.exe word|excel|ppt` (exit 0 = OK)

- [ ] **Step 1: Failing smoke test** — `Word.Application` yaratadi (Visible=false), yangi hujjatga: "Ўзбекистон **пойтахти** Тошкент." (pойтахти qalin), ikkinchi paragrafda hyperlink "сайт" → dellcom.uz, kolontitulda "Шаҳар". `WordConverter.Convert(app, CyrToLat)` dan keyin tekshiradi: matn `"Özbekiston poytaxti Toşkent."`; "poytaxti" `Bold==-1`; hyperlink soni 1; kolontitul "Şahar". Hujjatsiz app bilan `Convert` → 0
- [ ] **Step 2:** smoke → FAIL
- [ ] **Step 3:** Doira: `Selection.Start != Selection.End` bo'lsa `Selection.Range`, aks holda `doc.StoryRanges` va har birining `NextStoryRange` zanjiri. Har paragraf `Range`: `PlanEdits(range.Text, d)`; har tahrir: `r = doc.Range(start+e.Start, start+e.Start+e.Length)`, `r.Text` eski so'zga teng bo'lsa `r.Text = e.NewText` (aks holda o'tkazib yuboriladi). `ScreenUpdating=false` (finally'da qaytariladi); Word 2010+ da `UndoRecord.StartCustomRecord("Yangi Alifbo")`/`EndCustomRecord` (try/catch — 2007 da yo'q). Himoyalangan hujjat istisnosi → 0 va xabar
- [ ] **Step 4:** smoke `word` → PASS
- [ ] **Step 5:** commit `feat(office): Word o'giruvchisi`

### Task 7: Excel

**Files:** Create `src/Office/ExcelConverter.cs`; Modify `tests/OfficeSmoke.cs`
**Interfaces — Produces:** `public class ExcelConverter : IDocumentConverter`

- [ ] **Step 1: Failing smoke** — A1 "Шаҳар", A2 "=A1&\"!\"" (formula), B1 "Тошкент" (qizil fon), C1 123. CyrToLat dan keyin: A1 "Şahar", A2 `HasFormula==true` va formula matni o'zgarmagan, B1 "Toşkent" va fon rangi saqlangan, C1 123
- [ ] **Step 2:** smoke → FAIL
- [ ] **Step 3:** Doira: `Selection.Count > 1` → Selection; aks holda `ActiveSheet.UsedRange` va oldin `MessageBox` tasdiq "Butun varaq ögirilsinmi? Excel'da bu amalni bekor qilib bölmaydi." (smoke uchun `ExcelConverter.Confirm` — `Func<bool>` statik maydon, testda `() => true`). `SpecialCells(2, 2)` (konstanta, matn; topilmasa istisno → 0); har `Area` uchun `Value2` massivi o'qiladi, satrlar `ConvertText` bilan, o'zgargan bo'lsa massiv qayta yoziladi. Hisob — o'zgargan so'zlar soni (`PlanEdits(...).Count`)
- [ ] **Step 4:** smoke `excel` → PASS
- [ ] **Step 5:** commit `feat(office): Excel o'giruvchisi`

### Task 8: PowerPoint

**Files:** Create `src/Office/PowerPointConverter.cs`; Modify `tests/OfficeSmoke.cs`
**Interfaces — Produces:** `public class PowerPointConverter : IDocumentConverter`

- [ ] **Step 1: Failing smoke** — slayd: sarlavha "Янги алифбо", guruhlangan 2 shakl ("чой", "қўл"), 2×1 jadval ("ғишт", "шаҳар"), sarlavhada "алифбо" qalin. CyrToLat → "Yangi alifbo" ("alifbo" qalin), "çoy", "qöl", "ğişt", "şahar"
- [ ] **Step 2:** smoke → FAIL
- [ ] **Step 3:** Doira: `ActiveWindow.Selection.Type` 3 → `Selection.TextRange`; 2 → `Selection.ShapeRange` shakllari; aks holda barcha slaydlar shakllari. Shakl: `Type==6` (guruh) → `GroupItems` rekursiv; `HasTable` → har katak `Shape`; `HasTextFrame && TextFrame.HasText` → `TextRange`. Tahrir: `PlanEdits(tr.Text, d)`, `c = tr.Characters(e.Start+1, e.Length)`, `c.Text` mos kelsa almashtiriladi. Taqdimot yo'q → 0
- [ ] **Step 4:** smoke `ppt` → PASS
- [ ] **Step 5:** commit `feat(office): PowerPoint o'giruvchisi`

### Task 9: Paket va yakuniy tekshiruv

**Files:** Modify `build.cmd`, `PROGRESS.md`, `CHANGELOG.md`; Create `docs/test-checklist-v1.md`

- [ ] **Step 1:** `build.cmd release` → `out\YangiAlifbo_v1.0.0\` (Core.dll, Office.dll, Setup.exe, README.txt o'zbekcha) va `.zip`
- [ ] **Step 2:** Barcha testlar: `build.cmd test` (Core) + `OfficeSmoke.exe word`, `excel`, `ppt` → hammasi exit 0
- [ ] **Step 3:** Qo'lda checklist (docs/test-checklist-v1.md): har dasturda 3 tugma × (belgilangan / butun hujjat); Word'da Ctrl+Z; Excel tasdiq oynasi; 32 va 64-bit Office (qaysi biri bor bo'lsa); `/uninstall`
- [ ] **Step 4:** PROGRESS.md da natijalar, CHANGELOG.md `v1.0.0`; commit `release: v1.0.0`
