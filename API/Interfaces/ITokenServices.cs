using API.Entities;

namespace API.Interfaces
{
    public interface ITokenServices
    {
        string GetJWTToken(AppUser user);
    }
}