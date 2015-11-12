using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace ImgAzyobuziNet.Controllers
{
    [Route("api/v3/[action]")]
    public class ApiV3Controller : Controller
    {
        [HttpGet]
        public IEnumerable<string> Regex()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
