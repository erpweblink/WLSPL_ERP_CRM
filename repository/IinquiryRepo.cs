using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.repository
{
    public interface IinquiryRepo
    {
        Task<int> Insertinquiry(Inquiry Model, string Action);

        Task<List<Inquiry>> GetInquiries();

        Task<List<Inquiry>> GetInquiriesFromDatabase();

        Task<List<Employee>> GetSalesPersons();

        Task<int> InsertWhatsappinquiry(Inquiry Model, string Action);

        Task<List<Inquiry>> GetWhatsappInquiries();

        Task<List<Inquiry>> GetWhatsappInquiriesFromDatabase();
        Task<int> AssignSalesPerson(int inquiryId, string salesEmpCode, string Action);

        Task<List<Inquiry>> Getlead();

    }
}
