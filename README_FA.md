# رفع خطای «This cannot be used during play mode» — راهنمای فوری

اگر در Unity هنگام انتخاب سازنده مرحله این خطا را دیدی:

`InvalidOperationException: This cannot be used during play mode`

علت این است که `Tools > Last Shift > Build Final Ward 13 - Real Patient FBX` را **در حالت Play (دکمه آبی بالای Unity)** اجرا کرده‌ای. ذخیره و بازسازی Scene در حالت Play مجاز نیست؛ پس FBX در صحنه جدید قرار نگرفته است.

1. روی دکمه Play بالای Unity یک بار کلیک کن تا خاموش شود و به **Edit Mode** برگردی.
2. اگر صحنه را دستی ویرایش کرده‌ای، آن را قبل از تولید مجدد جداگانه Backup کن. سازنده صحنهٔ فعلی Ward13_Prototype را از نو ایجاد می‌کند.
3. بعد از بستن Unity آخرین ZIP گیت‌هاب را بگیر و پوشه `Assets/LastShift` را تمیز جایگزین کن؛ برای خود این خطا نیازی به دانلود مجدد FBX به‌صورت جداگانه نیست.
4. Unity را باز کن و از منوی `Tools > Last Shift > Build Final Ward 13 - Real Patient FBX` **در حالت Edit** استفاده کن.
5. منو در نسخه به‌روز، هنگام Play یا Compile غیرفعال خواهد شد. بعد از ساخت صحنه، در Hierarchy بخش `THE WATCHER` را باز کن؛ باید `The Patient - Ghost Girl Animated (real FBX)` را ببینی. والد دشمن ابتدا غیرفعال است.
6. در Scene View برای مشاهده مدل، با احتیاط والد را فعال کن و روی مدل F بزن؛ سپس قبل از شروع Play آن را به حالت غیرفعال برگردان.
7. در Console دنبال پیام `The Patient is VISIBLE and READY` یا خطاهای `[LastShift / Model]` بگرد. اگر مدل زیر والد وجود نداشت، ابتدا پیام همین واردکننده را بررسی کن.
8. اکنون Play را اجرا کن؛ نخستین ظهور در ثانیه‌های ابتدایی طراحی شده و پس از دومین فیوز تعقیب فعال می‌شود.

پیام‌های `Reduced additional punctual light shadows resolution` هشدار بودجه سایه‌های URP هستند، نه دلیل ندیدن مدل و نه علت خطای Play Mode. پس از اینکه مدل قابل مشاهده شد، می‌توان بودجه نور و سایه را بهینه کرد.

---

# LastShiftVR — Ghost Girl Animated / The Patient

## وضعیت واقعی نسخه فعلی

مدل واقعی سه‌بعدی **Ghost Girl Animated** در ریپوزیتوری موجود است:
`Assets/LastShift/Characters/ThePatient/Model/shabannn.fbx` — ۲۱٬۸۵۳٬۹۳۶ بایت.

این همان Asset موجود در GitHub است، نه مدل تکه‌تکه‌شده از عکس. سورس نسخه جدید:
- فایل FBX را با نام واقعی آن پیدا و در صحنه وارد می‌کند.
- مدل 3D را به قد تقریبی ۱٫۷۲ متر می‌رساند و کف پای آن را روی زمین تنظیم می‌کند.
- متریال‌های غیر URP را برای نمونه مدل به URP/Lit تبدیل می‌کند و بافت‌های موجود را حفظ می‌کند.
- انیمیشن‌های واقعاً موجود در فایل FBX و پوشه `Animations` را در Unity شناسایی و به ۱۰ حالت بازی نگاشت می‌کند.
- هنگام نبود بعضی انیمیشن‌ها، از کلیپ موجود استفاده می‌کند؛ اگر هیچ انیمیشنی نباشد، مدل واقعی را با حرکت کلی ساده نمایش می‌دهد، نه اینکه دوباره نامرئی شود.
- ظهور، مشاهده، تعقیب آهسته/سریع، واکنش به چراغ‌قوه، حمله و محوشدن را به هوش مصنوعی/رویدادهای فعلی متصل می‌کند.
- ترس‌های محیطی، خاموشی‌ها، زمزمه، کوبیده‌شدن در و فیوزباکس را حفظ می‌کند.

