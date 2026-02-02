using System;
using System.Collections.Generic;

namespace Kemet_api.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalUsers { get; set; }
        public int TotalViews { get; set; }
        public int FeatureUses { get; set; }
        public double GrowthRate { get; set; }
    }

    public class GrowthTrendDto
    {
        public string Month { get; set; } = string.Empty;
        public int NewUsers { get; set; }
    }

    public class DestinationPopularityDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class FeatureUsageDto
    {
        public string FeatureName { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public double Percentage { get; set; }
    }

    public class DailyActivityDto
    {
        public string Day { get; set; } = string.Empty;
        public int ActivityCount { get; set; }
    }

    public class DashboardDataDto
    {
        public DashboardSummaryDto Summary { get; set; } = new();
        public List<GrowthTrendDto> UserGrowthTrend { get; set; } = new();
        public List<DestinationPopularityDto> DestinationPopularity { get; set; } = new();
        public List<FeatureUsageDto> FeatureUsageDistribution { get; set; } = new();
        public List<DailyActivityDto> DailyActivity { get; set; } = new();
    }
}
