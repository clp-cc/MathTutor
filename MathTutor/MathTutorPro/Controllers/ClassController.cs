using MathTutorPro.DAL;
using MathTutorPro.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace MathTutorPro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Teacher")]
    public class ClassController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public ClassController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/class/my-classes
        //获取当前登录教师创建的所有班级列表
        [HttpGet("my-classes")]
        public async Task<IActionResult> GetMyClasses()
        {
            //从 JWT Token 中解析出教师ID 
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var classes = await _context.Class
                .Where(c => c.TeacherId == teacherId)
                .Select(c => new
                {
                    c.ClassId,
                    c.ClassName,
                    StudentCount = c.ClassDetail.Count
                })
                .ToListAsync();
            return Ok(new { code = 20000, data = classes });
        }

        // POST: api/class
        //创建新班级
        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] ClassCreateRequest request)
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var newClass = new Class
            {
                ClassName = request.ClassName,
                TeacherId = teacherId
            };

            _context.Class.Add(newClass);
            await _context.SaveChangesAsync();

            return Ok(new { code = 20000, data = newClass.ClassId });
        }

        public class ClassCreateRequest
        {
            [Required]
            public string ClassName { get; set; }
        }
    }

}
