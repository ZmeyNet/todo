using System;
using WebToDoAPI.Data;
using WebToDoAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
                    return BadRequest(new RegistrationResponse()
                    {
                        Result = false,
                        Errors = new List<string>(){"Email already exist"}
                    });
                }

                ApplicationUser newUser = new (){ 
                    Email = user.Email, 
                    UserName = user.Email 
                };

                var generatedPwd = passwordGenerator.Generate(32, 4);
                var isCreated = await userManager.CreateAsync(newUser, generatedPwd);
                //send email to end user with pwd assigned
                _ = emailSender.SendPasswordToNewlyCreatedUserAsync(newUser.Email, generatedPwd);

                if (isCreated.Succeeded)
                {
                    logger.LogInformation("Registration. New {user}", user);
                    return Ok(new RegistrationResponse()
                    {
                        Result = true,                       
                    });
                }

                return new JsonResult(new RegistrationResponse()
                {
                    Result = false,
                    Errors = isCreated.Errors.Select(x => x.Description).ToList()
                })
                { StatusCode = (int)HttpStatusCode.InternalServerError };
            }
            logger.LogInformation("Registration. model is invalid");

            return BadRequest(new RegistrationResponse()
            {
                Result = false,
                Errors = new List<string>() { "Invalid payload" }
            });
        }


        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel user)
        {
            if (ModelState.IsValid)
            {
                // check if the user with the same email exist
                var existingUser = await userManager.FindByEmailAsync(user.Email);

                if (existingUser == null)
                {
                    // We dont want to give to much information on why the request has failed for security reasons
                    return BadRequest(new RegistrationResponse()
                    {
                        Result = false,
                        Errors = new List<string>(){ "Invalid authentication" }
                    });
                }

                // verfy user pwd    
                if (await userManager.CheckPasswordAsync(existingUser, user.Password))
                {
                    var jwtToken = GenerateJwtToken(existingUser);

                    return Ok(new RegistrationResponse()
                    {
                        Result = true,
                        Token = jwtToken
                    });
                }
                else
                {                    
                    return BadRequest(new RegistrationResponse()
                    {
                        Result = false,
                        Errors = new List<string>(){"Invalid authentication"}
                    });
                }
            }

            return BadRequest(new RegistrationResponse()
            {
                Result = false,
                Errors = new List<string>(){"Invalid payload"}
            });
        }

        private string GenerateJwtToken(ApplicationUser user)
        {           
            var tokenHandler = new JwtSecurityTokenHandler();

            // We get our secret from the appsettings
            var key = Encoding.ASCII.GetBytes(jwtConfig.Secret);

            // we define our token descriptor
            // We need to utilise claims which are properties in our token which gives information about the token
            // which belong to the specific user who it belongs to
            // so it could contain their id, name, email the good part is that these information
            // are generated by our server and identity framework which is valid and trusted
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            }),
                Expires = DateTime.UtcNow.AddHours(jwtConfig.ExpirationHours),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = tokenHandler.WriteToken(token);

            return jwtToken;
        }
    }
}

