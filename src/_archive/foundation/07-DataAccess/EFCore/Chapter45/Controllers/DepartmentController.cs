using EFCoreDeleteRecords.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCoreDeleteRecords.Controllers
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
        public async Task<IActionResult> Delete(int id)
        {
            var emp = new Employee() { Id = id };
            context.Remove(emp);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
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
        public IActionResult Index()
        {
            return View(context.Department.AsNoTracking());
        }
    }
}
