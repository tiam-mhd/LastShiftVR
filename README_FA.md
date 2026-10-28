# نسخه جدید — ورود واقعی Ghost Girl Animated به LastShiftVR

**فایل پیدا و متصل شد:** `Assets/LastShift/Characters/ThePatient/Model/shabannn.fbx` (حدود ۲۱.۹ مگابایت). نام فایل دیگر نیازی به تغییر ندارد.

### مشکل نسخه قبلی و اصلاح
واردکننده قبلی فقط `ThePatient.fbx` را می‌شناخت و فقط زمانی دشمن را فعال می‌کرد که چهار کلیپ با نام‌های دقیق موجود باشند. نسخه جدید هر دو محدودیت را برطرف می‌کند، از خود FBX موجود در GitHub استفاده می‌کند، آن را به مقیاس انسانی نزدیک می‌کند و متریال‌های ناسازگار با URP را اصلاح می‌کند.

### اجرای نسخه جدید روی کامپیوتر
1. Unity را ببند و از پروژه‌ات نسخه پشتیبان بگیر.
2. آخرین نسخه شاخه `main` را با `Code > Download ZIP` دریافت کن. حتماً بررسی کن که `Assets/LastShift/Characters/ThePatient/Model/shabannn.fbx` داخل ZIP وجود داشته باشد.
3. به‌جای کپی روی پوشه قبلی، `Assets/LastShift` قبلی را خارج از Assets بکاپ بگیر و پوشه جدید را کپی کن؛ یا از `tools/Sync-LastShift.ps1` استفاده کن.
4. Unity را باز کن، منتظر Import فایل FBX بمان و Console را برای خطای قرمز بررسی کن.
5. تنها منوی نهایی: `Tools > Last Shift > Build Final Ward 13 - Real Patient FBX`. این مرحله **باید از نو انجام شود** چون صحنه قبلی مدل واقعی ندارد.
6. در Hierarchy زیر `THE WATCHER` باید شیء `The Patient - Ghost Girl Animated (real FBX)` ساخته شده باشد (والد در شروع غیرفعال است). مدل را در Scene View با انتخاب و فشردن F بررسی کن.
7. Game > Play. حدود ۲.۵ ثانیه بعد باید دختر را در راهرو ببینی. پس از فیوز دوم، دشمن در حالت تعقیب فعال می‌شود و بعد از نزدیک‌شدن به بازیکن، حمله و Game Over اتفاق می‌افتد. ترس‌های محیطی قبلی حفظ شده‌اند.
8. Console گزارش `[LastShift / Model]` و `[LastShift / Anim]` را می‌دهد: نام همه AnimationClipهای واقعی FBX و اینکه هر حالت Idle, Walk, Run, Stare, Scare, Attack, Appear, Vanish, Frightened, Stunned به کدام کلیپ نگاشت شده است.
9. در نبود کلیپ‌های مناسب، **مدل سه‌بعدی واقعی قابل دیدن و تعقیب است** اما حرکت اندام‌ها جایگزین حرفه‌ای ندارد. انیمیشن‌های دقیق فقط با کلیپ‌های واقعی و سازگار فعال می‌شوند. با نبود کامل کلیپ، حرکت کلی بدن بسیار ساده است تا دشمن نامرئی نباشد.
10. در صورت نبود تکسچر/صورتی‌شدن مدل، هشدار `[LastShift / Materials]` را بررسی کن و تکسچرهای FBX را از Inspector > Materials استخراج یا متصل کن. منبع FBX در پوشه مدل دست‌نخورده باقی می‌ماند.

### رفتارهای متصل
- Idle / Stare: توقف و نگاه در لحظه مشاهده
- Appear / Vanish: ظهور اولیه و ناپدیدشدن
- Walk / Run: تعقیب آهسته و سپس سریع‌تر
- Frightened / Stunned: واکنش به نور چراغ‌قوه
- Scare: شروع تعقیب
- Attack: نزدیک‌شدن و جامپ‌اسکر کنترل‌شده
- ترس محیطی: ضربه‌ها، زمزمه پشت سر، بسته‌شدن در و تغییرات نور

