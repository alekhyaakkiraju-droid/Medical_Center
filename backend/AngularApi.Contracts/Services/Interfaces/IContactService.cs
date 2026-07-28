using AngularApi.Contracts.DTO;

namespace AngularApi.Contracts.Services.Interfaces
{
    public interface IContactService
    {
        Task<bool> SubmitInquiryAsync(ContactInquiryDTO dto, CancellationToken cancellationToken = default);
    }
}
