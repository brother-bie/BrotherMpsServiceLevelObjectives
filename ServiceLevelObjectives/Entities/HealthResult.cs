
namespace Brother.MPS.ServiceLevelObjectives.Entities
{
    public sealed record HealthResult
    {
        public required HealthStatus Status { get; init; }

        public required IReadOnlyList<string> Reasons { get; init; }

        public decimal SuccessRate { get; init; }

        public decimal RetryRate { get; init; }

        public decimal SlaCompliance { get; init; }

        public decimal BacklogMultiplier { get; init; }

        public decimal OldestPendingSlaMultiplier { get; init; }
    }
}
