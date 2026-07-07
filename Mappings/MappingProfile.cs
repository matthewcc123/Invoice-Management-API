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
            CreateMap<VendorRequest, Vendor>();

            CreateMap<User, UserResponse>().ReverseMap();
            CreateMap<UserUpdateRequest, User>();
            CreateMap<UserDeleteRequest, User>();

            CreateMap<AuthRegisterRequest, User>().ReverseMap();
            CreateMap<AuthUpdatePasswordRequest, User>();
            CreateMap<AuthUpdateRoleRequest, User>();

            CreateMap<Invoice, InvoiceResponse>().ReverseMap();
            CreateMap<Invoice, InvoiceDetailResponse>().ReverseMap();
            CreateMap<InvoiceCreateRequest, Invoice>();

            CreateMap<InvoiceUpdateRequest, Invoice>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Attachment, AttachmentResponse>().ReverseMap();
        }

    }
}
