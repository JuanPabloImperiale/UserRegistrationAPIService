using dotnet_registration_api.Data;
using dotnet_registration_api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace dotnet_registration_api.Helpers
{
    public class DataHelper
    {
        private readonly UserDataContext _userDataContext;
        public DataHelper(UserDataContext userDataContext)
        {
            _userDataContext = userDataContext;
        }
        public void SeedData()
        {
            if (_userDataContext.Users.Any())
                return;
            var users = new List<User>();
            for (var i = 1; i < 50; i++)
                users.Add(new User { FirstName = "Test", LastName = $"User{i}", Username = $"test{i}", PasswordHash = HashHelper.HashPassword("test") });
            _userDataContext.Users.AddRange(users);
            _userDataContext.SaveChanges();
        }
        public async Task<User> GetUser()
        {
            return _userDataContext.Users.Where(x => x.FirstName == "Test").OrderBy(p => Guid.NewGuid()).FirstOrDefault();
        }
    }
}
