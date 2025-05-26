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
        public async Task<IActionResult> GetMyTasks(
    [FromForm] string status,
    [FromForm] string dueDateFrom,
    [FromForm] string dueDateTo,
    [FromForm] string sortColumn,
    [FromForm] string sortDir,
    [FromForm] int start = 0,
    [FromForm] int length = 10,
    [FromForm] string searchValue = null) 
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email) ??
                               User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userEmail))
                {
                    return Json(new
                    {
                        draw = Request.Form["draw"].FirstOrDefault(),
                        error = "User not authenticated"
                    });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null)
                {
                    return Json(new
                    {
                        draw = Request.Form["draw"].FirstOrDefault(),
                        error = "User not found"
                    });
                }

                var query = _context.UserTasks
                    .Where(ut => ut.UserId == user.Id)
                    .Select(ut => ut.Task);

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(t => t.Status == status);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    searchValue = searchValue.ToLower();
                    query = query.Where(t =>
                        t.Title.ToLower().Contains(searchValue) ||
                        t.Status.ToLower().Contains(searchValue) ||
                        t.DueDate.ToString().ToLower().Contains(searchValue));
                }

                int recordsTotal = await query.CountAsync();

                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDir))
                {
                    try
                    {
                        var sortProperty = sortColumn switch
                        {
                            "title" => "Title",
                            "dueDate" => "DueDate",
                            "status" => "Status",
                            "priority" => "Priority",
                            _ => "DueDate" 
                        };

                        query = query.OrderBy($"{sortProperty} {sortDir}");
                    }
                    catch
                    {
                        query = query.OrderBy(t => t.DueDate);
                    }
                }
                else
                {
                    query = query.OrderBy(t => t.DueDate);
                }

                int recordsFiltered = await query.CountAsync();

                var data = await query
                    .Skip(start)
                    .Take(length)
                    .Select(t => new
                    {
                        id = t.Id,
                        title = t.Title,
                        dueDate = t.DueDate.ToString("yyyy-MM-dd"),
                        status = t.Status,
                        priority =  "Medium" 
                    })
                    .ToListAsync();

                return Json(new
                {
                    draw = Request.Form["draw"].FirstOrDefault(),
                    recordsTotal,
                    recordsFiltered, 
                    data
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = Request.Form["draw"].FirstOrDefault(),
                    error = "An error occurred while processing your request",
                    details = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email) ??
                               User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userEmail))
                {
                    return Json(new { success = false, error = "User not authenticated" });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null)
                {
                    return Json(new { success = false, error = "User not found" });
                }

                var userTask = await _context.UserTasks
                    .Include(ut => ut.Task)
                    .FirstOrDefaultAsync(ut => ut.UserId == user.Id && ut.TaskId == id);

                if (userTask == null)
                {
                    return Json(new { success = false, error = "Task not assigned to user" });
                }

                userTask.Task.Status = status;
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
