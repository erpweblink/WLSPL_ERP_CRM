using Microsoft.AspNetCore.Mvc;

namespace WEBLINK_CRM.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
