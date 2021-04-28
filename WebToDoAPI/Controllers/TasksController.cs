using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebToDoAPI.Configuration;
using WebToDoAPI.Data;
using WebToDoAPI.Data.Entities;
using WebToDoAPI.Models;


namespace WebToDoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ToDoDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<TasksController> logger;

        public TasksController(ILogger<TasksController> logger
            , UserManager<ApplicationUser> userManager
            , ToDoDbContext dbContext)
        {
            this.logger = logger;
            this.userManager = userManager;
            this.dbContext = dbContext;
        }

        private string GetUid()
        {
            return User.FindFirstValue("Id");
        }

        // GET: /api/Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToDoTask>>> GetMyTasks()
        {
            return await dbContext.Tasks
                .Where(c => c.User.Id == GetUid())
                .Select(c => new ToDoTask(c)).ToListAsync();
        }

        // GET: api/Tasks/AllUsersTasks
        [HttpGet]
        [Route("AllUsersTasks")]
        [Authorize(Roles = AppUserRoles.Administrator)]
        public async Task<ActionResult<IEnumerable<ToDoTask>>> GetAllTasks()
        {
            return await dbContext.Tasks.Select(c => new ToDoTask(c)).ToListAsync();
        }

        // GET: api/Task/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ToDoTask>> GetMyTask(int id)
        {
            var toDoTask = await dbContext.Tasks.FirstOrDefaultAsync(c => c.Id == id && c.User.Id == GetUid());

            if (toDoTask == null)
            {
                return NotFound();
            }
            return new ToDoTask(toDoTask);
        }

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
                .FirstOrDefault(c => c.Id == id && c.User.Id == userManager.GetUserId(User));
            if (taskFromDb == null)
            {
                //current user is not owner of this task
                //for security reason send him a 404
                logger.LogWarning("User [{user}] try update not owned task {Task} ", task, userManager.GetUserName(User));
                return NotFound();
            }

            dbContext.Entry(task).State = EntityState.Modified;

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exception)
            {
                if (dbContext.Tasks.Any(c => c.Id == id && c.User.Id == userManager.GetUserId(User)))
                {
                    return NoContent();
                }

                logger.LogError(exception, "Exception during note saving");
                return Problem(statusCode: 500);
            }

            return NoContent();
        }

        // POST: api/Tasks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]

        public async Task<ActionResult<ToDoTask>> PostToDoTask(ToDoTask task)
        {
            var entityTask = new TaskEntity
            {
                Name = task.Name,
                Description = task.Description,
                Id = task.Id,
                IsCompleted = task.IsCompleted,
            };

            var savedTask = await dbContext.Tasks.AddAsync(entityTask);
            savedTask.Entity.User.Id = GetUid();
            //assign ownership
            await dbContext.SaveChangesAsync();

            return CreatedAtAction("GetMyTask", "Tasks", new { id = task.Id }, task);
        }

        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMyTask(int id)
        {
            var task = await dbContext.Tasks.FirstOrDefaultAsync(c => c.Id == id && c.User.Id == GetUid());
            if (task == null)
            {
                return NotFound();
            }

            dbContext.Tasks.Remove(task);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Tasks/UserTasks/uid
        [HttpDelete("UserTasks/{userid}")]
        [Authorize(Roles = AppUserRoles.Administrator)]
        public async Task<IActionResult> DeleteAllTaskByUserId(string userId)
        {
            var tasks = await dbContext.Tasks.Where(c => c.User.Id == userId)
                .Select(c => new TaskEntity { Id = c.Id }).ToListAsync();

            if (tasks == null || tasks.Count == 0)
            {
                //nothing to delete 
                return NoContent();
            }

            dbContext.Tasks.RemoveRange(tasks);

            await dbContext.SaveChangesAsync();

            return NoContent();
        }


        // DELETE: api/Tasks/UserTasks/uid/taskId
        [HttpDelete("UserTask/{userid}/{taskId}")]
        [Authorize(Roles = AppUserRoles.Administrator)]
        public async Task<IActionResult> DeleteUserTask(string userId, int taskId)
        {
            var task = await dbContext.Tasks.FirstOrDefaultAsync(c => c.Id == taskId && c.User.Id == userId);
            if (task == null)
            {
                //nothing to delete or task not found
                return NotFound();
            }

            dbContext.Tasks.Remove(task);

            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
