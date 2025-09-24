﻿using System.ComponentModel.DataAnnotations;

namespace MultiVendorAPI.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public ICollection<User>? Users { get; set; }
    }
}
