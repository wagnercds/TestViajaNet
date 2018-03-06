using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace testLog.Controllers
{
    public class HomeController : Controller
    {
        [Log]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View();
        }

        [Log]
        public IActionResult Parameters(string name, string value)
        {
            ViewBag.name = name;
            ViewBag.value = value;
            return View();
        }
    }
}
