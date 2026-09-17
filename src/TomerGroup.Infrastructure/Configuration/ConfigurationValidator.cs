using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace TomerGroup.Infrastructure.Configuration;

public class ConfigurationValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public static class ConfigurationValidator
{
    public static ConfigurationValidationResult ValidateProductionSettings(IConfiguration configuration)
    {
        var result = new ConfigurationValidationResult();

        // 1. JWT Settings Validation
        var jwtSecret = configuration["JwtSettings:Secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret))
        {
            result.Errors.Add("JwtSettings:Secret is missing or empty.");
        }
        else if (jwtSecret.Length < 32)
        {
            result.Errors.Add($"JwtSettings:Secret is too short ({jwtSecret.Length} chars). Must be at least 32 characters for secure HMAC-SHA256.");
        }

        var jwtIssuer = configuration["JwtSettings:Issuer"];
        if (string.IsNullOrWhiteSpace(jwtIssuer))
        {
            result.Errors.Add("JwtSettings:Issuer is missing or empty.");
        }

        var jwtAudience = configuration["JwtSettings:Audience"];
        if (string.IsNullOrWhiteSpace(jwtAudience))
        {
            result.Errors.Add("JwtSettings:Audience is missing or empty.");
        }

        // 2. Database Connection String Validation
        var dbProvider = configuration["DatabaseProvider"] ?? "PostgreSQL";
        var defaultConn = configuration.GetConnectionString("DefaultConnection");
        if (dbProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(defaultConn))
            {
                result.Errors.Add("ConnectionStrings:DefaultConnection is required for PostgreSQL database provider.");
            }
            else if (!defaultConn.Contains("Host=", StringComparison.OrdinalIgnoreCase) && !defaultConn.Contains("Server=", StringComparison.OrdinalIgnoreCase))
            {
                result.Warnings.Add("ConnectionStrings:DefaultConnection does not contain standard Host/Server specification.");
            }
        }

        // 3. Brand Settings Validation
        var agencyName = configuration["BrandSettings:AgencyName"];
        if (string.IsNullOrWhiteSpace(agencyName))
        {
            result.Errors.Add("BrandSettings:AgencyName is missing.");
        }

        var primaryColor = configuration["BrandSettings:PrimaryColor"];
        if (!string.IsNullOrWhiteSpace(primaryColor) && !Regex.IsMatch(primaryColor, @"^#[0-9A-Fa-f]{6}$"))
        {
            result.Errors.Add($"BrandSettings:PrimaryColor '{primaryColor}' is not a valid 6-digit hex color format (e.g. #1B365D).");
        }

        return result;
    }
}

