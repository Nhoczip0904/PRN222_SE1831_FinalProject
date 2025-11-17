using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BLL.Helpers;
using BLL.DTOs;
using BLL.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN222_FinalProject.Pages.Staff.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IAuctionService _auctionService;
        private readonly IContractService _contractService;
        private readonly IUserService _userService;

        public IndexModel(IProductService productService, IAuctionService auctionService, IContractService contractService, IUserService userService)
        {
            _productService = productService;
            _auctionService = auctionService;
            _contractService = contractService;
            _userService = userService;
        }

        // Dashboard Statistics
        public int ProductsPendingApproval { get; set; }
        public int ProductsApproved { get; set; }
        public int ContractsPendingApproval { get; set; }
        public int ActiveAuctions { get; set; }
        public int AuctionsPendingApproval { get; set; }

        // Recent Activities
        public List<DashboardProductDto> RecentProducts { get; set; } = new();
        public List<ContractListDto> RecentContracts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is staff
            var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
            if (currentUser == null || currentUser.Role != "staff")
            {
                return RedirectToPage("/Account/Login");
            }

            await LoadDashboardData();
            return Page();
        }

        private async Task LoadDashboardData()
        {
            var now = DateTime.Now;
            
            // Get all products for statistics
            var allProducts = await _productService.GetAllProductsAsync();
            var products = allProducts.ToList();
            ProductsPendingApproval = products.Count(p => p.ApprovalStatus == "pending");
            ProductsApproved = products.Count(p => p.ApprovalStatus == "approved");

            // Get all contracts for statistics
            var allContracts = await _contractService.GetAllContractsAsync();
            var contracts = allContracts.ToList();
            ContractsPendingApproval = contracts.Count(c => c.Status == "pending");

            // Get active auctions
            var allAuctions = await _auctionService.GetAllAuctionsAsync();
            ActiveAuctions = allAuctions
                .Where(a => a.Status == "active" && a.ApprovalStatus == "approved" && a.StartTime <= now && a.EndTime >= now)
                .Count();
            AuctionsPendingApproval = allAuctions.Count(a => a.ApprovalStatus == "pending");

            // Get recent activities
            RecentProducts = await GetRecentProducts();
            RecentContracts = await GetRecentContracts();
        }


        private async Task<List<DashboardProductDto>> GetRecentProducts()
        {
            var allProducts = await _productService.GetAllProductsAsync();
            var products = allProducts.OrderByDescending(p => p.CreatedAt).Take(5).ToList();
            
            var result = new List<DashboardProductDto>();
            foreach (var product in products)
            {
                string sellerName = "Unknown";
                if (product.SellerId.HasValue)
                {
                    var seller = await _userService.GetUserByIdAsync(product.SellerId.Value);
                    sellerName = seller?.FullName ?? "Unknown";
                }
                
                result.Add(new DashboardProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    SellerName = sellerName,
                    ApprovalStatus = product.ApprovalStatus,
                    CreatedAt = product.CreatedAt
                });
            }
            return result;
        }

        private async Task<List<ContractListDto>> GetRecentContracts()
        {
            var allContracts = await _contractService.GetAllContractsAsync();
            return allContracts.OrderByDescending(c => c.CreatedAt).Take(5).ToList();
        }
    }

    // Dashboard-specific DTOs
    public class DashboardProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public string? ApprovalStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
