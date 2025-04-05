using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using doan.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

public class LessonController : Controller
{
    private readonly ApplicationDbContext _context;

    public LessonController(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IActionResult Details(int id)
    {
        if (_context == null)
        {
            return Problem("Database context is not available.");
        }

        var lesson = _context.Baihoc
            .Include(l => l.tuvung)
            .Include(l => l.nguphap)
            .FirstOrDefault(l => l.Id == id);

        if (lesson == null)
        {
            return NotFound();
        }

        return View("~/Views/Level/Details.cshtml", lesson);
    }

    public IActionResult Create()
    {
        ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Baihoc lesson)
    {
        if (ModelState.IsValid)
        {
            _context.Baihoc.Add(lesson);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        foreach (var err in errors)
        {
            Console.WriteLine("⚠️ Model Error: " + err);
        }

        ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name", lesson.LevelId);
        return View(lesson);
    }

    public async Task<IActionResult> Index()
    {
        var lessons = await _context.Baihoc.Include(l => l.Level).ToListAsync();
        return View(lessons);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var lesson = await _context.Baihoc.FindAsync(id);
        if (lesson == null) return NotFound();

        ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name", lesson.LevelId);
        return View(lesson);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Baihoc lesson)
    {
        if (id != lesson.Id) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(lesson);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name", lesson.LevelId);
        return View(lesson);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var lesson = await _context.Baihoc.Include(l => l.Level).FirstOrDefaultAsync(l => l.Id == id);
        if (lesson == null) return NotFound();

        return View(lesson);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var lesson = await _context.Baihoc.FindAsync(id);
        if (lesson != null)
        {
            _context.Baihoc.Remove(lesson);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> DetailsFull(int? id)
    {
        if (id == null) return NotFound();

        var lesson = await _context.Baihoc.Include(l => l.Level)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (lesson == null) return NotFound();

        return View(lesson);
    }
}
