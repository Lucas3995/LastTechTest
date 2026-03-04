namespace LastTechTest.Dominio;

/// <summary>RA-3: Named actions for anticipation request state transitions. Use these instead of magic strings.</summary>
public static class AnticipationTransitionAction
{
    public const string Approve = "Approve";
    public const string Reject = "Reject";
    public const string Cancel = "Cancel";
}
