using System;

namespace WPF.Configuration
{
    /// <summary>
    /// Strong-typed wrapper for application-level settings.
    /// Values are normally loaded from appsettings.json or user secrets
    /// at start-up and registered as a singleton in DI.
    /// </summary>
    public sealed class AppSettings
    {
        public const string SectionName = "AppSettings";

        public string? ApiBaseUrl { get; init; }
        public string? DefaultUser { get; init; }
        public string? DefaultPassword { get; init; }
        public string? DefaultUserName { get; init; }
        public bool EnableDetailedErrors { get; init; } = false;
        public TimeSpan HttpTimeout { get; init; } = TimeSpan.FromSeconds(30);
    }
}