using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Metheo.Tools
{
    public interface ITokenService
    {
        string GenerateToken(IEnumerable<Claim> claims);
    }

    public class TokenService : ITokenService
    {
        private readonly string _secretKey = "hm7T5BIVNhUiDbOlIPAX7RaSNJtcJ6uMm9a5OMtuVMM79";

        public string GenerateToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "metheodatalatitude.com",
                audience: "metheodatalatitude.com",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}