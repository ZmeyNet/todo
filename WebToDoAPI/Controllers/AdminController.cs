using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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
using WebToDoAPI.Models;
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

        // GET: api/Admin/GetAllTasks
        [HttpGet]
        [Route("GetAllTasks")]
        public async Task<ActionResult<IEnumerable<TaskEntity>>> GetAllTasks()
        {
            return await dbContext.Tasks.ToListAsync();
        }


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



        // GET: api/Admin/GetAllUsers
        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<ActionResult<IEnumerable<UserRecord>>> GetAllUsers()
        {
            return await dbContext.Users.Select(c =>
                    new UserRecord { Id = c.Id, Name = c.UserName, Email = c.Email })
                .ToListAsync();
        }


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

            var opResult = await userManager.SetLockoutEnabledAsync(requestedUser, changeUserStatusRequest.UserActiveStatusToBeSet);
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

            var opResult = await userManager.DeleteAsync(requestedUser);
            if (opResult.Succeeded)
            {
                return Ok();
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
