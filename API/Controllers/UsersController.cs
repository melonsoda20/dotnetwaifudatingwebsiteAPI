using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper){
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers(){
            var users = await _userRepository.GetMembersAsync();
            return Ok(users);
        }
        
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDTO>> GetUser(string username){
            var user = await _userRepository.GetMemberAsync(username);

            // return _mapper.Map<MemberDTO>(user);
            return user;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            //get username from the token
            string username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            AppUser user = await _userRepository.GetUserByUsernameAsync(username);

            _mapper.Map(memberUpdateDTO, user);

            _userRepository.Update(user);

            if(await _userRepository.SaveAllAsync()){
                return NoContent();
            }
            else{
                return BadRequest("Failed to update user");
            }
        }
    }
}