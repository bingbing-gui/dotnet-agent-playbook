using EFCoreReadRecords.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCoreReadRecords.Controllers
{
public class DepartmentController : Controller
{
    private CompanyContext context;
    public DepartmentController(CompanyContext cc)
    {
        context = cc;
    }
    public IActionResult Create()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Create(Department dept)
    {
        context.Add(dept);
        await context.SaveChangesAsync();
        return View();
    }
    public IActionResult Index()
    {
        return View(context.Department.AsNoTracking());
    }
}
}
