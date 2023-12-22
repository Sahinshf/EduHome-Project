using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Project.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.Security.Policy;

namespace Project.Models
{
    public class Course : BaseEntity
    {
        public int Id { get; set; }
        [Required , MaxLength(256)]
        public string? Name { get; set; }
        [Required, MaxLength(2000)]
        public string? Description { get; set; }
        public string? Image { get; set; }
        public DateTime Start { get; set; }
        public string? Duration { get; set; }
        public string? ClassDuration { get; set; }
        public string? SkillLevel { get; set; }
        public string? Language { get; set; }
        public string? StudentCount { get; set; }
        public decimal Price { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<CourseCategory>? CourseCategories { get; set; }
    }
}
