using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using TestCore9.Api.Interface;
namespace TestCore9.Api;
public class ClimsApi(string privateKeyPem) : IClimsApi
{
    public List<Claim> GetClims(Dictionary<string, string> attr, string userid, string role) =>
      [.. attr.Select(a => new Claim(a.Key, a.Value))
       .ToList()
       .Union([
           new Claim(ClaimTypes.Name,userid),
            new Claim(ClaimTypes.Role,role)
       ])];


    public string GetToken(List<Claim> claims, DateTime exp)
    {
        var key = RSA.Create();        
        key.ImportFromPem(privateKeyPem);
        var rkey = new RsaSecurityKey(key);
        var token = new JwtSecurityToken(
            issuer: "STUST",
            audience: "STUST",
            claims: claims,
            expires: exp,
            signingCredentials: new SigningCredentials(rkey, SecurityAlgorithms.RsaSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
