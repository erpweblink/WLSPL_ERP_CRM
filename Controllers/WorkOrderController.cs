using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using WEBLINK_CRM.Models;
using WEBLINK_CRM.repository;


namespace WEBLINK_CRM.Controllers
{
    public class WorkOrderController : Controller
    {
        private readonly IWorkOrder objWorkOrder;
        public WorkOrderController(IWorkOrder workOrder)
        {
            objWorkOrder = workOrder;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string pageSize = "10";
            var list = await objWorkOrder.GetWorkOrderList(pageSize);
            return View(list);

        }
      
        [HttpPost]
        public async Task<IActionResult> GetWorkOrderDEtailsByID(string ID)
        {
            try
            {
                var list = await objWorkOrder.GetWorkOrderDEtailsByID(ID);

                return Json(new { Success = true, Data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWorkOrder(int ID)
        {
            try
            {
                if (ID <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid Work Order ID."
                    });
                }

                var result = await objWorkOrder.DeleteWorkOrder(ID);

                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Work Order deleted successfully."
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Work Order could not be deleted."
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

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrEdit([FromBody] VM_WorkOrder DataList)
        {
            try
            {
                if (DataList == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Invalid request."
                    });
                }

                int workOrderID = await objWorkOrder.SaveWorkOrder(DataList);

                return Json(new
                {
                    success = true,
                    WorkOrderID = workOrderID,
                    Message = "Work Order saved successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        public async Task<IActionResult> GetCompany(string Status)
        {
            try
            {
                if (HttpContext.Session.GetString("EmployeeId") != null)
                {
                    var loginId = HttpContext.Session.GetString("EmployeeId");
                    if (loginId != null)
                    {
                        var list = await objWorkOrder.GetCompanyList(Status);
                        if (list != null)
                        {
                            return Json(new { Success = true, Data = list });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Data Not Found.......!" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

        public async Task<IActionResult> GetCompanyDataByCode(string ID)
        {
            try
            {
                if (HttpContext.Session.GetString("EmployeeId") != null)
                {
                    var loginId = HttpContext.Session.GetString("EmployeeId");
                    if (loginId != null)
                    {
                        var list = await objWorkOrder.GetCompanyDataByCode(ID);
                        if (list != null)
                        {
                            return Json(new { Success = true, Data = list });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Data Not Found.......!" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }
        public async Task<IActionResult> BindServiceList(string Dept)
        {
            try
            {
                if (HttpContext.Session.GetString("EmployeeId") != null)
                {
                    var loginId = HttpContext.Session.GetString("EmployeeId");
                    if (loginId != null)
                    {
                        var list = await objWorkOrder.GetServices(Dept);
                        if (list != null)
                        {
                            return Json(new { Success = true, Data = list });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Data Not Found.......!" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

        public async Task<IActionResult> GetServiceByID(string ID)
        {
            try
            {
                if (HttpContext.Session.GetString("EmployeeId") != null)
                {
                    var loginId = HttpContext.Session.GetString("EmployeeId");
                    if (loginId != null)
                    {
                        var list = await objWorkOrder.GetServiceByID(ID);
                        if (list != null)
                        {
                            return Json(new { Success = true, Data = list });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Data Not Found.......!" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }
        public async Task<IActionResult> GetDepartment(string Status)
        {
            try
            {
                if (HttpContext.Session.GetString("EmployeeId") != null)
                {
                    var loginId = HttpContext.Session.GetString("EmployeeId");
                    if (loginId != null)
                    {
                        var list = await objWorkOrder.GetDepartmentlist(Status);
                        if (list != null)
                        {
                            return Json(new { success = true, message = list });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Data Not Found.......!" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

        public async Task<IActionResult> GetWorkOrderDataById(string ID)
        {
            try
            {
                if (HttpContext.Session.GetString("EmployeeId") == null)
                {
                    return RedirectToAction("Login", "Login");
                }

                var loginId = HttpContext.Session.GetString("EmployeeId");

                if (string.IsNullOrEmpty(loginId))
                {
                    return RedirectToAction("Login", "Login");
                }

                // IMPORTANT: await async methods
                var list = await objWorkOrder.GetWorkOrderDataById(ID);
                var Dtlslist = await objWorkOrder.GetWorkDetailsById(ID);
                var Banklist = await objWorkOrder.GetBankDetailsById(ID);

                if (list == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Work Order not found"
                    });
                }

                var getList = new
                {
                    WorkOrderID = list.ID,
                    Type = list.Type,
                    WOStatus = list.WOStatus,
                    WONo = list.WONo,
                    CompanyName = list.CompanyName,
                    OwnerName = list.OwnerName,
                    GSTNO = list.GSTNO,
                    EmailID = list.EmailID,
                    Address = list.Address,
                    TodayDate = list.TodayDt,
                    RenewalDate = list.RenewalDt,
                    PaymentMode = list.PaymentMode,

                    TotalDealBasicAmount = list.TotalDealBasicAmount,
                    TotalDealGSTAmount = list.TotalDealGSTAmount,

                    BasicAmountReceived = list.BasicAmountReceived,
                    GSTAmountReceived = list.GSTAmountReceived,

                    BalanceBasicAmount = list.BalanceBasicAmount,
                    BalanceGSTAmount = list.BalanceGSTAmount,
                    TotalAmountBalance = list.TotalAmountBalance,

                };

                var WorkorderDtlsList = Dtlslist?
        .Select(c => new
        {
            ID = c.ID,
            Department = c.Department,
            ServicesDescription = c.ServicesDescription,
            Remark = c.Remark,
            Qty = c.Qty,
            NoofYr = c.NoofYr,
            Rate = c.Rate,
            Amount = c.Amount,
            IsComplete = c.IsComplete,
            Status = c.Status
        })
        .ToList();

                var bankList = Banklist?
                    .Select(c => new
                    {
                        ID = c.ID,
                        BankName = c.BankName,
                        ChequeNo = c.ChequeNo,
                        ChequeDate = c.ChequeDate,
                        Amount = c.Amount
                    })
                    .ToList();

                var result = new
                {
                    WorkOrderHdr = getList,
                    WorkOrderDtls = WorkorderDtlsList,
                    WorkOrderBankList = bankList
                };

                return Json(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    Success = false,
                    Message = e.Message
                });
            }
        }
    }
}
