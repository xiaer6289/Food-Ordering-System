using Microsoft.AspNetCore.Mvc;

namespace WMS.Controllers
{
    public class Login : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
