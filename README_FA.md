# راهنمای اجرای LastShiftVR روی ویندوز

**وضعیت پروژه:** نمونه اولیه (greybox) از بازی ترسناک، نه EXE آماده و نه بازی نهایی. Unity Editor و Quest واقعی هنوز تست نشده‌اند. پوشه GeneratedMaterials و Scene بعد از اجرای ابزار سازنده در Unity ساخته می‌شوند.

## سریع‌ترین مسیر برای دیدن روی مانیتور (فعلاً بدون هدست)

1. در ویندوز Unity Hub را نصب کن. Unity **6.1 یا جدیدتر** + Windows Build Support را نصب کن.
2. Unity Hub > New Project > **Universal 3D** > نام `LastShiftVR` > Create.
3. در پروژه جدید Unity، از Window > Package Manager، پکیج‌های **XR Plugin Management**، **OpenXR Plugin**، **XR Hands** و **Input System** را نصب کن. اگر از تو پرسید Input System فعال شود و Editor دوباره راه‌اندازی شود، موافقت کن.
4. از ریپوزیتوری GitHub دکمه Code > Download ZIP را بزن و ZIP را استخراج کن.
5. فقط پوشه `Assets/LastShift` از GitHub را به پوشه `Assets` پروژه Unity جدید کپی کن (مسیر نهایی باید `Assets/LastShift/Scripts/Runtime` باشد). **پوشه‌های ProjectSettings و Packages مخزن را روی پروژه تازه جایگزین نکن**؛ آن‌ها تنظیمات کامل یک پروژه URP نیستند.
6. منتظر تکمیل Import/Compile در Unity بمان. Console را بررسی کن؛ باید بدون خطای قرمز باشد.
7. از منوی بالای Unity گزینه `Tools > Last Shift > Generate Prototype Ward 13` را اجرا کن.
8. فایل صحنه `Assets/LastShift/Scenes/Ward13_Prototype.unity` ساخته می‌شود و در Editor باز است. تب Game را باز کن و دکمه ▶ Play را بزن.
9. وقتی هدست وصل نیست، حالت DesktopPreview خودکار اجرا می‌شود. برای فعال یا غیرفعال‌کردن اجباری آن F8 را بزن.
10. کنترل‌ها: **WASD** حرکت، **Mouse** چرخاندن نگاه، **Left Click یا E** تعامل با فیوز/جعبه برق/در، **F** چراغ‌قوه، **Left/Right Arrow** چرخش ۳۰ درجه‌ای، **Esc** آزاد کردن موس؛ روی Game View کلیک کن تا موس دوباره قفل شود.
11. هدف: سه فیوز را بردار؛ با جمع‌آوری فیوز دوم، دشمن فعال می‌شود؛ به POWER BOX نزدیک شروع برگرد، برق را فعال و از EXIT فرار کن.

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
