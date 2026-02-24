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
            CreateMap<TeacherRegisterDto, Teacher>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Token, opt => opt.Ignore());
            CreateMap<Teacher, TeacherResponseDto>();

            // Student mappings
            CreateMap<StudentCreateDto, Student>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TeacherId, opt => opt.Ignore())
                .ForMember(dest => dest.Teacher, opt => opt.Ignore())
                .ForMember(dest => dest.Total, opt => opt.Ignore())
                .ForMember(dest => dest.Average, opt => opt.Ignore())
                .ForMember(dest => dest.Percentage, opt => opt.Ignore())
                .ForMember(dest => dest.PerformanceLevel, opt => opt.Ignore());
            CreateMap<StudentUpdateDto, Student>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TeacherId, opt => opt.Ignore())
                .ForMember(dest => dest.Teacher, opt => opt.Ignore())
                .ForMember(dest => dest.Total, opt => opt.Ignore())
                .ForMember(dest => dest.Average, opt => opt.Ignore())
                .ForMember(dest => dest.Percentage, opt => opt.Ignore())
                .ForMember(dest => dest.PerformanceLevel, opt => opt.Ignore());
            CreateMap<Student, StudentResponseDto>()
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total))
                .ForMember(dest => dest.Average, opt => opt.MapFrom(src => src.Average))
                .ForMember(dest => dest.Percentage, opt => opt.MapFrom(src => src.Percentage))
                .ForMember(dest => dest.PerformanceLevel, opt => opt.MapFrom(src => src.PerformanceLevel));
        }
    }
}
