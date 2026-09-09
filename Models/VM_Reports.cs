namespace WEBLINK_CRM.Models
{
    public class VM_Reports
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }

        public string MonthName => System.Globalization.CultureInfo
            .CurrentCulture.DateTimeFormat.GetMonthName(Month);
    }
}
