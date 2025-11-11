using BLL.Helpers;
using BLL.DTOs;
using DAL.Repositories;
using Microsoft.AspNetCore.Http;

namespace BLL.Services;

public class CartService : ICartService
{
    private readonly IProductRepository _productRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IProductRepository productRepository, IHttpContextAccessor httpContextAccessor)
    {
        _productRepository = productRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public List<CartItemDto> GetCart(string sessionKey)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null) return new List<CartItemDto>();

        var cart = session.GetObjectFromJson<List<CartItemDto>>(sessionKey);
        return cart ?? new List<CartItemDto>();
    }

    public (bool Success, string Message) AddToCart(string sessionKey, int productId, int quantity = 1)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return (false, "Session không khả dụng");

        var product = _productRepository.GetByIdWithDetailsAsync(productId).Result;
        
        if (product == null)
            return (false, "Sản phẩm không tồn tại");

        if (product.IsActive == false)
            return (false, "Sản phẩm không còn khả dụng");

        if (product.IsSold == true)
            return (false, "Sản phẩm đã được bán");

        var cart = GetCart(sessionKey);
        var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

        if (existingItem != null)
        {
            var newQuantity = existingItem.Quantity + quantity;
            
            // Check stock availability
            if (newQuantity > product.Quantity)
                return (false, $"Chỉ còn {product.Quantity} sản phẩm trong kho");
            
            existingItem.Quantity = newQuantity;
        }
        else
        {
            // Check stock availability for new item
            if (quantity > product.Quantity)
                return (false, $"Chỉ còn {product.Quantity} sản phẩm trong kho");
            
            var firstImage = product.Images?.Split(',').FirstOrDefault();
            cart.Add(new CartItemDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity,
                Image = firstImage,
                SellerId = product.SellerId,
                SellerName = product.Seller?.FullName,
                IsAvailable = product.IsActive ?? false
            });
        }

        session.SetObjectAsJson(sessionKey, cart);
        return (true, "Đã thêm vào giỏ hàng");
    }

    public (bool Success, string Message) UpdateCartItem(string sessionKey, int productId, int quantity)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return (false, "Session không khả dụng");

        if (quantity <= 0)
            return RemoveFromCart(sessionKey, productId);

        var cart = GetCart(sessionKey);
        var item = cart.FirstOrDefault(c => c.ProductId == productId);

        if (item == null)
            return (false, "Sản phẩm không có trong giỏ hàng");

        // Check stock availability
        var product = _productRepository.GetByIdAsync(productId).Result;
        if (product != null && quantity > product.Quantity)
            return (false, $"Chỉ còn {product.Quantity} sản phẩm trong kho");

        item.Quantity = quantity;
        session.SetObjectAsJson(sessionKey, cart);

        return (true, "Đã cập nhật giỏ hàng");
    }

    public (bool Success, string Message) RemoveFromCart(string sessionKey, int productId)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return (false, "Session không khả dụng");

        var cart = GetCart(sessionKey);
        var item = cart.FirstOrDefault(c => c.ProductId == productId);

        if (item == null)
            return (false, "Sản phẩm không có trong giỏ hàng");

        cart.Remove(item);
        session.SetObjectAsJson(sessionKey, cart);

        return (true, "Đã xóa khỏi giỏ hàng");
    }

    public void ClearCart(string sessionKey)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.Remove(sessionKey);
    }

    public int GetCartItemCount(string sessionKey)
    {
        var cart = GetCart(sessionKey);
        return cart.Sum(c => c.Quantity);
    }

    public decimal GetCartTotal(string sessionKey)
    {
        var cart = GetCart(sessionKey);
        return cart.Sum(c => c.TotalPrice);
    }
}
