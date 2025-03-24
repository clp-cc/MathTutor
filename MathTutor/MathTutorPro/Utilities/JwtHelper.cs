using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MathTutorPro.Utilities
{
    public class JwtHelper
    {

        private readonly IConfiguration _config;
        // 通过构造函数注入 IConfiguration
        public JwtHelper(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateJwtToken(IdentityUser user)
        {
            // 创建一个对称安全密钥
            var securityKey = new SymmetricSecurityKey(
                // 将密钥字符串转换为字节数组
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );
            // 创建 JWT 签名凭据
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            // 创建一个新的 JWT 安全令牌
            var token = new JwtSecurityToken(
                // 设置令牌的发行人、受众
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                // 定义令牌中的声明（claims）
                claims: new[]
                {
                     // 用户id声明
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    // JWT ID 声明，用于标识 JWT
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, user.UserName)
                },
                // 设置令牌的过期时间
                expires: DateTime.Now.AddHours(3), // Token 有效期（示例：3小时）
                // 设置令牌的签名凭据
                signingCredentials: credentials
            );
            // 使用 JWT 安全令牌处理器将令牌序列化为字符串
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
