namespace LoyaltySystem.Application.Features.Users.Queries.GetStaffs;

public record GetStaffsResult(
    List<StaffDto> Items,
    int TotalRecords,
    int PageNumber,
    int PageSize,
    int TotalPages
);

public record StaffDto(
    int UserId,
    string UserName,
    string PhoneNumber,
    int TotalOrdersHandled
);