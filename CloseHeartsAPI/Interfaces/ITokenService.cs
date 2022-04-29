using CloseHeartsAPI.Entities;

namespace CloseHeartsAPI.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser appUser);
    }

}
