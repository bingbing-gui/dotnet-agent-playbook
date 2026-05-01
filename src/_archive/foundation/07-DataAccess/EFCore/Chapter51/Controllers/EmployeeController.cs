using EFCoreExecuteRawSql.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EFCoreExecuteRawSql.Controllers
{
    public class EmployeeController : Controller
    {
        public readonly EmployeeDbContext _employeeDbContext;
        public EmployeeController(EmployeeDbContext employeeDbContext)
        {
            _employeeDbContext = employeeDbContext;
        }
        public IActionResult Index()
        {
            var emp = _employeeDbContext
                .Employees
                .FromSqlRaw("Select * from Employees")
                .OrderBy(x => x.Name).ToList();

            var employees =_employeeDbContext
                .Employees
                .FromSqlRaw("Select * from Employees where Department = 'Admin'")
                .Include(e=>e.Project).ToList();
            return View();
        }
    }
}
