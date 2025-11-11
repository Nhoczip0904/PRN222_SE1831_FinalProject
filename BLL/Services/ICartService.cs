using BLL.DTOs;

namespace BLL.Services;

public interface ICartService
{
    List<CartItemDto> GetCart(string sessionKey);
    (bool Success, string Message) AddToCart(string sessionKey, int productId, int quantity = 1);
    (bool Success, string Message) UpdateCartItem(string sessionKey, int productId, int quantity);
    (bool Success, string Message) RemoveFromCart(string sessionKey, int productId);
    void ClearCart(string sessionKey);
    int GetCartItemCount(string sessionKey);
    decimal GetCartTotal(string sessionKey);
}
