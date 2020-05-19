using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestProject.Data;
using TestProject.Models;
using TestProject.Services.Breadcrumbs;

namespace TestProject.Controllers.Fish
{
    public class RedFishController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IFishBreadcrumbService _breadcrumbService;

        public RedFishController(
            MyDbContext context,
            IFishBreadcrumbService breadcrumbService)
        {
            _context = context;
            _breadcrumbService = breadcrumbService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.RedFish.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redFish = await _context.RedFish
                .FirstOrDefaultAsync(m => m.Id == id);
            if (redFish == null)
            {
                return NotFound();
            }

            ViewData["BreadcrumbNode"] = _breadcrumbService.RedFishDetailsBreadcrumb(id);

            return View(redFish);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Propery1,Propery2,Propery3,Propery4")] RedFish redFish)
        {
            if (ModelState.IsValid)
            {
                _context.Add(redFish);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(redFish);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redFish = await _context.RedFish.FindAsync(id);
            if (redFish == null)
            {
                return NotFound();
            }
            return View(redFish);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Propery1,Propery2,Propery3,Propery4")] RedFish redFish)
        {
            if (id != redFish.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(redFish);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RedFishExists(redFish.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(redFish);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redFish = await _context.RedFish
                .FirstOrDefaultAsync(m => m.Id == id);
            if (redFish == null)
            {
                return NotFound();
            }

            return View(redFish);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var redFish = await _context.RedFish.FindAsync(id);
            _context.RedFish.Remove(redFish);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RedFishExists(int id)
        {
            return _context.RedFish.Any(e => e.Id == id);
        }
    }
}
