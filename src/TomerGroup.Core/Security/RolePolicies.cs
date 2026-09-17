using TomerGroup.Core.Enums;

namespace TomerGroup.Core.Security;

public static class RolePolicies
{
    // Roles
    public const string Admin = nameof(UserRole.Admin);
    public const string Manager = nameof(UserRole.Manager);
    public const string Sales = nameof(UserRole.Sales);
    public const string Operations = nameof(UserRole.Operations);
    public const string Finance = nameof(UserRole.Finance);
    public const string Guide = nameof(UserRole.Guide);
    public const string Driver = nameof(UserRole.Driver);
    public const string Customer = nameof(UserRole.Customer);

    // Policy Names
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireManagerOrAdmin = "RequireManagerOrAdmin";
    public const string RequireSales = "RequireSales";
    public const string RequireOperations = "RequireOperations";
    public const string RequireFinance = "RequireFinance";
    public const string RequireGuide = "RequireGuide";
    public const string RequireDriver = "RequireDriver";
    public const string RequireCustomer = "RequireCustomer";
    public const string RequireStaff = "RequireStaff";
}

