using System.Diagnostics;
using ECommerceCsvImport.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCsvImport.Controllers
{
    public class HomeController : Controller
    {
        // Главная страница сразу открывает список товаров
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Products");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
