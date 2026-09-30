using Brother.MPS.ServiceLevelObjectives.Entities;

namespace Brother.MPS.ServiceLevelObjectives.Rules
{
    public sealed class HealthEvaluator
    {
        private readonly HealthThresholds _thresholds;

        public HealthEvaluator(HealthThresholds? thresholds = null)
        {
            _thresholds = thresholds ?? new HealthThresholds();
        }

        public HealthResult Evaluate(JobMetrics metrics)
        {
            Validate(metrics);

            var successRate = CalculateSuccessRate(metrics);
            var retryRate = CalculateRetryRate(metrics);
            var slaCompliance = CalculateSlaCompliance(metrics);
            var backlogMultiplier = CalculateBacklogMultiplier(metrics);
            var oldestPendingSlaMultiplier = CalculateOldestPendingSlaMultiplier(metrics);

            var statuses = new List<HealthStatus>();
            var reasons = new List<string>();

            EvaluateSuccessRate(successRate, statuses, reasons);

            EvaluateRetryRate(retryRate, statuses, reasons);

            EvaluateSlaCompliance(slaCompliance, statuses, reasons);

            EvaluateBacklog(backlogMultiplier, statuses, reasons);

            EvaluateOldestPendingJob( oldestPendingSlaMultiplier, statuses, reasons);

            if (metrics.CriticalFailure)
            {
                statuses.Add(HealthStatus.Red);
                reasons.Add("A critical failure was detected.");
            }

            var overallStatus = statuses.Max();

            return new HealthResult
            {
                Status = overallStatus,
                Reasons = reasons,
                SuccessRate = successRate,
                RetryRate = retryRate,
                SlaCompliance = slaCompliance,
                BacklogMultiplier = backlogMultiplier,
                OldestPendingSlaMultiplier = oldestPendingSlaMultiplier
            };
        }

        private void EvaluateSuccessRate(decimal value, ICollection<HealthStatus> statuses, ICollection<string> reasons)
        {
            if (value < _thresholds.AmberSuccessRate)
            {
                statuses.Add(HealthStatus.Red);
                reasons.Add($"Success rate is {value:F2}%, below the red threshold of {_thresholds.AmberSuccessRate:F2}%.");
            }
            else if (value < _thresholds.GreenSuccessRate)
            {
                statuses.Add(HealthStatus.Amber);
                reasons.Add($"Success rate is {value:F2}%, below the green threshold of {_thresholds.GreenSuccessRate:F2}%.");
            }
            else
            {
                statuses.Add(HealthStatus.Green);
            }
        }

        private void EvaluateRetryRate(decimal value, ICollection<HealthStatus> statuses, ICollection<string> reasons)
        {
            if (value > _thresholds.AmberRetryRate)
            {
                statuses.Add(HealthStatus.Red);
                reasons.Add($"Retry rate is {value:F2}%, above the red threshold of {_thresholds.AmberRetryRate:F2}%.");
            }
            else if (value >= _thresholds.GreenRetryRate)
            {
                statuses.Add(HealthStatus.Amber);
                reasons.Add($"Retry rate is {value:F2}%, above the green threshold of {_thresholds.GreenRetryRate:F2}%.");
            }
            else
            {
                statuses.Add(HealthStatus.Green);
            }
        }

        private void EvaluateSlaCompliance(decimal value, ICollection<HealthStatus> statuses, ICollection<string> reasons)
        {
            if (value < _thresholds.AmberSlaCompliance)
            {
                statuses.Add(HealthStatus.Red);
                reasons.Add($"SLA compliance is {value:F2}%, below the red threshold of {_thresholds.AmberSlaCompliance:F2}%.");
            }
            else if (value < _thresholds.GreenSlaCompliance)
            {
                statuses.Add(HealthStatus.Amber);
                reasons.Add($"SLA compliance is {value:F2}%, below the green threshold of {_thresholds.GreenSlaCompliance:F2}%.");
            }
            else
            {
                statuses.Add(HealthStatus.Green);
            }
        }

        private void EvaluateBacklog(decimal value, ICollection<HealthStatus> statuses, ICollection<string> reasons)
        {
            if (value >= _thresholds.AmberBacklogMultiplier)
            {
                statuses.Add(HealthStatus.Red);
                reasons.Add($"Backlog is {value:F2}x normal.");
            }
            else if (value >= _thresholds.GreenBacklogMultiplier)
            {
                statuses.Add(HealthStatus.Amber);
                reasons.Add($"Backlog is {value:F2}x normal.");
            }
            else
            {
                statuses.Add(HealthStatus.Green);
            }
        }

        private void EvaluateOldestPendingJob(decimal value, ICollection<HealthStatus> statuses, ICollection<string> reasons)
        {
            if (value > _thresholds.AmberOldestPendingSlaMultiplier)
            {
                statuses.Add(HealthStatus.Red);
                reasons.Add($"Oldest pending job is {value:F2}x the SLA.");
            }
            else if (value > _thresholds.GreenOldestPendingSlaMultiplier)
            {
                statuses.Add(HealthStatus.Amber);
                reasons.Add($"Oldest pending job is {value:F2}x the SLA.");
            }
            else
            {
                statuses.Add(HealthStatus.Green);
            }
        }

        private static decimal CalculateSuccessRate(JobMetrics metrics)
        {
            if (metrics.TotalJobs == 0)
                return 100m;

            return (metrics.SuccessfulJobs / metrics.TotalJobs) * 100m;
        }

        private static decimal CalculateRetryRate(JobMetrics metrics)
        {
            if (metrics.TotalJobs == 0)
                return 0m;

            return (metrics.RetriedJobs / metrics.TotalJobs) * 100m;
        }

        private static decimal CalculateSlaCompliance(JobMetrics metrics)
        {
            if (metrics.TotalJobs == 0)
                return 100m;

            return (metrics.JobsCompletedWithinSla / metrics.TotalJobs) * 100m;
        }

        private static decimal CalculateBacklogMultiplier(JobMetrics metrics)
        {
            if (metrics.NormalBacklog == 0)
            {
                return metrics.PendingJobs == 0
                    ? 0m
                    : decimal.MaxValue;
            }

            return metrics.PendingJobs / (decimal)metrics.NormalBacklog;
        }

        private static decimal CalculateOldestPendingSlaMultiplier(JobMetrics metrics)
        {
            if (metrics.Sla <= TimeSpan.Zero)
                return 0m;

            return (decimal)(metrics.OldestPendingJobAge.TotalSeconds / metrics.Sla.TotalSeconds);
        }

        private static void Validate(JobMetrics metrics)
        {
            if (metrics.TotalJobs < 0)
                throw new ArgumentOutOfRangeException(nameof(metrics.TotalJobs));

            if (metrics.SuccessfulJobs < 0 ||
                metrics.FailedJobs < 0 ||
                metrics.RetriedJobs < 0 ||
                metrics.JobsCompletedWithinSla < 0 ||
                metrics.PendingJobs < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(metrics),"Job counts cannot be negative.");
            }

            if (metrics.SuccessfulJobs > metrics.TotalJobs)
                throw new ArgumentException("Successful jobs cannot exceed total jobs.");

            if (metrics.JobsCompletedWithinSla > metrics.TotalJobs)
                throw new ArgumentException("Jobs completed within SLA cannot exceed total jobs.");

            if (metrics.NormalBacklog < 0)
                throw new ArgumentOutOfRangeException(nameof(metrics.NormalBacklog));

            if (metrics.Sla <= TimeSpan.Zero)
                throw new ArgumentException("SLA must be greater than zero.");
        }
    }
}
