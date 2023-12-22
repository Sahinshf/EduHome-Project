namespace Project.Models;

public class SocialMedia
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; }

    public string Name { get; set; }    
    public string Account { get; set; }

    public int TeacherId{ get; set; }
    public Teacher Teacher { get; set; }
}
