using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestProject.Data;
using TestProject.Models;

namespace TestProject.Controllers.Things
{
    public class Thing2Controller : Controller
    {
        private readonly MyDbContext _context;

        public Thing2Controller(MyDbContext context)
        {
            _context = context;
        }

        private async Task<IActionResult> Index()
        {
            return View(await _context.Thing2.ToListAsync());
        }

        [NonAction]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thing2 = await _context.Thing2
                .FirstOrDefaultAsync(m => m.Id == id);
            if (thing2 == null)
            {
                return NotFound();
            }

            return View(thing2);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Propery1,Propery2,Propery3,Propery4")] Thing2 thing2)
        {
            if (ModelState.IsValid)
            {
                _context.Add(thing2);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(thing2);
        }

        private async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thing2 = await _context.Thing2.FindAsync(id);
            if (thing2 == null)
            {
                return NotFound();
            }
            return View(thing2);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        private async Task<IActionResult> Edit(int id, [Bind("Id,Propery1,Propery2,Propery3,Propery4")] Thing2 thing2)
        {
            if (id != thing2.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(thing2);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Thing2Exists(thing2.Id))
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
            return View(thing2);
        }

        [NonAction]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thing2 = await _context.Thing2
                .FirstOrDefaultAsync(m => m.Id == id);
            if (thing2 == null)
            {
                return NotFound();
            }

            return View(thing2);
        }

        [NonAction]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var thing2 = await _context.Thing2.FindAsync(id);
            _context.Thing2.Remove(thing2);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Thing2Exists(int id)
        {
            return _context.Thing2.Any(e => e.Id == id);
        }
    }
}