**محدودیت تأیید:** حجم و محل فایل FBX در GitHub بررسی شده است، اما محتویات باینری آن برای این ابزار قابل خواندن نبود. تعداد واقعی کلیپ‌ها و اینکه تکسچرها داخل FBX تعبیه شده‌اند را گزارش Unity مشخص می‌کند. امکان اجرای عملی Unity/Quest از این محیط وجود نداشت.

## راه نصب از صفر روی سیستم فعلی ویندوز

1. Unity را کامل ببند و از پوشه پروژه خودت بکاپ بگیر.
2. آخرین ZIP ریپوزیتوری `https://github.com/tiam-mhd/LastShiftVR` از شاخه `main` را دانلود و Extract کن.
3. بررسی کن داخل فایل ZIP پوشه `Assets/LastShift/Characters/ThePatient/Model` دارای فایل `shabannn.fbx` باشد.
4. **پوشه قبلی `Assets/LastShift` را کامل بیرون از Assets بکاپ بگیر** و پوشه جدید را جایگزین کن؛ صرفاً Merge نکن. یا ابزار `tools/Sync-LastShift.ps1` را استفاده کن.
5. Unity را باز کن؛ منتظر Import FBX و Compile بمان. Console نباید خطای قرمز C# داشته باشد.
6. فقط منوی نهایی را اجرا کن: `Tools > Last Shift > Build Final Ward 13 - Real Patient FBX`.
7. در Hierarchy، زیر `THE WATCHER` باید `The Patient - Ghost Girl Animated (real FBX)` باشد. چون والد در شروع صحنه غیرفعال است، برای دیدن در Scene View می‌توانی موقتاً آن را فعال کنی.
8. در Console پیام `[LastShift / Model] Imported .../shabannn.fbx` و گزارش `[LastShift / Anim]` را بررسی کن. برای هر حرکت مشخص می‌شود کدام کلیپ واقعی و کدام fallback انتخاب شده است.
9. در تب Game دکمه Play را بزن: حدود ۲٫۵ ثانیه بعد نخستین ظهور باید رخ دهد؛ با فیوز دوم تعقیب فعال می‌شود.
10. کنترل‌های ویندوز: WASD حرکت، موس نگاه، E تعامل، F چراغ‌قوه. کنترلرهای Quest Link: استیک چپ حرکت، استیک راست چرخش، تریگر راست تعامل و گریپ راست چراغ‌قوه.

### نقشه انیمیشن
`Idle`, `Walk`, `Run`, `Stare`, `Frightened`, `Scare`, `Attack`, `Appear`, `Vanish`, `Stunned`.

این‌ها **حالت‌های نرم‌افزاری** هستند؛ اگر داخل FBX کلیپ‌های متفاوتی برای همه آن‌ها نباشد، حالت‌ها ممکن است کلیپ مشترک و سرعت متفاوت بگیرند. برای انیمیشن‌های طبیعی بیشتر، فایل‌های واقعی Humanoid/Generic سازگار را در `Assets/LastShift/Characters/ThePatient/Animations` اضافه کن.

### اگر مدل نمایش داده نشد

- Console باید پیام `[LastShift] The Patient is VISIBLE and READY` بدهد. اگر به‌جای آن خطای `No renderable 3D meshes` دیدی، FBX به‌عنوان مش قابل رندر Import نشده است.
- اگر `Importer failed to load` دیدی، در Unity روی `shabannn.fbx` کلیک کن و از Inspector دوباره Import را بررسی کن.
- اگر مدل صورتی/بی‌تکسچر است، هشدارهای `[LastShift / Materials]` را بخوان. بافت‌ها را در FBX Inspector > Materials استخراج/Relink کن.
- اگر مدل از بغل یا پشت دیده می‌شود، جهت محورهای FBX در Unity را بررسی کن؛ با تنظیم چرخش ریشه مدل می‌توان آن را اصلاح کرد.
- برای هر بار تغییر مدل یا انیمیشن، صحنه را با **همان یک منوی نهایی** دوباره بساز.
- اگر منوهای DEBUG قدیمی هنوز ظاهر هستند، پوشه قبلی پاک نشده؛ جایگزینی تمیز انجام بده.

### اعتبار مدل و مجوز
**Ghost Girl Animated** اثر **Shaban Hafizsalim / shabaninteractive** از Sketchfab، تحت مجوز **CC BY 4.0**.
https://sketchfab.com/3d-models/ghost-girl-animated-1091b3ee172f4951aaeafa2581f9de1d

متن اعتبار کامل در `Assets/LastShift/Characters/ThePatient/ATTRIBUTION.md` است و هنگام انتشار باید حفظ شود.
