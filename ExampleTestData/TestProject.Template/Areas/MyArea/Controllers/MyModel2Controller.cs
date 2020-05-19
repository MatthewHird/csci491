using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestProject.Areas.MyArea.Models;
using TestProject.Data;

namespace TestProject.Areas.MyArea.Controllers
{
    [Area("MyArea")]
    public class MyModel2Controller : Controller
    {
        private readonly MyDbContext _context;

        public MyModel2Controller(MyDbContext context)
        {
            _context = context;
        }

        // GET: MyArea/MyModel2
        public async Task<IActionResult> Index()
        {
            return View(await _context.MyModel2.ToListAsync());
        }

        // GET: MyArea/MyModel2/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myModel2 = await _context.MyModel2
                .FirstOrDefaultAsync(m => m.Id == id);
            if (myModel2 == null)
            {
                return NotFound();
            }

            return View(myModel2);
        }

        // GET: MyArea/MyModel2/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MyArea/MyModel2/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Propery1,Propery2,Propery3,Propery4")] MyModel2 myModel2)
        {
            if (ModelState.IsValid)
            {
                _context.Add(myModel2);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(myModel2);
        }

        // GET: MyArea/MyModel2/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myModel2 = await _context.MyModel2.FindAsync(id);
            if (myModel2 == null)
            {
                return NotFound();
            }
            return View(myModel2);
        }

        // POST: MyArea/MyModel2/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Propery1,Propery2,Propery3,Propery4")] MyModel2 myModel2)
        {
            if (id != myModel2.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(myModel2);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MyModel2Exists(myModel2.Id))
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
            return View(myModel2);
        }

        // GET: MyArea/MyModel2/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myModel2 = await _context.MyModel2
                .FirstOrDefaultAsync(m => m.Id == id);
            if (myModel2 == null)
            {
                return NotFound();
            }

            return View(myModel2);
        }

        // POST: MyArea/MyModel2/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var myModel2 = await _context.MyModel2.FindAsync(id);
            _context.MyModel2.Remove(myModel2);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MyModel2Exists(int id)
        {
            return _context.MyModel2.Any(e => e.Id == id);
        }
    }
}
