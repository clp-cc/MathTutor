using MathTutorPro.Models.Entities;
using MathTutorPro.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MathTutorPro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtHelper _jwtHelper;
        private readonly IConfiguration _config;
        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration config, JwtHelper jwtHelper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _jwtHelper = jwtHelper;
        }

        // 注册
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = new User
            {
                UserName = request.UserName
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                return Ok(new { Message = "注册成功！" });
            }

            return BadRequest(result.Errors);
        }


        // 登录
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _signInManager.PasswordSignInAsync(
                request.UserName,
                request.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(request.UserName);
                var token = _jwtHelper.GenerateJwtToken(user);
                return Ok(new { code = 20000, data = new { token } });
            }

            return Unauthorized("用户名或密码错误");
        }

        [HttpGet("userinfo")]
        [Authorize] // 需要认证
        public async Task<IActionResult> GetUserInfo()
        {
            // 获取用户 ID 声明
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { code = 401, message = "Token 中缺少用户 ID" });
            }

            // 根据 ID 查找用户
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized(new { code = 401, message = "用户不存在" });
            }
            return Ok(new
            {
                code = 20000,
                data = new
                {
                    Token = "",
                    Introduction = "I am a super administrator",
                    Name = user.UserName,
                    Roles = new[] { "admin" }, // 根据实际角色逻辑修改
                    Avatar = "https://wpimg.wallstcn.com/f778738c-e4f8-4870-b634-56703b4acafe.gif"
                }
            });
        }

        public class RegisterRequest
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        public class LoginRequest
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }
}
