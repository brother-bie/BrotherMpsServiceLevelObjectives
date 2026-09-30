
namespace Brother.MPS.ServiceLevelObjectives.Entities
{
    public sealed record JobMetrics
    {
        public required string CountryCode { get; init; }

        public required ServiceType Service { get; init; }

        public decimal TotalJobs { get; init; }

        public decimal SuccessfulJobs { get; init; }

        public decimal FailedJobs { get; init; }

        public decimal RetriedJobs { get; init; }

        /// <summary>
        /// Number of completed jobs that completed within the agreed SLA.
        /// </summary>
        public decimal JobsCompletedWithinSla { get; init; }

        /// <summary>
        /// Number of jobs currently waiting to be processed.
        /// </summary>
        public decimal PendingJobs { get; init; }

        /// <summary>
        /// Age of the oldest currently pending job.
        /// </summary>
        public TimeSpan OldestPendingJobAge { get; init; }

        /// <summary>
        /// The SLA for this service/country combination.
        /// </summary>
        public TimeSpan Sla { get; init; }

        /// <summary>
        /// Whether a critical business/technical failure occurred.
        /// </summary>
        public bool CriticalFailure { get; init; }

        /// <summary>
        /// Normal expected backlog for this country/service.
        /// </summary>
        public decimal NormalBacklog { get; init; }
    }
}
