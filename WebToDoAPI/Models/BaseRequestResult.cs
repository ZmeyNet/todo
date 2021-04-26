using System.Collections.Generic;

namespace WebToDoAPI.Models
{
    public class BaseRequestResult
    {
        public bool Result { get; set; }
        public List<string> Errors { get; set; }
    }
}
