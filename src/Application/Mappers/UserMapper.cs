using Mind_Manager.Domain.Entities;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager.Application.Mappers;

public static class UserMapper
{
    public static UserResponse ToResponseDto(User user)
    {
        UserProfileResponse? profile = null;
        
        if (user.PsychologistProfile != null)
        {
            profile = new UserProfileResponse(PsychologistProfile: PsychologistMapper.ToResponseDto(user.PsychologistProfile));
        }
        else if (user.PatientProfile != null)
        {
            profile = new UserProfileResponse(PatientProfile: user.PatientProfile.ToDto());
        }

        return new UserResponse(
            Id: user.Id,
            Name: user.Name,
            Email: user.Email,
            Phone: user.Phone,
            IsActive: user.IsActive,
            Role: user.Role.ToString(),
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt,
            Profile: profile
        );
    }

    public static IEnumerable<UserResponse> ToResponseDtoList(IEnumerable<User> users)
    {
        return users.Select(ToResponseDto);
    }
}
