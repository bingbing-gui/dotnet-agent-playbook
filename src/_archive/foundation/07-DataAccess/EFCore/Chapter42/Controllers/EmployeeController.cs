using EFCoreInsertRecords.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EFCoreInsertRecords.Controllers
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
            //var employee = context.Employee.Where(emp => emp.Name == "Matt")
            //                    .Include(s => s.Department)
            //                    .FirstOrDefault();

            //var emp = await context.Employee.Where(e => e.Name == "Matt")
            //    .FirstOrDefaultAsync();
            //await context.Entry(emp).Reference(s => s.Department).LoadAsync();

            Employee emp = await context.Employee.Where(e => e.Name == "Matt")
                            .FirstOrDefaultAsync();
            string deptName = emp.Department.Name;
            return View();
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

        //[HttpPost]
        //[ActionName("Create")]
        //public async Task<IActionResult> Create_Post()
        //{
        //    var emptyEmployee = new Employee();
        //    if (await TryUpdateModelAsync<Employee>(emptyEmployee, "", s => s.Name, s => s.DepartmentId, s => s.Designation))
        //    {
        //        context.Employee.Add(emptyEmployee);
        //        await context.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    return View();
        //}
    }
}
