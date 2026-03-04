namespace LastTechTest.Aplicacao.Common.Exceptions;

/// <summary>Thrown when a requested resource (e.g. anticipation request) does not exist. API maps to 404.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}