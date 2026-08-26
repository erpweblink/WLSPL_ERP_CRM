using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBLINK_CRM.Models;
using WEBLINK_CRM.repository;

namespace WEBLINK_CRM.Controllers
{
    [Authorize]
    public class LeadController : Controller
    {
        private readonly ILeadRepository _leadRepository;
        private readonly IinquiryRepo _inquiryRepository;

        public LeadController(
            IinquiryRepo inquiryRepo,
            ILeadRepository leadRepository)
        {
            _inquiryRepository = inquiryRepo;
            _leadRepository = leadRepository;
        }
        // GET: /Lead/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var Lead = await _inquiryRepository.Getlead();

                if (Lead == null)
                {
                    return NotFound();
                }

                return View(Lead);
            }
            catch (Exception ex)
            {
                return Content("Lead Error: " + ex.Message);
            }
        }

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
        public async Task<IActionResult> InquiryList()

        {
            try
            {


                var apiInquiries =
                    await _inquiryRepository.GetInquiries();

                if (apiInquiries != null && apiInquiries.Any())
                {
                    foreach (var inquiry in apiInquiries)
                    {
                        await _inquiryRepository.Insertinquiry(
                            inquiry,
                            "Insert"
                        );
                    }
                }


                var inquiries =
                    await _inquiryRepository.GetInquiriesFromDatabase();


                var employees =
                    await _inquiryRepository.GetSalesPersons();



                var salesPersons =
                 employees ?? new List<Employee>();

                foreach (var inquiry in inquiries)
                {
                    inquiry.SalesPersons = salesPersons;
                }

                return View(inquiries);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return View(new List<Inquiry>());
            }
        }

        public async Task<IActionResult> Whatsappinquiry()
        {
            // 1. Get inquiries from WhatsApp API
            var apiInquiries =
                await _inquiryRepository.GetWhatsappInquiries();

            // 2. Save WhatsApp inquiries into database
            if (apiInquiries != null && apiInquiries.Any())
            {
                foreach (var inquiry in apiInquiries)
                {
                    await _inquiryRepository.InsertWhatsappinquiry(
                        inquiry,
                        "Insertwhatsappinquiry"
                    );
                }
            }

            // 3. Get WhatsApp inquiries from database
            var inquiries =
                await _inquiryRepository.GetWhatsappInquiriesFromDatabase();

            // 4. Get salespersons
            var employees =
                await _inquiryRepository.GetSalesPersons();

            var salesPersons =
                employees ?? new List<Employee>();

            // 5. Attach salespersons to every inquiry
            foreach (var inquiry in inquiries)
            {
                inquiry.SalesPersons = salesPersons;
            }

            // 6. Send to View
            return View(inquiries);
        }


        [HttpPost]
        public async Task<IActionResult> AssignSalesPerson(int inquiryId, string salesEmpCode, string Action)
        {
            try
            {
                if (inquiryId <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid inquiry ID."
                    });
                }

                if (string.IsNullOrWhiteSpace(salesEmpCode))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please select a sales person."
                    });
                }

                int result = await _inquiryRepository.AssignSalesPerson(
                    inquiryId,
                    salesEmpCode, Action
                );

                if (result > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Sales person assigned successfully."
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Inquiry not found or assignment failed."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}