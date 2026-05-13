using AutoMapper;

namespace TrackMyGradeAPI.Mapping
{
    /// <summary>
    /// Static configuration class for AutoMapper initialization and lazy-loaded singleton mapper instance.
    /// </summary>
    public static class AutoMapperConfig
    {
        private static volatile IMapper _mapper;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the lazy-loaded singleton IMapper instance. Initializes on first access.
        /// </summary>
        public static IMapper Mapper
        {
            get
            {
                if (_mapper == null)
                    Initialize();
                return _mapper;
            }
        }

        /// <summary>
        /// Initializes the AutoMapper configuration and creates the mapper instance.
        /// Thread-safe using double-checked locking pattern.
        /// </summary>
        public static void Initialize()
        {
            if (_mapper != null) return;

            lock (_lock)
            {
                if (_mapper != null) return;

                var config = new MapperConfiguration(cfg =>
                    cfg.AddMaps(typeof(MappingProfile).Assembly));

                config.AssertConfigurationIsValid();
                config.CompileMappings();
                _mapper = config.CreateMapper();
            }
        }
    }
}
