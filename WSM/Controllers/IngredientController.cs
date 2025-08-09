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
        var searched = db.Ingredients
                      .Where(s => s.Id.Contains(id) || s.Name.Contains(id));

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


        // (3) Paging ---------------------------
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

    [HttpGet]
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
                    .FirstOrDefault(i => i.Name == ingredient.Name);

                if (ing != null)
                {
                    ing.Quantity += ingredient.Quantity ?? 0;
                    ing.Kilogram += ingredient.Kilogram ?? 0;
                }
                else
                {
                    var Id = NextId();
                    var newIngredient = new Ingredient
                    {
                        Id = Id,
                        Name = ingredient.Name,
                        Quantity = ingredient.Quantity,
                        Kilogram = ingredient.Kilogram,
                        Price = ingredient.Price
                    };

                    db.Ingredients.Add(newIngredient);
                }
            }
            db.SaveChanges();

            TempData["Info"] = $"Ingredient id inserted.";
            return RedirectToAction("ReadIngredient", "Ingredient");

        }


        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState)
            {
                Console.WriteLine($"Field {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }
        }

        return RedirectToAction("ReadIngredient");
    }

    private string NextId()
    {
        // Get max ID from the database
        string maxDbId = db.Ingredients.Max(i => i.Id) ?? "I000";

        // Get max ID from unsaved entries in the change tracker
        string maxTrackedId = db.ChangeTracker.Entries<Ingredient>()
            .Where(e => e.State == EntityState.Added) // new but not saved
            .Select(e => e.Entity.Id)
            .DefaultIfEmpty("I000")
            .Max();

        // Take the larger one between DB and tracked entries
        string maxId = string.Compare(maxDbId, maxTrackedId) > 0 ? maxDbId : maxTrackedId;

        // Increment
        int n = int.Parse(maxId[1..]);
        return (n + 1).ToString("'I'000");
    }

    [HttpPost]
    public IActionResult Delete(string? id)
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

    [HttpGet]
    public IActionResult UpdateIngredient(string? id)
    {
        var ingredient = db.Ingredients.Find(id);
        if (ingredient == null)
        {
            return RedirectToAction("ReadIngredient");
        }

        var vm = new IngredientVM
        {
            Id = ingredient.Id,
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


        var i = db.Ingredients.Find(vm.Id);
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
            return RedirectToAction("ReadIngredient", "Ingredient");
        }


        return View(vm);
    }
}


