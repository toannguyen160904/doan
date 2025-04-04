using doan.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace doan.Areas.Admin.Controllers
{ 
    [Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class GrammarsController : Controller
{
    private readonly ApplicationDbContext _context;

    public GrammarsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var grammars = await _context.nguphap.Include(g => g.Lesson).ToListAsync();
        return View(grammars);
    }

    public IActionResult Create()
    {
        ViewData["LessonId"] = new SelectList(_context.Baihoc, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GrammarStructure grammar)
    {
        if (ModelState.IsValid)
        {
            _context.nguphap.Add(grammar);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(grammar);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var grammar = await _context.nguphap.FindAsync(id);
        if (grammar == null) return NotFound();
        ViewData["LessonId"] = new SelectList(_context.Baihoc, "Id", "Name", grammar.LessonId);
        return View(grammar);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GrammarStructure grammar)
    {
        if (id != grammar.Id) return NotFound();
        if (ModelState.IsValid)
        {
            _context.Update(grammar);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(grammar);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var grammar = await _context.nguphap.Include(g => g.Lesson).FirstOrDefaultAsync(g => g.Id == id);
        if (grammar == null) return NotFound();
        return View(grammar);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var grammar = await _context.nguphap.FindAsync(id);
        if (grammar != null)
        {
            _context.nguphap.Remove(grammar);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
}
