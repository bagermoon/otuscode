namespace RestoRate.SharedKernel.Filters;

public record BaseFilter(int? PageNumber = null, int? PageSize = null, string? OrderBy = null)
{
    public int? PageNumber { get; set; } = PageNumber;
    public int? PageSize { get; set; } = PageSize;
    public string? OrderBy { get; set; } =  OrderBy;
}