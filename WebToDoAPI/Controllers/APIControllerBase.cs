using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebToDoAPI.Data;

namespace WebToDoAPI.Controllers
{
    public class ApiControllerBase : ControllerBase
    {
        private readonly ToDoDbContext dbContext;
        private readonly ILogger<ApiControllerBase> logger;

        public ApiControllerBase(ILogger<ApiControllerBase> logger
            , ToDoDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }
    }
}
