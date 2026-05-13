using AutoMapper;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Mapping
{
    /// <summary>
    /// AutoMapper profile defining all entity-to-DTO mappings for the application.
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the MappingProfile class and configures entity mappings.
        /// </summary>
        public MappingProfile()
        {
            // ── Teacher ────────────────────────────────────────────────────
            // Registration removed — admin creates teachers now.
            // Login response: map Teacher → TeacherResponseDto (Token = JWT)
            CreateMap<Teacher, TeacherResponseDto>()
                .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.Token));

            // ── Student (read — teacher/admin view) ────────────────────────
            // StudentCreateDto / StudentUpdateDto mappings removed —
            // admin uses inline creation in AdminService (no AutoMapper needed).
            CreateMap<Student, StudentResponseDto>();

            // ── Student auth response (includes JWT token) ─────────────────
            CreateMap<Student, StudentAuthResponseDto>()
                .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.Token));
        }
    }
}
