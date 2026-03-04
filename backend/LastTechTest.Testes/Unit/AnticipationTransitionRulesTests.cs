using FluentAssertions;

using LastTechTest.Dominio;
using LastTechTest.Dominio.Authorization;
using LastTechTest.Dominio.Enums;
using LastTechTest.Dominio.Services;

namespace LastTechTest.Testes.Unit;

/// <summary>RA-3 unit tests: transition rules (production). All state × action × role combinations via AnticipationTransitionRules.</summary>
[Trait("Category", "Unit")]
public class AnticipationTransitionRulesTests
{
    // --- Valid transitions: analysis pending (Created or Pending) → Approved / Rejected / CanceledByCreator ---

    [Fact]
    public void From_Pending_AsAnalista_Approve_Should_BeAllowed()
    {
        var allowed = AnticipationTransitionRules.CanApprove(AnticipationRequestStatus.Pending, KnownRoles.Analista);
        allowed.Should().BeTrue();
        var next = AnticipationTransitionRules.GetNextState(
            AnticipationRequestStatus.Pending,
            AnticipationTransitionAction.Approve,
            KnownRoles.Analista,
            false);
        next.Should().Be(AnticipationRequestStatus.Approved);
    }

    [Fact]
    public void From_Created_AsAnalista_Approve_Should_BeAllowed()
    {
        AnticipationTransitionRules.CanApprove(AnticipationRequestStatus.Created, KnownRoles.Analista).Should().BeTrue();
        AnticipationTransitionRules.GetNextState(
            AnticipationRequestStatus.Created,
            AnticipationTransitionAction.Approve,
            KnownRoles.Analista,
            false).Should().Be(AnticipationRequestStatus.Approved);
    }

    [Fact]
    public void From_Pending_AsAdmin_Approve_Should_BeAllowed()
    {
        AnticipationTransitionRules.CanApprove(AnticipationRequestStatus.Pending, KnownRoles.Admin).Should().BeTrue();
        AnticipationTransitionRules.GetNextState(
            AnticipationRequestStatus.Pending,
            AnticipationTransitionAction.Approve,
            KnownRoles.Admin,
            false).Should().Be(AnticipationRequestStatus.Approved);
    }

    [Fact]
    public void From_Pending_AsAnalista_Reject_Should_BeAllowed()
    {
        AnticipationTransitionRules.CanReject(AnticipationRequestStatus.Pending, KnownRoles.Analista).Should().BeTrue();
        AnticipationTransitionRules.GetNextState(
            AnticipationRequestStatus.Pending,
            AnticipationTransitionAction.Reject,
            KnownRoles.Analista,
            false).Should().Be(AnticipationRequestStatus.Rejected);
    }

    [Fact]
    public void From_Pending_AsAdmin_Reject_Should_BeAllowed()
    {
        AnticipationTransitionRules.CanReject(AnticipationRequestStatus.Pending, KnownRoles.Admin).Should().BeTrue();
        AnticipationTransitionRules.GetNextState(
            AnticipationRequestStatus.Pending,
            AnticipationTransitionAction.Reject,
            KnownRoles.Admin,
            false).Should().Be(AnticipationRequestStatus.Rejected);
    }

    [Fact]
    public void From_Pending_AsCreator_Owner_Cancel_Should_BeAllowed()
    {
        AnticipationTransitionRules.CanCancel(AnticipationRequestStatus.Pending, KnownRoles.Creator, isOwner: true).Should().BeTrue();
        AnticipationTransitionRules.GetNextState(
            AnticipationRequestStatus.Pending,
            AnticipationTransitionAction.Cancel,
            KnownRoles.Creator,
            isOwner: true).Should().Be(AnticipationRequestStatus.CanceledByCreator);
    }

    [Fact]
    public void From_Pending_AsAdmin_Cancel_Should_BeAllowed()
    {
        AnticipationTransitionRules.CanCancel(AnticipationRequestStatus.Pending, KnownRoles.Admin, isOwner: false).Should().BeTrue();
        AnticipationTransitionRules.GetNextState(
            AnticipationRequestStatus.Pending,
            AnticipationTransitionAction.Cancel,
            KnownRoles.Admin,
            isOwner: false).Should().Be(AnticipationRequestStatus.CanceledByCreator);
    }

    // --- Creator cannot approve or reject ---

    [Fact]
    public void From_Pending_AsCreator_Approve_Should_BeDenied()
    {
        AnticipationTransitionRules.CanApprove(AnticipationRequestStatus.Pending, KnownRoles.Creator).Should().BeFalse();
        AnticipationTransitionRules.GetNextState(
            AnticipationRequestStatus.Pending,
            AnticipationTransitionAction.Approve,
            KnownRoles.Creator,
            true).Should().BeNull();
    }

