using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using NPOI.SS.Formula.Functions;

namespace MathTutorPro.Models.Entities
{
    public class Class
    {

        [Key]
        public int ClassId { get; set; }

        [Required]
        [StringLength(100)]
        public string ClassName { get; set; }  // 如 "微积分_21计科3班"

        [ForeignKey("Teacher")]
        public string TeacherId { get; set; }  // 关联到AspNetUsers表的Id

        // 导航属性
        public virtual User Teacher { get; set; }
        public virtual ICollection<ClassDetail> ClassDetail { get; set; }
    }
}
