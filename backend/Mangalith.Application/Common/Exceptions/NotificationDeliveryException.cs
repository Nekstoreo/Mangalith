namespace Mangalith.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when notification delivery fails
/// </summary>
public class NotificationDeliveryException : AppException
{
    public NotificationDeliveryException(string notificationType, string recipient, string message) 
        : base("NOTIFICATION_DELIVERY_FAILED", message)
    {
        NotificationType = notificationType;
        Recipient = recipient;
    }

    public NotificationDeliveryException(string notificationType, string recipient, string message, Exception innerException) 
        : base("NOTIFICATION_DELIVERY_FAILED", message, innerException)
    {
        NotificationType = notificationType;
        Recipient = recipient;
    }

    public string NotificationType { get; }
    public string Recipient { get; }
}