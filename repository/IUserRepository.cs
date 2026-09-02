using Microsoft.AspNetCore.Mvc.Rendering;
using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.repository
{
    public interface IUserRepository
    {
        List<RegisterUserr> GetAllUsers();

        List<RegisterUserr> GetFilteredUsers(string managerEmpCode, string status, string search);

        RegisterUserr GetUserById(int id);

        bool CreateUser(RegisterUserr model);

        bool UpdateUser(RegisterUserr model);

        bool DeleteUser(int id);

        List<SelectListItem> GetSalesTLManagers();

        bool UpdateUserAvatar(int userId, string avatarPath);

        bool UpdateUserProfile(RegisterUserr model);
    }
}
