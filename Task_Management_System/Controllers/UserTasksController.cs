using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;
using System.Threading.Tasks;
using Task_Management_System.Data;
using Task_Management_System.Models;
using Task_Management_System.Models.DTOs;

namespace Task_Management_System.Controllers
{
    public class UserTasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult GetAll()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumnIndex = Convert.ToInt32(Request.Form["order[0][column]"]);
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][data]"];
            var sortDir = Request.Form["order[0][dir]"];
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var dataQuery = _context.UserTasks
                .Include(ut => ut.User)
                .Include(ut => ut.Task)
                .Where(ut => ut.User.Status == "Active" && ut.Task.Status == "Completed")
                .Select(ut => new
                {
                    ut.Id,
                    UserName = ut.User.Name,
                    TaskTitle = ut.Task.Title,
                    TaskStatus = ut.Task.Status,
                    AssignedDate = ut.User.JoinDate
                });

            if (!string.IsNullOrEmpty(searchValue))
            {
                dataQuery = dataQuery.Where(ut =>
                    ut.UserName.Contains(searchValue) ||
                    ut.TaskTitle.Contains(searchValue) ||
                    ut.TaskStatus.Contains(searchValue) ||
                    ut.AssignedDate.ToString().Contains(searchValue)
                );
            }

            int recordsTotal = dataQuery.Count();

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDir))
            {
                try
                {
                    dataQuery = dataQuery.OrderBy($"{sortColumn} {sortDir}");
                }
                catch (ParseException)
                {
                    dataQuery = dataQuery.OrderBy("UserName asc");
                }
            }

            var data = dataQuery
                .Skip(skip)
                .Take(pageSize)
                .Select(ut => new
                {
                    ut.Id,
                    ut.UserName,
                    ut.TaskTitle,
                    ut.TaskStatus,
                    AssignedDate = ut.AssignedDate.ToString("yyyy-MM-dd")
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
            ViewBag.Users = _context.Users.Where(u => u.Status == "Active").ToList();
            ViewBag.Tasks = _context.Tasks.Where(t => t.Status == "Completed").ToList();
            return PartialView("_UserTaskFormPartial", new CreateOrEditUserTaskDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrEditUserTaskDto dto)
        {
            if (ModelState.IsValid)
            {
                bool exists = _context.UserTasks.Any(ut => ut.UserId == dto.UserId && ut.TaskId == dto.TaskId);
                if (exists)
                {
                    ModelState.AddModelError("UserId", "This task is already assigned to the selected user.");
                    ViewBag.Users = _context.Users.Where(u => u.Status == "Active").ToList();
                    ViewBag.Tasks = _context.Tasks.Where(t => t.Status == "Completed").ToList();
                    return PartialView("_UserTaskFormPartial", dto);
                }

                var userTask = new UserTask
                {
                    UserId = dto.UserId,
                    TaskId = dto.TaskId
                };

                _context.UserTasks.Add(userTask);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            ViewBag.Users = _context.Users.Where(u => u.Status == "Active").ToList();
            ViewBag.Tasks = _context.Tasks.Where(t => t.Status == "Completed").ToList();
            return PartialView("_UserTaskFormPartial", dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userTask = await _context.UserTasks.FindAsync(id);
            if (userTask == null) return NotFound();

            var dto = new CreateOrEditUserTaskDto
            {
                Id = userTask.Id,
                UserId = userTask.UserId,
                TaskId = userTask.TaskId
            };

            ViewBag.Users = _context.Users.Where(u => u.Status == "Active").ToList();
            ViewBag.Tasks = _context.Tasks.Where(t => t.Status == "Completed").ToList();
            return PartialView("_UserTaskFormPartial", dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CreateOrEditUserTaskDto dto)
        {
            if (ModelState.IsValid)
            {
                bool exists = _context.UserTasks.Any(ut => ut.UserId == dto.UserId
                                                           && ut.TaskId == dto.TaskId
                                                           && ut.Id != dto.Id);
                if (exists)
                {
                    ModelState.AddModelError("UserId", "This task is already assigned to the selected user.");
                    ViewBag.Users = _context.Users.Where(u => u.Status == "Active").ToList();
                    ViewBag.Tasks = _context.Tasks.Where(t => t.Status == "Completed").ToList();
                    return PartialView("_UserTaskFormPartial", dto);
                }

                var userTask = await _context.UserTasks.FindAsync(dto.Id);
                if (userTask == null) return NotFound();

                userTask.UserId = dto.UserId;
                userTask.TaskId = dto.TaskId;

                _context.UserTasks.Update(userTask);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            ViewBag.Users = _context.Users.Where(u => u.Status == "Active").ToList();
            ViewBag.Tasks = _context.Tasks.Where(t => t.Status == "Completed").ToList();
            return PartialView("_UserTaskFormPartial", dto);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userTask = await _context.UserTasks.FindAsync(id);
            if (userTask == null) return Json(new { success = false });

            _context.UserTasks.Remove(userTask);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
