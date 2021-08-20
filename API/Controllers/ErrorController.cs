using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ErrorController : BaseApiController
    {
        private readonly DataContext _context;

        public ErrorController(DataContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret(){
            return "Secret Text!";
        }

        [Authorize]
        [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound(){
            AppUser user = _context.Users.Find(-1);

            if(user == null){
                return NotFound();
            }

            return Ok(user);
        }

        [Authorize]
        [HttpGet("server-error")]
        public ActionResult<string> GetServerError(){
            AppUser user = _context.Users.Find(-1);

            string userToReturn = user.ToString();

            return userToReturn;
        }

        [Authorize]
        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest(){
            return BadRequest("This was not a good request");
        }
    }
}