using Microsoft.AspNetCore.Mvc.Rendering;
using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.repository
{
    public interface IUserRepository
    {
        List<RegisterUserr> GetAllUsers();

        RegisterUserr GetUserById(int id);

        bool CreateUser(RegisterUserr model);

        bool UpdateUser(RegisterUserr model);

        bool DeleteUser(int id);

        List<SelectListItem> GetSalesTLManagers();
    }
}
