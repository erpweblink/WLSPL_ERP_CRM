using Microsoft.AspNetCore.Mvc;

namespace WEBLINK_CRM.Controllers
{
    public class NewController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
