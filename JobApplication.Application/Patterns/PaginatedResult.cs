using System.Collections.Generic;

namespace JobApplication.Application.Patterns
{
    public class PaginatedResult<T>
    {
        public string? Message { get; set; }
        public bool IsSuccess { get; set; } = true;
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public PaginationMetadata Pagination { get; set; } = new PaginationMetadata();
    }
}
