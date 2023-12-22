using Microsoft.EntityFrameworkCore;

namespace Project.Controllers;

public class TeacherController : Controller
{
    private readonly AppDbContext _context;

    public TeacherController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        List<Teacher> teachers = await _context.Teachers.Where(c => !c.IsDeleted).Include(c => c.SocialMedias).ToListAsync();

        return View(teachers);
    }
    

    public async Task<IActionResult> Details(int id)
    {
        Teacher teacher  = await _context.Teachers.Include(c => c.SocialMedias).Include(c => c.Skills).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);


        TeachersSkillsAccounts  teachersSkillsAccounts = new()
        {
            Teacher = teacher,
            Skills = teacher.Skills.Where(c=>!c.IsDeleted).ToList(),
            SocialMedias = teacher.SocialMedias.Where(c=>!c.IsDeleted).ToList()
        };


        return View(teachersSkillsAccounts);
    }


}
