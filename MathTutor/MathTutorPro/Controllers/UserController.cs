using MathTutorPro.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace MathTutorPro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private static readonly Dictionary<string, string> Tokens = new Dictionary<string, string>
        {
            { "admin", "admin-token" },
            { "editor", "editor-token" }
        };
        private static readonly Dictionary<string, User2> Users = new Dictionary<string, User2>
        {
            { "admin-token", new User2
                {
                    Token = "admin-token",
                    Roles = new[] { "admin" },
                    Introduction = "I am a super administrator",
                    Avatar = "https://wpimg.wallstcn.com/f778738c-e4f8-4870-b634-56703b4acafe.gif",
                    Name = "Super Admin"
                }
            },
            { "editor-token", new User2
                {
                    Token = "editor-token",
                    Roles = new[] { "editor" },
                    Introduction = "I am an editor",
                    Avatar = "https://wpimg.wallstcn.com/f778738c-e4f8-4870-b634-56703b4acafe.gif",
                    Name = "Normal Editor"
                }
            }
        };

        // POST: api/user/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest2 request)
        {
            if (!Tokens.TryGetValue(request.Username, out var token))
            {
                return BadRequest(new { code = 60204, message = "Account and password are incorrect." });
            }

            return Ok(new { code = 20000, data = new { token } });
        }

        // GET: api/user/info
        [HttpGet("info")]
        public IActionResult GetUserInfo([FromQuery] string token)
        {
            if (!Users.TryGetValue(token, out var user))
            {
                return BadRequest(new { code = 50008, message = "Login failed, unable to get user details." });
            }

            return Ok(new { code = 20000, data = user });
        }

        // POST: api/user/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { code = 20000, data = "success" });
        }

        public class LoginRequest2
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }


    }
}
