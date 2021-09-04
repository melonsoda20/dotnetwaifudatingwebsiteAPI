using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly ITokenServices _tokenServices;
        private readonly IAccountServices _accountServices;
        private readonly IMapper _mapper;

        public AccountController(ITokenServices tokenServices, 
                                IAccountServices accountServices,
                                IMapper mapper)
        {
            _tokenServices = tokenServices;
            _accountServices = accountServices;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO){
            bool isUserAlreadyExists = await _accountServices.UserExists(registerDTO.Username);
            
            AppUser user = _mapper.Map<AppUser>(registerDTO);

            if(isUserAlreadyExists){
                return BadRequest("Username is taken");
            }

            byte[] encodedPassword = _accountServices.EncodePassword(registerDTO.Password);
            
            using HMACSHA512 hmac = new HMACSHA512();

            user.UserName = registerDTO.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
            user.PasswordSalt = hmac.Key;

            bool isRegisterSuccessful = _accountServices.RegisterUser(user);
            if(!isRegisterSuccessful){
                return BadRequest("Something went wrong");
            }
            
            bool isChangesSaved = await _accountServices.SaveChangesToUser();
            if(!isChangesSaved){
                return BadRequest("Something went wrong");
            }

            return new UserDTO{
                Username = user.UserName,
                Token = _tokenServices.GetJWTToken(user),
                KnownAs = user.KnownAs
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO){
            AppUser user = await _accountServices.GetUserData(loginDTO.Username);
            
            if(user == null){
                return Unauthorized("Invalid username");
            }

            byte[] computedHash = _accountServices.GetComputedHash(loginDTO.Password, user.PasswordSalt);
            bool isUserValid = _accountServices.CheckUserPassword(computedHash, user.PasswordHash);
            
            if(!isUserValid){
                return Unauthorized("Invalid password");
            }

            return new UserDTO{
                Username = user.UserName,
                Token = _tokenServices.GetJWTToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs
            };
        }
    }
}