using System.Globalization;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Core.Localization;

public class LocalizationService : ILocalizationService
{
    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
    {
        ["he"] = new()
        {
            [LocalizationKeys.AppName] = "Tomer Group",
            [LocalizationKeys.Tagline] = "חוויות טיול בפרו",
            [LocalizationKeys.Welcome] = "ברוכים הבאים",
            [LocalizationKeys.Loading] = "טוען...",
            [LocalizationKeys.Retry] = "נסה שוב",
            [LocalizationKeys.Error] = "שגיאה",
            [LocalizationKeys.Save] = "שמור",
            [LocalizationKeys.Cancel] = "ביטול",
            [LocalizationKeys.Close] = "סגור",
            [LocalizationKeys.Online] = "מחובר",
            [LocalizationKeys.Offline] = "לא מחובר (מצב לא מקוון)",

            [LocalizationKeys.NavHome] = "בית",
            [LocalizationKeys.NavMyTrip] = "הטיול שלי",
            [LocalizationKeys.NavBookings] = "הזמנות",
            [LocalizationKeys.NavDocuments] = "מסמכים",
            [LocalizationKeys.NavProfile] = "פרופיל",
            [LocalizationKeys.NavMore] = "עוד",

            [LocalizationKeys.Greeting] = "שלום 👋",
            [LocalizationKeys.YourTripWith] = "הטיול שלך עם Tomer Group",
            [LocalizationKeys.CountryPeru] = "פרו 🇵🇪",
            [LocalizationKeys.NextActivity] = "הפעילות הבאה:",
            [LocalizationKeys.UpcomingActivities] = "פעילויות קרובות",
            [LocalizationKeys.HotelPickup] = "05:30 🚐 איסוף מהמלון",
            [LocalizationKeys.MachuPicchuTour] = "08:30 🏔️ מאצ'ו פיצ'ו",
            [LocalizationKeys.ReturnTrain] = "16:00 🚂 רכבת חזרה",
            [LocalizationKeys.ContactAgency] = "קשר עם הסוכנות",
            [LocalizationKeys.EmergencyContact] = "מוקד חירום 24/7",
            [LocalizationKeys.MyTripButton] = "הטיול שלי",
            [LocalizationKeys.MyBookingsButton] = "ההזמנות שלי",
            [LocalizationKeys.MyDocumentsButton] = "המסמכים שלי",
            [LocalizationKeys.ContactUsButton] = "צור קשר",

            [LocalizationKeys.Day] = "יום",
            [LocalizationKeys.Location] = "מיקום",
            [LocalizationKeys.Guide] = "מדריך",
            [LocalizationKeys.Driver] = "נהג",
            [LocalizationKeys.Instructions] = "הנחיות חשובות",
            [LocalizationKeys.ScheduledTime] = "שעה מתוכננת",

            [LocalizationKeys.Login] = "התחברות",
            [LocalizationKeys.Email] = "דואר אלקטרוני",
            [LocalizationKeys.Password] = "סיסמה",
            [LocalizationKeys.ForgotPassword] = "שכחתי סיסמה",
            [LocalizationKeys.CustomerLogin] = "כניסת מטייל",
            [LocalizationKeys.AgencyStaffLogin] = "כניסת צוות סוכנות",
            [LocalizationKeys.Logout] = "התנתק",

            [LocalizationKeys.AgencyDashboard] = "לוח פעילות סוכנות",
            [LocalizationKeys.TodayArrivals] = "נחיתות היום",
            [LocalizationKeys.TodayDepartures] = "עזיבות היום",
            [LocalizationKeys.TodayTours] = "סיורים פעילים היום",
            [LocalizationKeys.ActiveTravelers] = "מטיילים בפרו",
            [LocalizationKeys.Revenue] = "הכנסות",
            [LocalizationKeys.Expenses] = "הוצאות",
            [LocalizationKeys.GrossProfit] = "רווח גולמי",
            [LocalizationKeys.OutstandingPayments] = "תשלומים פתוחים",

            [LocalizationKeys.Settings] = "הגדרות",
            [LocalizationKeys.Language] = "שפה",
            [LocalizationKeys.Hebrew] = "עברית (עברית)",
            [LocalizationKeys.English] = "English (אנגלית)",
            [LocalizationKeys.Spanish] = "Español (ספרדית)",
            [LocalizationKeys.Theme] = "ערכת נושא",
            [LocalizationKeys.LightMode] = "מצב בהיר",
            [LocalizationKeys.DarkMode] = "מצב כהה",

            [LocalizationKeys.ProfileTitle] = "פרופיל מטייל",
            [LocalizationKeys.PersonalInfo] = "פרטים אישיים",
            [LocalizationKeys.HebrewName] = "שם בעברית",
            [LocalizationKeys.PassportName] = "שם באנגלית (כמו בדרכון)",
            [LocalizationKeys.PassportNumber] = "מספר דרכון",
            [LocalizationKeys.PassportExpiration] = "תוקף דרכון",
            [LocalizationKeys.PassportExpiringWarning] = "שים לב: הדרכון יפוג תוך פחות מ-6 חודשים!",
            [LocalizationKeys.IsraelId] = "תעודת זהות",
            [LocalizationKeys.RevealSensitive] = "הצג פרטים רגישים",
            [LocalizationKeys.HideSensitive] = "הסתר",
            [LocalizationKeys.EmergencyContactSection] = "איש קשר לחירום",
            [LocalizationKeys.DietaryPreferencesSection] = "העדפות תזונה וכשרות",
            [LocalizationKeys.MedicalNotesSection] = "הערות רפואיות וגבהים (סורוצ'ה)",
            [LocalizationKeys.AltitudeSicknessNotice] = "בפרו (קוסקו, 3,400 מ') יש סיכון למחלת גבהים (סורוצ'ה). שתה הרבה מים ותה קוקה.",
            [LocalizationKeys.InsuranceDetails] = "ביטוח נסיעות וחילוץ",
            [LocalizationKeys.SaveProfile] = "שמור שינויים",
            [LocalizationKeys.ProfileUpdatedSuccess] = "הפרופיל עודכן בהצלחה",
            [LocalizationKeys.AgencyCustomers] = "ניהול מטיילים",
            [LocalizationKeys.SearchTravelers] = "חיפוש מטיילים...",
            [LocalizationKeys.ActiveInPeruTag] = "שוהה כעת בפרו"
        },
        ["en"] = new()
        {
            [LocalizationKeys.AppName] = "Tomer Group",
            [LocalizationKeys.Tagline] = "Peru Travel Experience",
            [LocalizationKeys.Welcome] = "Welcome",
            [LocalizationKeys.Loading] = "Loading...",
            [LocalizationKeys.Retry] = "Retry",
            [LocalizationKeys.Error] = "Error",
            [LocalizationKeys.Save] = "Save",
            [LocalizationKeys.Cancel] = "Cancel",
            [LocalizationKeys.Close] = "Close",
            [LocalizationKeys.Online] = "Online",
            [LocalizationKeys.Offline] = "Offline Mode",

            [LocalizationKeys.NavHome] = "Home",
            [LocalizationKeys.NavMyTrip] = "My Trip",
            [LocalizationKeys.NavBookings] = "Bookings",
            [LocalizationKeys.NavDocuments] = "Documents",
            [LocalizationKeys.NavProfile] = "Profile",
            [LocalizationKeys.NavMore] = "More",

            [LocalizationKeys.Greeting] = "Hello 👋",
            [LocalizationKeys.YourTripWith] = "Your trip with Tomer Group",
            [LocalizationKeys.CountryPeru] = "Peru 🇵🇪",
            [LocalizationKeys.NextActivity] = "Next Activity:",
            [LocalizationKeys.UpcomingActivities] = "Upcoming Activities",
            [LocalizationKeys.HotelPickup] = "05:30 🚐 Hotel Pickup",
            [LocalizationKeys.MachuPicchuTour] = "08:30 🏔️ Machu Picchu",
            [LocalizationKeys.ReturnTrain] = "16:00 🚂 Return Train",
            [LocalizationKeys.ContactAgency] = "Contact Agency",
            [LocalizationKeys.EmergencyContact] = "24/7 Cusco Emergency",
            [LocalizationKeys.MyTripButton] = "My Trip",
            [LocalizationKeys.MyBookingsButton] = "My Bookings",
            [LocalizationKeys.MyDocumentsButton] = "My Documents",
            [LocalizationKeys.ContactUsButton] = "Contact Us",

            [LocalizationKeys.Day] = "Day",
            [LocalizationKeys.Location] = "Location",
            [LocalizationKeys.Guide] = "Guide",
            [LocalizationKeys.Driver] = "Driver",
            [LocalizationKeys.Instructions] = "Important Instructions",
            [LocalizationKeys.ScheduledTime] = "Scheduled Time",

            [LocalizationKeys.Login] = "Login",
            [LocalizationKeys.Email] = "Email",
            [LocalizationKeys.Password] = "Password",
            [LocalizationKeys.ForgotPassword] = "Forgot Password?",
            [LocalizationKeys.CustomerLogin] = "Traveler Login",
            [LocalizationKeys.AgencyStaffLogin] = "Agency Staff Login",
            [LocalizationKeys.Logout] = "Logout",

            [LocalizationKeys.AgencyDashboard] = "Agency Operations Dashboard",
            [LocalizationKeys.TodayArrivals] = "Today's Arrivals",
            [LocalizationKeys.TodayDepartures] = "Today's Departures",
            [LocalizationKeys.TodayTours] = "Active Tours Today",
            [LocalizationKeys.ActiveTravelers] = "Active Travelers in Peru",
            [LocalizationKeys.Revenue] = "Revenue",
            [LocalizationKeys.Expenses] = "Expenses",
            [LocalizationKeys.GrossProfit] = "Gross Profit",
            [LocalizationKeys.OutstandingPayments] = "Pending Payments",

            [LocalizationKeys.Settings] = "Settings",
            [LocalizationKeys.Language] = "Language",
            [LocalizationKeys.Hebrew] = "עברית (Hebrew)",
            [LocalizationKeys.English] = "English",
            [LocalizationKeys.Spanish] = "Español (Spanish)",
            [LocalizationKeys.Theme] = "Theme",
            [LocalizationKeys.LightMode] = "Light Mode",
            [LocalizationKeys.DarkMode] = "Dark Mode",

            [LocalizationKeys.ProfileTitle] = "Traveler Profile",
            [LocalizationKeys.PersonalInfo] = "Personal Information",
            [LocalizationKeys.HebrewName] = "Hebrew Name",
            [LocalizationKeys.PassportName] = "Passport Name",
            [LocalizationKeys.PassportNumber] = "Passport Number",
            [LocalizationKeys.PassportExpiration] = "Passport Expiration",
            [LocalizationKeys.PassportExpiringWarning] = "Warning: Passport expires in less than 6 months!",
            [LocalizationKeys.IsraelId] = "National ID / Teudat Zehut",
            [LocalizationKeys.RevealSensitive] = "Reveal Sensitive Details",
            [LocalizationKeys.HideSensitive] = "Hide Details",
            [LocalizationKeys.EmergencyContactSection] = "Emergency Contact",
            [LocalizationKeys.DietaryPreferencesSection] = "Dietary & Kosher Preferences",
            [LocalizationKeys.MedicalNotesSection] = "Medical Notes & Altitude Awareness",
            [LocalizationKeys.AltitudeSicknessNotice] = "In Cusco (3,400m / 11,150ft), stay hydrated and take it easy on your first day.",
            [LocalizationKeys.InsuranceDetails] = "Travel & Rescue Insurance",
            [LocalizationKeys.SaveProfile] = "Save Profile",
            [LocalizationKeys.ProfileUpdatedSuccess] = "Profile updated successfully",
            [LocalizationKeys.AgencyCustomers] = "Travelers Directory",
            [LocalizationKeys.SearchTravelers] = "Search travelers...",
            [LocalizationKeys.ActiveInPeruTag] = "Active in Peru"
        },
        ["es"] = new()
        {
            [LocalizationKeys.AppName] = "Tomer Group",
            [LocalizationKeys.Tagline] = "Experiencia de Viaje en Perú",
            [LocalizationKeys.Welcome] = "Bienvenido",
            [LocalizationKeys.Loading] = "Cargando...",
            [LocalizationKeys.Retry] = "Reintentar",
            [LocalizationKeys.Error] = "Error",
            [LocalizationKeys.Save] = "Guardar",
            [LocalizationKeys.Cancel] = "Cancelar",
            [LocalizationKeys.Close] = "Cerrar",
            [LocalizationKeys.Online] = "En línea",
            [LocalizationKeys.Offline] = "Modo Desconectado",

            [LocalizationKeys.NavHome] = "Inicio",
            [LocalizationKeys.NavMyTrip] = "Mi Viaje",
            [LocalizationKeys.NavBookings] = "Reservas",
            [LocalizationKeys.NavDocuments] = "Documentos",
            [LocalizationKeys.NavProfile] = "Perfil",
            [LocalizationKeys.NavMore] = "Más",

            [LocalizationKeys.Greeting] = "Hola 👋",
            [LocalizationKeys.YourTripWith] = "Tu viaje con Tomer Group",
            [LocalizationKeys.CountryPeru] = "Perú 🇵🇪",
            [LocalizationKeys.NextActivity] = "Siguiente Actividad:",
            [LocalizationKeys.UpcomingActivities] = "Próximas Actividades",
            [LocalizationKeys.HotelPickup] = "05:30 🚐 Recojo del hotel",
            [LocalizationKeys.MachuPicchuTour] = "08:30 🏔️ Machu Picchu",
            [LocalizationKeys.ReturnTrain] = "16:00 🚂 Tren de regreso",
            [LocalizationKeys.ContactAgency] = "Contacto Agencia",
            [LocalizationKeys.EmergencyContact] = "Emergencia 24/7",
            [LocalizationKeys.MyTripButton] = "Mi Viaje",
            [LocalizationKeys.MyBookingsButton] = "Mis Reservas",
            [LocalizationKeys.MyDocumentsButton] = "Mis Documentos",
            [LocalizationKeys.ContactUsButton] = "Contáctenos",

            [LocalizationKeys.Day] = "Día",
            [LocalizationKeys.Location] = "Ubicación",
            [LocalizationKeys.Guide] = "Guía",
            [LocalizationKeys.Driver] = "Conductor",
            [LocalizationKeys.Instructions] = "Instrucciones Importantes",
            [LocalizationKeys.ScheduledTime] = "Hora Programada",

            [LocalizationKeys.Login] = "Iniciar Sesión",
            [LocalizationKeys.Email] = "Correo Electrónico",
            [LocalizationKeys.Password] = "Contraseña",
            [LocalizationKeys.ForgotPassword] = "¿Olvidó su contraseña?",
            [LocalizationKeys.CustomerLogin] = "Acceso Viajero",
            [LocalizationKeys.AgencyStaffLogin] = "Acceso Personal de Agencia",
            [LocalizationKeys.Logout] = "Cerrar Sesión",

            [LocalizationKeys.AgencyDashboard] = "Panel de Operaciones de Agencia",
            [LocalizationKeys.TodayArrivals] = "Llegadas de Hoy",
            [LocalizationKeys.TodayDepartures] = "Salidas de Hoy",
            [LocalizationKeys.TodayTours] = "Tours Activos Hoy",
            [LocalizationKeys.ActiveTravelers] = "Viajeros Activos en Perú",
            [LocalizationKeys.Revenue] = "Ingresos",
            [LocalizationKeys.Expenses] = "Gastos",
            [LocalizationKeys.GrossProfit] = "Ganancia Bruta",
            [LocalizationKeys.OutstandingPayments] = "Pagos Pendientes",

            [LocalizationKeys.Settings] = "Configuración",
            [LocalizationKeys.Language] = "Idioma",
            [LocalizationKeys.Hebrew] = "עברית (Hebreo)",
            [LocalizationKeys.English] = "English (Inglés)",
            [LocalizationKeys.Spanish] = "Español",
            [LocalizationKeys.Theme] = "Tema",
            [LocalizationKeys.LightMode] = "Modo Claro",
            [LocalizationKeys.DarkMode] = "Modo Oscuro",

            [LocalizationKeys.ProfileTitle] = "Perfil del Viajero",
            [LocalizationKeys.PersonalInfo] = "Información Personal",
            [LocalizationKeys.HebrewName] = "Nombre en Hebreo",
            [LocalizationKeys.PassportName] = "Nombre en Pasaporte",
            [LocalizationKeys.PassportNumber] = "Número de Pasaporte",
            [LocalizationKeys.PassportExpiration] = "Vencimiento del Pasaporte",
            [LocalizationKeys.PassportExpiringWarning] = "Atención: ¡El pasaporte vence en menos de 6 meses!",
            [LocalizationKeys.IsraelId] = "DNI / Identificación",
            [LocalizationKeys.RevealSensitive] = "Mostrar Datos Protegidos",
            [LocalizationKeys.HideSensitive] = "Ocultar",
            [LocalizationKeys.EmergencyContactSection] = "Contacto de Emergencia",
            [LocalizationKeys.DietaryPreferencesSection] = "Preferencias Dietéticas y Kosher",
            [LocalizationKeys.MedicalNotesSection] = "Notas Médicas y Mal de Altura (Soroche)",
            [LocalizationKeys.AltitudeSicknessNotice] = "En Cusco (3,400 msnm), manténgase hidratado y descanse el primer día.",
            [LocalizationKeys.InsuranceDetails] = "Seguro de Viaje y Rescate",
            [LocalizationKeys.SaveProfile] = "Guardar Perfil",
            [LocalizationKeys.ProfileUpdatedSuccess] = "Perfil actualizado exitosamente",
            [LocalizationKeys.AgencyCustomers] = "Directorio de Viajeros",
            [LocalizationKeys.SearchTravelers] = "Buscar viajeros...",
            [LocalizationKeys.ActiveInPeruTag] = "Activo en Perú"
        }
    };

