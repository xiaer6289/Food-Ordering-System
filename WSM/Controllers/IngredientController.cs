using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WSM.Models;
using X.PagedList.Extensions;

namespace WSM.Controllers;
#nullable disable warnings

public class IngredientController : Controller
{
    private readonly DB db;

    public IngredientController(DB db)
    {
        this.db = db;
    }

    public IActionResult ReadIngredient(string? id, string? sort, string? dir, int page = 1)
    {
        // For Layout.cshtml search bar
        ViewBag.SearchContext = "Ingredient";
        ViewBag.SearchPlaceholder = "Search by Ingredient ID/Name";

        // Searching
        ViewBag.Name = id = id?.Trim() ?? "";
        var searched = db.Ingredients.AsQueryable();
        
        if (!string.IsNullOrEmpty(id))
        {
            // Try parse id as integer for Id search
            if (int.TryParse(id, out int searchId))
            {
                searched = searched.Where(s => s.Id == searchId || s.Name.Contains(id));
            }
            else
            {
                searched = searched.Where(s => s.Name.Contains(id));
            }
        }

        //sorting
        ViewBag.Sort = sort;
        ViewBag.Dir = dir;

        Func<Ingredient, object> fn = sort switch
        {
            "Id" => s => s.Id,
            "Name" => s => s.Name,
            "Quantity" => s => s.Quantity,
            "Weight(kg)" => s => s.Kilogram,
            "Price" => s => s.Price,
            "Total" => s => s.TotalPrice,
            _ => s => s.Id
        };

        var sorted = dir == "des" ?
                 searched.OrderByDescending(fn) :
                 searched.OrderBy(fn);

        // Paging
        if (page < 1)
        {
            return RedirectToAction(null, new { id, sort, dir, page = 1 });
        }

        var m = sorted.ToPagedList(page, 10);

        if (page > m.PageCount && m.PageCount > 0)
        {
            return RedirectToAction(null, new { id, sort, dir, page = m.PageCount });
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("ReadIngredient", m);
        }

        ViewBag.Ingredients = db.Ingredients.ToList();
        return View(m);
    }

    public IActionResult CreateIngredient()
    {
        var model = new List<IngredientVM>
        {
            new IngredientVM() // at least one item!
        };
        return View(model);
    }

    //POST
    [HttpPost]
    public IActionResult CreateIngredient(List<IngredientVM> vm)
    {
        if (ModelState.IsValid)
        {
            foreach (var ingredient in vm)
            {
                var ing = db.Ingredients
                    .FirstOrDefault(i => i.Name.ToLower() == ingredient.Name.ToLower());

                if (ing != null)
                {
                    ing.Quantity += ingredient.Quantity ?? 0;
                    ing.Kilogram += ingredient.Kilogram ?? 0;
                    ing.Price = ingredient.Price;
                }
                else
                {
                    var newIngredient = new Ingredient
                    {
                        Name = ingredient.Name,
                        Quantity = ingredient.Quantity,
                        Kilogram = ingredient.Kilogram,
                        Price = ingredient.Price
                    };

                    db.Ingredients.Add(newIngredient);
                }
            }
            db.SaveChanges();

            TempData["Info"] = "Ingredient(s) added successfully.";
            return RedirectToAction("ReadIngredient");
        }

        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState)
            {
                Console.WriteLine($"Field {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }
        }

        return View(vm);
    }

    [HttpPost]
    public IActionResult Delete(int? id)
    {
        var i = db.Ingredients.Find(id);

        if (i != null)
        {
            db.Ingredients.Remove(i);
            db.SaveChanges();

            TempData["Info"] = "Record deleted.";
        }
        return RedirectToAction("ReadIngredient");
    }

    [HttpPost]
    public IActionResult DeleteMany(int[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            TempData["Error"] = "No ingredients selected for deletion.";
            return RedirectToAction(nameof(ReadIngredient));
        }

        try
        {
            var ingredients = db.Ingredients.Where(i => ids.Contains(i.Id));
            db.Ingredients.RemoveRange(ingredients);
            int n = db.SaveChanges();

            TempData["Info"] = $"{n} ingredient(s) deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while deleting the ingredients.";
        }

        return RedirectToAction(nameof(ReadIngredient));
    }



    [HttpGet]
    public IActionResult UpdateIngredient(int? id)
    {
        var ingredient = db.Ingredients.Find(id);
        if (ingredient == null)
        {
            return RedirectToAction("ReadIngredient");
        }

        var vm = new IngredientVM
        {
            Id = ingredient.Id.ToString(),
            Name = ingredient.Name,
            Quantity = ingredient.Quantity,
            Kilogram = ingredient.Kilogram,
            Price = ingredient.Price
        };

        return View(vm);
    }

    // POST
    [HttpPost]
    public IActionResult UpdateIngredient(IngredientVM vm, string CombinedInput)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            Console.WriteLine("ModelState Errors: " + string.Join("; ", errors));
            return View(vm);
        }

        if (!string.IsNullOrWhiteSpace(CombinedInput))
        {
            CombinedInput = CombinedInput.Trim().ToLower();

            if (CombinedInput.EndsWith("pcs"))
            {
                if (int.TryParse(CombinedInput.Replace("pcs", "").Trim(), out int qty))
                {
                    vm.Quantity = qty;
                    vm.Kilogram = null;
                }
            }
            else if (CombinedInput.EndsWith("kg"))
            {
                if (decimal.TryParse(CombinedInput.Replace("kg", "").Trim(), out decimal kg))
                {
                    vm.Kilogram = kg;
                    vm.Quantity = null;
                }
            }
        }

        if (!int.TryParse(vm.Id, out int ingredientId))
        {
            TempData["Error"] = "Invalid ingredient ID.";
            return RedirectToAction("ReadIngredient");
        }

        var i = db.Ingredients.Find(ingredientId);
        if (i == null)
        {
            TempData["Error"] = "Ingredient not found.";
            return RedirectToAction("ReadIngredient");
        }

        if (ModelState.IsValid)
        {
            i.Name = vm.Name;
            i.Quantity = vm.Quantity;
            i.Kilogram = vm.Kilogram;
            i.Price = vm.Price;
            db.SaveChanges();

            TempData["Info"] = $"Ingredient {vm.Id} updated.";
            return RedirectToAction("ReadIngredient");
        }

        return View(vm);
    }
}


