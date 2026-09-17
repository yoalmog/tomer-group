namespace TomerGroup.Core.Models;

public class PasswordResetToken : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
}

public class PhoneVerificationCode : BaseEntity
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int AttemptsCount { get; set; } = 0;
    public bool IsVerified { get; set; } = false;
}

