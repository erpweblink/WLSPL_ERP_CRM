namespace WLSPL_ERP_CRM.repository
{
    public interface IReports
    {
        Task<List<object>> GetTopInvoiceList();
    }
}
