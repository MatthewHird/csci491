using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestProject.Areas.MyArea.Models;
using TestProject.Data;

namespace TestProject.Areas.MyArea.Controllers.Level1.Level2
{
    [Area("MyArea")]
    public class MyModel4Controller : Controller
    {
        private readonly MyDbContext _context;

        public MyModel4Controller(MyDbContext context)
        {
            _context = context;
        }

        // GET: MyArea/MyModel4
        public async Task<IActionResult> Index()
        {
            return View(await _context.MyModel4.ToListAsync());
        }

        // GET: MyArea/MyModel4/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myModel4 = await _context.MyModel4
                .FirstOrDefaultAsync(m => m.Id == id);
            if (myModel4 == null)
            {
                return NotFound();
            }

            return View(myModel4);
        }

        // GET: MyArea/MyModel4/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MyArea/MyModel4/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Propery1,Propery2,Propery3,Propery4")] MyModel4 myModel4)
        {
            if (ModelState.IsValid)
            {
                _context.Add(myModel4);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(myModel4);
        }

        // GET: MyArea/MyModel4/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myModel4 = await _context.MyModel4.FindAsync(id);
            if (myModel4 == null)
            {
                return NotFound();
            }
            return View(myModel4);
        }

        // POST: MyArea/MyModel4/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Propery1,Propery2,Propery3,Propery4")] MyModel4 myModel4)
        {
            if (id != myModel4.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(myModel4);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MyModel4Exists(myModel4.Id))
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
            return View(myModel4);
        }

        // GET: MyArea/MyModel4/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myModel4 = await _context.MyModel4
                .FirstOrDefaultAsync(m => m.Id == id);
            if (myModel4 == null)
            {
                return NotFound();
            }

            return View(myModel4);
        }

        // POST: MyArea/MyModel4/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var myModel4 = await _context.MyModel4.FindAsync(id);
            _context.MyModel4.Remove(myModel4);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MyModel4Exists(int id)
        {
            return _context.MyModel4.Any(e => e.Id == id);
        }
    }
}
