using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MathTutorPro.Models.Entities
{
    public class ClassDetail
    {
        [Key]
        public int ClassDetailId { get; set; }

        [ForeignKey("Class")]
        public int ClassId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }  // 关联到AspNetUsers表的Id

        // 导航属性
        public virtual Class Class { get; set; }
        public virtual User User { get; set; }
    }
}
