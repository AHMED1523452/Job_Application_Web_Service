using JobApplication.Application.Interfaces;
using JobApplication.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JobApplication.Infrastructure.Services
{
    public class GenerateJwtToken : IGenerateJwtToken
    {
        private readonly IOptions<JwtSettings> _jwtSettings;

        public GenerateJwtToken(IOptions<JwtSettings> jwtSettings)
        {
            this._jwtSettings = jwtSettings;
        }

        //. Sending the role as parameter will be for the Admin Registeration page not the candidate or normal user
        public async Task<string> GenerateAccessTokenAsync(string Id,
                                                           string Email,
                                                           string? UserName,
                                                           string FullName, 
                                                           string? role ,CancellationToken cancellationToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Value.SecretKEY);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Id),
            new(JwtRegisteredClaimNames.Email, Email),
            new(JwtRegisteredClaimNames.Email,UserName),
            new(JwtRegisteredClaimNames.Name, FullName),
            new(ClaimTypes.NameIdentifier, Id)
        };
            //. Validate for existing Role in the DB or not 
            if (role is not null)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            else
            {
                // Default role fallback
                claims.Add(new Claim(ClaimTypes.Role, "User"));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.Value.ExpiryMinutes),
                Issuer = _jwtSettings.Value.Issuer,
                Audience = _jwtSettings.Value.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync(string Id,
                                                           string Email,
                                                           string? UserName,
                                                           string FullName,
                                                           string? role, CancellationToken cancellationToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Value.SecretKEY);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Id),
            new(JwtRegisteredClaimNames.Email, Email),
            new(JwtRegisteredClaimNames.Email, UserName),
            new(JwtRegisteredClaimNames.Name, FullName),
            new(ClaimTypes.NameIdentifier, Id)
        };
            if (role is not null)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            else
            {
                // Default role fallback
                claims.Add(new Claim(ClaimTypes.Role, "User"));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _jwtSettings.Value.Issuer,
                Audience = _jwtSettings.Value.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
