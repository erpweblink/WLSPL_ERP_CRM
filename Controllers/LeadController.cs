using Microsoft.AspNetCore.Mvc;
using WEBLINK_CRM.Models;
using WEBLINK_CRM.repository;

namespace WEBLINK_CRM.Controllers
{
    public class LeadController : Controller
    {
        private readonly ILeadRepository _leadRepository;

        public LeadController(ILeadRepository leadRepository)
        {
            _leadRepository = leadRepository;
        }

        // GET: /Lead/Index
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var leads = _leadRepository.GetAllLeads();

                if (leads == null)
                {
                    leads = new List<LeadGenration>();
                }

                return View(leads);
            }
            catch (Exception ex)
            {
                return Content("Lead Error: " + ex.Message);
            }
        }

        // GET: /Lead/CreateOrEdit
        // GET: /Lead/CreateOrEdit/5
        [HttpGet]
        public IActionResult CreateOrEdit(int? id)
        {
            if (id == null)
            {
                var model = new LeadGenration
                {
                    Status = "New",
                    Quantity = 1,
                    CreatedOn = DateTime.Now,
                    UpdatedOn = DateTime.Now,
                    IsDeleted = false
                };

                return View(model);
            }

            var lead = _leadRepository.GetLeadById(id.Value);

            if (lead == null)
                return NotFound();

            return View(lead);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateOrEdit(LeadGenration model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string userName = User.Identity?.Name ?? "System";

            if (model.ID == 0)
            {
                // New record
                model.Createdby = userName;
                model.CreatedOn = DateTime.Now;

                model.UpdatedBy = userName;
                model.UpdatedOn = DateTime.Now;

                model.IsDeleted = false;

                bool result = _leadRepository.CreateLead(model);

                if (!result)
                {
                    ModelState.AddModelError("", "Unable to save lead.");
                    return View(model);
                }
            }
            else
            {
                // Existing record
                model.UpdatedBy = userName;
                model.UpdatedOn = DateTime.Now;

                bool result = _leadRepository.UpdateLead(model);

                if (!result)
                {
                    ModelState.AddModelError("", "Unable to update lead.");
                    return View(model);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Lead/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            string deletedBy = User.Identity?.Name ?? "System";

            bool result = _leadRepository.DeleteLead(id, deletedBy);

            if (!result)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public IActionResult InquiryList()
        {
            return View();
        }
    }
}