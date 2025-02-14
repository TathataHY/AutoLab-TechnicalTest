namespace AutoLab.Web.Models
{
    public class DataTableResponse
    {
        public int Total { get; set; }
        public List<VehicleViewModel> Items { get; set; }
    }
}