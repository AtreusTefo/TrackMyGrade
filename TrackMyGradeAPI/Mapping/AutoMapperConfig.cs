using AutoMapper;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Mapping
{
    /// <summary>
    /// Static configuration for AutoMapper.
    /// Handles the mapping between database entities and Data Transfer Objects (DTOs).
    /// </summary>
    public static class AutoMapperConfig
    {
        /// <summary>
        /// Gets the initialized IMapper instance.
        /// </summary>
        public static IMapper Mapper { get; private set; }

        /// <summary>
        /// Initializes the AutoMapper mapping engine.
        /// </summary>
        public static void Initialize()
        {
            var config = new MapperConfiguration(cfg =>
            {
                // Add profiles or mappings here as the API grows
                // Example: cfg.CreateMap<Student, StudentDto>();
            });
            Mapper = config.CreateMapper();
        }
    }
}