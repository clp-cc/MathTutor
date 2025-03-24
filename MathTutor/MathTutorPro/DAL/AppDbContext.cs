using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MathTutorPro.Models.Entities;

namespace MathTutorPro.DAL
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Class> Class { get; set; }
        public DbSet<ClassDetail> ClassDetail { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 配置Class实体
            builder.Entity<Class>(entity =>
            {
                entity.HasIndex(c => c.ClassName).IsUnique();
                entity.HasOne(c => c.Teacher)
                    .WithMany()
                    .HasForeignKey(c => c.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置ClassDetail实体
            builder.Entity<ClassDetail>(entity =>
            {
                entity.HasIndex(cd => new { cd.ClassId, cd.UserId }).IsUnique();
            });
        }
    }


}
