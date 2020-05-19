using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestProject.Areas.MyArea.Models;
using TestProject.Data;

namespace TestProject.Areas.MyArea.Controllers.Level1
{
    [Area("MyArea")]
    public class MyModel3Controller : Controller
    {
        private readonly MyDbContext _context;

        public MyModel3Controller(MyDbContext context)
        {
            _context = context;
        }

        // GET: MyArea/MyModel3
        public async Task<IActionResult> Index()
        {
            return View(await _context.MyModel3.ToListAsync());
        }

        // GET: MyArea/MyModel3/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myModel3 = await _context.MyModel3
                .FirstOrDefaultAsync(m => m.Id == id);
            if (myModel3 == null)
            {
                return NotFound();
            }

            return View(myModel3);
        }

        // GET: MyArea/MyModel3/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MyArea/MyModel3/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Propery1,Propery2,Propery3,Propery4")] MyModel3 myModel3)
        {
            if (ModelState.IsValid)
            {
                _context.Add(myModel3);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(myModel3);
        }

        // GET: MyArea/MyModel3/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myModel3 = await _context.MyModel3.FindAsync(id);
            if (myModel3 == null)
            {
                return NotFound();
            }
            return View(myModel3);
        }

        // POST: MyArea/MyModel3/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Propery1,Propery2,Propery3,Propery4")] MyModel3 myModel3)
        {
            if (id != myModel3.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(myModel3);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MyModel3Exists(myModel3.Id))
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
            return View(myModel3);
        }

        // GET: MyArea/MyModel3/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myModel3 = await _context.MyModel3
                .FirstOrDefaultAsync(m => m.Id == id);
            if (myModel3 == null)
            {
                return NotFound();
            }

            return View(myModel3);
        }

        // POST: MyArea/MyModel3/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var myModel3 = await _context.MyModel3.FindAsync(id);
            _context.MyModel3.Remove(myModel3);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MyModel3Exists(int id)
        {
            return _context.MyModel3.Any(e => e.Id == id);
        }
    }
}
