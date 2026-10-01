namespace JobApplication.Application.Patterns;

public static class ResultMessages
{
    public const string AuthenticationRequired = "Authentication is required.";
    public const string NotificationNotFound = "Notification was not found.";
    public const string NotificationsLoaded = "Notifications loaded successfully.";
    public const string NotificationMarkedRead = "Notification marked as read.";
    public const string NotificationsMarkedRead = "All notifications marked as read.";
    public const string NotificationDeleted = "Notification deleted successfully.";
    public const string OperationCompleted = "Operation completed successfully.";
    public const string PaginatedQueryCompleted = "Results loaded successfully.";
}

public static class PaginatedResultMessages
{
    public const string NotificationsLoaded = "Notifications loaded successfully.";
    public const string Loaded = "Page loaded successfully.";
    public const string InvalidPagination = "Page must be greater than zero and page size must be between 1 and 100.";
    public const string AuthenticationRequired = "Authentication is required to load notifications.";
}
