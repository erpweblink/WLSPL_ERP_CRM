using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.repository
{
    public interface ILeadRepository
    {
        List<LeadGenration> GetAllLeads();

        LeadGenration GetLeadById(int id);

        bool CreateLead(LeadGenration model);

        bool UpdateLead(LeadGenration model);

        bool DeleteLead(int id, string deletedBy);
    }
}