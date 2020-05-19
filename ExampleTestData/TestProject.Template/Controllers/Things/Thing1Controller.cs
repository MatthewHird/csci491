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
    public class Thing1Controller : Controller
    {
        private readonly MyDbContext _context;

        public Thing1Controller(MyDbContext context)
        {
            _context = context;
        }

        // GET: Thing1
        public async Task<IActionResult> Index()
        {
            return View(await _context.Thing1.ToListAsync());
        }

        // GET: Thing1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thing1 = await _context.Thing1
                .FirstOrDefaultAsync(m => m.Id == id);
            if (thing1 == null)
            {
                return NotFound();
            }

            return View(thing1);
        }

        // GET: Thing1/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Thing1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Propery1,Propery2,Propery3,Propery4")] Thing1 thing1)
        {
            if (ModelState.IsValid)
            {
                _context.Add(thing1);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(thing1);
        }

        // GET: Thing1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thing1 = await _context.Thing1.FindAsync(id);
            if (thing1 == null)
            {
                return NotFound();
            }
            return View(thing1);
        }

        // POST: Thing1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Propery1,Propery2,Propery3,Propery4")] Thing1 thing1)
        {
            if (id != thing1.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(thing1);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Thing1Exists(thing1.Id))
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
            return View(thing1);
        }

        // GET: Thing1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thing1 = await _context.Thing1
                .FirstOrDefaultAsync(m => m.Id == id);
            if (thing1 == null)
            {
                return NotFound();
            }

            return View(thing1);
        }

        // POST: Thing1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var thing1 = await _context.Thing1.FindAsync(id);
            _context.Thing1.Remove(thing1);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Thing1Exists(int id)
        {
            return _context.Thing1.Any(e => e.Id == id);
        }
    }
}
