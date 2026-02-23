using System.ComponentModel.DataAnnotations;

namespace Mind_Manager.src.Domain.DTO;

// === Generic Pagination DTOs ===
public record PaginationRequest
(
    [Range(1, int.MaxValue)] int Page = 1,
    [Range(1, 100)] int Limit = 10
);

public record PagedResponse<T>
(
    IReadOnlyCollection<T> Data,
    int Total,
    int Page,
    int Limit,
    int TotalPages
) where T : class
{
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
};

// === Generic API Response Wrapper ===
public record ApiResponse<T>
(
    T? Data = default,
    bool Success = true,
    string? Message = null,
    IReadOnlyCollection<string>? Errors = null
) where T : class;

public record ApiResponse
(
    bool Success = true,
    string? Message = null,
    IReadOnlyCollection<string>? Errors = null
);
