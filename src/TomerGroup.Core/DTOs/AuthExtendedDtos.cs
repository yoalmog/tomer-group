namespace TomerGroup.Core.DTOs;

public class ForgotPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class SendPhoneCodeRequestDto
{
    public string PhoneNumber { get; set; } = string.Empty;
}

public class VerifyPhoneCodeRequestDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
}

public class SessionStatusDto
{
    public bool IsAuthenticated { get; set; }
    public Guid? UserId { get; set; }
    public string? Role { get; set; }
    public string? Email { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public TimeSpan? TimeRemaining { get; set; }
}

