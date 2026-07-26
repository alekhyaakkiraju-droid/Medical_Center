using AngularApi.DTO;

namespace AngularApi.Services.Interfaces
{
    public interface IContactService
    {
        Task<bool> SubmitInquiryAsync(ContactInquiryDTO dto, CancellationToken cancellationToken = default);
    }
}
