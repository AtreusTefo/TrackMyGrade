using AutoMapper;

namespace TrackMyGradeAPI.Mapping
{
    public static class AutoMapperConfig
    {
        private static volatile IMapper _mapper;
        private static readonly object _lock = new object();

        public static IMapper Mapper
        {
            get
            {
                if (_mapper == null)
                    Initialize();
                return _mapper;
            }
        }

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
