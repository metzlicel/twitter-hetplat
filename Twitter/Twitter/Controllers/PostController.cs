using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Twitter.Data;
using Twitter.Models;
using IdentityUser = Microsoft.AspNetCore.Identity.IdentityUser;

namespace Twitter.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager; // ✅

        public PostsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Take(30)
                .ToListAsync();

            return View(posts);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction(nameof(Index));

            var user = await _userManager.GetUserAsync(User);

            var post = new Post
            {
                Content = content,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            };

            _context.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (post.UserId != user.Id) return Forbid();

            if ((DateTime.Now - post.CreatedAt).TotalMinutes > 5)
            {
                TempData["Error"] = "Solo puedes editar publicaciones con menos de 5 minutos.";
                return RedirectToAction(nameof(Index));
            }

            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string content)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (post.UserId != user.Id) return Forbid();

            if ((DateTime.Now - post.CreatedAt).TotalMinutes > 5)
            {
                TempData["Error"] = "Solo puedes editar publicaciones con menos de 5 minutos.";
                return RedirectToAction(nameof(Index));
            }

            post.Content = content;
            post.EditedAt = DateTime.Now;
            _context.Update(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
