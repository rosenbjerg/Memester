using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Memester.Core
{
    public static class TokenGenerator
    {
        public static string Generate(int length = 18)
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);

            return Base64UrlEncoder.Encode(bytes);
        }
    }
}