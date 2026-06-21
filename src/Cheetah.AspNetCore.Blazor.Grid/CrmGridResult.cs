namespace Cheetah.AspNetCore.Blazor.Grid;

public class CrmGridResult<T>
{
    public IEnumerable<T> Data { get; set; } = [];
    public int Total { get; set; }
}
