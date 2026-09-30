
namespace Brother.MPS.ServiceLevelObjectives.Entities
{
    public sealed record HealthThresholds
    {
        // Success rate
        public decimal GreenSuccessRate { get; init; } = 99.5m;
        public decimal AmberSuccessRate { get; init; } = 98.0m;

        // Retry rate
        public decimal GreenRetryRate { get; init; } = 2.0m;
        public decimal AmberRetryRate { get; init; } = 5.0m;

        // SLA compliance
        public decimal GreenSlaCompliance { get; init; } = 99.0m;
        public decimal AmberSlaCompliance { get; init; } = 95.0m;

        // Backlog multiplier
        public decimal GreenBacklogMultiplier { get; init; } = 1.5m;
        public decimal AmberBacklogMultiplier { get; init; } = 3.0m;

        // Oldest pending job
        public decimal GreenOldestPendingSlaMultiplier { get; init; } = 1.0m;
        public decimal AmberOldestPendingSlaMultiplier { get; init; } = 2.0m;
    }
}
