using AutoMapper;

namespace TrackMyGradeAPI.Mapping
{
    public static class AutoMapperConfig
    {
        private static IMapper _mapper;
        private static readonly object _lock = new object();

        public static IMapper Mapper
        {
            get
            {
                if (_mapper == null)
                {
                    lock (_lock)
                    {
                        if (_mapper == null)
                            Initialize();
                    }
                }
                return _mapper;
            }
        }

        public static void Initialize()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }
    }
}
