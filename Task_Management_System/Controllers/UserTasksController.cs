using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;
using System.Security.Claims;
using System.Threading.Tasks;
using Task_Management_System.Data;
using Task_Management_System.Models;
using Task_Management_System.Models.DTOs;

namespace Task_Management_System.Controllers
{
    [Authorize(Roles = "Manager,User,Admin")]
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
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
            var sortColumnIndex = Convert.ToInt32(Request.Form["order[0][column]"]);
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][data]"];
            var sortDir = Request.Form["order[0][dir]"];
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var statusFilter = Request.Form["statusFilter"].FirstOrDefault();

            // Base query including related entities
            var dataQuery = _context.UserTasks
                .Include(ut => ut.User)
                .Include(ut => ut.Task)
                .Where(ut => ut.User.Status == "Active"); // Always active users

            // Role based filtering of tasks
            //if (User.IsInRole("Manager"))
            //{
            //    dataQuery = dataQuery.Where(ut => ut.Task.Status == "ToDo");
            //}
            //else
            //{
            //    dataQuery = dataQuery.Where(ut => ut.Task.Status == "Completed");
            //}

            // Apply status filter if specified and not "All"
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                dataQuery = dataQuery.Where(ut => ut.Task.Status == statusFilter);
            }

            // Project to anonymous for frontend
            var projectedQuery = dataQuery.Select(ut => new
            {
                ut.Id,
                UserName = ut.User.Name,
                TaskTitle = ut.Task.Title,
                TaskStatus = ut.Task.Status,
                AssignedDate = ut.User.JoinDate
            });

            // Search across multiple columns
            if (!string.IsNullOrEmpty(searchValue))
            {
                projectedQuery = projectedQuery.Where(ut =>
                    ut.UserName.Contains(searchValue) ||
                    ut.TaskTitle.Contains(searchValue) ||
                    ut.TaskStatus.Contains(searchValue) ||
                    ut.AssignedDate.ToString().Contains(searchValue));
            }

            var recordsTotal = projectedQuery.Count();

            // Sorting with error handling
            try
            {
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDir))
                    projectedQuery = projectedQuery.OrderBy($"{sortColumn} {sortDir}");
            }
            catch (ParseException)
            {
                projectedQuery = projectedQuery.OrderBy("UserName asc");
            }

            // Pagination
            var data = projectedQuery
                .Skip(start)
                .Take(length)
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
            LoadUsers();
            LoadTasksBasedOnRole();
            return PartialView("_UserTaskFormPartial", new CreateOrEditUserTaskDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrEditUserTaskDto dto)
        {
            if (ModelState.IsValid)
            {
                var isAssigned = await _context.UserTasks.AnyAsync(ut =>
                    ut.UserId == dto.UserId && ut.TaskId == dto.TaskId);

                if (isAssigned)
                {
                    return Json(new { success = false, message = "This task is already assigned to the selected user." });
                }

                _context.UserTasks.Add(new UserTask
                {
                    UserId = dto.UserId,
                    TaskId = dto.TaskId
                });

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Task assigned successfully." });
            }

            LoadUsers();
            LoadTasksBasedOnRole();
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

            LoadUsers();
            LoadTasksBasedOnRole();
            return PartialView("_UserTaskFormPartial", dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CreateOrEditUserTaskDto dto)
        {
            if (ModelState.IsValid)
            {
                var isAssigned = await _context.UserTasks.AnyAsync(ut =>
                    ut.UserId == dto.UserId && ut.TaskId == dto.TaskId && ut.Id != dto.Id);

                if (isAssigned)
                {
                    ModelState.AddModelError("", "This task is already assigned to the selected user.");
                }
                else
                {
                    var userTask = await _context.UserTasks.FindAsync(dto.Id);
                    if (userTask == null) return NotFound();

                    userTask.UserId = dto.UserId;
                    userTask.TaskId = dto.TaskId;

                    _context.UserTasks.Update(userTask);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Task assignment updated successfully." });
                }
            }

            LoadUsers();
            LoadTasksBasedOnRole();
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

        private void LoadUsers()
        {
            var userRole = _context.Roles.FirstOrDefault(r => r.Name == "User");
            if (userRole != null)
            {
                var userIds = _context.UserRoles
                    .Where(ur => ur.RoleId == userRole.Id)
                    .Select(ur => ur.UserId)
                    .ToList();

                ViewBag.Users = _context.Users
                    .Where(u => u.Status == "Active" && userIds.Contains(u.Id))
                    .ToList();
            }
            else
            {
                ViewBag.Users = _context.Users
                    .Where(u => u.Status == "Active")
                    .ToList();
            }
        }

        private void LoadTasksBasedOnRole()
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                ViewBag.Tasks = _context.Tasks
                    .Where(t => t.Status == "ToDo")
                    .ToList();
                ViewBag.TaskPlaceholder = "-- Select ToDo Task --";
            }
            else
            {
                ViewBag.Tasks = _context.Tasks
                    .Where(t => t.Status == "Completed")
                    .ToList();
                ViewBag.TaskPlaceholder = "-- Select Completed Task --";
            }
        }
        public IActionResult MyTasks()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetMyTasks(string status, DateTime? dueDateFrom, DateTime? dueDateTo, string sortColumn, string sortDir, int start, int length)
        {
            // Get current user id
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Get tasks assigned to this user
            var query = _context.UserTasks
                .Where(ut => ut.UserId == userId)
                .Select(ut => ut.Task)
                .AsQueryable();

            // Filter by Status
            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            // Filter by DueDate range
            if (dueDateFrom.HasValue)
                query = query.Where(t => t.DueDate.Date >= dueDateFrom.Value.Date);

            if (dueDateTo.HasValue)
                query = query.Where(t => t.DueDate.Date <= dueDateTo.Value.Date);

            int recordsTotal = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDir))
            {
                query = query.OrderBy($"{sortColumn} {sortDir}");
            }
            else
            {
                query = query.OrderBy(t => t.DueDate);
            }

            var data = await query.Skip(start).Take(length)
    .Select(t => new
    {
        t.Id,
        t.Title,
        DueDate = t.DueDate.ToString("yyyy-MM-dd"),
        t.Status,
        Priority = "Medium" // or any computed value or default
    }).ToListAsync();


            return Json(new
            {
                recordsTotal,
                recordsFiltered = recordsTotal,
                data
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Check if task assigned to this user
            var userTask = await _context.UserTasks
                .Include(ut => ut.Task)
                .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TaskId == id);

            if (userTask == null) return Forbid();

            // Update status with allowed flow only
            var allowedStatuses = new[] { "ToDo", "InProgress", "Completed" };
            if (!allowedStatuses.Contains(status))
                return BadRequest("Invalid status.");

            // Optional: Enforce status flow logic (e.g. no skipping)
            // For simplicity, just assign the status
            userTask.Task.Status = status;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

    }
}
