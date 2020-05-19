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
    public class MyModel1Controller : Controller
    {
        private readonly MyDbContext _context;

        public MyModel1Controller(MyDbContext context)
        {
            _context = context;
        }

        // GET: MyArea/MyModel1
        public async Task<IActionResult> Index()
        {
            return View(await _context.MyModel1.ToListAsync());
        }

        // GET: MyArea/MyModel1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myModel1 = await _context.MyModel1
                .FirstOrDefaultAsync(m => m.Id == id);
            if (myModel1 == null)
            {
                return NotFound();
            }

            return View(myModel1);
        }

        // GET: MyArea/MyModel1/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MyArea/MyModel1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Propery1,Propery2,Propery3,Propery4")] MyModel1 myModel1)
        {
            if (ModelState.IsValid)
            {
                _context.Add(myModel1);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(myModel1);
        }

        // GET: MyArea/MyModel1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myModel1 = await _context.MyModel1.FindAsync(id);
            if (myModel1 == null)
            {
                return NotFound();
            }
            return View(myModel1);
        }

        // POST: MyArea/MyModel1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Propery1,Propery2,Propery3,Propery4")] MyModel1 myModel1)
        {
            if (id != myModel1.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(myModel1);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MyModel1Exists(myModel1.Id))
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
            return View(myModel1);
        }

        // GET: MyArea/MyModel1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myModel1 = await _context.MyModel1
                .FirstOrDefaultAsync(m => m.Id == id);
            if (myModel1 == null)
            {
                return NotFound();
            }

            return View(myModel1);
        }

        // POST: MyArea/MyModel1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var myModel1 = await _context.MyModel1.FindAsync(id);
            _context.MyModel1.Remove(myModel1);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MyModel1Exists(int id)
        {
            return _context.MyModel1.Any(e => e.Id == id);
        }
    }
}
