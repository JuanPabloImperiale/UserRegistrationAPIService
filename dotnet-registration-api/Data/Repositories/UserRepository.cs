using dotnet_registration_api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace dotnet_registration_api.Data.Repositories
{
    public class UserRepository
    {
        private readonly UserDataContext _context;
        public UserRepository(UserDataContext context)
        {
            _context = context;
        }
        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }
        public async Task<User> GetUserByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        }
        public async Task<User> GetUserByUsernameAndPassword(string username, string password)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Username == username && x.PasswordHash == password);
        }
        public async Task<User> GetUserById(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<User> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<User> UpdateUser(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task DeleteUser(int id)
        {
            var user = await GetUserById(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
