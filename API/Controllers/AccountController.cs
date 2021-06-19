using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;

        public AccountController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AppUser>> Register(string username, string password){
            // Using ensures that class will be disposed after the operation is done
            using HMACSHA512 hmac = new HMACSHA512();
            byte[] encodedPassword = Encoding.UTF8.GetBytes(password);
            
            AppUser user  = new AppUser{
                UserName = username,
                PasswordHash = hmac.ComputeHash(encodedPassword),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }
    }
}