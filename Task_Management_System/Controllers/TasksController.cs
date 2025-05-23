﻿using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Admin")]

    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
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
            var searchValue = Request.Form["search[value]"].ToString();
            var statusFilter = Request.Form["status"].FirstOrDefault();

            var query = _context.Tasks.AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isDateSearch = DateTime.TryParse(searchValue, out var searchDate);
                query = query.Where(t =>
                    t.Title.Contains(searchValue) ||
                    t.Description.Contains(searchValue) ||
                    t.Status.Contains(searchValue) ||
                    (isDateSearch && t.DueDate.Date == searchDate.Date)
                );
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(t => t.Status == statusFilter);
            }

            int recordsTotal = query.Count();

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDir))
                query = query.OrderBy($"{sortColumn} {sortDir}");

            var data = query.Skip(start).Take(length)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    DueDate = t.DueDate.ToString("yyyy-MM-dd"),
                    t.Status
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
            var dto = new CreateOrEditTaskDto
            {
                DueDate = DateTime.Now.AddDays(7),
                Status = "ToDo"    // <-- default status when creating new task
            };
            return PartialView("_TaskFormPartial", dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrEditTaskDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var task = new Tasks
                    {
                        Title = dto.Title,
                        Description = dto.Description,
                        DueDate = dto.DueDate,
                        Status = string.IsNullOrEmpty(dto.Status) ? "ToDo" : dto.Status
                    };
                    _context.Tasks.Add(task);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    // Save the exception message to TempData
                    TempData["ErrorMessage"] = $"An error occurred while saving the task: {ex.Message}";
                }
            }
            else
            {
                // Extract model validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ErrorMessage"] = string.Join(" | ", errors);
            }

            // If we reach here, either validation failed or an exception occurred
            return PartialView("_TaskFormPartial", dto);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            var dto = new CreateOrEditTaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Status = task.Status
            };

            return PartialView("_TaskFormPartial", dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CreateOrEditTaskDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var task = await _context.Tasks.FindAsync(dto.Id);
                    if (task == null)
                    {
                        TempData["ErrorMessage"] = "Task not found.";
                        return PartialView("_TaskFormPartial", dto);
                    }

                    task.Title = dto.Title;
                    task.Description = dto.Description;
                    task.DueDate = dto.DueDate;
                    task.Status = string.IsNullOrEmpty(dto.Status) ? "ToDo" : dto.Status;

                    _context.Tasks.Update(task);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    string GetFullExceptionMessage(Exception exception)
                    {
                        if (exception == null) return string.Empty;
                        return exception.Message + (exception.InnerException != null ? " --> " + GetFullExceptionMessage(exception.InnerException) : "");
                    }

                    TempData["ErrorMessage"] = "An error occurred while updating the task: " + GetFullExceptionMessage(ex);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ErrorMessage"] = string.Join(" | ", errors);
            }

            return PartialView("_TaskFormPartial", dto);
        }



        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return Json(new { success = false });

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
