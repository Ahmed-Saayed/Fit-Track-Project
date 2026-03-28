using FitTrack_Pro.Models;

namespace FitTrack_Pro.Helpers
{
    public interface IAccountHelper
    {
        Task<string?> RegisterUser(RegisterModel registerViewModel, bool signIn = true);
    }
}
