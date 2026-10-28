# رفع خطای CurrentSpeed، منوهای قدیمی و صفحه صورتی — به‌روزرسانی جدید

**Unity تصویر قبلی را هنوز از Scene قدیمی و اسکریپت‌های باقی‌مانده نشان می‌داد.** معمولی کپی‌کردن پوشه جدید روی پوشه قبلی، فایل‌های حذف‌شده در GitHub را پاک نمی‌کند.

### ۱. حتماً Unity را ببند؛ منبع تازه را تمیز جایگزین کن
1. از پروژه خودت یک نسخه پشتیبان بگیر، مخصوصاً اگر Scene یا Material را شخصاً تغییر داده‌ای.
2. آخرین نسخه GitHub را با Code > Download ZIP دانلود و Extract کن.
3. ساده‌ترین روش: از ZIP جدید، فایل `tools/Sync-LastShift.ps1` را با PowerShell اجرا کن؛ مسیر پروژه Unity را در پارامتر `-UnityProject` بده.

```powershell
powershell -ExecutionPolicy Bypass -File ".\tools\Sync-LastShift.ps1" -UnityProject "C:\PATH\TO\YOUR\UNITY\PROJECT"
```

**فراموش نکن:** دستور بالا را باید از داخل پوشه استخراج‌شدهٔ GitHub اجرا کنی (یا مسیر کامل فایل اسکریپت را بدهی). داخل `UnityProject` مسیر فولدری را بده که `Assets`, `Packages`, `ProjectSettings` دارد، نه خود `Assets`. اسکریپت پوشه قبلی `Assets/LastShift` را در `LastShift_Backups` بیرون از Assets ذخیره و نسخه جدید را جایگزین می‌کند. کامپایل دوباره در شروع Unity انجام می‌شود.

**روش دستی جایگزین:** Unity را ببند، فولدر `Assets/LastShift` قدیمی را از پروژه **به محل بکاپ خارج از Assets منتقل کن**، سپس `Assets/LastShift` جدید را کپی کن. به جای Merge باید فولدر قبلی کامل از Assets خارج شده باشد. فایل‌های `Packages` و `ProjectSettings` پروژه اصلی دست‌نخورده بمانند.

### ۲. Unity > Console
Unity را باز کن. خطای قبلی `CS1061: LastShiftMonster.CurrentSpeed` در سورس جدید رفع شده است. اگر خطای قرمز دیگری هست، ابتدا آن را برطرف کن؛ نبودن خطای کامپایل برای ظاهرشدن منو ضروری است.

### ۳. تنها منوی نهایی
`Tools > Last Shift > Build Final Ward 13 - Real Patient FBX`

منوهای `DEBUG` و `The Patient 3D` قدیمی در **سورس جدید وجود ندارند**. اگر هنوز می‌بینی، کدهای محلی قدیمی درست پاک نشده‌اند یا پروژه دیگری باز است.

### ۴. رنگ صورتی
باید در `Edit > Project Settings > Graphics` و `Quality` یک **Universal Render Pipeline Asset** فعال باشد؛ و در `Window > Package Manager` پکیج Universal RP نصب باشد. در Unity 6 می‌توانی از `Window > Rendering > Render Pipeline Converter` برای تبدیل متریال‌های Built-in به URP استفاده کنی (پیش از تبدیل، بکاپ بگیر).

سازنده جدید قبل از ساخت، فعال‌بودن URP را بررسی می‌کند؛ متریال‌های قدیمی خراب را هم با Shader `Universal Render Pipeline/Lit` بازیابی می‌کند. **بعد از اصلاح، صحنه را از منوی بالا دوباره بساز**؛ صرفاً بازکردن Scene قبلی کافی نیست. اگر در صحنه قدیمی GeneratedMaterials حذف شده باشد، بازسازی صحنه ارجاعات را تجدید می‌کند.

### ۵. درباره دشمن
تا وقتی `Assets/LastShift/Characters/ThePatient/Model/ThePatient.fbx` و کلیپ‌های واقعی `Idle, Walk, Run, Attack` همراه Humanoid Avatar معتبر وجود نداشته باشند، سیستم عمداً دشمن نامرئی را غیرفعال نگه می‌دارد. وجود تصویر مرجع به‌تنهایی مدل 3D محسوب نمی‌شود.

---

# LastShiftVR — خط تولید حرفه‌ای شخصیت سه‌بعدی The Patient

**وضعیت:** سورس Unity 6 برای Windows PCVR و Meta Quest Link. سیستم قبلی ساخت مش از عکس، بیلبورد دوبعدی، مش OBJ تکه‌تکه و استخوان/انیمیشن‌های مصنوعی حذف شده‌اند. مدل سه‌بعدی حرفه‌ای نهایی **هنوز در مخزن وجود ندارد**؛ برای فعال شدن دشمن باید FBX یا Prefab واقعی و انیمیشن‌های معتبر اضافه شوند.

## چه چیزی حذف شد؟
- `GhostGirlVisual.cs` (مش ساختگی تصویر)
- `PatientAnimationDriver.cs` (ژست‌های مصنوعی)
- `ThePatient_ScanVolume.obj` و فایل `.mtl`
- مراحل تولید مش از پیکسل‌های تصویر و تصویرسازی impostor

تصویر اصلی `Assets/LastShift/Resources/GhostGirlReference.png` فقط **مرجع ظاهری مدل‌ساز** است و در موتور به‌عنوان بدن دشمن رندر نمی‌شود.

