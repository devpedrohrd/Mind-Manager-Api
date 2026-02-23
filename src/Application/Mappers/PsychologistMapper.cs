using Mind_Manager.Domain.Entities;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager.Application.Mappers;

public static class PsychologistMapper
{
    public static PsychologistResponse ToResponseDto(PsychologistProfile psychologist)
    {
        return new PsychologistResponse(
            Id: psychologist.Id,
            UserId: psychologist.UserId,
            Specialty: psychologist.Specialty,
            Crp: psychologist.Crp
        );
    }

    public static IEnumerable<PsychologistResponse> ToResponseDtoList(IEnumerable<PsychologistProfile> psychologists)
    {
        return psychologists.Select(ToResponseDto);
    }
}