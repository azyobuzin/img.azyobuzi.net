using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace ImgAzyobuziV3
{
    public class MainModule : NancyModule
    {
        public MainModule()
        {
            Get["/"] = _ => "Hello";
        }
    }
}
