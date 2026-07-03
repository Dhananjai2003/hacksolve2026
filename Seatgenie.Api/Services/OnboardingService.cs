using Seatgenie.Api.Models;
using Seatgenie.Api.Repositories;

namespace Seatgenie.Api.Services;

public interface IOnboardingService
{
    Task<OnboardingSelection?> GetAsync(CancellationToken ct = default);
    Task<OnboardingSelection> CreateAsync(OnboardingSelectionInput input, CancellationToken ct = default);
    Task<OnboardingSelection?> UpdateAsync(OnboardingSelectionInput input, CancellationToken ct = default);
    Task<bool> SubmitAsync(CancellationToken ct = default);
}

public class OnboardingService : IOnboardingService
{
    private readonly IOnboardingRepository _onboarding;
    private readonly ICurrentUserContext _currentUser;

    public OnboardingService(IOnboardingRepository onboarding, ICurrentUserContext currentUser)
    {
        _onboarding = onboarding;
        _currentUser = currentUser;
    }

    public async Task<OnboardingSelection?> GetAsync(CancellationToken ct = default)
        => await _onboarding.GetByUserAsync(_currentUser.RequireUserId(), ct) is { } sel ? sel.ToDto() : null;

    public async Task<OnboardingSelection> CreateAsync(OnboardingSelectionInput input, CancellationToken ct = default)
    {
        var entity = input.ToEntity();
        entity.UserId = _currentUser.RequireUserId();
        var created = await _onboarding.AddAsync(entity, ct);
        return created.ToDto();
    }

    public async Task<OnboardingSelection?> UpdateAsync(OnboardingSelectionInput input, CancellationToken ct = default)
    {
        if (await _onboarding.GetByUserAsync(_currentUser.RequireUserId(), ct) is not { } existing)
        {
            return null;
        }

        // GetByUserAsync is read-only; load the tracked entity before mutating.
        if (await _onboarding.GetByIdAsync(existing.Id, ct) is not { } tracked)
        {
            return null;
        }

        input.Apply(tracked);
        await _onboarding.UpdateAsync(tracked, ct);
        return tracked.ToDto();
    }

    public Task<bool> SubmitAsync(CancellationToken ct = default)
        => _onboarding.MarkSubmittedAsync(_currentUser.RequireUserId(), ct);
}
