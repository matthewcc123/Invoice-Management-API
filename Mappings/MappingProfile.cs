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
        }

    }
}
