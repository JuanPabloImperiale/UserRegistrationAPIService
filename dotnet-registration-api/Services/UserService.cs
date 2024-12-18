using dotnet_registration_api.Data.Entities;
using dotnet_registration_api.Data.Models;
using dotnet_registration_api.Data.Repositories;
using dotnet_registration_api.Helpers;
using Mapster;

namespace dotnet_registration_api.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }
         public async Task<User> GetByUsername(string username)
    {
        return await _userRepository.GetUserByUsername(username);
    }


        public async Task<List<User>> GetAll()
        {
            return await _userRepository.GetAllUsers();
        }

        public async Task<User> GetById(int id)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null) throw new NotFoundException("User not found");
            return user;
        }

        public async Task<User> Login(LoginRequest login)
        {
            // Check if the user exists
            var user = await _userRepository.GetUserByUsername(login.Username);
            if (user == null || user.PasswordHash != HashHelper.HashPassword(login.Password))
                throw new AppException("Invalid credentials");

            return user;
        }

        public async Task<User> Register(RegisterRequest register)
        {
            var existingUser = await _userRepository.GetUserByUsername(register.Username);
            if (existingUser != null)
                throw new AppException("Username already exists");

            var user = new User
            {
                FirstName = register.FirstName,
                LastName = register.LastName,
                Username = register.Username,
                PasswordHash = HashHelper.HashPassword(register.Password)
            };

            return await _userRepository.CreateUser(user);
        }

        public async Task<User> Update(int id, UpdateRequest updateRequest)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null) throw new NotFoundException("User not found");

            // Validate that at least one field is being updated
            if (string.IsNullOrEmpty(updateRequest.FirstName) &&
                string.IsNullOrEmpty(updateRequest.LastName) &&
                string.IsNullOrEmpty(updateRequest.Username) &&
                string.IsNullOrEmpty(updateRequest.NewPassword))
            {
                throw new AppException("At least one field should be updated.");
            }

            // Validate the old password before updating
            if (!string.IsNullOrEmpty(updateRequest.OldPassword) && user.PasswordHash != HashHelper.HashPassword(updateRequest.OldPassword))
            {
                throw new AppException("Old password is incorrect.");
            }

            // Check for unique username
            var existingUser = await _userRepository.GetUserByUsername(updateRequest.Username);
            if (existingUser != null && existingUser.Id != id)
            {
                throw new AppException("Username is already taken.");
            }

            // Update password if a new password is provided
            if (!string.IsNullOrEmpty(updateRequest.NewPassword))
            {
                user.PasswordHash = HashHelper.HashPassword(updateRequest.NewPassword);
            }

            // Update other fields (FirstName, LastName, Username) if provided
            if (!string.IsNullOrEmpty(updateRequest.FirstName)) user.FirstName = updateRequest.FirstName;
            if (!string.IsNullOrEmpty(updateRequest.LastName)) user.LastName = updateRequest.LastName;
            if (!string.IsNullOrEmpty(updateRequest.Username)) user.Username = updateRequest.Username;

            // Save the updated user
            return await _userRepository.UpdateUser(user);
        }

        public async Task Delete(int id)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null) throw new NotFoundException("User not found");

            await _userRepository.DeleteUser(id);
        }
    }
}