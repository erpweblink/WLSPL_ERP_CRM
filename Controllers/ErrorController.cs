using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WEBLINK_CRM.Models;

namespace WLSPL_ERP_CRM.Controllers
{
    public class ErrorController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(IWebHostEnvironment env, ILogger<ErrorController> logger)
        {
            _env = env;
            _logger = logger;
        }

        [AllowAnonymous]
        [Route("Error/{statusCode:int?}")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index(int? statusCode)
        {
            var code = statusCode ?? 500;
            var exFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var reExecute = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            if (exFeature?.Error != null)
            {
                _logger.LogError(exFeature.Error, "Unhandled exception at {Path}", exFeature.Path);
                code = 500;
            }

            var model = new ErrorViewModel
            {
                StatusCode = code,
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Path = exFeature?.Path ?? reExecute?.OriginalPath,
                Details = _env.IsDevelopment() ? exFeature?.Error.Message : null
            };

            switch (code)
            {
                case 403:
                    model.Title = "Access Denied";
                    model.Message = "You don't have permission to view this page. If you think this is a mistake, please contact your administrator.";
                    model.Icon = "fa-lock";
                    model.Theme = "warning";
                    break;
                case 401:
                    model.Title = "Session Expired";
                    model.Message = "Please sign in again to continue.";
                    model.Icon = "fa-user-lock";
                    model.Theme = "warning";
                    break;
                case 404:
                    model.Title = "Page Not Found";
                    model.Message = "The page you're looking for doesn't exist or has been moved.";
                    model.Icon = "fa-map-location-dot";
                    model.Theme = "info";
                    break;
                default:
                    model.Title = "Something Went Wrong";
                    model.Message = "An unexpected error occurred. Our team has been notified. Please try again in a moment.";
                    model.Icon = "fa-triangle-exclamation";
                    model.Theme = "danger";
                    break;
            }

            Response.StatusCode = code;
            return View("Error", model);
        }
    }
}
