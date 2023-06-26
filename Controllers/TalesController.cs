using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using fff.Data;
using fff.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;


namespace fff.Controllers
{
    public class TalesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TalesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }


        // GET: Tales
        public async Task<IActionResult> Index()
        {
            return _context.Tale != null ?
                        View(await _context.Tale.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Tale'  is null.");
        }


        // GET: Jokes/ShowSearchResults
        public async Task<IActionResult> ShowSearchResults(string SearchPhrase)
        {
            return View("Index", await _context.Tale.Where(j => j.Text.Contains(SearchPhrase) || j.Description.Contains(SearchPhrase) || j.Title.Contains(SearchPhrase)).ToListAsync());

        }


        // GET: Tales/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tale == null)
            {
                return NotFound();
            }

            var tale = await _context.Tale
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tale == null)
            {
                return NotFound();
            }

            return View(tale);
        }

        // GET: Tales/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tales/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Text")] Tale tale, IFormFile File)
        {
            if (ModelState.IsValid)
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                tale.UserId = userId;

                // Inside the Create method or wherever you handle file upload
                if (File != null && File.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + File.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await File.CopyToAsync(stream);
                    }
                    tale.File = Path.Combine("uploads", uniqueFileName); // Store the relative path
                }


                _context.Add(tale);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tale);
        }


        // GET: Tales/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tale == null)
            {
                return NotFound();
            }

            var tale = await _context.Tale.FindAsync(id);

            if (tale.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            if (tale == null)
            {
                return NotFound();
            }
            return View(tale);
        }

        // POST: Tales/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Text")] Tale tale, IFormFile File)
        {
            if (id != tale.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (File != null && File.Length > 0)
                {
                    if (!string.IsNullOrEmpty(tale.File))
                    {
                        string dfilePath = Path.Combine(_webHostEnvironment.WebRootPath, tale.File);
                        if (System.IO.File.Exists(dfilePath))
                        {
                            System.IO.File.Delete(dfilePath);
                        }
                    }

                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + File.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await File.CopyToAsync(stream);
                    }
                    tale.File = Path.Combine("uploads", uniqueFileName); // Store the relative path
                }

                try
                {
                    _context.Update(tale);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaleExists(tale.Id))
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
            return View(tale);
        }

        // GET: Tales/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tale == null)
            {
                return NotFound();
            }

            var tale = await _context.Tale
                .FirstOrDefaultAsync(m => m.Id == id);


            if (tale == null)
            {
                return NotFound();
            }

            if (tale.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            return View(tale);
        }

        // POST: Tales/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tale == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tale' is null.");
            }

            var tale = await _context.Tale.FindAsync(id);

            if (tale != null)
            {
                // Delete the associated file
                if (!string.IsNullOrEmpty(tale.File))
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, tale.File);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Tale.Remove(tale);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        private bool TaleExists(int id)
        {
            return (_context.Tale?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
