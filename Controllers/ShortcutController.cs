using Microsoft.AspNetCore.Mvc;
using WLSPL_ERP_CRM.repository;

public class ShortcutController : Controller
{
    private readonly IShortcutRepo _shortcuts;

    public ShortcutController(IShortcutRepo shortcut)
    {
        _shortcuts = shortcut;
    }

    [HttpGet]
    public IActionResult GetShortcuts()
    {
        var shortcuts = _shortcuts.GetShortcutsAsync().Result;
        return Json(shortcuts);
    }

    [HttpGet]
    public async Task<IActionResult> SearchEmployees(string q)
    {
        string userRole = HttpContext.Session.GetString("Role")?.ToString() ?? "NA";
        string userCode = HttpContext.Session.GetString("EmpCode")?.ToString() ?? "NA";

        if (string.IsNullOrWhiteSpace(q))
            return Json(new List<object>());

        var employees = await _shortcuts.SearchEmployees(q,userRole,userCode);
        return Json(employees);
    }

    [HttpGet]
    public async Task<IActionResult> SearchCompanies(string q)
    {
        string userRole = HttpContext.Session.GetString("Role") ?? "NA";
        string userCode = HttpContext.Session.GetString("EmpCode") ?? "NA";

        if (string.IsNullOrWhiteSpace(q))
            return Json(new List<object>());

        var companies = await _shortcuts.SearchCompanies(q, userRole, userCode);
        return Json(companies);
    }
}
