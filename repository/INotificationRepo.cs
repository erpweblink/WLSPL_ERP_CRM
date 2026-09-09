namespace WLSPL_ERP_CRM.repository
{
    public interface INotificationRepo
    {
        Task<List<dynamic>> GetNotificationsAsync();
    }
}
