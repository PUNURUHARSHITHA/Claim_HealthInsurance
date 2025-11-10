using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;

namespace ClaimWise.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // 🔹 Claim ↔ ClaimDto (includes DocumentMetadata)
            CreateMap<Claim, ClaimDto>()
                .ForMember(dest => dest.DocumentMetadata, opt => opt.MapFrom(src => src.DocumentMetadata))
                .ReverseMap();


            CreateMap<DocumentAccessLog, DocumentAccessLogDto>().ReverseMap();
            CreateMap<ClaimActionLog, ClaimActionLogDto>();
            // 🔹 Other mappings
            CreateMap<Agent, AgentDto>().ReverseMap();
            CreateMap<Hospital, HospitalDto>().ReverseMap();
            CreateMap<Policyholder, PolicyholderDto>().ReverseMap();
            CreateMap<Dependent, DependentDto>().ReverseMap();
            CreateMap<Treatment, TreatmentDto>().ReverseMap();
            CreateMap<PolicyType, PolicyTypeDto>().ReverseMap();
            CreateMap<EligibilityCheck, EligibilityCheckDto>().ReverseMap();
            CreateMap<Payout, PayoutDto>().ReverseMap();
            CreateMap<ClaimsReport, ClaimsReportDto>().ReverseMap();
        }
    }
}
