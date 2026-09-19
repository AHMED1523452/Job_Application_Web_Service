using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Text;

namespace JobApplication.Domain.Entities
{
    public class Jobs : baseClass
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Title is required"),
                MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; }
        public string? Description { get; set; } 
        public ICollection<JobApplications> jobApplications { get; set; } = new List<JobApplications>();
    }
}
