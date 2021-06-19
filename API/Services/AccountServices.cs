using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;

namespace API.Services
{
    public class AccountServices
    {
        private readonly DataContext _context;

        public AccountServices(DataContext context)
        {
            _context = context;
        }
        public async Task<bool> UserExists(string username){
            return await _context.Users.AnyAsync(user => user.UserName == username.ToLower());
        }

        public bool RegisterUser(AppUser user){
            try{
                _context.Users.Add(user);
                return true;
            }
            catch{
                return false;
            }
        }

        public async Task<AppUser> GetUserData(string username){
            AppUser user = await _context.Users
                .SingleOrDefaultAsync(user => user.UserName == username);

            return user;
        }

        public bool CheckUserPassword(byte[] computedHash, byte[] passwordHash){
            for(int i = 0; i < computedHash.Length; i++){
                if(computedHash[i] != passwordHash[i]){
                    return false;
                }
            }
            return true;
        }

        public byte[] EncodePassword(string password){
            byte[] encodedPassword = Encoding.UTF8.GetBytes(password);
            return encodedPassword;
        }

        public byte[] GetComputedHash(string password, byte[] passwordSalt){
            using HMACSHA512 hmac = new HMACSHA512(passwordSalt);

            byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            return computedHash;
        }

        public AppUser GenerateAppUserProject(string username, byte[] passwordHashData){           
            // Using ensures that class will be disposed after the operation is done
            using HMACSHA512 hmac = new HMACSHA512();

            AppUser user  = new AppUser{
                UserName = username,
                PasswordHash = hmac.ComputeHash(passwordHashData),
                PasswordSalt = hmac.Key
            };

            return user;
        }

        public async Task<bool> SaveChangesToUser()
        {
            try{
                await _context.SaveChangesAsync();
                return true;
            }
            catch{
                return false;
            }
        }
    }
}