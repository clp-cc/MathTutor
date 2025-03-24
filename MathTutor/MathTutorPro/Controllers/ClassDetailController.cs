using MathTutorPro.DAL;
using MathTutorPro.Models.Entities;
using MathTutorPro.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace MathTutorPro.Controllers
{
    [ApiController]
    [Route("api/class/{classId}/[controller]")]
    //[Authorize(Roles = "Teacher")]
    public class ClassDetailController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ClassDetailController> _logger;

        public ClassDetailController(AppDbContext context, UserManager<User> userManager, ILogger<ClassDetailController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;

            _userManager.UserValidators.Clear();
        }


        // GET: api/class/{classId}/classdetail
        //获取指定班级的成员详细信息
        [HttpGet]
        public async Task<IActionResult> GetClassDetail(int classId, [FromQuery] string keyword = null)
        {
            var query = _context.ClassDetail
                        .Where(cd => cd.ClassId == classId)
                        .Include(cd => cd.User)
                        .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(cd =>
                    cd.User.StudentNo.Contains(keyword) ||
                    cd.User.UserName.Contains(keyword));
            }

            var details = await query
                            .Select(cd => new
                            {
                                cd.UserId,
                                cd.User.StudentNo,
                                cd.User.UserName,
                                cd.User.PhoneNumber,
                                cd.User.Gender,
                                cd.User.Status
                            })
                            .ToListAsync();


            return Ok(new { code = 20000, data = details });
        }

        // POST: api/classes/{classId}/students
        // 添加班级明细（成员）
        [HttpPost]
        public async Task<IActionResult> CreateStudent(
            int classId,
            [FromBody] StudentCreateDto dto)
        {
            //验证班级归属
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var targetClass = await _context.Class
                .FirstOrDefaultAsync(c => c.ClassId == classId && c.TeacherId == teacherId);

            if (targetClass == null)
                return NotFound(new { code = 40400, message = "班级不存在或无权操作" });

            // 验证学号唯一性
            if (await _context.Users.AnyAsync(u => u.StudentNo == dto.StudentNo))
                return BadRequest(new { code = 40001, message = "学号已存在" });

            // 创建用户
            var user = new User
            {
                UserName = dto.UserName,
                StudentNo = dto.StudentNo,
                PhoneNumber = dto.PhoneNumber,
                Gender = dto.Gender,
                Status = 0
            };

            var createResult = await _userManager.CreateAsync(user, "abc123456"); // 初始密码
            if (!createResult.Succeeded)
            {
                _logger.LogError("用户创建失败：{Errors}", createResult.Errors);
                return BadRequest(new { code = 40002, message = "用户创建失败", errors = createResult.Errors });
            }

            // 分配学生角色
            await _userManager.AddToRoleAsync(user, "Student");

            // 5. 添加班级关联
            _context.ClassDetail.Add(new ClassDetail
            {
                ClassId = classId,
                UserId = user.Id
            });

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetStudent),
                new { classId, studentId = user.Id },
                new { code = 20000, data = user.Id });
        }
        [HttpGet("{studentId}")]
        public async Task<IActionResult> GetStudent(int classId, string studentId)
        {
            var student = await _context.Users
                .Where(u => u.Id == studentId)
                .Select(u => new
                {
                    u.Id,
                    u.StudentNo,
                    u.UserName,
                    u.PhoneNumber,
                    u.Gender,
                    u.Status
                })
                .FirstOrDefaultAsync();

            return student == null
                ? NotFound()
                : Ok(new { code = 20000, data = student });
        }


        // PUT: api/class/{classId}/ClassDetail/{userId}
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateStudent(
            int classId,
            string userId,
            [FromBody] StudentUpdateDto dto)
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var classExists = await _context.Class
                .AnyAsync(c => c.ClassId == classId && c.TeacherId == teacherId);
            if (!classExists)
                return NotFound(new { code = 40400, message = "班级不存在或无权操作" });

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            var isInClass = await _context.ClassDetail
            .AnyAsync(cd => cd.ClassId == classId && cd.UserId == userId);
            if (user == null || !isInClass)
                return NotFound(new { code = 40401, message = "学生不存在或不在本班级" });

            // 更新
            user.PhoneNumber = dto.PhoneNumber;
            user.Gender = dto.Gender;
            user.UserName = dto.UserName;

            // 保存更改
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogError("用户更新失败：{Errors}", updateResult.Errors);
                return BadRequest(new { code = 40003, message = "用户更新失败", errors = updateResult.Errors });
            }

            return Ok(new { code = 20000, message = "更新成功" });
        }

        // POST: api/classes/{classId}/classdetail/import
        //通过Excel批量导入班级成员
        [HttpPost("import")]
        public async Task<IActionResult> ImportStudents(
            int classId,
            [FromForm] IFormFile file)
        {
            // 1. 验证班级存在性
            var targetClass = await _context.Class.FindAsync(classId);
            if (targetClass == null) return NotFound();

            // 2. 读取Excel文件
            using var stream = file.OpenReadStream();
            var students = ExcelHelper.ReadStudentList(stream); // 需实现Excel解析逻辑

            // 3. 批量创建用户
            var roleId = "686BA288-0C7E-41FA-A04F-86B5B97E283C"; // 学生角色ID
            foreach (var student in students)
            {
                var user = new User
                {
                    UserName = student.UserName,
                    StudentNo = student.StudentNo,
                    PhoneNumber = student.Phone,
                    Gender = student.Gender,
                    Status = 0 // 未激活状态
                };

                var result = await _userManager.CreateAsync(user, "abc123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Student");
                    _context.ClassDetail.Add(new ClassDetail
                    {
                        ClassId = classId,
                        UserId = user.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { code = 20000, message = "导入成功" });
        }



        // DTO 类
        public class StudentCreateDto
        {
            [Required]
            [StringLength(20)]
            public string StudentNo { get; set; }

            [Required]
            [StringLength(50)]
            public string UserName { get; set; }

            [Required]
            [Phone]
            public string PhoneNumber { get; set; }

            [Required]
            [Range(0, 1)]
            public int Gender { get; set; }
        }

        public class StudentUpdateDto
        {
            [StringLength(20)]
            public string UserName { get; set; }

            [Phone]
            public string PhoneNumber { get; set; }

            [Range(0, 1)]
            public int Gender { get; set; }
        }


    }
}
