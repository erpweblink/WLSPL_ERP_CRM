using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.Repositories
{
    public interface IEmployeeRepository
    {
        Employee? Login(string username, string password);
    }
}