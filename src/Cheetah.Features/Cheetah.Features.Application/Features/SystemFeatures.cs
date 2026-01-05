namespace Cheetah.Features.Application.Features;

/// <summary>
/// System-wide feature flag definitions
/// </summary>
public static class SystemFeatures
{
    /// <summary>
    /// Export users to various formats (CSV, Excel, etc.)
    /// </summary>
    public const string UsersExport = "Users.Export";

    /// <summary>
    /// Advanced reporting capabilities
    /// </summary>
    public const string ReportsAdvanced = "Reports.Advanced";

    /// <summary>
    /// API access for external integrations
    /// </summary>
    public const string ApiAccess = "Api.Access";

    /// <summary>
    /// Email notifications
    /// </summary>
    public const string EmailNotifications = "Email.Notifications";

    /// <summary>
    /// SMS notifications
    /// </summary>
    public const string SmsNotifications = "Sms.Notifications";

    /// <summary>
    /// Custom branding and themes
    /// </summary>
    public const string CustomBranding = "Custom.Branding";

    /// <summary>
    /// Audit log access
    /// </summary>
    public const string AuditLog = "Audit.Log";
}
