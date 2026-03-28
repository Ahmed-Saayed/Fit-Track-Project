using Common;
using FitTrack_Pro.Helpers;
using FitTrack_Pro.Interfaces;
using FitTrack_Pro.Models;
using FitTrack_Pro.ViewModels;

namespace FitTrack_Pro.Services
{
    public class TrainerService(ITrainerRepository trainerRepo, IUnitOfWork uow,IAccountHelper accountHelper) : ITrainerService
    {
        private readonly IAccountHelper _accountHelper = accountHelper;
        // ────────────────────────────────────────────────────────────
        //  PAGED LIST  (with optional search)
        // ────────────────────────────────────────────────────────────
        public async Task<TrainerIndexViewModel> GetPagedTrainersAsync(
            int page, int pageSize, string? search)
        {
            IEnumerable<Trainer> trainers;
            int total;

            if (!string.IsNullOrWhiteSpace(search))
            {
                trainers = await trainerRepo.SearchAsync(search);
                total = trainers.Count();
                trainers = trainers
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);
            }
            else
            {
                total = await trainerRepo.CountAsync(t => !t.IsDeleted);
                trainers = await trainerRepo.GetPagedAsync(
                    page, pageSize, t => !t.IsDeleted);
            }

            return new TrainerIndexViewModel
            {
                Trainers = trainers.Select(MapToRow),
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                SearchQuery = search
            };
        }

        // ────────────────────────────────────────────────────────────
        //  DETAILS
        // ────────────────────────────────────────────────────────────
        public async Task<TrainerDetailsViewModel?> GetTrainerDetailsAsync(int id)
        {
            var trainer = await trainerRepo.GetWithClassesAsync(id);
            if (trainer is null) return null;

            return new TrainerDetailsViewModel
            {
                Id = trainer.Id,
                FullName = trainer.FullName,
                PhoneNumber = trainer.PhoneNumber,
                Specialty = trainer.Specialty,
                SalaryOrPercentage = trainer.SalaryOrPercentage,
                CreatedAt = trainer.CreatedAt,
                AssignedClasses = trainer.Classes?.Select(MapClass) ?? []
            };
        }

        // ────────────────────────────────────────────────────────────
        //  FORM  (for both Create and Edit)
        // ────────────────────────────────────────────────────────────
        public Task<TrainerFormViewModel> GetCreateFormAsync()
            => Task.FromResult(new TrainerFormViewModel());

        public async Task<TrainerFormViewModel?> GetEditFormAsync(int id)
        {
            var trainer = await trainerRepo.GetByIdAsync(id);
            if (trainer is null || trainer.IsDeleted) return null;

            return new TrainerFormViewModel
            {
                Id = trainer.Id,
                FullName = trainer.FullName,
                PhoneNumber = trainer.PhoneNumber,
                Specialty = trainer.Specialty,
                SalaryOrPercentage = trainer.SalaryOrPercentage
            };
        }

        // ────────────────────────────────────────────────────────────
        //  CREATE
        // ────────────────────────────────────────────────────────────
        public async Task<(bool Success, string? Error, int NewId)> CreateTrainerAsync(
            TrainerFormViewModel model)
        {
            RegisterModel registerModel = new RegisterModel
            {
                FullName = model.FullName,
                UserName  = model.UserName, 
                Password = model.Password, 
                Role = "Trainer"
            };
             string userId = await _accountHelper.RegisterUser(registerModel, signIn: false);
            if(userId == null)
                return (false, "Failed to create user account for the trainer.", 0);    
            var trainer = new Trainer
            {
                FullName = model.FullName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                Specialty = model.Specialty.Trim(),
                SalaryOrPercentage = model.SalaryOrPercentage,
                CreatedAt = DateTime.Now,
                UserId = userId
            };

            await trainerRepo.AddAsync(trainer);
            await uow.CompleteAsync();

            return (true, null, trainer.Id);
        }

        // ────────────────────────────────────────────────────────────
        //  UPDATE
        // ────────────────────────────────────────────────────────────
        public async Task<(bool Success, string? Error)> UpdateTrainerAsync(
            TrainerFormViewModel model)
        {
            var trainer = await trainerRepo.GetByIdAsync(model.Id);
            if (trainer is null || trainer.IsDeleted)
                return (false, "Trainer not found.");

            trainer.FullName = model.FullName.Trim();
            trainer.PhoneNumber = model.PhoneNumber.Trim();
            trainer.Specialty = model.Specialty.Trim();
            trainer.SalaryOrPercentage = model.SalaryOrPercentage;

            trainerRepo.Update(trainer);
            await uow.CompleteAsync();

            return (true, null);
        }

        // ────────────────────────────────────────────────────────────
        //  SOFT DELETE
        // ────────────────────────────────────────────────────────────
        public async Task<(bool Success, string? Error)> DeleteTrainerAsync(int id)
        {
            var trainer = await trainerRepo.GetByIdAsync(id);
            if (trainer is null || trainer.IsDeleted)
                return (false, "Trainer not found.");

            trainer.IsDeleted = true;
            trainerRepo.Update(trainer);
            await uow.CompleteAsync();

            return (true, null);
        }

        public async Task<TrainerDetailsViewModel?> GetTrainerProfileByUserIdAsync(string userId)
        {
            var trainer = await trainerRepo.GetByUserIdWithClassesAndAccountAsync(userId);
            if (trainer is null) return null;

            return new TrainerDetailsViewModel
            {
                Id = trainer.Id,
                FullName = trainer.FullName,
                PhoneNumber = trainer.PhoneNumber,
                Specialty = trainer.Specialty,
                SalaryOrPercentage = trainer.SalaryOrPercentage,
                Email = trainer.UserAccount?.Email ?? string.Empty,
                CreatedAt = trainer.CreatedAt,
                AssignedClasses = trainer.Classes?.Select(MapClass) ?? []
            };
        }

        // ────────────────────────────────────────────────────────────
        //  PRIVATE MAPPERS
        // ────────────────────────────────────────────────────────────
        private static TrainerRowViewModel MapToRow(Trainer t) => new()
        {
            Id = t.Id,
            FullName = t.FullName,
            PhoneNumber = t.PhoneNumber,
            Specialty = t.Specialty,
            SalaryOrPercentage = t.SalaryOrPercentage,
            ClassCount = t.Classes?.Count ?? 0,
            CreatedAt = t.CreatedAt
        };

        private static TrainerClassViewModel MapClass(GymClass c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            ScheduleTime = c.ScheduleTime,
            DurationInMinutes = c.DurationInMinutes,
            MaxCapacity = c.MaxCapacity
        };
    }
}
