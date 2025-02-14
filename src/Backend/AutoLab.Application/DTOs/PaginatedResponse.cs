namespace AutoLab.Application.DTOs
{
    public class PaginatedResponse<T>
    {
        public IEnumerable<T> Items { get; }
        public int Total { get; }

        public PaginatedResponse(IEnumerable<T> items, int total)
        {
            Items = items;
            Total = total;
        }
    }
} 