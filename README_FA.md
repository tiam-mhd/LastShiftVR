# LAST SHIFT VR — نسخه v0.3 / The Patient

**وضعیت:** سورس قابل بازکردن در Unity و سازندهٔ صحنه قابل‌بازی Windows PCVR؛ هنوز فایل EXE از پیش کامپایل‌شده یا تست موفق روی هدست واقعی ارائه نشده است. برای تأیید اجرا، Compile/Play در Unity باید روی سیستم خودت انجام شود.

## چه چیزهایی در v0.3 اضافه شده است؟

- **همان تصویر شخصیت ارسالی** در `Assets/LastShift/Resources/GhostGirlReference.png` به‌صورت شفاف اضافه شده و هنگام اجرای بازی به‌عنوان Sprite-Impostor روبه‌دوربین برای **The Patient** نمایش داده می‌شود.
- نمایش این تصویر **سه‌بعدی اسکلت‌دار نیست**؛ برای کاراکتر انیمیشن‌دار واقعی به مدل `.fbx` یا `.glb` ریگ‌شده، با تکسچر و انیمیشن مناسب، نیاز است.
- دو بخش قابل ورود: `WARD 12 / TREATMENT` و `WARD 14 / SURGERY`، در کنار راهرو Ward 13.
- تخت بیمارستان، کمد، وسایل جراحی، نور هشدار، سطح‌های چرک و زنگ‌زدهٔ رویه‌ای، محیط کم‌نور و صدای محیطی تولیدشده.
- **سه فیوز در سه موقعیت**: اولین در راهرو، دومی اتاق سمت راست، سومی اتاق سمت چپ.
- فیوزباکس چندمرحله‌ای: برای هر فیوز یک بار E/Trigger؛ سپس یک بار اضافی برای قطع‌کننده اصلی.
- مرحله ترس: بعد از فیوز اول، ظهور کوتاه در انتهای راهرو؛ بعد از فیوز دوم، تعقیب؛ نور چراغ‌قوه می‌تواند سرعت موجود را کم کند.
- HUD مرحله، شمارندهٔ `FUSES` و `SLOTS` و راهنمای `E / CLICK` در **حالت Desktop Preview**.
- منوی `Tools > Last Shift > Validate Ward 13 v0.3` برای بررسی ساختار صحنه بدون نیاز به اجرای بازی.

## اجرا — فقط مراحل جدید

1. **قبل از هر چیز از صحنه و پروژهٔ فعلی یک نسخه پشتیبان بگیر.** ابزار تولید نسخه جدید، صحنهٔ `Ward13_Prototype.unity` را از اول می‌سازد و ویرایش‌های دستی روی همان صحنه ممکن است حذف شوند.
2. Unity را از حالت Play خارج کن؛ پروژهٔ قدیمی را باز نگه دار.
3. از https://github.com/tiam-mhd/LastShiftVR در شاخه main گزینهٔ `Code > Download ZIP` را بزن و Extract کن.
4. فقط پوشه `Assets/LastShift` نسخه جدید را روی `Assets/LastShift` پروژه Unity کپی کن (Copy/Replace). **پوشه‌های `ProjectSettings` و `Packages` را جایگزین نکن.**
5. Unity > `Assets > Refresh`، Console را باز کن و مطمئن شو خطای قرمز C# وجود ندارد. پکیج‌های `Input System`, `XR Hands`, `XR Plug-in Management`, `OpenXR Plugin` باید نصب باشند.
6. از منوی **`Tools > Last Shift > Generate Ward 13 - The Patient v0.3`** استفاده کن. گزینه v0.2 را به‌جای آن اجرا نکن.
7. از منوی **`Tools > Last Shift > Validate Ward 13 v0.3`** وضعیت اسکریپت‌ها، تصویر، فیوزها و اتاق‌ها را بررسی کن.
8. تب Game > Play و حالت Desktop Preview: `WASD` حرکت، موس نگاه، `E` تعامل، `F` چراغ، `Esc` آزادکردن موس، `F8` اجبار حالت Desktop.
9. فیوز نارنجی اول داخل راهرو را پیدا و E بزن؛ شبح یک لحظه در انتهای راهرو ظاهر می‌شود. وارد دو اتاق جانبی شو، دو فیوز دیگر را بردار، از The Patient فرار کن، به جعبه برق نزدیک ابتدای راهرو بازگرد. **سه بار برای نصب و یک بار برای کلید اصلی E بزن**. سپس در EXIT را باز کن.

