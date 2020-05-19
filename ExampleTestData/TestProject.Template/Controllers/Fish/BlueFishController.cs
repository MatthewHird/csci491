using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestProject.Data;
using TestProject.Models;

namespace TestProject.Controllers.Fish
{
    public class BlueFishController : Controller
    {
        private readonly MyDbContext _context;

        public BlueFishController(MyDbContext context)
        {
            _context = context;
        }

        // GET: BlueFish
        public async Task<IActionResult> Index()
        {
            return View(await _context.BlueFish.ToListAsync());
        }

        // GET: BlueFish/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blueFish = await _context.BlueFish
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blueFish == null)
            {
                return NotFound();
            }

            return View(blueFish);
        }

        // GET: BlueFish/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BlueFish/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Propery1,Propery2,Propery3,Propery4")] BlueFish blueFish)
        {
            if (ModelState.IsValid)
            {
                _context.Add(blueFish);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(blueFish);
        }

        // GET: BlueFish/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blueFish = await _context.BlueFish.FindAsync(id);
            if (blueFish == null)
            {
                return NotFound();
            }
            return View(blueFish);
        }

        // POST: BlueFish/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Propery1,Propery2,Propery3,Propery4")] BlueFish blueFish)
        {
            if (id != blueFish.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(blueFish);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlueFishExists(blueFish.Id))
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
            return View(blueFish);
        }

        // GET: BlueFish/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blueFish = await _context.BlueFish
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blueFish == null)
            {
                return NotFound();
            }

            return View(blueFish);
        }

        // POST: BlueFish/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blueFish = await _context.BlueFish.FindAsync(id);
            _context.BlueFish.Remove(blueFish);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlueFishExists(int id)
        {
            return _context.BlueFish.Any(e => e.Id == id);
        }
    }
}
