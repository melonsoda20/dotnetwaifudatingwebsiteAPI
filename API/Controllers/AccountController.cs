using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        // private readonly AccountServices _accountServices;

        public AccountController(DataContext context)
        {
            _context = context;
            // _accountServices = accountServices;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AppUser>> Register(RegisterDTO registerDTO){
            if(await UserExists(registerDTO.Username)){
                return BadRequest("Username is taken");
            }
            
            // Using ensures that class will be disposed after the operation is done
            using HMACSHA512 hmac = new HMACSHA512();
            byte[] encodedPassword = Encoding.UTF8.GetBytes(registerDTO.Password);
            
            AppUser user  = new AppUser{
                UserName = registerDTO.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(encodedPassword),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AppUser>> Login(LoginDTO loginDTO){
            AppUser user = await _context.Users
                .SingleOrDefaultAsync(user => user.UserName == loginDTO.Username);

            if(user == null){
                return Unauthorized("Invalid username");
            }

            using HMACSHA512 hmac = new HMACSHA512(user.PasswordSalt);

            byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

            for(int i = 0; i < computedHash.Length; i++){
                if(computedHash[i] != user.PasswordHash[i]){
                    return Unauthorized("Invalid password");
                }
            }

            return user;
        }

        private async Task<bool> UserExists(string username){
            return await _context.Users.AnyAsync(user => user.UserName == username.ToLower());
        }
    }
}