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

            case "Admin":
                return ((q == "" || q == null)? RedirectToAction("Admins", "Admin") :
                    RedirectToAction("Admins", "Admin", new { id = q }));

            case "Staff":
                return ((q == "" || q == null) ? RedirectToAction("ReadStaff", "Staff") :
                    RedirectToAction("ReadStaff", "Staff", new { id = q }));

            case "Category":
                return ((q == "" || q == null) ? RedirectToAction("Categories", "Category") :
                    RedirectToAction("Categories", "Category", new { id = q }));

            case "Food":
                return ((q == "" || q == null) ? RedirectToAction("Foods", "Food") :
                    RedirectToAction("Foods", "Food", new { id = q }));

            default:
                return RedirectToAction("Both", "Home");
        }
    }
}
