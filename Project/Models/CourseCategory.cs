using System.Security.Policy;
namespace Project.Models;

public class CourseCategory
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }

}
