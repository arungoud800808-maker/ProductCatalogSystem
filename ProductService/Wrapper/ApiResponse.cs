namespace ProductService.Wrappers;

public class ApiResponse<T>
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public T? Data { get; set; }

    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    public ApiResponse()
    {
    }

    public ApiResponse(bool success, string message, T? data)
    {
        Success = success;
        Message = message;
        Data = data;
        TimeStamp = DateTime.UtcNow;
    }
}