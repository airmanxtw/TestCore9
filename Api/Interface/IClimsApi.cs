using System.Security.Claims;

namespace TestCore9.Api.Interface;
public interface IClimsApi
{
    List<Claim> GetClims(Dictionary<string, string> attr, string userid, string role);
    string GetToken(List<Claim> claims, DateTime exp);
}