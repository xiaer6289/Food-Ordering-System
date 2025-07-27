using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WSM.Models;
using X.PagedList.Extensions;

namespace WSM.Controllers;

public class IngredientController : Controller
{
    private readonly DB db;

    public IngredientController(DB db)
    {
        this.db = db;
    }

    public IActionResult Restock()
    {

        var model = db.Ingredients.ToPagedList(1, 10);
        return View("~/Views/Home/Restock.cshtml", model);
    }

    //POST
    [HttpPost]
    public IActionResult PurchaseOrder(IngredientVM vm)
    {
        if (ModelState.IsValid)
        {
            var ing = db.Ingredients
                .FirstOrDefault(i => i.Name == vm.Name);

            if (ing != null)
            {
                ing.Quantity += vm.Quantity ?? 0;
                ing.Kilogram += vm.Kilogram ?? 0;
            }
            else
            {
                vm.Id = NextId();

                db.Ingredients.Add(new()
                {
                    Id = vm.Id,
                    Name = vm.Name,
                    Quantity = vm.Quantity,
                    Kilogram = vm.Kilogram,
                    Price = vm.Price,
                    //TotalPrice = vm.TotalPrice
                });
            }

            db.SaveChanges();

            TempData["Info"] = $"Ingredient {vm.Id} inserted.";
            return RedirectToAction("Restock", "Home");

        }


        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState)
            {
                Console.WriteLine($"Field {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }
        }

        return RedirectToAction("PurchaseOrder");
    }

    private string NextId()
    {
        string max = db.Ingredients.Max(i => i.Id) ?? "I000";
        int n = int.Parse(max[1..]);
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
        return RedirectToAction("Restock");

    }

    [HttpGet]
    public IActionResult Edit(string id)
    {
        var ingredient = db.Ingredients.Find(id);
        if (ingredient == null)
        {
            return RedirectToAction("Restock");
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
    public IActionResult Edit(IngredientVM vm, string CombinedInput)
    {
        if (!ModelState.IsValid)
        {
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
                    vm.Kilogram = 0;
                }
            }
            else if (CombinedInput.EndsWith("kg"))
            {
                if (decimal.TryParse(CombinedInput.Replace("kg", "").Trim(), out decimal kg))
                {
                    vm.Kilogram = kg;
                    vm.Quantity = 0;
                }
            }
        }


        var i = db.Ingredients.Find(vm.Id);
        if (i == null)
        {
            return RedirectToAction("Restock");
        }

        if (ModelState.IsValid)
        {
            i.Name = vm.Name;
            i.Price = vm.Price;
            db.SaveChanges();
        }


        TempData["Info"] = $"Ingredient {i.Id} updated.";
        return RedirectToAction("Restock");
    }
}