    private string _currentLanguage = "he"; // Default Hebrew

    public event Action? LanguageChanged;

    public string CurrentLanguage => _currentLanguage;

    public bool IsRightToLeft => _currentLanguage.Equals("he", StringComparison.OrdinalIgnoreCase);

    public void SetLanguage(string language)
    {
        var normalized = language.ToLowerInvariant();
        if (normalized != "he" && normalized != "en" && normalized != "es")
        {
            normalized = "he";
        }

        if (_currentLanguage != normalized)
        {
            _currentLanguage = normalized;
            LanguageChanged?.Invoke();
        }
    }

    public string GetString(string key, string? language = null)
    {
        var lang = (language ?? _currentLanguage).ToLowerInvariant();
        if (!Translations.ContainsKey(lang))
        {
            lang = "he";
        }

        if (Translations[lang].TryGetValue(key, out var val))
        {
            return val;
        }

        // Fallback to Hebrew, then English
        if (lang != "he" && Translations["he"].TryGetValue(key, out var heVal))
        {
            return heVal;
        }

        if (Translations["en"].TryGetValue(key, out var enVal))
        {
            return enVal;
        }

        return key;
    }

    /// <summary>
    /// Enforces LTR display for phone numbers even when Hebrew RTL is active.
    /// Uses Unicode Left-to-Right Embedding (U+202A) and Pop Directional Formatting (U+202C).
    /// </summary>
    public static string FormatPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        return $"\u202A{phone.Trim()}\u202C";
    }

    /// <summary>
    /// Enforces LTR display for technical IDs (e.g. TG-2026-00482) and passport numbers.
    /// </summary>
    public static string FormatTechnicalId(string technicalId)
    {
        if (string.IsNullOrWhiteSpace(technicalId)) return string.Empty;
        return $"\u202A{technicalId.Trim()}\u202C";
    }

    /// <summary>
    /// Formats currency readable in both RTL and LTR contexts.
    /// </summary>
    public static string FormatCurrency(decimal amount, Currency currency)
    {
        return currency switch
        {
            Currency.USD => $"${amount:N0} USD",
            Currency.PEN => $"S/ {amount:N0} PEN",
            Currency.ILS => $"₪{amount:N0} ILS",
            _ => $"${amount:N0}"
        };
    }
}

