using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService){
            _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers(){
            var users = await _userRepository.GetMembersAsync();
            return Ok(users);
        }
        
        [HttpGet("{username}", Name ="GetUser")]
        public async Task<ActionResult<MemberDTO>> GetUser(string username){
            var user = await _userRepository.GetMemberAsync(username);

            // return _mapper.Map<MemberDTO>(user);
            return user;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            string username = User.GetUsername();
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

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            AppUser user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
        
            var result = await _photoService.AddPhotoAsync(file);

            if(result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }

            Photo photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if(user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if(!await _userRepository.SaveAllAsync())
            {
                return BadRequest("Problem adding photo");
            }

            // return _mapper.Map<PhotoDTO>(photo);
            return CreatedAtRoute("GetUser", new {username = user.UserName} , _mapper.Map<PhotoDTO>(photo));
        }
    }
}