## اجرای PCVR در Quest 2 / 3 / 3S

- Meta Quest Link (USB-C یا Air Link) را روی ویندوز اجرا کن و به عنوان OpenXR Runtime فعال انتخاب کن.
- Unity > Project Settings > XR Plug-in Management > PC, Mac & Linux Standalone > OpenXR و Oculus Touch Controller Profile را فعال کن.
- Play را با Quest Link فعال شروع کن. کنترلر Meta: استیک چپ حرکت، راست چرخش، Trigger تعامل، Grip چراغ.
- ردیابی دست فقط اگر XR Hands در این Runtime دست را واقعاً گزارش کند فعال می‌شود؛ **در EXE ویندوز پشتیبانی رسمی تضمین نیست**. با کنترلر تست کن.
- پس از تست بازی: File > Build Profiles > Windows x86_64 > Build و فایل‌های خروجی را همراه هم نگه دار.

## محدودیت‌های مهم این نسخه

1. تصویر The Patient واقعاً در بازی استفاده می‌شود، ولی صفحه‌ای دوبعدی در فضای سه‌بعدی است، نه مدل Rigged با انیمیشن مو و لباس.
2. سطح‌ها Procedural هستند؛ واقع‌گرایی فوتواسکن/PBR سینمایی، انیمیشن واقعی دشمن، صدای ضبط‌شده و تعامل فیزیکی کامل اشیا هنوز نیازمند Asset و کار بیشترند.
3. گرفتن فیوز در این نسخه به شکل `Aim + E/Trigger` انجام می‌شود و در پنل مرحله‌ای درج می‌شود؛ حمل و گرفتن فیزیکی کامل هنوز پیاده‌سازی نشده است.
4. سیستم مسیر‌یابی دشمن محدود به همین نقشه و نقاط ورودی اتاق‌هاست، نه NavMesh سراسری.
5. روی کامپیوتر/هدست واقعی تست نشده و ممکن است خطای کامپایل یا گیم‌پلی باقی مانده باشد؛ Console را معیار قرار بده.

## اگر با خطا مواجه شدی

- منوی v0.3 نیست: Console را بررسی کن؛ فایل `Assets/LastShift/Editor/Ward13PatientBuilder.cs` را پیدا کن.
- تصویر شبح نیست: وجود و Import فایل `Assets/LastShift/Resources/GhostGirlReference.png` را بررسی کن.
- نقشه مثل نسخه قدیمی است: گزینه درست v0.3 را انتخاب و صحنه را دوباره بساز.
- تعامل عمل نمی‌کند: وسط تصویر روی Fuse/Power Panel هدف بگیر؛ پیام HUD نمایش داده می‌شود.
- هدست کار نمی‌کند: یک بار بدون هدست تست کن، سپس Quest Link و OpenXR Runtime را بررسی کن.

---

## راهنمای پیشین v0.2 (برای ارجاع)


**وضعیت پروژه:** نمونه اولیه (greybox) از بازی ترسناک، نه EXE آماده و نه بازی نهایی. Unity Editor و Quest واقعی هنوز تست نشده‌اند. پوشه GeneratedMaterials و Scene بعد از اجرای ابزار سازنده در Unity ساخته می‌شوند.

## سریع‌ترین مسیر برای دیدن روی مانیتور (فعلاً بدون هدست)

