using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Repo.Interfaces;
using BookLibwithSub.Service.Interfaces;
using BookLibwithSub.Service.Models.User;

namespace BookLibwithSub.Service.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;

        public UserService(
            IUserRepository userRepository,
            ISubscriptionRepository subscriptionRepository,
            ISubscriptionPlanRepository subscriptionPlanRepository)
        {
            _userRepository = userRepository;
            _subscriptionRepository = subscriptionRepository;
            _subscriptionPlanRepository = subscriptionPlanRepository;
        }

        public async Task<List<UserProfileDto>> GetAllProfilesAsync()
        {
            var allUsers = await _userRepository.GetAllAsync();
            var result = new List<UserProfileDto>(allUsers.Count);

            foreach (var user in allUsers)
            {
                var latest = await _subscriptionRepository.GetLatestByUserAsync(user.UserID);
                CurrentSubscriptionDto? current = null;

                if (latest != null)
                {
                    var plan = await _subscriptionPlanRepository.GetByIdAsync(latest.SubscriptionPlanID);
                    current = new CurrentSubscriptionDto
                    {
                        SubscriptionId = latest.SubscriptionID,
                        PlanName = plan?.PlanName ?? "Unknown",
                        Status = latest.Status,
                        StartDate = latest.StartDate,
                        EndDate = latest.EndDate,
                        MaxPerDay = plan?.MaxPerDay ?? 0,
                        MaxPerMonth = plan?.MaxPerMonth ?? 0
                    };
                }

                result.Add(new UserProfileDto
                {
                    UserId = user.UserID,
                    Username = user.Username,
                    Role = user.Role,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    CreatedDate = user.CreatedDate,
                    CurrentSubscription = current
                });
            }

            return result;
        }

        public Task<User?> GetByIdAsync(int userId)
            => _userRepository.GetByIdAsync(userId);

        public async Task UpdateProfileAsync(int userId, UpdateMeRequest req)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                       ?? throw new Exception("User not found.");

            if (!string.IsNullOrWhiteSpace(req.FullName))
                user.FullName = req.FullName.Trim();

            if (!string.IsNullOrWhiteSpace(req.PhoneNumber))
                user.PhoneNumber = req.PhoneNumber.Trim();

            await _userRepository.UpdateAsync(user);
        }

        public async Task<UserProfileDto> GetProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                       ?? throw new Exception("User not found.");

            var latest = await _subscriptionRepository.GetLatestByUserAsync(userId);

            CurrentSubscriptionDto? current = null;
            if (latest != null)
            {
                var plan = await _subscriptionPlanRepository.GetByIdAsync(latest.SubscriptionPlanID);
                current = new CurrentSubscriptionDto
                {
                    SubscriptionId = latest.SubscriptionID,
                    PlanName = plan?.PlanName ?? "Unknown",
                    Status = latest.Status,
                    StartDate = latest.StartDate,
                    EndDate = latest.EndDate,
                    MaxPerDay = plan?.MaxPerDay ?? 0,
                    MaxPerMonth = plan?.MaxPerMonth ?? 0
                };
            }

            return new UserProfileDto
            {
                UserId = user.UserID,
                Username = user.Username,
                Role = user.Role,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedDate = user.CreatedDate,
                CurrentSubscription = current
            };
        }
    }
}
