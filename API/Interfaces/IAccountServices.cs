using System.Threading.Tasks;
using API.Entities;

namespace API.Interfaces
{
    public interface IAccountServices
    {
        Task<bool> UserExists(string username);
        bool RegisterUser(AppUser user);
        Task<AppUser> GetUserData(string username);
        bool CheckUserPassword(byte[] computedHash, byte[] passwordHash);
        byte[] EncodePassword(string password);
        AppUser GenerateAppUserProject(string username, byte[] passwordHashData);
        Task<bool> SaveChangesToUser();
        byte[] GetComputedHash(string password, byte[] passwordSalt);
    }
}