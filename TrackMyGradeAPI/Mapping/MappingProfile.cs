using AutoMapper;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Teacher mappings
            CreateMap<TeacherRegisterDto, Teacher>();
            CreateMap<Teacher, TeacherResponseDto>();

            // Student mappings
            CreateMap<StudentCreateDto, Student>();
            CreateMap<StudentUpdateDto, Student>();
            CreateMap<Student, StudentResponseDto>()
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total))
                .ForMember(dest => dest.Average, opt => opt.MapFrom(src => src.Average))
                .ForMember(dest => dest.Percentage, opt => opt.MapFrom(src => src.Percentage))
                .ForMember(dest => dest.PerformanceLevel, opt => opt.MapFrom(src => src.PerformanceLevel));
        }
    }
}
