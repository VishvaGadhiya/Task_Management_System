using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Task_Management_System.Data;
using Task_Management_System.Models;
using Task_Management_System.Models.Dtos;

namespace Task_Management_System.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult GetAll()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            int start = int.Parse(Request.Form["start"]);
            int length = int.Parse(Request.Form["length"]);
            var sortColumnIndex = int.Parse(Request.Form["order[0][column]"]);
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][data]"];
            var sortDir = Request.Form["order[0][dir]"];
            var searchValue = Request.Form["search[value]"];

            var genderFilter = Request.Form["gender"].FirstOrDefault();
            var statusFilter = Request.Form["status"].FirstOrDefault();

            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(u =>
                    u.Name.Contains(searchValue) ||
                    u.Gender.Contains(searchValue) ||
                    u.JoinDate.ToString().Contains(searchValue) ||
                    u.Status.Contains(searchValue)
                );
            }

            if (!string.IsNullOrEmpty(genderFilter))
                query = query.Where(u => u.Gender == genderFilter);

            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(u => u.Status == statusFilter);

            int recordsTotal = query.Count();

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDir))
                query = query.OrderBy($"{sortColumn} {sortDir}");

            var data = query.Skip(start).Take(length)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
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
            var dto = new CreateOrEditUserDto
            {
                JoinDate = DateTime.Now,
                Status = "Active"
            };
            return PartialView("_UserFormPartial", dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrEditUserDto dto)
        {
            if (_context.Users.Any(u => u.Name == dto.Name && u.Gender == dto.Gender))
            {
                ModelState.AddModelError("Name", "A user with the same Name and Gender already exists.");
            }

            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Name = dto.Name,
                    Gender = dto.Gender,
                    JoinDate = dto.JoinDate,
                    Status = dto.Status
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return PartialView("_UserFormPartial", dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var dto = new CreateOrEditUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Gender = user.Gender,
                JoinDate = user.JoinDate,
                Status = user.Status
            };

            return PartialView("_UserFormPartial", dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CreateOrEditUserDto dto)
        {
            if (_context.Users.Any(u => u.Id != dto.Id && u.Name == dto.Name && u.Gender == dto.Gender))
            {
                ModelState.AddModelError("Name", "A user with the same Name and Gender already exists.");
            }

            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(dto.Id);
                if (user == null) return NotFound();

                user.Name = dto.Name;
                user.Gender = dto.Gender;
                user.JoinDate = dto.JoinDate;
                user.Status = dto.Status;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return PartialView("_UserFormPartial", dto);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return Json(new { success = false });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
