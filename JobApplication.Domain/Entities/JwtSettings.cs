using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKEY { get; init; } = null!;
    public int ExpiryMinutes { get; init; }
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
}
