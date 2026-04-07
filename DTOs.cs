namespace StudyHub.API.DTOs;

// Auth DTOs
public record RegisterDto(string Username, string Email, string Password);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, string Username, string Role);
public record UserProfileDto(string Username, string Email, string Role, DateTime CreatedAt);
public record ChangePasswordDto(string CurrentPassword, string NewPassword);

// Material DTOs
public record MaterialDto(
    int Id, string Title, string Description,
    string Category, decimal Price, string FileUrl, DateTime CreatedAt, bool IsArchived);

public record CreateMaterialDto(
    string Title, string Description,
    string Category, decimal Price, string FileUrl);

public record UpdateMaterialDto(
    string Title, string Description,
    string Category, decimal Price, string FileUrl);

public record MaterialsQueryDto(
    string? Search, string? Category,
    decimal? MinPrice, decimal? MaxPrice,
    bool IncludeArchived = false,
    int Page = 1, int PageSize = 10);

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);

// Order DTOs
public record OrderDto(int Id, MaterialDto Material, DateTime PurchasedAt);
