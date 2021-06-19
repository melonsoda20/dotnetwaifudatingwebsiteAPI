using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenServices _tokenServices;
        AccountServices accountServices;

        public AccountController(DataContext context, ITokenServices tokenServices)
        {
            _context = context;
            _tokenServices = tokenServices;
            accountServices = new AccountServices(_context);
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO){
            bool isUserAlreadyExists = await accountServices.UserExists(registerDTO.Username);
            
            if(isUserAlreadyExists){
                return BadRequest("Username is taken");
            }

            byte[] encodedPassword = accountServices.EncodePassword(registerDTO.Password);
            
            AppUser user  = accountServices.GenerateAppUserProject(registerDTO.Username, encodedPassword);

            bool isRegisterSuccessful = accountServices.RegisterUser(user);
            if(!isRegisterSuccessful){
                return BadRequest("Something went wrong");
            }
            bool isChangesSaved = await accountServices.SaveChangesToUser();
            if(!isChangesSaved){
                return BadRequest("Something went wrong");
            }

            return new UserDTO{
                Username = user.UserName,
                Token = _tokenServices.GetJWTToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO){
            AppUser user = await accountServices.GetUserData(loginDTO.Username);
            
            if(user == null){
                return Unauthorized("Invalid username");
            }

            byte[] computedHash = accountServices.GetComputedHash(loginDTO.Password, user.PasswordSalt);
            bool isUserValid = accountServices.CheckUserPassword(computedHash, user.PasswordHash);
            
            if(!isUserValid){
                return Unauthorized("Invalid password");
            }

            return new UserDTO{
                Username = user.UserName,
                Token = _tokenServices.GetJWTToken(user)
            };
        }
    }
}