1. در ویندوز Unity Hub را نصب کن. Unity **6.1 یا جدیدتر** + Windows Build Support را نصب کن.
2. Unity Hub > New Project > **Universal 3D** > نام `LastShiftVR` > Create.
3. در پروژه جدید Unity، از Window > Package Manager، پکیج‌های **XR Plugin Management**، **OpenXR Plugin**، **XR Hands** و **Input System** را نصب کن. اگر از تو پرسید Input System فعال شود و Editor دوباره راه‌اندازی شود، موافقت کن.
4. از ریپوزیتوری GitHub دکمه Code > Download ZIP را بزن و ZIP را استخراج کن.
5. فقط پوشه `Assets/LastShift` از GitHub را به پوشه `Assets` پروژه Unity جدید کپی کن (مسیر نهایی باید `Assets/LastShift/Scripts/Runtime` باشد). **پوشه‌های ProjectSettings و Packages مخزن را روی پروژه تازه جایگزین نکن**؛ آن‌ها تنظیمات کامل یک پروژه URP نیستند.
6. منتظر تکمیل Import/Compile در Unity بمان. Console را بررسی کن؛ باید بدون خطای قرمز باشد.
7. از منوی بالای Unity گزینه `Tools > Last Shift > Generate Ward 13 - Atmospheric Preview v0.2` را اجرا کن.
8. فایل صحنه `Assets/LastShift/Scenes/Ward13_Prototype.unity` ساخته می‌شود و در Editor باز است. تب Game را باز کن و دکمه ▶ Play را بزن.
9. وقتی هدست وصل نیست، حالت DesktopPreview خودکار اجرا می‌شود. برای فعال یا غیرفعال‌کردن اجباری آن F8 را بزن.
10. کنترل‌ها: **WASD** حرکت، **Mouse** چرخاندن نگاه، **Left Click یا E** تعامل با فیوز/جعبه برق/در، **F** چراغ‌قوه، **Left/Right Arrow** چرخش ۳۰ درجه‌ای، **Esc** آزاد کردن موس؛ روی Game View کلیک کن تا موس دوباره قفل شود.
11. هدف: سه فیوز را بردار؛ با جمع‌آوری فیوز دوم، دشمن فعال می‌شود؛ به POWER BOX نزدیک شروع برگرد، برق را فعال و از EXIT فرار کن.

## ارتقای محیط: نسخه Atmospheric Preview v0.2

اگر قبلاً فقط یک اتاق مستطیلی با مکعب و چند چراغ دیدی، آن محیط **Greybox پایه** بوده است. یک Scene Builder پیشرفته‌تر وجود دارد که محیط را با کاشی، تخت، در بخش، کمد، لوله، نور چشمک‌زن، علامت فیوزها، مدل اولیه موجود انسان‌نما و HUD قابل استفاده روی مانیتور می‌سازد.

برای دریافت به‌روزرسانی:
1. از Play خارج شو. اگر صحنه فعلی را ویرایش کرده‌ای، قبل از ادامه جداگانه Save As بگیر؛ ابزار جدید صحنه قبلی `Ward13_Prototype.unity` را دوباره می‌سازد.
2. GitHub > Code > Download ZIP را مجدداً دانلود کن و Extract بگیر.
3. محتویات آخرین `Assets/LastShift` را روی `Assets/LastShift` پروژه Unity خودت جایگزین کن. فقط همین پوشه را به‌روزرسانی کن، نه `ProjectSettings` یا `Packages`.
4. Unity > Assets > Refresh؛ از منوی Window > General > Console مطمئن شو هیچ خطای **قرمز C#** وجود ندارد.
5. این بار به‌جای گزینه قبلی، **Tools > Last Shift > Generate Ward 13 - Atmospheric Preview v0.2** را بزن. این گزینه سازنده قدیمی را اجرا و بعد صحنه را ارتقا می‌دهد.
6. تب Game > Play؛ در حالت مانیتور هدف مأموریت، شمارنده فیوز، چراغ‌قوه، نشانه‌گیر وسط تصویر و راهنمای تعامل نمایش داده می‌شود.
7. به جلو حرکت کن؛ سه فیوز نارنجی روی سکوهای نورانی مشخص شده‌اند. به هرکدام نگاه کن تا پیام **TAKE FUSE** نمایش داده شود، سپس **E** را بزن.
8. بعد از فیوز دوم، موجود تعقیب‌کننده فعال می‌شود. برای توقف کوتاه آن، نور چراغ را روی بدن موجود نگه دار. فیوز سوم را بردار، به جعبه برق در نزدیکی ورودی برگرد و E بزن؛ سپس به EXIT پشت نقطه شروع برو.

مهم: این نسخه همچنان یک **نمونه اولیه ساخته‌شده با Primitiveهای Unity** است، نه بیمارستان فتورئالیستی. مدل/تکسچر/صداگذاری حرفه‌ای و تست عملی داخل Unity همچنان باقی است.

