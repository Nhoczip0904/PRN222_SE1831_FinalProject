using BLL.Services;
using DAL;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using PRN222_FinalProject.Hubs;
using PRN222_FinalProject.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

// Configure DbContext
builder.Services.AddDbContext<EvBatteryTrading2Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================
// UNIT OF WORK
// ============================================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ============================================
// REPOSITORIES (Data Access Layer)
// ============================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddScoped<IBidRepository, BidRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
builder.Services.AddScoped<ISystemRevenueRepository, SystemRevenueRepository>();

// NEW - 7 Steps Features Repositories
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();

// ============================================
// SERVICES (Business Logic Layer)
// ============================================

// Core Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Product & Category Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Order & Cart Services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartService, CartService>();

// Auction Services
builder.Services.AddScoped<IAuctionService, AuctionService>();
builder.Services.AddScoped<IBidService, BidService>();
builder.Services.AddScoped<IAuctionNotificationService, AuctionNotificationService>();

// Payment Services
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IVNPayService, VNPayService>();

// Contract Service
builder.Services.AddScoped<IContractService, ContractService>();

// NEW - 7 Steps Features Services
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Revenue Service
builder.Services.AddScoped<IRevenueService, RevenueService>();

// TODO: Implement these services later
// builder.Services.AddScoped<IMessageService, MessageService>();
// builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();

// Add HttpContextAccessor for CartService
builder.Services.AddHttpContextAccessor();

// Add Memory Cache for login attempts tracking
builder.Services.AddMemoryCache();

// Configure Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1440); // 24 hours
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable Session
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

// Map SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
