using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Core;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAsyncRepository repository;
        
        public HomeController(IAsyncRepository repository)
        {
            this.repository = repository;
        }

        public IActionResult Index()
        {
            //var rr = await repository.GetById<InventoryItem>(Guid.Parse("14e4d3b4e1bc4d7890c6d19ae1c52d9f"));
            //Console.WriteLine("Nino: " + rr.Id);
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
