// server/Mapping/MappingProfile.cs
using AutoMapper;
using server.Domain.Entities;
using server.Presentation.DTOs;

namespace server.Presentation.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User -> UserResponse
            CreateMap<User, UserResponse>();

            // RegisterRequest -> User
            CreateMap<RegisterRequest, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) 
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}