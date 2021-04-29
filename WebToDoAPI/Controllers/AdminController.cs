using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebToDoAPI.Configuration;
using WebToDoAPI.Data;
using WebToDoAPI.Data.Entities;
using WebToDoAPI.Models.Admin;
using WebToDoAPI.Utils;

namespace WebToDoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = AppUserRoles.Administrator)]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ToDoDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AdminController> logger;

        public AdminController(ILogger<AdminController> logger
            , UserManager<ApplicationUser> userManager
            , ToDoDbContext dbContext)
        {
            this.logger = logger;
            this.userManager = userManager;
            this.dbContext = dbContext;
        }
        /// <summary>
        /// Get all tasks in a system
        /// </summary>
        /// <returns>list of ToDoTaskManaged</returns>
        /// <response code="200">Return requested items</response>
        // GET: api/Admin/GetAllTasks
        [HttpGet]
        [Route("GetAllTasks")]
        public async Task<ActionResult<IEnumerable<ToDoTaskManaged>>> GetAllTasks()
        {
            //this one can be huge  
            return Ok(await dbContext.Tasks.Select(
                c => new ToDoTaskManaged()
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsCompleted = c.IsCompleted,
                    UserId = c.User.Id
                }

            ).AsNoTracking().ToListAsync());
        }

        /// <summary>
        /// Delete all task for exact user
        /// </summary>
        /// <returns>No content</returns>
        /// <response code="204">If item operation done</response>
        // DELETE: api/Admin/UserTasks/uid
        [HttpDelete]
        [Route("DeleteUserTasks")]
        public async Task<IActionResult> DeleteAllTaskByUserId(DeleteUserTasksRequest request)
        {
            var tasks = await dbContext.Tasks
                .Where(c => c.User.Id == request.UserId)
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



        /// <summary>
        /// Delete exact task for exact user
        /// </summary>
        /// <returns>No content</returns>
        /// <response code="404">If task not found</response>
        /// <response code="204">If item operation done</response>
        // DELETE: api/Admin/UserTasks/uid/taskId
        [HttpDelete]
        [Route("DeleteUserTask")]
        public async Task<IActionResult> DeleteUserTask(DeleteUserTaskRequest request)
        {
            var task = await dbContext.
                Tasks
                .FirstOrDefaultAsync(c => c.Id == request.TaskId && c.User.Id == request.UserId);
            if (task == null)
            {
                //nothing to delete or task not found
                return NotFound();
            }

            dbContext.Tasks.Remove(task);

            await dbContext.SaveChangesAsync();

            return NoContent();
        }


        /// <summary>
        /// List of user with Emails
        /// </summary>
        /// <returns>List of user data(PII name\email)</returns>
        /// <response code="200">with items</response>
        // GET: api/Admin/GetAllUsers
        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<ActionResult<IEnumerable<UserRecord>>> GetAllUsers()
        {
            return Ok(await dbContext.Users.Select(c =>
                    new UserRecord { Id = c.Id, Name = c.UserName, Email = c.Email })
                .ToListAsync());
        }

        /// <summary>
        /// Activate or Deactivate user record
        /// </summary>
        /// <returns>No content</returns>
        /// <response code="400">If request with wrong parameters</response>
        /// <response code="404">If user not found</response>
        /// <response code="200">If operation done</response>
        /// <response code="500">If server error happen during operation</response>
        // PUT: api/Admin/SetUserStatus
        [HttpPut]
        [Route("SetUserStatus")]
        public async Task<IActionResult> SetUserStatus(ChangeUserStatusRequest changeUserStatusRequest)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("User not found {userid} request send by [{admin}]", changeUserStatusRequest.UserId, User.Uid());
                return BadRequest();
            }

            var requestedUser = await userManager.Users.FirstOrDefaultAsync(c => c.Id == changeUserStatusRequest.UserId);


            if (requestedUser == null)
            {
                logger.LogWarning("User not found {userid} request send by [{admin}]", changeUserStatusRequest.UserId, User.Uid());
                return NotFound("user with id [{userId}] not found");
            }

            requestedUser.IsDisabled = changeUserStatusRequest.IsDisabled;

            var opResult = await userManager.UpdateAsync(requestedUser);
            if (opResult.Succeeded)
            {
                return Ok();
            }

            return Problem(
                title: "User was not set to proper status", detail:
                opResult.Errors.Select(d => d.Description)
                    .Aggregate((a, b) => $"{a},{b}"),
                statusCode:
                (int)HttpStatusCode.InternalServerError);

        }


        /// <summary>
        /// Delete exact user from system including all assigned tasks data
        /// </summary>
        /// <returns>No content</returns>
        /// <response code="400">If bad request or we are going to ourselfs or user is admin</response>
        /// <response code="204">If item operation done</response>
        /// <response code="404">If user not found</response>
        /// <response code="500">If error happen during process</response>
        // PUT: api/Admin/SetUserStatus
        [HttpDelete]
        [Route("RemoveUser")]
        public async Task<IActionResult> RemoveUser(RemoveUserRequest removeUserRequest)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("User not found {userid} request send by [{admin}]", removeUserRequest.UserId, User.Uid());
                return BadRequest();
            }

            var requestedUser = await userManager.Users.FirstOrDefaultAsync(c => c.Id == removeUserRequest.UserId);

            if (requestedUser == null)
            {
                logger.LogWarning("User not found {userid} request send by [{admin}]", removeUserRequest.UserId, User.Uid());
                return NotFound("user with id [{userId}] not found");
            }

            if (removeUserRequest.UserId == User.Uid())
            {
                //looks like user is removing himself
                //not allowed 
                logger.LogWarning("User try remove himself {userid} request send by [{admin}]", removeUserRequest.UserId, User.Uid());
                return BadRequest("please provide another UID");
            }

            if (await userManager.IsInRoleAsync(requestedUser, AppUserRoles.Administrator))
            {
                //looks we a removing admin user
                //not allowed
                logger.LogWarning("User try remove other admin {userid} request send by [{admin}]", removeUserRequest.UserId, User.Uid());
                return BadRequest("please provide another UID");
            }

            var tasks = await dbContext.Tasks
                .Where(c => c.User.Id == removeUserRequest.UserId)
                .Select(c => new TaskEntity { Id = c.Id }).ToListAsync();

            if (tasks != null && tasks.Count != 0)
            {
                dbContext.Tasks.RemoveRange(tasks);

                await dbContext.SaveChangesAsync();
            }

            var opResult = await userManager.DeleteAsync(requestedUser);
            if (opResult.Succeeded)
            {
                return NoContent();
            }

            return Problem(
                title: "User was not deleted properly", detail:
                opResult.Errors.Select(d => d.Description)
                    .Aggregate((a, b) => $"{a},{b}"),
                statusCode:
                (int)HttpStatusCode.InternalServerError);

        }


    }
}
