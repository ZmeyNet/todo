﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using WebToDoAPI.Configuration;
using WebToDoAPI.Data;
using WebToDoAPI.Data.Entities;
using WebToDoAPI.Models;
using WebToDoAPI.Utils;


namespace WebToDoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = AppUserRoles.User)]
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class TasksController : ControllerBase
    {
        private readonly ToDoDbContext dbContext;
        private readonly ILogger<TasksController> logger;
        private readonly UserManager<ApplicationUser> userManager;

        public TasksController(ILogger<TasksController> logger
            , ToDoDbContext dbContext
            , UserManager<ApplicationUser> userManager)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        /// <summary>
        /// Get all user task
        /// </summary>
        /// <returns>List of current user tasks</returns>
        /// <response code="200">Return requested items</response>
        // GET: /api/Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToDoTask>>> GetMyTasks()
        {
            return Ok(await dbContext.Tasks
                .Where(c => c.User.Id == User.Uid())
                .Select(c => new ToDoTask(c)).ToListAsync());
        }

        /// <summary>
        /// Get task by id
        /// </summary>
        /// <param name="id">task id</param>
        /// <returns>To Do task list</returns>
        /// <response code="200">Return requested item</response>
        /// <response code="404">If the item is null</response>  
        // GET: api/Task/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ToDoTask>> GetMyTask(int id)
        {
            var toDoTask = await dbContext.Tasks.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.User.Id == User.Uid());

            if (toDoTask == null)
            {
                return NotFound();
            }
            return Ok(new ToDoTask(toDoTask));
        }


        /// <summary>
        /// Update to do task
        /// </summary>
        /// <param name="id">id of the task</param>
        /// <param name="task">task object itself with updated fields</param>
        /// <returns></returns>
        /// <response code="400">If id is wrong</response>
        /// <response code="200">Return 200 if update succeed</response>
        /// <response code="404">If updated item does not exit or belong to other user</response> 
        /// <response code="500">If updated failed due to db error</response> 
        // PUT: api/MyTasks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMyTask(int id, ToDoTask task)
        {
            if (id != task.Id)
            {
                return BadRequest();
            }

            //verify task ownership 
            var taskFromDb = dbContext.Tasks
                .FirstOrDefault(c => c.Id == id && c.User.Id == User.Uid());
            if (taskFromDb == null)
            {
                //current user is not owner of this task
                //for security reason send him a 404
                logger.LogWarning("User [{user}] try update not owned task {Task} ", task, User.Uid());
                return NotFound();
            }

            taskFromDb.Description = task.Description;
            taskFromDb.IsCompleted = task.IsCompleted;
            taskFromDb.Name = task.Name;

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exception)
            {
                if (dbContext.Tasks.Any(c => c.Id == id && c.User.Id == User.Uid()))
                {
                    return NoContent();
                }

                logger.LogError(exception, "Exception during note saving");
                return Problem(statusCode: (int)HttpStatusCode.InternalServerError);
            }

            return Ok();
        }


        /// <summary>
        /// Add new To Do task
        /// </summary>
        /// <param name="task">to do task</param>
        /// <returns>created object</returns>
        /// <response code="201">result with created item</response> 
        // POST: api/Tasks
        [HttpPost]
        public async Task<ActionResult<ToDoTask>> PostToDoTask(ToDoTask task)
        {
            var entityTask = new TaskEntity
            {
                Name = task.Name,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                User = await userManager.FindByIdAsync(User.Uid())
            };

            var savedTask = await dbContext.Tasks.AddAsync(entityTask);

            await dbContext.SaveChangesAsync();

            var result = new ToDoTask(savedTask.Entity);

            return CreatedAtAction("GetMyTask", "Tasks", new { id = result.Id }, result);
        }
        /// <summary>
        /// Delete task from system
        /// </summary>
        /// <param name="id"></param>
        /// <returns>204 if task deleted successfully</returns>
        /// <response code="204">200 if task deleted successfully</response> 
        /// <response code="404">if task not found</response> 
        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMyTask(int id)
        {
            var task = await dbContext.Tasks.FirstOrDefaultAsync(c => c.Id == id && c.User.Id == User.Uid());
            if (task == null)
            {
                return NotFound();
            }

            dbContext.Tasks.Remove(task);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }

    }
}
