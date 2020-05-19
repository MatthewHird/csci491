using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestProject.Data;
using TestProject.Models;

namespace TestProject.Controllers
{
    public class DrSuessController : Controller
    {
        private readonly MyDbContext _context;

        public DrSuessController(MyDbContext context)
        {
            _context = context;
        }

        // GET: DrSuesses
        public async Task<IActionResult> Index()
        {
            return View(await _context.DrSuess.ToListAsync());
        }

        // GET: DrSuesses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drSuess = await _context.DrSuess
                .FirstOrDefaultAsync(m => m.Id == id);
            if (drSuess == null)
            {
                return NotFound();
            }

            return View(drSuess);
        }

        // GET: DrSuesses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DrSuesses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Propery1,Propery2,Propery3,Propery4")] DrSuess drSuess)
        {
            if (ModelState.IsValid)
            {
                _context.Add(drSuess);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(drSuess);
        }

        // GET: DrSuesses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drSuess = await _context.DrSuess.FindAsync(id);
            if (drSuess == null)
            {
                return NotFound();
            }
            return View(drSuess);
        }

        // POST: DrSuesses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Propery1,Propery2,Propery3,Propery4")] DrSuess drSuess)
        {
            if (id != drSuess.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(drSuess);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DrSuessExists(drSuess.Id))
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
            return View(drSuess);
        }

        // GET: DrSuesses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drSuess = await _context.DrSuess
                .FirstOrDefaultAsync(m => m.Id == id);
            if (drSuess == null)
            {
                return NotFound();
            }

            return View(drSuess);
        }

        // POST: DrSuesses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var drSuess = await _context.DrSuess.FindAsync(id);
            _context.DrSuess.Remove(drSuess);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DrSuessExists(int id)
        {
            return _context.DrSuess.Any(e => e.Id == id);
        }
    }
}
