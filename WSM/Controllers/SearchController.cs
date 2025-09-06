using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;

namespace WMS.Controllers;


public class SearchController : Controller
{
    public IActionResult Global(string q, string context)
    {
        switch (context)
        {
            case "Ingredient":
                return ((q == "" || q == null) ? RedirectToAction("ReadIngredient", "Ingredient") : 
                    RedirectToAction("ReadIngredient", "Ingredient", new { id = q }));

            case "OrderHistory": 
                return ((q == "" || q == null) ? RedirectToAction("OrderHistory", "OrderHistory") : 
                    RedirectToAction("OrderHistory", "OrderHistory", new { id = q }));

            default:
                return RedirectToAction("Both", "Home");
        }
    }
}
