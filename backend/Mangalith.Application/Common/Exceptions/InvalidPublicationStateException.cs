using Mangalith.Domain.Enums;

namespace Mangalith.Application.Common.Exceptions;

public class InvalidPublicationStateException : PublicationException
{
    public InvalidPublicationStateException(PublicationStatus currentStatus, PublicationStatus targetStatus)
        : base("INVALID_PUBLICATION_STATE", $"Cannot transition from {currentStatus} to {targetStatus}")
    {
        CurrentStatus = currentStatus;
        TargetStatus = targetStatus;
    }

    public PublicationStatus CurrentStatus { get; }
    public PublicationStatus TargetStatus { get; }
}
