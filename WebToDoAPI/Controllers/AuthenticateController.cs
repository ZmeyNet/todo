using System;
using WebToDoAPI.Data;
using WebToDoAPI.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using WebToDoAPI.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net;
using WebToDoAPI.Utils;
using Microsoft.Extensions.Logging;

namespace WebToDoAPI.Controllers
{
    [Route("api/[controller]")] // api/Authenticate
    public class AuthenticateController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IPasswordGenerator passwordGenerator;
        private readonly IEmailSender emailSender;
        private readonly JwtConfig jwtConfig;
        private readonly ILogger<AuthenticateController> logger;

        public AuthenticateController(ILogger<AuthenticateController> logger
            , UserManager<ApplicationUser> userManager
            , IOptionsMonitor<JwtConfig> optionsMonitor
            , IPasswordGenerator passwordGenerator
            , IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.passwordGenerator = passwordGenerator;
            this.emailSender = emailSender;
            this.jwtConfig = optionsMonitor.CurrentValue;
            this.logger = logger;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistration user)
        {
            // Check if the incoming request is valid
            if (ModelState.IsValid)
            {
                // check i the user with the same email exist
                var existingUser = await userManager.FindByEmailAsync(user.Email);

                if (existingUser != null)
                {
                    logger.LogWarning("Registration. Same email.{Email}", user.Email);
                    return BadRequest(new RegistrationResponse { Errors = new List<string> { "Email already exist" } });
                }

                ApplicationUser newUser = new()
                {
                    Email = user.Email,
                    UserName = user.Email
                };

                var generatedPwd = passwordGenerator.Generate(32, 4);
                var isCreated = await userManager.CreateAsync(newUser, generatedPwd);
                //send email to end user with pwd assigned
                await emailSender.SendPasswordToNewlyCreatedUserAsync(newUser.Email, generatedPwd);

                if (isCreated.Succeeded)
                {
                    logger.LogInformation("Registration. New {user}", user);
                    return Ok(new RegistrationResponse { Result = true });
                }

                return new JsonResult(new RegistrationResponse()
                {
                    Result = false,
                    Errors = isCreated.Errors.Select(x => x.Description).ToList()
                })
                { StatusCode = (int)HttpStatusCode.InternalServerError };
            }
            logger.LogInformation("Registration. model is invalid");

            return BadRequest(new RegistrationResponse { Errors = new List<string> { "Invalid payload" } });
        }


        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new RegistrationResponse { Errors = new List<string> { "Invalid payload" } });
            }
            // check if the user with the same email exist
            var existingUser = await userManager.FindByEmailAsync(user.Email);

            if (existingUser == null)
            {
                // We don't want to give to much information on why the request has failed for security reasons
                return BadRequest(new RegistrationResponse { Errors = new List<string> { "Invalid authentication" } });
            }

            // verify user pwd    
            if (!await userManager.CheckPasswordAsync(existingUser, user.Password))
            {
                return BadRequest(new RegistrationResponse { Errors = new List<string> { "Invalid authentication" } });
            }
            var jwtToken = TokenUtils.GenerateJwtToken(jwtConfig, existingUser);

            return Ok(new RegistrationResponse()
            {
                Result = true,
                Token = jwtToken
            });

        }

        [HttpPost]
        [Route("Forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest resetPassword)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResetPasswordResponse { Errors = new List<string> { "Invalid payload" } });
            }

            // check if the user with the same email exist
            var existingUser = await userManager.FindByEmailAsync(resetPassword.Email);

            if (existingUser == null)
            {
                logger.LogInformation("Reset pwd. Requested for wrong email {Email}", resetPassword.Email);
                return NotFound(new RegistrationResponse
                {
                    Result = false,
                    Errors = new List<string> { "Wrong email" }
                });
            }

            logger.LogWarning("ForgotPassword. Requested for {Email}", resetPassword.Email);
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(existingUser);

            //Create URL with above token and send it to email
            var resetPwdLink = Url.Action("ResetPassword", "Authenticate",
                new
                {
                    email = existingUser.UserName,
                    code = resetToken
                });

            await emailSender.SendResetPasswordLinkToUserAsync(existingUser.Email, resetPwdLink);

            return Ok(new ResetPasswordResponse { Result = true });
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetPassword)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResetPasswordResponse { Errors = new List<string> { "Invalid payload" } });
            }

            // check if the user with the same email exist
            var existingUser = await userManager.FindByEmailAsync(resetPassword.Email);

            if (existingUser == null)
            {
                logger.LogWarning("ResetPassword. Requested for wrong email {Email}", resetPassword.Email);
                return NotFound(new ResetPasswordResponse { Errors = new List<string> { "Wrong email" } });
            }

            //verify new password against Validators
            foreach (var validator in userManager.PasswordValidators)
            {
                var validPassword =
                    await validator.ValidateAsync(userManager, existingUser, resetPassword.NewPassword);
                if (!validPassword.Succeeded)
                {
                    return BadRequest(new ResetPasswordResponse()
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                            "Password to simple",
                            validPassword.Errors.Select(c => c.Description)
                                .Aggregate((a, b) => $"{a},{b}")
                        }
                    });
                }
            }

            var passwordChangeResult = await userManager.ResetPasswordAsync(
                    existingUser,
                    resetPassword.ResetToken,
                    resetPassword.NewPassword);
            

            if (passwordChangeResult.Succeeded)
            {
                logger.LogInformation("ResetPassword. done for {Email}", existingUser.Email);
                return Ok(new ResetPasswordResponse {Result = true});
            }

            logger.LogError("ResetPassword. error for {Email}, {Errors}",
                existingUser.Email,
                passwordChangeResult.Errors.Select(d => d.Description)
                    .Aggregate((a, b) => $"{a},{b}"));

            return BadRequest(new ResetPasswordResponse { Errors = new List<string> { "Invalid payload" } });
        }

       
    }
}

