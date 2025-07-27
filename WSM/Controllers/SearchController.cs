using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;

namespace WMS.Controllers;


public class SearchController : Controller
{
    public IActionResult Global(string q, string context)
    {
        switch (context)
        {
            case "Restock":
                return RedirectToAction("Restock", "Home", new { id = q });
            default:
                return RedirectToAction("Index", "Home");
        }
    }
}
