using AutoMapper;
using InvoiceManagement.Api.DTOs;
using InvoiceManagement.Api.Models;

namespace InvoiceManagement.Api.Mappings
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<Vendor, VendorResponse>().ReverseMap();
            CreateMap<VendorRequest, Vendor>().ReverseMap();

            CreateMap<User, UserResponse>().ReverseMap();
            CreateMap<UserUpdateRequest, User>().ReverseMap();
            CreateMap<AuthRegisterRequest, User>().ReverseMap();
            CreateMap<AuthUpdatePasswordRequest, User>().ReverseMap();
            CreateMap<AuthUpdateRoleRequest, User>().ReverseMap();

            CreateMap<Invoice, InvoiceResponse>().ReverseMap();
            CreateMap<Invoice, InvoiceDetailResponse>().ReverseMap();
            CreateMap<InvoiceCreateRequest, Invoice>().ReverseMap();
            CreateMap<InvoiceUpdateRequest, Invoice>().ReverseMap();
        }

    }
}
