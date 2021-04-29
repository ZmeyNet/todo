using System.Collections.Generic;

namespace WebToDoAPI.Models
{
    public class BaseRequestResult
    {
        public bool Result { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
