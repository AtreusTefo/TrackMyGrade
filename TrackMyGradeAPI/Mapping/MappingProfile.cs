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
                .ForMember(dest => dest.Teacher, opt => opt.Ignore());

            CreateMap<StudentUpdateDto, Student>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TeacherId, opt => opt.Ignore())
                .ForMember(dest => dest.Teacher, opt => opt.Ignore());

            // Total, Average, Percentage, PerformanceLevel are computed properties on Student
            // with matching names on StudentResponseDto — AutoMapper convention maps them automatically.
            CreateMap<Student, StudentResponseDto>();
        }
    }
}
