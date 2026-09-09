using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WEBLINK_CRM.Models;

public class ShortcutController : Controller
{
    private readonly List<ShortcutItem> _shortcuts;

    public ShortcutController(IOptions<List<ShortcutItem>> shortcuts)
    {
        _shortcuts = shortcuts.Value;
    }

    [HttpGet]
    public IActionResult GetShortcuts()
    {
        return Json(_shortcuts);
    }
}
