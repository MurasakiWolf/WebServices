using WebService.Application.Core.Mapping;
using WebService.Domain;

namespace WebService.Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Event, Event>();
    }
}