### محدودیت بررسی
نام فایل، حجم و محل قرارگیری آن مستقیماً در GitHub تأیید شده‌اند. محتوای داخلی فایل باینری FBX (تعداد و نام کلیپ‌های واقعی) از محیط فعلی قابل خواندن نبود؛ گزارش Import داخل Unity برای تأیید آن لازم است. خود کامپایل Unity و نمایش مدل روی هدست نیز هنوز تست نشده‌اند. درباره کیفیت انیمیشن‌ها ادعای فراتر از Asset موجود نداریم.

### حق مؤلف
مدل Ghost Girl Animated اثر Shaban Hafizsalim با مجوز CC BY 4.0 از Sketchfab است. لینک و اعتبار کامل در `Assets/LastShift/Characters/ThePatient/ATTRIBUTION.md` ثبت شده‌اند؛ هنگام انتشار عمومی بازی این اعتبار باید حفظ شود.

---

# اصلاح تجربه ترس در نبود مدل واقعی — محیط فعال

دو اسکرین‌شات Unity نشان می‌دادند که `real model / animation clips not installed`.
این یعنی **دشمن سه‌بعدی واقعاً در پروژه وجود ندارد**. نه دوربین خراب است، نه دشمن جایی پنهان شده؛ ما عمداً به‌خاطر نبودن FBX و انیمیشن واقعی تعقیب و Game Over نامرئی را غیرفعال کرده‌ایم.

## به‌روزرسانی تجربه مستقل از دشمن
کد `WardHorrorEvents.cs` به سازنده صحنه متصل شده است. این رویدادها **بدون مدل دختر شبحی هم اجرا می‌شوند**:
- بعد از برداشتن فیوز اول، ضربه‌های سنگین از انتهای راهرو و یک پالس نور
- بعد از فیوز دوم، صدای نفس/زمزمه از پشت سر، صدای کوبیده‌شدن در و حرکت کوتاه یک در در محیط
- بعد از فیوز سوم، صدای تهدیدآمیز و پالس نور موضعی
- بعد از فعال‌شدن کلید اصلی، توقف رویدادهای جاری

این‌ها افکت‌های *موقت و تولیدشده در کد* هستند، نه صداگذاری حرفه‌ای استودیویی؛ شدت صداها محدود شده است و به‌جای استروب سریع از پالس‌های کوتاه و نرم استفاده می‌شود.

### اصلاح دید
نوشته‌های بزرگ و معکوس شناور از راهروی اصلی حذف و متن مأموریت روی دیوار کوچک شده است؛ برای دیدن تغییرات **حتماً صحنه را مجدداً تولید کن**.

### نصب تمیز و اجرا
1. Unity را کامل ببند و از پروژه/صحنه قبلی بکاپ بگیر.
2. آخرین ZIP مخزن `main` را دانلود و استخراج کن.
3. از پوشه استخراج‌شده، اسکریپت `tools/Sync-LastShift.ps1` را برای جابه‌جایی پوشه قبلی به بکاپ و کپی کامل نسخه جدید اجرا کن. یا پوشه قبلی `Assets/LastShift` را دستی از Assets خارج و سپس پوشه جدید را جایگزین کن.
4. Unity را باز کن؛ Console باید فاقد خطای قرمز باشد.
5. گزینه واحد `Tools > Last Shift > Build Final Ward 13 - Real Patient FBX` را بزن.
6. Game > Play؛ با WASD به فیوزها برس و E بزن. به صداهای استریوی هدست یا هدفون گوش بده.

### فعال‌شدن **واقعی** The Patient
برای اینکه خود دختر با چهره و لباس مدنظر در راهرو بایستد، باید یک فایل `ThePatient.fbx` با اسکلت Humanoid و فایل‌های `Idle`، `Walk`، `Run`، `Attack` (یا کلیپ‌های FBX با همان نام) فراهم شود. مسیر ورودی:
`Assets/LastShift/Characters/ThePatient/Model/ThePatient.fbx`.
تصویر مرجع PNG چنین مدلی نیست و این به‌روزرسانی هم ادعای تولید FBX نمی‌کند.

---

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

