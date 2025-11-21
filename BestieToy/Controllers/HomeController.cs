using BestieToy.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestieToy.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthService _authService;

        public HomeController(IAuthService authService)
        {
            _authService = authService;
        }

        public IActionResult Index()
        {
            ViewBag.IsLoggedIn = _authService.IsLoggedIn(HttpContext);
            ViewBag.CurrentUser = _authService.GetCurrentUser(HttpContext);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
