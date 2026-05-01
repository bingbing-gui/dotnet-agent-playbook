using EFCoreInsertRecords.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCoreInsertRecords.Controllers
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
        //... action methods creating record
        //public async Task<IActionResult> Create()
        //{
        //    //var dept = new Department()
        //    //{
        //    //    Name = "Designing"
        //    //};
        //    //context.Entry(dept).State = EntityState.Added;
        //    //context.SaveChanges();
        //    //await context.SaveChangesAsync();

        //    //var dept1 = new Department() { Name = "Development" };
        //    //var dept2 = new Department() { Name = "HR" };
        //    //var dept3 = new Department() { Name = "Marketing" };
        //    //context.AddRange(dept1, dept2, dept3);
        //    //await context.SaveChangesAsync();

        //    //var dept1 = new Department() { Name = "Development" };
        //    //var dept2 = new Department() { Name = "HR" };
        //    //var dept3 = new Department() { Name = "Marketing" };
        //    //var deps = new List<Department>() { dept1, dept2, dept3 };
        //    //context.AddRange(deps);
        //    //await context.SaveChangesAsync();

        //    var dept = new Department()
        //    {
        //        Name = "Admin"
        //    };
        //    var emp = new Employee()
        //    {
        //        Name = "Matt",
        //        Designation = "Head",
        //        Department = dept
        //    };
        //    context.Add(emp);
        //    await context.SaveChangesAsync();
        //    return View();
        //}
    }
}
