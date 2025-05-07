using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using doan.Models;
using System.Linq;
using System.Threading.Tasks;

namespace doan.Controllers
{
    public class TuDienController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TuDienController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TuDien
        public async Task<IActionResult> Index(string searchTerm, string filter = "TuVung")
        {
            // Only implement vocabulary search for now
            if (filter != "TuVung")
            {
                // Return view with empty list or handle other filters later
                return View(new List<Vocabulary>());
            }

            // Start with all vocabulary items
            var query = _context.tuvung.AsQueryable();
            query = query.OrderBy(v => v.Tuvung);

            // Apply search term filter if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim(); // Trim whitespace
                query = query.Where(v => 
                    (v.Tuvung != null && v.Tuvung.ToLower().Contains(searchTerm)) || // Use Tuvung property
                    (v.Nghia != null && v.Nghia.ToLower().Contains(searchTerm)) || // Use Nghia property
                    (v.HanTu != null && v.HanTu.ToLower().Contains(searchTerm)) || // Use HanTu property
                    (v.PhatAm != null && v.PhatAm.ToLower().Contains(searchTerm)) // Use PhatAm property
                );
            }

            var vocabularies = await query.ToListAsync(); // Use Vocabulary model
            
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Filter = filter; 

            return View(vocabularies); 
        }
    }
} 