## اجرای آزمایشی داخل هدست Meta Quest (PCVR)

1. از کامپیوتر ویندوزی استفاده کن و **Meta Quest Link** را نصب کن.
2. Quest 2 / 3 / 3S را از طریق کابل USB مناسب یا Air Link وصل کن. در داخل هدست گزینه Quest Link / Launch را فعال کن.
3. در برنامه Meta Quest Link ویندوز، آن را به‌عنوان **OpenXR Runtime فعال** انتخاب کن (نام منو بسته به نسخه نرم‌افزار ممکن است متفاوت باشد).
4. در Unity: Edit > Project Settings > **XR Plug-in Management** را باز کن؛ برای **PC, Mac & Linux Standalone** گزینه **OpenXR** را فعال کن.
5. در XR Plug-in Management > OpenXR > Interaction Profiles، **Oculus Touch Controller Profile** را فعال کن. در Project Validation اخطارهای قابل رفع را برطرف کن.
6. هنگام فعال بودن Quest Link صحنه ساخته‌شده را در Unity ▶ Play کن. اگر در حالت DesktopPreview ماند، با F8 حالت اجباری را خاموش کن و وضعیت دستگاه OpenXR را بررسی کن.
7. دسته‌ها: استیک چپ حرکت؛ استیک راست چرخش؛ Trigger راست تعامل؛ Grip راست روشن/خاموش کردن چراغ‌قوه.
8. Hand Tracking: این قابلیت فقط هنگامی به‌صورت خودکار فعال می‌شود که XRHandSubsystem داده دست را واقعاً ارائه کند. در Windows EXE پشتیبانی رسمی تضمین نشده؛ کنترلرها روش اصلی هستند.

## خروجی Windows EXE

1. Unity > File > Build Profiles > Windows, Mac, Linux > Platform = Windows > Architecture = x86_64.
2. صحنه `Ward13_Prototype.unity` باید در لیست Scenes In Build باشد (ابزار Scene Builder آن را اضافه می‌کند).
3. Build را بزن، یک پوشه مثل `Builds/Windows` انتخاب کن.
4. نتیجه شامل `LastShiftVR.exe` (نام فایل طبق تنظیمات ساخت) به‌همراه پوشه `_Data` و فایل‌های جانبی است؛ همه را کنار هم نگه دار.
5. برای تجربه VR پیش از اجرای EXE، Quest Link را روی هدست فعال کن؛ برای حالت مانیتور بدون هدست، DesktopPreview فعال است.

## رفع مشکلات رایج

- **منوی Tools > Last Shift وجود ندارد:** Console را باز کن؛ احتمالا پکیج XR Hands یا Input System نصب نشده یا خطای C# وجود دارد.
- **صفحه بازی سیاه است:** بررسی کن صحنه `Ward13_Prototype` باز و Game View فعال باشد؛ GameObject `Tracked Head` باید Camera با Tag `MainCamera` داشته باشد.
- **در Play حرکت نمی‌کند:** ابتدا روی Game View کلیک کن، F8 را یک بار امتحان کن، و مطمئن شو Keyboard در بازی تشخیص داده می‌شود.
- **هدست تصویر نمی‌گیرد:** برنامه Meta Quest Link، OpenXR PC provider و فعال بودن Link داخل هدست را بررسی کن.
- **دست واقعی در فایل EXE کار نمی‌کند:** این محدودیت مستند Meta برای Quest Link Windows است؛ از دسته‌ها استفاده کن.

## توجه

این مخزن **سورس prototype + ابزار تولید محیط** را نگه می‌دارد. خود صحنه `.unity` و متریال‌های Generated پس از اجرای ابزار Unity تولید می‌شوند؛ مدل گرافیکی سینمایی، فایل صوتی حرفه‌ای و Windows EXE از پیش ساخته‌شده در مخزن وجود ندارد.

Meta docs:
- https://developers.meta.com/vr/documentation/unity/unity-link/
- https://developers.meta.com/vr/documentation/unity/unity-handtracking-overview/
- https://developers.meta.com/vr/documentation/unity/unity-xr-plugin/