    [Fact]
    public void From_Pending_AsCreator_Reject_Should_BeDenied()
    {
        AnticipationTransitionRules.CanReject(AnticipationRequestStatus.Pending, KnownRoles.Creator).Should().BeFalse();
        AnticipationTransitionRules.GetNextState(
            AnticipationRequestStatus.Pending,
            AnticipationTransitionAction.Reject,
            KnownRoles.Creator,
            true).Should().BeNull();
    }

    // --- Creator cannot cancel when not owner ---

    [Fact]
    public void From_Pending_AsCreator_NotOwner_Cancel_Should_BeDenied()
    {
        AnticipationTransitionRules.CanCancel(AnticipationRequestStatus.Pending, KnownRoles.Creator, isOwner: false).Should().BeFalse();
        AnticipationTransitionRules.GetNextState(
            AnticipationRequestStatus.Pending,
            AnticipationTransitionAction.Cancel,
            KnownRoles.Creator,
            isOwner: false).Should().BeNull();
    }

    // --- No transitions from final states (Approved, Rejected, CanceledByCreator) ---

    [Fact]
    public void From_Approved_Approve_Should_BeDenied()
    {
        AnticipationTransitionRules.CanApprove(AnticipationRequestStatus.Approved, KnownRoles.Analista).Should().BeFalse();
        AnticipationTransitionRules.GetNextState(
            AnticipationRequestStatus.Approved,
            AnticipationTransitionAction.Approve,
            KnownRoles.Analista,
            false).Should().BeNull();
    }

    [Fact]
    public void From_Approved_Reject_Should_BeDenied()
    {
        AnticipationTransitionRules.CanReject(AnticipationRequestStatus.Approved, KnownRoles.Analista).Should().BeFalse();
    }

    [Fact]
    public void From_Approved_Cancel_Should_BeDenied()
    {
        AnticipationTransitionRules.CanCancel(AnticipationRequestStatus.Approved, KnownRoles.Creator, true).Should().BeFalse();
    }

    [Fact]
    public void From_Rejected_Approve_Should_BeDenied()
    {
        AnticipationTransitionRules.CanApprove(AnticipationRequestStatus.Rejected, KnownRoles.Analista).Should().BeFalse();
    }

    [Fact]
    public void From_Rejected_Reject_Should_BeDenied()
    {
        AnticipationTransitionRules.CanReject(AnticipationRequestStatus.Rejected, KnownRoles.Analista).Should().BeFalse();
    }

    [Fact]
    public void From_Rejected_Cancel_Should_BeDenied()
    {
        AnticipationTransitionRules.CanCancel(AnticipationRequestStatus.Rejected, KnownRoles.Creator, true).Should().BeFalse();
    }

    [Fact]
    public void From_CanceledByCreator_Approve_Should_BeDenied()
    {
        AnticipationTransitionRules.CanApprove(AnticipationRequestStatus.CanceledByCreator, KnownRoles.Analista).Should().BeFalse();
    }

    [Fact]
    public void From_CanceledByCreator_Reject_Should_BeDenied()
    {
        AnticipationTransitionRules.CanReject(AnticipationRequestStatus.CanceledByCreator, KnownRoles.Analista).Should().BeFalse();
    }

    [Fact]
    public void From_CanceledByCreator_Cancel_Should_BeDenied()
    {
        AnticipationTransitionRules.CanCancel(AnticipationRequestStatus.CanceledByCreator, KnownRoles.Creator, true).Should().BeFalse();
    }

    // --- A→A (same-state) idempotence: IsSameStateTransition ---

    [Fact]
    public void Approve_WhenAlreadyApproved_IsSameStateTransition()
    {
        AnticipationTransitionRules.IsSameStateTransition(AnticipationRequestStatus.Approved, AnticipationTransitionAction.Approve).Should().BeTrue();
    }

    [Fact]
    public void Reject_WhenAlreadyRejected_IsSameStateTransition()
    {
        AnticipationTransitionRules.IsSameStateTransition(AnticipationRequestStatus.Rejected, AnticipationTransitionAction.Reject).Should().BeTrue();
    }

    [Fact]
    public void Cancel_WhenAlreadyCanceledByCreator_IsSameStateTransition()
    {
        AnticipationTransitionRules.IsSameStateTransition(AnticipationRequestStatus.CanceledByCreator, AnticipationTransitionAction.Cancel).Should().BeTrue();
    }

    [Fact]
    public void Approve_WhenPending_IsNotSameStateTransition()
    {
        AnticipationTransitionRules.IsSameStateTransition(AnticipationRequestStatus.Pending, AnticipationTransitionAction.Approve).Should().BeFalse();
    }

    [Fact]
    public void Cancel_WhenPending_IsNotSameStateTransition()
    {
        AnticipationTransitionRules.IsSameStateTransition(AnticipationRequestStatus.Pending, AnticipationTransitionAction.Cancel).Should().BeFalse();
    }
}