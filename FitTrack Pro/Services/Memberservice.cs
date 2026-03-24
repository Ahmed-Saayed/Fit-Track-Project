using Common;
using FitTrack_Pro.Interfaces;
using FitTrack_Pro.Models;
using FitTrack_Pro.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FitTrack_Pro.Services
{
    public class MemberService(IMemberRepository memberRepo, IUnitOfWork uow) : IMemberService
    {
        // ────────────────────────────────────────────────────────────
        //  PAGED LIST  (with optional search)
        // ────────────────────────────────────────────────────────────
        public async Task<MemberIndexViewModel> GetPagedMembersAsync(
            int page, int pageSize, string? search)
        {
            IEnumerable<Member> members;
            int total;

            if (!string.IsNullOrWhiteSpace(search))
            {
                members = await memberRepo.SearchAsync(search);
                total = members.Count();
                // Apply paging manually on search results
                members = members
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);
            }
            else
            {
                total = await memberRepo.CountAsync(m => !m.IsDeleted);
                members = await memberRepo.GetPagedAsync(
                    page, pageSize, m => !m.IsDeleted);
            }

            return new MemberIndexViewModel
            {
                Members = members.Select(MapToRow),
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
        public async Task<MemberDetailsViewModel?> GetMemberDetailsAsync(int id)
        {
            var member = await memberRepo.GetWithActiveSubscriptionAsync(id);
            if (member is null) return null;

            // also load full subscription history
            var allSubs = (await uow.MemberSubscriptions
                .GetAllAsync())
                .Where(s => s.MemberId == id)
                .OrderByDescending(s => s.StartDate)
                .ToList();

            var vm = new MemberDetailsViewModel
            {
                Id = member.Id,
                FullName = member.FullName,
                PhoneNumber = member.PhoneNumber,
                BirthDate = member.BirthDate,
                Gender = member.Gender,
                Barcode = member.Barcode,
                MedicalNotes = member.MedicalNotes,
                CreatedAt = member.CreatedAt,
                ActiveSubscription = member.Subscriptions
                    .Where(s => s.IsActive && s.EndDate >= DateTime.Today)
                    .Select(MapSubscription)
                    .FirstOrDefault(),
                SubscriptionHistory = allSubs.Select(MapSubscription)
            };

            return vm;
        }

        // ────────────────────────────────────────────────────────────
        //  FORM  (for both Create and Edit)
        // ────────────────────────────────────────────────────────────
        public Task<MemberFormViewModel> GetCreateFormAsync()
            => Task.FromResult(new MemberFormViewModel());

        public async Task<MemberFormViewModel?> GetEditFormAsync(int id)
        {
            var member = await memberRepo.GetByIdAsync(id);
            if (member is null || member.IsDeleted) return null;

            return new MemberFormViewModel
            {
                Id = member.Id,
                FullName = member.FullName,
                PhoneNumber = member.PhoneNumber,
                BirthDate = member.BirthDate,
                Gender = member.Gender,
                Barcode = member.Barcode!,
                MedicalNotes = member.MedicalNotes
            };
        }

        // ────────────────────────────────────────────────────────────
        //  CREATE
        // ────────────────────────────────────────────────────────────
        public async Task<(bool Success, string? Error, int NewId)> CreateMemberAsync(
            MemberFormViewModel model)
        {
            if (await memberRepo.BarcodeExistsAsync(model.Barcode))
                return (false, "Barcode is already in use by another member.", 0);

            var member = new Member
            {
                FullName = model.FullName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                BirthDate = model.BirthDate,
                Gender = model.Gender,
                Barcode = model.Barcode.Trim(),
                MedicalNotes = model.MedicalNotes?.Trim(),
                CreatedAt = DateTime.Now
            };

            await memberRepo.AddAsync(member);
            await uow.CompleteAsync();

            return (true, null, member.Id);
        }

        // ────────────────────────────────────────────────────────────
        //  UPDATE
        // ────────────────────────────────────────────────────────────
        public async Task<(bool Success, string? Error)> UpdateMemberAsync(
            MemberFormViewModel model)
        {
            var member = await memberRepo.GetByIdAsync(model.Id);
            if (member is null || member.IsDeleted)
                return (false, "Member not found.");

            if (await memberRepo.BarcodeExistsAsync(model.Barcode, model.Id))
                return (false, "Barcode is already in use by another member.");

            member.FullName = model.FullName.Trim();
            member.PhoneNumber = model.PhoneNumber.Trim();
            member.BirthDate = model.BirthDate;
            member.Gender = model.Gender;
            member.Barcode = model.Barcode.Trim();
            member.MedicalNotes = model.MedicalNotes?.Trim();

            memberRepo.Update(member);
            await uow.CompleteAsync();

            return (true, null);
        }

        // ────────────────────────────────────────────────────────────
        //  SOFT DELETE
        // ────────────────────────────────────────────────────────────
        public async Task<(bool Success, string? Error)> DeleteMemberAsync(int id)
        {
            var member = await memberRepo.GetByIdAsync(id);
            if (member is null || member.IsDeleted)
                return (false, "Member not found.");

            member.IsDeleted = true;
            memberRepo.Update(member);
            await uow.CompleteAsync();

            return (true, null);
        }

        // ────────────────────────────────────────────────────────────
        //  DASHBOARD STATS
        // ────────────────────────────────────────────────────────────
        public async Task<MemberDashboardStatsViewModel> GetDashboardStatsAsync()
        {
            var allMembers = (await memberRepo.GetAllAsync())
                .Where(m => !m.IsDeleted)
                .ToList();

            var allSubs = (await uow.MemberSubscriptions.GetAllAsync()).ToList();

            var activeMemberIds = allSubs
                .Where(s => s.IsActive && s.EndDate >= DateTime.Today)
                .Select(s => s.MemberId)
                .Distinct()
                .ToHashSet();

            var expiringIds = allSubs
                .Where(s => s.IsActive &&
                            s.EndDate >= DateTime.Today &&
                            s.EndDate <= DateTime.Today.AddDays(7))
                .Select(s => s.MemberId)
                .Distinct()
                .ToHashSet();

            var thisMonthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            return new MemberDashboardStatsViewModel
            {
                TotalMembers = allMembers.Count,
                ActiveMembers = activeMemberIds.Count,
                ExpiredMembers = allMembers.Count(m => !activeMemberIds.Contains(m.Id)),
                ExpiringIn7Days = expiringIds.Count,
                NewThisMonth = allMembers.Count(m => m.CreatedAt >= thisMonthStart)
            };
        }

        // ────────────────────────────────────────────────────────────
        //  PRIVATE MAPPERS
        // ────────────────────────────────────────────────────────────
        private static MemberRowViewModel MapToRow(Member m)
        {
            var activeSub = m.Subscriptions?
                .FirstOrDefault(s => s.IsActive && s.EndDate >= DateTime.Today);

            var status = activeSub is not null
                ? MemberStatus.Active
                : m.Subscriptions?.Any() == true
                    ? MemberStatus.Expired
                    : MemberStatus.NeverSubscribed;

            return new MemberRowViewModel
            {
                Id = m.Id,
                FullName = m.FullName,
                PhoneNumber = m.PhoneNumber,
                Gender = m.Gender,
                Barcode = m.Barcode,
                Age = (int)((DateTime.Today - m.BirthDate).TotalDays / 365.25),
                ActivePlanName = activeSub?.SubscriptionPlan?.Name,
                SubscriptionEnd = activeSub?.EndDate,
                Status = status,
                CreatedAt = m.CreatedAt
            };
        }

        private static MemberSubscriptionViewModel MapSubscription(MemberSubscription s) => new()
        {
            Id = s.Id,
            PlanName = s.SubscriptionPlan?.Name ?? "—",
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            PaidAmount = s.PaidAmount,
            IsActive = s.IsActive
        };
    }
}