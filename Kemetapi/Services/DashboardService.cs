using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kemet_api.Data;
using Kemet_api.DTOs;
using Kemet_api.Models;
using Microsoft.EntityFrameworkCore;

namespace Kemet_api.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDataDto> GetDashboardDataAsync()
        {
            var now = DateTime.UtcNow;
            var sevenMonthsAgo = new DateTime(now.Year, now.Month, 1).AddMonths(-6);

            // 1. User Growth Trend (Last 7 Months)
            var userGrowth = await _context.Users
                .Where(u => u.CreatedAt >= sevenMonthsAgo)
                .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                .Select(g => new GrowthTrendDto
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),
                    NewUsers = g.Count()
                })
                .ToListAsync();

            // Fill missing months with 0
            var allMonths = Enumerable.Range(0, 7)
                .Select(i => sevenMonthsAgo.AddMonths(i))
                .Select(d => new GrowthTrendDto { Month = d.ToString("MMM"), NewUsers = 0 })
                .ToList();

            foreach (var month in allMonths)
            {
                var found = userGrowth.FirstOrDefault(g => g.Month == month.Month);
                if (found != null) month.NewUsers = found.NewUsers;
            }

            // 2. Destination Popularity (Based on DayActivities and UserFavorites)
            // If we have AnalyticsEvents for DestinationView, we use those. 
            // Otherwise, we use DayActivities as a proxy.
            var destinationPopularity = await _context.Destinations
                .Select(d => new DestinationPopularityDto
                {
                    Name = d.Name,
                    Count = (d.VirtualTour != null ? 10 : 0) + // Dummy weight for virtual tours
                            _context.DayActivities.Count(da => da.DestinationId == d.Id) +
                            _context.UserFavorites.Count(uf => uf.DestinationId == d.Id)
                })
                .OrderByDescending(d => d.Count)
                .Take(5)
                .ToListAsync();

            // 3. Feature Usage Distribution
            var featureUsage = await _context.AnalyticsEvents
                .Where(e => e.EventType == "FeatureUsage")
                .GroupBy(e => e.Category)
                .Select(g => new FeatureUsageDto
                {
                    FeatureName = g.Key ?? "Unknown",
                    UsageCount = g.Count()
                })
                .ToListAsync();

            // If empty, provide some default features with 0
            if (!featureUsage.Any())
            {
                featureUsage = new List<FeatureUsageDto>
                {
                    new() { FeatureName = "Chatbot", UsageCount = 0 },
                    new() { FeatureName = "VR Tours", UsageCount = 0 },
                    new() { FeatureName = "Taxi Estimator", UsageCount = 0 },
                    new() { FeatureName = "Translator", UsageCount = 0 }
                };
            }

            var totalFeatureUses = featureUsage.Sum(f => f.UsageCount);
            foreach (var f in featureUsage)
            {
                f.Percentage = totalFeatureUses > 0 ? (double)f.UsageCount / totalFeatureUses * 100 : 0;
            }

            // 4. Daily Activity (Activities per day of week)
            var dailyActivity = await _context.Trips
                .GroupBy(t => t.CreatedAt.DayOfWeek)
                .Select(g => new DailyActivityDto
                {
                    Day = g.Key.ToString().Substring(0, 3),
                    ActivityCount = g.Count()
                })
                .ToListAsync();

            var daysOfWeek = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            var fullDailyActivity = daysOfWeek.Select(d => new DailyActivityDto
            {
                Day = d,
                ActivityCount = dailyActivity.FirstOrDefault(da => da.Day == d)?.ActivityCount ?? 0
            }).ToList();

            // 5. Summary Statistics
            var totalUsers = await _context.Users.CountAsync();
            var totalViews = await _context.AnalyticsEvents.CountAsync(e => e.EventType == "DestinationView") + 
                            await _context.DayActivities.CountAsync(); // Using DayActivities as views fallback

            // Growth Rate Calculation (this month vs last month)
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            
            var thisMonthUsers = await _context.Users.CountAsync(u => u.CreatedAt >= thisMonthStart);
            var lastMonthUsers = await _context.Users.CountAsync(u => u.CreatedAt >= lastMonthStart && u.CreatedAt < thisMonthStart);
            
            double growthRate = 0;
            if (lastMonthUsers > 0)
            {
                growthRate = ((double)(thisMonthUsers - lastMonthUsers) / lastMonthUsers) * 100;
            }
            else if (thisMonthUsers > 0)
            {
                growthRate = 100;
            }

            return new DashboardDataDto
            {
                Summary = new DashboardSummaryDto
                {
                    TotalUsers = totalUsers,
                    TotalViews = totalViews,
                    FeatureUses = totalFeatureUses,
                    GrowthRate = Math.Round(growthRate, 1)
                },
                UserGrowthTrend = allMonths,
                DestinationPopularity = destinationPopularity,
                FeatureUsageDistribution = featureUsage,
                DailyActivity = fullDailyActivity
            };
        }

        public async Task TrackEventAsync(string eventType, string? category, Guid? userId = null)
        {
            var analyticsEvent = new AnalyticsEvent
            {
                Id = Guid.NewGuid(),
                EventType = eventType,
                Category = category,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyticsEvents.Add(analyticsEvent);
            await _context.SaveChangesAsync();
        }
    }
}
