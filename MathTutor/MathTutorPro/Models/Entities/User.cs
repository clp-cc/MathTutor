using Microsoft.AspNetCore.Identity;

namespace MathTutorPro.Models.Entities
{
    public class User : IdentityUser
    {
        public string StudentNo { get; set; } // 学号
        public int Gender { get; set; }   // 性别
        public int Status { get; set; }      // 账号状态 (0未激活、1正常)
    }
}
