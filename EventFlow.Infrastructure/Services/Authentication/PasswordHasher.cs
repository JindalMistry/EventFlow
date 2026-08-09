using BCrypt.Net;
using EventFlow.Application.Interfaces;
using Org.BouncyCastle.Crypto.Generators;

namespace EventFlow.Infrastructure.Services.Authentication
{
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}