## چه چیزهایی جایگزین شد؟
- `PatientCharacterRig.cs`: اتصال یک مدل واقعی و SkinnedMeshRenderer به Animator و AI
- `PatientCharacterImporter.cs`: پیدا کردن و Import فایل FBX/Prefab، اتصال Avatar و ساخت Animator Controller از کلیپ‌های واقعی
- `Assets/LastShift/Characters/ThePatient/ART_BRIEF.md`: شرح دقیق الزامات بصری، چهره، مو، لباس و ریگ
- `Characters/ThePatient/Model`: محل فایل مدل واقعی
- `Characters/ThePatient/Animations`: محل فایل انیمیشن واقعی

### فرمت و نام مورد انتظار
```text
Assets/
  LastShift/
    Characters/
      ThePatient/
        Model/
          ThePatient.fbx
        Animations/
          Idle.anim     (یا FBX دارای کلیپ Idle)
          Walk.anim
          Run.anim
          Attack.anim
          Stare.anim      (اختیاری)
          Frightened.anim (اختیاری)
          Scare.anim      (اختیاری)
          Appear.anim     (اختیاری)
          Vanish.anim     (اختیاری)
          Stunned.anim    (اختیاری)
```

**ضروری برای فعال شدن دشمن:** یک مدل دارای `SkinnedMeshRenderer`، Avatar معتبر Humanoid، و کلیپ‌های واقعی **Idle, Walk, Run, Attack**. بقیه حرکت‌ها اختیاری هستند و در صورت نبودشان سیستم به Idle برمی‌گردد.

## تنظیم وارد کردن مدل
1. FBX واقعی را از Blender یا نرم‌افزار مدل‌سازی خروجی بگیر؛ صورت، مو، لباس، پشت و پهلو باید هندسه واقعی داشته باشند.
2. در Unity فایل `ThePatient.fbx` را انتخاب کن: `Rig > Animation Type: Humanoid` و `Avatar Definition: Create From This Model`، سپس Apply.
3. در تب Model، مقیاس را بررسی کن؛ قد تقریبی شخصیت باید واقع‌گرایانه و مطابق طراحی باشد.
4. در Material، تکسچرهای PBR و متریال‌های URP/Lit را متصل کن.
5. کلیپ‌های انیمیشن را با نام‌های فوق در `Animations` وارد کن. کلیپ‌ها باید Humanoid و با Avatar همین مدل سازگار باشند؛ Walk/Run را **in-place** و Idle/Walk/Run را looping قرار بده.
6. فقط منوی نهایی زیر را اجرا کن. Editor یک `ThePatient.controller` می‌سازد و مدل سه‌بعدی را به دشمن متصل می‌کند.

## اجرای نسخه جدید
1. از Unity Play خارج شو؛ از پروژه و صحنه `Ward13_Prototype.unity` پشتیبان بگیر (سازنده صحنه را بازسازی می‌کند).
2. از شاخه `main` در https://github.com/tiam-mhd/LastShiftVR آخرین ZIP را دانلود کن.
3. **فقط** `Assets/LastShift` را در Unity جایگزین کن. پوشه‌های `Packages` و `ProjectSettings` پروژه URP را دست نزن.
4. فایل‌های قدیمی محلی را که دیگر در مخزن نیستند **دستی حذف کن**:
   - `Assets/LastShift/Scripts/Runtime/GhostGirlVisual.cs`
   - `Assets/LastShift/Scripts/Runtime/PatientAnimationDriver.cs`
   - `Assets/LastShift/Models/ThePatient_ScanVolume.obj` و `.mtl`
   - در صورت وجود، `Assets/LastShift/GeneratedMeshes/ThePatient_3D_Rigged.asset` (فقط مش قدیمی ساختگی)
   کپی/Replace ساده ممکن است فایل‌های حذف‌شده را در پروژه Unity نگه دارد؛ بنابراین پاک‌سازی آنها مهم است.
5. `Assets > Refresh`، سپس Console را برای هر خطای قرمز بررسی کن.
6. منوی تنها گزینه باقی‌مانده:

   `Tools > Last Shift > Build Final Ward 13 - Real Patient FBX`
7. تا وقتی مدل واقعی و چهار کلیپ ضروری وارد نشده‌اند، بازی محیط، فیوزها و سیستم نور را دارد، اما دشمن غیرفعال است تا **نامرئی Game Over نکند**.
8. بعد از اضافه کردن مدل و کلیپ‌ها همین گزینه را دوباره اجرا کن تا صحنه به‌روزرسانی شود؛ از Scene View مدل را در `THE WATCHER` ببین.
9. Desktop: `WASD`, Mouse, `E` تعامل، `F` چراغ‌قوه. PCVR: Meta Quest Link و پروفایل Oculus Touch با OpenXR.

## وضعیت و محدودیت
- **هیچ فایل مدل‌ سه‌بعدی حرفه‌ای آمادهٔ FBX و هیچ انیمیشن هنرمندساخته‌شده‌ای در این تغییرات تولید نشده است.** این کار باید به‌صورت مدل‌سازی/ریگ و انیمیشن واقعی تأمین شود.
- کد تولید `AnimatorController` از کلیپ‌های واقعی طبق API استاندارد Unity نوشته شده، اما کامپایل و اجرای عملی Unity/Quest در این محیط انجام نشده است.
- اگر FBX داری ولی مدل داخل بازی فعال نمی‌شود، بخش Console را بررسی کن؛ Importer نام کلیپ‌ها و وضعیت Avatar را گزارش می‌دهد.
- هیچ گزینه Test/DEBUG و هیچ نسخه قدیمی در منوی `Tools > Last Shift` وجود ندارد؛ فقط سازنده نهایی نمایش داده می‌شود.

