
using EFCoreReadRecords.Models;
using EFCoreUpdateRecords.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EFCoreReadRecords.Controllers
{
    public class EmployeeController : Controller
    {
        private CompanyContext context;
        public EmployeeController(CompanyContext cc)
        {
            context = cc;
        }
        public async Task<IActionResult> Index()
        {
            var employee = context.Employee.Include(s => s.Department);
            return View(employee);
        }

        public async Task<IActionResult> Update(int id)
        {
            var emp = await context.Employee.Where(e => e.Id == id).FirstOrDefaultAsync();
            List<SelectListItem> dept = new List<SelectListItem>();
            dept = context.Department.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
            ViewBag.Department = dept;
            return View(emp);
        }
        [HttpPost]
        public async Task<IActionResult> Update(Employee emp)
        {
            context.Update(emp);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public IActionResult Create()
        {
            List<SelectListItem> dept = new List<SelectListItem>();
            dept = context.Department.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
            ViewBag.Department = dept;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Employee emp)
        {
            context.Add(emp);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var emp = new Employee() { Id = id };
            context.Remove(emp);
            await context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
