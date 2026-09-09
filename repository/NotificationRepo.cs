using Microsoft.Data.SqlClient;
using System.Data;

namespace WLSPL_ERP_CRM.repository
{
    public class NotificationRepo : INotificationRepo
    {
        private readonly IConfiguration _configuration;

        public NotificationRepo(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<dynamic>> GetNotificationsAsync()
        {
            var date = DateTime.Today;  // new DateTime(2026, 4, 10); 

            var query = @"
                   SELECT 
                        CAST(i.id AS VARCHAR)                             AS Id,
                        'Invoice'                                         AS Type,
                        'Invoice #' + i.invoiceno + ' has been created'   AS Message,
                        e.name                                            AS CreatedBy,
                        i.CreatedDate                                     AS Date
                    FROM InvoiceMain i
                    LEFT JOIN employees e ON e.empcode = i.sessionname
                    WHERE CAST(i.CreatedDate AS DATE) = CAST(@Date AS DATE)

                    UNION ALL

                    SELECT 
                        CAST(p.id AS VARCHAR)                                  AS Id,
                        'Proforma'                                             AS Type,
                        'Proforma #' + p.proformano + ' has been created'      AS Message,
                        e.name                                                 AS CreatedBy,
                        p.CreatedDate                                          AS Date
                    FROM stswlspl.tblProformaMain p
                    LEFT JOIN employees e ON e.empcode = p.sessionname
                    WHERE CAST(p.CreatedDate AS DATE) = CAST(@Date AS DATE)

                    UNION ALL

                    SELECT 
                        CAST(q.id AS VARCHAR)                                    AS Id,
                        'Quotation'                                              AS Type,
                        'Quotation #' + q.Quotationno + ' has been created'      AS Message,
                        e.name                                                   AS CreatedBy,
                        q.CreatedDate                                            AS Date
                    FROM stswlspl.tblQuotationMain q
                    LEFT JOIN employees e ON e.empcode = q.sessionname
                    WHERE CAST(q.CreatedDate AS DATE) = CAST(@Date AS DATE)

                    UNION ALL

                    SELECT 
                        CAST(f.ID_CommentHistory AS VARCHAR)   AS Id,
                        'Call & Meeting'                       AS Type,
                        f.cname                                AS Message,
                        e.name                                 AS CreatedBy,
                        f.commentdatetime                      AS Date
                    FROM stswlspl.VW_FollowUpRpt f
                    LEFT JOIN employees e ON e.empcode = f.sessionname
                    WHERE CAST(f.followupdate AS DATE) = CAST(@Date AS DATE)

                    UNION ALL

                    SELECT 
                        CAST(c.id AS VARCHAR)                              AS Id,
                        'Company'                                          AS Type,
                        'New company ' + c.ccode + ' has been added'       AS Message,
                        e.name                                             AS CreatedBy,
                        c.regdate                                          AS Date
                    FROM Company c
                    LEFT JOIN employees e ON e.empcode = c.sessionname
                    WHERE CAST(c.regdate AS DATE) = CAST(@Date AS DATE)

                    ORDER BY Date DESC";

            using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@Date", SqlDbType.Date).Value = date;

            using var reader = await command.ExecuteReaderAsync();

            var result = new List<dynamic>();

            while (await reader.ReadAsync())
            {
                result.Add(new
                {
                    Id = reader["Id"].ToString(),
                    Type = reader["Type"].ToString(),
                    Message = reader["Message"].ToString(),
                    CreatedBy = reader["CreatedBy"].ToString(),
                    Date = Convert.ToDateTime(reader["Date"])  // keep as DateTime
                });
            }

            return result;

        }

    }
}
