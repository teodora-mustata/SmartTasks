using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SmartTasksAPI.Contracts.Auth;
using SmartTasksAPI.Models;
using SmartTasksAPI.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartTasksAPI.Services
{
    public class AuthService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration) : IAuthService
    {
        public async Task<AuthResponse> RegisterAsync(string fullName, string email, string password)
        {
            var normalizedFullName = fullName.Trim();
            var normalizedEmail = email.Trim().ToLowerInvariant();

            var existingByEmail = await userRepository.GetByEmailAsync(normalizedEmail);
            if (existingByEmail is not null)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            var existingByFullName = await userRepository.GetByFullNameAsync(normalizedFullName);
            if (existingByFullName is not null)
            {
                throw new InvalidOperationException("A user with this username already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = normalizedFullName,
                Email = normalizedEmail
            };

            user.PasswordHash = passwordHasher.HashPassword(user, password);

            var createdUser = await userRepository.AddAsync(user);
            return BuildAuthResponse(createdUser);
        }

        public async Task<AuthResponse> LoginAsync(string identifier, string password)
        {
            var normalizedIdentifier = identifier.Trim();

            User? user;
            if (normalizedIdentifier.Contains('@'))
            {
                user = await userRepository.GetByEmailAsync(normalizedIdentifier.ToLowerInvariant());
            }
            else
            {
                user = await userRepository.GetByFullNameAsync(normalizedIdentifier);
            }

            if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var verifyResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            return BuildAuthResponse(user);
        }

        public async Task<AuthUserResponse?> GetMeAsync(Guid userId)
        {
            var user = await userRepository.GetByIdAsync(userId);
            return user is null ? null : MapUser(user);
        }

        private AuthResponse BuildAuthResponse(User user)
        {
            return new AuthResponse
            {
                Token = GenerateToken(user),
                User = MapUser(user)
            };
        }

        private AuthUserResponse MapUser(User user)
        {
            return new AuthUserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                CreatedAtUtc = user.CreatedAtUtc
            };
        }

        private string GenerateToken(User user)
        {
            var jwtKey = configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Missing JWT key configuration.");

            var issuer = configuration["Jwt:Issuer"] ?? "SmartTasksAPI";
            var audience = configuration["Jwt:Audience"] ?? "smarttasks-frontend";

            var expiresInMinutes = int.TryParse(configuration["Jwt:ExpiresInMinutes"], out var parsed)
                ? parsed
                : 60;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.UniqueName, user.FullName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
