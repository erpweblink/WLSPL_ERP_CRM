using WEBLINK_CRM.Models;
using System.Collections.Generic;
using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.repository
{
    public interface IServicesRepo
    {
        Task<List<Services>> GetDepartment(string Action);
        Task<int?> Submitservices(Services Model, string Action);
        Task<int> Delete(string ID, string CreatedBy);
       
    }
}
