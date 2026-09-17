using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Security;

namespace TomerGroup.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(TomerDbContext context, IPasswordHasher hasher, bool isDevelopment = false)
    {
        // 1. Seed BrandSettings if none exists
        if (!await context.BrandSettings.AnyAsync())
        {
            await context.BrandSettings.AddAsync(BrandSettings.CreateDefault());
        }

        // 2. Seed Roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = Enum.GetValues<UserRole>().Select(r => new Role
            {
                Name = r.ToString(),
                Description = $"{r} role for Tomer Group system",
                RoleType = r,
                Permissions = new List<string> { $"{r}:Read", $"{r}:Write" }
            }).ToList();

            await context.Roles.AddRangeAsync(roles);
        }

        // 3. Seed Users and Staff
        if (!await context.Users.AnyAsync())
        {
            var (adminHash, adminSalt) = hasher.HashPassword("TomerAdmin2026!");
            var (managerHash, managerSalt) = hasher.HashPassword("TomerManager2026!");
            var (salesHash, salesSalt) = hasher.HashPassword("TomerSales2026!");
            var (opsHash, opsSalt) = hasher.HashPassword("TomerOps2026!");
            var (financeHash, financeSalt) = hasher.HashPassword("TomerFinance2026!");
            var (guideHash, guideSalt) = hasher.HashPassword("TomerGuide2026!");
            var (driverHash, driverSalt) = hasher.HashPassword("TomerDriver2026!");
            var (customerHash, customerSalt) = hasher.HashPassword("Traveler2026!");

            var adminUser = new User
            {
                Email = "admin@tomergroup.com",
                PasswordHash = adminHash,
                Salt = adminSalt,
                FirstName = "Yossi",
                LastName = "Cohen",
                Phone = "+51 984 111 222",
                Role = UserRole.Admin,
                PreferredLanguage = "he"
            };

            var managerUser = new User
            {
                Email = "manager@tomergroup.com",
                PasswordHash = managerHash,
                Salt = managerSalt,
                FirstName = "Ronit",
                LastName = "Stern",
                Phone = "+51 984 222 333",
                Role = UserRole.Manager,
                PreferredLanguage = "he"
            };

            var salesUser = new User
            {
                Email = "sales@tomergroup.com",
                PasswordHash = salesHash,
                Salt = salesSalt,
                FirstName = "David",
                LastName = "Katz",
                Phone = "+51 984 333 111",
                Role = UserRole.Sales,
                PreferredLanguage = "he"
            };

            var opsUser = new User
            {
                Email = "ops@tomergroup.com",
                PasswordHash = opsHash,
                Salt = opsSalt,
                FirstName = "Alex",
                LastName = "Rodriguez",
                Phone = "+51 984 333 444",
                Role = UserRole.Operations,
                PreferredLanguage = "es"
            };

            var financeUser = new User
            {
                Email = "finance@tomergroup.com",
                PasswordHash = financeHash,
                Salt = financeSalt,
                FirstName = "Maya",
                LastName = "Bar",
                Phone = "+51 984 444 555",
                Role = UserRole.Finance,
                PreferredLanguage = "he"
            };

            var guideUser = new User
            {
                Email = "guide@tomergroup.com",
                PasswordHash = guideHash,
                Salt = guideSalt,
                FirstName = "Carlos",
                LastName = "Quispe",
                Phone = "+51 984 555 666",
                Role = UserRole.Guide,
                PreferredLanguage = "es"
            };

            var driverUser = new User
            {
                Email = "driver@tomergroup.com",
                PasswordHash = driverHash,
                Salt = driverSalt,
                FirstName = "Juan",
                LastName = "Flores",
                Phone = "+51 984 777 888",
                Role = UserRole.Driver,
                PreferredLanguage = "es"
            };

            var staffUsers = new List<User> { adminUser, managerUser, salesUser, opsUser, financeUser, guideUser, driverUser };
            await context.Users.AddRangeAsync(staffUsers);
            await context.SaveChangesAsync();

            // Seed Guide & Driver entities
            var guide = new Guide
            {
                FullName = "Carlos Quispe",
                Phone = "+51 984 555 666",
                WhatsApp = "+51 984 555 666",
                Languages = new List<string> { "Hebrew", "English", "Spanish", "Quechua" },
                Specialization = "Inca Sanctuary, Sacred Valley, High Altitude Flora/Fauna",
                IsAvailable = true
            };

            var driver = new Driver
            {
                FullName = "Juan Flores",
                Phone = "+51 984 777 888",
                WhatsApp = "+51 984 777 888",
                LicenseNumber = "PER-CUS-88492",
                IsAvailable = true
            };

            var vehicle = new Vehicle
            {
                Model = "Mercedes-Benz Sprinter Executive",
                LicensePlate = "X2A-941",
                PassengerCapacity = 15,
                Type = "Van",
                AssignedDriver = driver
            };

            await context.Guides.AddAsync(guide);
            await context.Drivers.AddAsync(driver);
            await context.Vehicles.AddAsync(vehicle);
            await context.SaveChangesAsync();

        }

        // 4. Seed Peru Destinations (Dynamic Destinations)
        if (!await context.Destinations.AnyAsync())
        {
            var destinations = new List<Destination>
            {
                new() { Name = "Cusco", HebrewName = "קוסקו", SpanishName = "Cusco", Region = "Cusco", AltitudeMeters = 3400, Description = "Historic capital of the Incan Empire and gateway to the Sacred Valley.", AltitudeWarning = "High altitude (3,400m). Acclimatization recommended.", IsPopular = true },
                new() { Name = "Machu Picchu", HebrewName = "מאצ'ו פיצ'ו", SpanishName = "Machu Picchu", Region = "Cusco", AltitudeMeters = 2430, Description = "Iconic 15th-century Incan citadel situated on a mountain ridge.", IsPopular = true },
                new() { Name = "Sacred Valley", HebrewName = "העמק הקדוש", SpanishName = "Valle Sagrado", Region = "Cusco", AltitudeMeters = 2800, Description = "Fertile valley containing vital Incan agricultural and religious ruins.", IsPopular = true },
                new() { Name = "Ollantaytambo", HebrewName = "אולאנטייטמבו", SpanishName = "Ollantaytambo", Region = "Cusco", AltitudeMeters = 2792, Description = "Living Incan town with monumental fortress ruins.", IsPopular = true },
                new() { Name = "Pisac", HebrewName = "פיסאק", SpanishName = "Písac", Region = "Cusco", AltitudeMeters = 2972, Description = "Famous Sunday handicraft textile market and terraced citadel.", IsPopular = true },
                new() { Name = "Aguas Calientes", HebrewName = "אגואס קליינטס", SpanishName = "Aguas Calientes", Region = "Cusco", AltitudeMeters = 2040, Description = "Town at the foot of Machu Picchu with thermal springs.", IsPopular = true },
                new() { Name = "Rainbow Mountain", HebrewName = "הר שבעת הצבעים", SpanishName = "Vinicunca", Region = "Cusco", AltitudeMeters = 5036, Description = "Mineral-striped rainbow mountain in the high Andes.", AltitudeWarning = "Extreme altitude (5,036m). Acclimatize before hiking.", IsPopular = true },
                new() { Name = "Humantay Lake", HebrewName = "לגונת הומנטאי", SpanishName = "Laguna Humantay", Region = "Cusco", AltitudeMeters = 4200, Description = "Glacial turquoise alpine lake beneath Mount Salkantay.", AltitudeWarning = "Very high altitude (4,200m).", IsPopular = true },
                new() { Name = "Lake Titicaca", HebrewName = "אגם טיטיקקה", SpanishName = "Lago Titicaca", Region = "Puno", AltitudeMeters = 3812, Description = "Highest navigable lake in the world, home to floating Uros islands.", AltitudeWarning = "High altitude (3,812m).", IsPopular = true },
                new() { Name = "Puno", HebrewName = "פונו", SpanishName = "Puno", Region = "Puno", AltitudeMeters = 3827, Description = "Folkloric capital of Peru on the shores of Lake Titicaca.", AltitudeWarning = "High altitude (3,827m).", IsPopular = true },
                new() { Name = "Lima", HebrewName = "לימה", SpanishName = "Lima", Region = "Lima", AltitudeMeters = 150, Description = "Gastronomic capital of South America on the Pacific coast.", IsPopular = true },
                new() { Name = "Arequipa", HebrewName = "ארקיפה", SpanishName = "Arequipa", Region = "Arequipa", AltitudeMeters = 2325, Description = "The White City of volcanic sillar stone beneath El Misti volcano.", IsPopular = true },
                new() { Name = "Colca Canyon", HebrewName = "קניון קולקה", SpanishName = "Cañón del Colca", Region = "Arequipa", AltitudeMeters = 3635, Description = "One of the world's deepest canyons, famous for Andean Condors.", IsPopular = true },
                new() { Name = "Amazon", HebrewName = "אמזונס", SpanishName = "Amazonía", Region = "Madre de Dios", AltitudeMeters = 180, Description = "Biodiverse tropical rainforest reserves along Madre de Dios river.", IsPopular = true },
                new() { Name = "Salkantay", HebrewName = "סלקנטאי", SpanishName = "Salkantay", Region = "Cusco", AltitudeMeters = 4630, Description = "Dramatic snow-capped peak and premier trekking alternative to Machu Picchu.", AltitudeWarning = "High mountain pass at 4,630m.", IsPopular = true },
                new() { Name = "Inca Trail", HebrewName = "שביל האינקה", SpanishName = "Camino Inca", Region = "Cusco", AltitudeMeters = 4215, Description = "Ancient pilgrimage foot trail through Andean cloud forests to Machu Picchu.", AltitudeWarning = "Warmiwañusqa (Dead Woman's Pass) at 4,215m.", IsPopular = true }
            };

            await context.Destinations.AddRangeAsync(destinations);
            await context.SaveChangesAsync();
        }

        // 11. Seed Canonical Peru Tours (Section 11)
        if (!await context.Tours.AnyAsync())
        {
            var tours = new List<Tour>
            {
                new()
                {
                    Name = "Machu Picchu Classic Citadel Tour",
                    HebrewName = "מאצ'ו פיצ'ו סיור מצודה קלאסי",
                    Description = "Comprehensive guided tour of the iconic Incan citadel with panoramic train journey and expert Hebrew/English speaking guide.",
                    HebrewDescription = "סיור מודרך מקיף במצודת האינקה המפורסמת, כולל נסיעה ברכבת פנורמית והדרכה מקצועית.",
                    Destination = "Machu Picchu",
                    Category = TourCategory.DayTour,
                    Duration = "Full Day",
                    DurationDays = 1,
                    Difficulty = "Moderate",
                    MaxCapacity = 16,
                    AltitudeMaxMeters = 2430,
                    RequiresAcclimatization = false,
                    AdultPrice = 380,
                    ChildPrice = 280,
                    PrivatePrice = 650,
                    AgencyCost = 260,
                    Currency = Currency.USD,
                    IncludedServices = new() { "Vistadome train tickets", "Consettur bus passes", "Circuit 2 sanctuary entrance", "Certified bilingual guide", "Kosher lunch box from Chabad" },
                    ExcludedServices = new() { "Huayna Picchu climb ticket", "Personal tips" },
                    PickupInformation = "Hotel lobby pickup in Cusco at 05:30 AM",
                    MeetingPoint = "Plaza Regocijo, Cusco",
                    LocationsVisited = new() { "Ollantaytambo", "Aguas Calientes", "Machu Picchu Sanctuary" },
                    CancellationPolicy = "Non-refundable entrance tickets; train refundable up to 48 hours prior.",
                    KosherFoodAvailable = true,
                    KosherCertificationDetails = "Glatt Kosher lunch box provided by Chabad House Cusco",
                    BookingCutoffHours = 48
                },
                new()
                {
                    Name = "Salkantay Trek to Machu Picchu",
                    HebrewName = "טרק סלקנטאי למאצ'ו פיצ'ו 5 ימים",
                    Description = "Voted among top 25 treks in the world. Cross the high Salkantay Pass (4,630m) down into lush cloud forests.",
                    HebrewDescription = "אחד מ-25 הטרקים היפים בעולם. חציית מעבר סלקנטאי בגובה 4,630 מ' וירידה ליערות העננים.",
                    Destination = "Salkantay",
                    Category = TourCategory.Trek,
                    Duration = "5 Days / 4 Nights",
                    DurationDays = 5,
                    Difficulty = "Challenging",
                    MaxCapacity = 12,
                    AltitudeMaxMeters = 4630,
                    RequiresAcclimatization = true,
                    AdultPrice = 650,
                    ChildPrice = 550,
                    PrivatePrice = 1100,
                    AgencyCost = 420,
                    Currency = Currency.USD,
                    IncludedServices = new() { "Mountain guide", "Horses and wranglers for luggage", "Four-season expedition tents", "All meals (Kosher trek cook available)", "Emergency oxygen tank" },
                    ExcludedServices = new() { "Sleeping bag rental", "Trekking poles rental" },
                    PickupInformation = "04:30 AM pickup from Cusco accommodations",
                    MeetingPoint = "Tomer Group Operations Center, Cusco",
                    LocationsVisited = new() { "Mollepata", "Soraypampa", "Humantay Lake", "Salkantay Pass", "Chaullay", "Santa Teresa", "Aguas Calientes", "Machu Picchu" },
                    CancellationPolicy = "Cancellations 30 days prior eligible for 70% refund.",
                    KosherFoodAvailable = true,
                    KosherCertificationDetails = "Dedicated kosher cookware and certified kosher trail food available",
                    BookingCutoffHours = 120
                },
                new()
                {
                    Name = "Rainbow Mountain & Red Valley Trek",
                    HebrewName = "הר שבעת הצבעים ועמק האדום",
                    Description = "Early morning expedition to the vibrant mineral peaks of Vinicunca (5,036m) and the stunning Red Valley.",
                    HebrewDescription = "יציאה מוקדמת לפסגות המרהיבות של ויניקונקה (5,036 מ') ועמק האדום עוצר הנשימה.",
                    Destination = "Rainbow Mountain",
                    Category = TourCategory.Trek,
                    Duration = "Full Day",
                    DurationDays = 1,
                    Difficulty = "Strenuous",
                    MaxCapacity = 14,
                    AltitudeMaxMeters = 5036,
                    RequiresAcclimatization = true,
                    AdultPrice = 120,
                    ChildPrice = 90,
                    PrivatePrice = 280,
                    AgencyCost = 75,
                    Currency = Currency.USD,
                    IncludedServices = new() { "Private transport", "Professional trekking guide", "Hot buffet breakfast and lunch", "Community entrance permits", "Medical first aid and oxygen kit" },
                    ExcludedServices = new() { "Horse rental for uphill ascent", "Personal snacks" },
                    PickupInformation = "04:00 AM hotel pickup in Cusco",
                    MeetingPoint = "Hotel lobby",
                    LocationsVisited = new() { "Cusipata", "Vinicunca", "Red Valley" },
                    CancellationPolicy = "Full refund if cancelled 24 hours prior.",
                    KosherFoodAvailable = true,
                    KosherCertificationDetails = "Kosher breakfast and packed trail lunch prepared by Chabad Cusco",
                    BookingCutoffHours = 24
                },
                new()
                {
                    Name = "Sacred Valley & Pisac Handicraft Market",
                    HebrewName = "העמק הקדוש ושוק עבודות היד פיסאק",
                    Description = "Scenic journey through Incan agricultural terraces, colorful indigenous artisan markets, and monumental Ollantaytambo fortress.",
                    HebrewDescription = "מסע נופי בין טרסות חקלאיות של האינקה, שוק האומנים בפיסאק ומבצר אולאנטייטמבו המונומנטלי.",
                    Destination = "Sacred Valley",
                    Category = TourCategory.Cultural,
                    Duration = "Full Day",
                    DurationDays = 1,
                    Difficulty = "Easy",
                    MaxCapacity = 18,
                    AltitudeMaxMeters = 2972,
                    RequiresAcclimatization = false,
                    AdultPrice = 150,
                    ChildPrice = 110,
                    PrivatePrice = 320,
                    AgencyCost = 90,
                    Currency = Currency.USD,
                    IncludedServices = new() { "Private tourist transport", "English/Hebrew bilingual guide", "Buffet lunch in Urubamba", "Sacred Valley tourist ticket access" },
                    ExcludedServices = new() { "Personal shopping purchases", "Gratuities" },
                    PickupInformation = "07:30 AM hotel pickup in Cusco",
                    MeetingPoint = "Hotel lobby",
                    LocationsVisited = new() { "Pisac Market", "Pisac Archaeological Site", "Urubamba", "Ollantaytambo Fortress" },
                    CancellationPolicy = "Free cancellation up to 24 hours in advance.",
                    KosherFoodAvailable = true,
                    KosherCertificationDetails = "Kosher dairy/parve lunch options certified by Chabad Cusco",
                    BookingCutoffHours = 24
                }
            };

            await context.Tours.AddRangeAsync(tours);
            await context.SaveChangesAsync();
        }

        // 12. Seed Canonical Peru Hotels (Section 15)
        if (!await context.Hotels.AnyAsync())
        {
            var hotels = new List<Hotel>
            {
                new()
                {
                    Name = "Palacio del Inka, A Luxury Collection Hotel",
                    HebrewName = "פלאסיו דל אינקה - קוסקו (לקשרי קולקשן)",
                    Address = "Plazoleta Santo Domingo 259, Cusco",
                    Destination = "Cusco",
                    Stars = 5,
                    Phone = "+51 84 231961",
                    WhatsApp = "+51 984 231961",
                    Email = "concierge@palaciodelinka.com",
                    Website = "https://www.marriott.com/palacio-del-inka",
                    HasOxygenEnrichedRooms = true,
                    HasOxygenConcentrators = true,
                    HasHeating = true,
                    IsKosherFriendly = true,
                    ShabbatFriendly = true,
                    WalkingDistanceToChabadCusco = true,
                    RoomTypes = new() { "Classic Room", "Deluxe Oxygen-Enriched", "Colonial Suite", "Presidential Suite" },
                    Notes = "Premier 5-star hotel in historic 500-year-old estate. Mechanical key locks available for Shabbat observance on lower floors."
                },
                new()
                {
                    Name = "Monasterio, A Belmond Hotel",
                    HebrewName = "מונסטריו בלמונד - קוסקו",
                    Address = "Calle Palacio 136, Plazoleta Nazarenas, Cusco",
                    Destination = "Cusco",
                    Stars = 5,
                    Phone = "+51 84 604000",
                    WhatsApp = "+51 984 604000",
                    Email = "reservations.mon@belmond.com",
                    Website = "https://www.belmond.com/hotels/south-america/peru/cusco/belmond-hotel-monasterio",
                    HasOxygenEnrichedRooms = true,
                    HasOxygenConcentrators = true,
                    HasHeating = true,
                    IsKosherFriendly = true,
                    ShabbatFriendly = true,
                    WalkingDistanceToChabadCusco = true,
                    RoomTypes = new() { "Superior Oxygen Room", "Deluxe Room", "Junior Suite" },
                    Notes = "Former 1592 monastery with world-renowned Spanish colonial art collection and oxygen-enrichment systems."
                },
                new()
                {
                    Name = "Casa Andina Premium Cusco",
                    HebrewName = "קאסה אנדינה פרימיום קוסקו",
                    Address = "Plazoleta Limacpampa Chico 473, Cusco",
                    Destination = "Cusco",
                    Stars = 4,
                    Phone = "+51 84 232610",
                    WhatsApp = "+51 984 232610",
                    Email = "reservas@casa-andina.com",
                    Website = "https://www.casa-andina.com",
                    HasOxygenEnrichedRooms = false,
                    HasOxygenConcentrators = true,
                    HasHeating = true,
                    IsKosherFriendly = true,
                    ShabbatFriendly = true,
                    WalkingDistanceToChabadCusco = true,
                    RoomTypes = new() { "Standard Traditional", "Superior Room", "Suite" },
                    Notes = "Beautiful colonial courtyard property 7 minutes walk to Cusco Plaza de Armas and Chabad."
                },
                new()
                {
                    Name = "Tambo del Inka Resort & Spa",
                    HebrewName = "טמבו דל אינקה - העמק הקדוש",
                    Address = "Avenida Ferrocarril s/n, Urubamba, Sacred Valley",
                    Destination = "Sacred Valley",
                    Stars = 5,
                    Phone = "+51 84 581777",
                    WhatsApp = "+51 984 581777",
                    Email = "concierge@tambodelinka.com",
                    Website = "https://www.marriott.com/tambo-del-inka",
                    HasOxygenEnrichedRooms = false,
                    HasOxygenConcentrators = true,
                    HasHeating = true,
                    IsKosherFriendly = true,
                    ShabbatFriendly = false,
                    WalkingDistanceToChabadCusco = false,
                    RoomTypes = new() { "Deluxe Garden View", "Deluxe River View", "Junior Suite" },
                    Notes = "Luxury riverside resort at lower altitude (2,870m) ideal for gradual acclimatization. Features private railway station to Machu Picchu."
                }
            };

            await context.Hotels.AddRangeAsync(hotels);
            await context.SaveChangesAsync();
        }

        // 13. Seed Vehicles & Drivers (Section 16)
        if (!await context.Vehicles.AnyAsync())
        {
            var driver = await context.Drivers.FirstOrDefaultAsync();
            var vehicles = new List<Vehicle>
            {
                new()
                {
                    Model = "Mercedes-Benz Sprinter 516 CDI (2024)",
                    LicensePlate = "X4T-892",
                    PassengerCapacity = 19,
                    LuggageCapacity = 20,
                    Type = "Minibus",
                    Year = 2024,
                    HasAirConditioning = true,
                    AssignedDriverId = driver?.Id,
                    IsActive = true
                },
                new()
                {
                    Model = "Hyundai H1 Grand Starex (2023)",
                    LicensePlate = "B9Z-415",
                    PassengerCapacity = 8,
                    LuggageCapacity = 8,
                    Type = "Van",
                    Year = 2023,
                    HasAirConditioning = true,
                    AssignedDriverId = driver?.Id,
                    IsActive = true
                },
                new()
                {
                    Model = "Toyota Fortuner 4x4 (2024)",
                    LicensePlate = "V1C-730",
                    PassengerCapacity = 4,
                    LuggageCapacity = 4,
                    Type = "Private 4x4",
                    Year = 2024,
                    HasAirConditioning = true,
                    AssignedDriverId = driver?.Id,
                    IsActive = true
                }
            };

            await context.Vehicles.AddRangeAsync(vehicles);
            await context.SaveChangesAsync();
        }
    }
}

