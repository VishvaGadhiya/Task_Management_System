using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Task_Management_System.Data;
using Task_Management_System.Models;
using Task_Management_System.Models.DTOs;

namespace Task_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public ManagerController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetAll()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            int start = int.Parse(Request.Form["start"]);
            int length = int.Parse(Request.Form["length"]);
            int sortColumnIndex = int.Parse(Request.Form["order[0][column]"]);
            string sortColumn = Request.Form[$"columns[{sortColumnIndex}][data]"];
            string sortDir = Request.Form["order[0][dir]"];
            string searchValue = Request.Form["search[value]"];

            var query = _userManager.GetUsersInRoleAsync("Manager").Result.AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(u =>
                    u.Name.Contains(searchValue) ||
                    u.Email.Contains(searchValue) ||
                    u.Gender.Contains(searchValue) ||
                    u.JoinDate.ToString().Contains(searchValue) ||
                    u.Status.Contains(searchValue)
                );
            }

            int recordsTotal = query.Count();

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDir))
            {
                query = query.OrderBy($"{sortColumn} {sortDir}");
            }

            var data = query.Skip(start).Take(length)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Gender,
                    JoinDate = u.JoinDate.ToString("yyyy-MM-dd"),
                    u.Status
                })
                .ToList();

            return Json(new
            {
                draw,
                recordsFiltered = recordsTotal,
                recordsTotal,
                data
            });
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_ManagerFormPartial", new ManagerViewModel
            {
                JoinDate = DateTime.Now,
                Status = "Active"
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(ManagerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = string.Join("; ", errors) });
            }

            try
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.Name,
                    Gender = model.Gender,
                    JoinDate = model.JoinDate,
                    Status = model.Status,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Manager");
                    return Json(new { success = true, message = "Manager added successfully." });
                }
                else
                {
                    var errorMessages = result.Errors.Select(e => e.Description).ToList();
                    return Json(new { success = false, message = string.Join("; ", errorMessages) });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred while creating the manager: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                TempData["Error"] = "Manager not found.";
                return RedirectToAction("Index"); // or wherever your main view is
            }

            var vm = new ManagerViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Gender = user.Gender,
                JoinDate = user.JoinDate,
                Status = user.Status
            };

            return PartialView("_ManagerFormPartial", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ManagerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = string.Join("; ", errors) });
            }

            try
            {
                var user = await _userManager.FindByIdAsync(model.Id.ToString());
                if (user == null)
                {
                    return Json(new { success = false, message = "Manager not found." });
                }

                user.Name = model.Name;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.Gender = model.Gender;
                user.JoinDate = model.JoinDate;
                user.Status = model.Status;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "Manager updated successfully." });
                }
                else
                {
                    var errorMessages = result.Errors.Select(e => e.Description).ToList();
                    return Json(new { success = false, message = string.Join("; ", errorMessages) });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred while updating the manager: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return Json(new { success = false, message = "Manager not found." });

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
                return Json(new { success = true, message = "Manager deleted successfully." });

            return Json(new { success = false, message = "Error deleting manager." });
        }
    }
}
