namespace Project.Models;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; }
    public int Percentage { get; set; }  

    public int TeacherId { get; set; }  
    public Teacher Teacher { get; set; }  
}
