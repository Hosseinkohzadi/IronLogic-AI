using Hangfire;
using IronLogic.Application.Interfaces;

namespace IronLogic.Api;

/// <summary>
/// Configures Hangfire recurring jobs for automatic email workflows.
/// </summary>
public static class EmailJobsBootstrapper
{
    /// <summary>
    /// Registers recurring email jobs.
    /// </summary>
    /// <param name="recurringJobManager">The Hangfire recurring job manager.</param>
    public static void Register(IRecurringJobManager recurringJobManager)
    {
        recurringJobManager.AddOrUpdate<IEmailAutomationService>(
            recurringJobId: "subscription-expiry-warning-email",
            methodCall: service => service.SendSubscriptionExpiryWarningsAsync(CancellationToken.None),
            cronExpression: Cron.Daily(8),
            options: new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        recurringJobManager.AddOrUpdate<IEmailAutomationService>(
            recurringJobId: "workout-reminder-email",
            methodCall: service => service.SendWorkoutRemindersAsync(CancellationToken.None),
            cronExpression: Cron.Daily(9),
            options: new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
    }
}
