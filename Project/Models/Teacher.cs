namespace Project.Models;

public class Teacher
{
    public int Id { get; set; }
    public string Image { get; set; }
    public string Name { get; set; }
    public string Profession { get; set; }
    public string Description { get; set; }
    public string Degree { get; set; }
    public string Experience { get; set; }
    public string Hobbies { get; set; }
    public string Faculty { get; set; }
    public string Mail { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<Skill> Skills { get; set; }
    public ICollection<SocialMedia> SocialMedias { get; set; }

}
