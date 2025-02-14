namespace AutoLab.Web.Models;

public class DataTableParameters
{
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; }
    public string Country { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string Year { get; set; }
    public string LicensePlate { get; set; }
    public string VinCode { get; set; }
    public Search Search { get; set; }
    public List<Order> Order { get; set; }
    public List<Column> Columns { get; set; }
}

public class Search
{
    public string Value { get; set; }
    public bool Regex { get; set; }
}

public class Order
{
    public int Column { get; set; }
    public string Dir { get; set; }
}

public class Column
{
    public string Data { get; set; }
    public string Name { get; set; }
    public bool Searchable { get; set; }
    public bool Orderable { get; set; }
    public Search Search { get; set; }
} 