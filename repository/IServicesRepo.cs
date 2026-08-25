
using System.Collections.Generic;
using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.repository
{
    public interface IServicesRepo
    {

        Task<int> SubmitServices(Services Model, string Action);

        Task<List<Services>> GetServices(Services Model, string Action);
        Task<List<Department>> Getdepartments(Department Model, string Action);
        Task<Services> GetServicesById(string ID);

        Task<int> DeleteServices(string ID, string UpdatedBy);
    }
}
