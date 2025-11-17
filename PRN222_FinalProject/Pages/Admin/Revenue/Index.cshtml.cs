using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PRN222_FinalProject.Pages.Admin.Revenue
{
    public class IndexModel : PageModel
    {
        private readonly IRevenueService _revenueService;

        public IndexModel(IRevenueService revenueService)
        {
            _revenueService = revenueService;
        }

        public RevenueDashboardDto RevenueDashboard { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommission { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public async Task OnGetAsync()
        {
            // Default to last 30 days
            if (!StartDate.HasValue)
            {
                StartDate = DateTime.Today.AddDays(-30);
            }
            if (!EndDate.HasValue)
            {
                EndDate = DateTime.Today;
            }

            var searchDto = new RevenueSearchDto
            {
                StartDate = StartDate,
                EndDate = EndDate,
                GroupBy = "day"
            };

            RevenueDashboard = await _revenueService.GetRevenueDashboardAsync(searchDto);
            TotalRevenue = RevenueDashboard.TotalRevenue;
            TotalCommission = RevenueDashboard.TotalCommission;
        }

        public async Task<IActionResult> OnPostFilterAsync(DateTime startDate, DateTime endDate, string monthFilter, int? categoryId, string groupBy)
        {
            StartDate = startDate;
            EndDate = endDate;

            // If month filter is selected, override date range
            if (!string.IsNullOrEmpty(monthFilter))
            {
                var year = int.Parse(monthFilter.Split('-')[0]);
                var month = int.Parse(monthFilter.Split('-')[1]);
                StartDate = new DateTime(year, month, 1);
                EndDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            }

            var searchDto = new RevenueSearchDto
            {
                StartDate = StartDate,
                EndDate = EndDate,
                CategoryId = categoryId,
                GroupBy = groupBy ?? "day"
            };

            RevenueDashboard = await _revenueService.GetRevenueDashboardAsync(searchDto);
            TotalRevenue = RevenueDashboard.TotalRevenue;
            TotalCommission = RevenueDashboard.TotalCommission;

            return Page();
        }
    }
}
