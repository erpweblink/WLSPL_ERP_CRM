using Microsoft.AspNetCore.Mvc;
using WLSPL_ERP_CRM.repository;

namespace WLSPL_ERP_CRM.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationRepo _notificationRepo;
        public NotificationController(INotificationRepo notificationRepo)
        {
            _notificationRepo = notificationRepo;
        }

        public async Task<IActionResult> GetCount()
        {
            var result = await _notificationRepo.GetNotificationsAsync();
            return Json(result.Count);
        }

        public async Task<IActionResult> Index()
        {
            var result =await _notificationRepo.GetNotificationsAsync();
            return PartialView("_Notifications", result);
        }
    }
}
