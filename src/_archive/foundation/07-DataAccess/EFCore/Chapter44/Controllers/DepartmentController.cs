using EFCoreReadRecords.Models;
using EFCoreUpdateRecords.Models;
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
        public async Task<IActionResult> Update(int id)
        {
            var dept = await context.Department.Where(e => e.Id == id).FirstOrDefaultAsync();
            return View(dept);
        }
        [HttpPost]
        public async Task<IActionResult> Update(Department dept)
        {
            context.Update(dept);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Create(Department dept)
        {
            context.Add(dept);
            await context.SaveChangesAsync();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var emp = new Department() { Id = id };
            context.Remove(emp);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public IActionResult Index()
        {
            return View(context.Department.AsNoTracking());
        }
    }
}
