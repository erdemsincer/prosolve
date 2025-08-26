namespace ProfileService.Application.Profiles;

public sealed record ProfileDto(Guid UserId, string Email, string Role, string City, string District, int RadiusKm, string[] Skills);
