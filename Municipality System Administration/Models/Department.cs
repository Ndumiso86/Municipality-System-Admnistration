using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Municipality_System_Administration.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [Display(Name = "Department")]
        [StringLength(100)]
        public string DepartmentName { get; set; }

        [Display(Name = "Description")]
        [StringLength(300)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<Staff> StaffMembers { get; set; }

        public Department()
        {
            StaffMembers = new HashSet<Staff>();
            IsActive = true;
        }
    }
}