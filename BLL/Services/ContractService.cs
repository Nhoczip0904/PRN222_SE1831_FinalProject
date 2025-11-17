using DAL.Entities;
using DAL.Repositories;
using BLL.DTOs;

namespace BLL.Services;

public interface IContractService
{
    Task<(bool Success, string Message, Contract? Contract)> CreateContractAsync(int orderId, int buyerId, int sellerId);
    Task<(bool Success, string Message, Contract? Contract)> CreateContractFromOrderAsync(int orderId);
    Task<Contract?> GetContractByIdAsync(int contractId);
    Task<Contract?> GetContractByOrderIdAsync(int orderId);
    Task<(bool Success, string Message)> BuyerConfirmAsync(int contractId, int userId, string? ipAddress);
    Task<(bool Success, string Message)> SellerConfirmAsync(int contractId, int userId, string? ipAddress);
    Task<(bool Success, string Message)> AdminApproveAsync(int contractId, int adminId, string? ipAddress);
    Task<(bool Success, string Message)> AdminRejectAsync(int contractId, int adminId, string reason, string? ipAddress);
    Task<List<Contract>> GetPendingContractsAsync();
    Task<List<Contract>> GetUserContractsAsync(int userId);
    Task<bool> IsContractApprovedAsync(int orderId);
    
    // DTO methods
    Task<List<ContractListDto>> GetPendingContractsDtoAsync();
    Task<List<ContractListDto>> GetAllContractsAsync();
    Task<List<ContractListDto>> GetUserContractsDtoAsync(int userId);
    Task<ContractDetailsDto?> GetContractDetailsByIdAsync(int contractId);
    Task<SimpleContractDto?> GetContractByOrderIdDtoAsync(int orderId);
}

public class ContractService : IContractService
{
    private readonly IContractRepository _contractRepository;
    private readonly IOrderRepository _orderRepository;

    public ContractService(IContractRepository contractRepository, IOrderRepository orderRepository)
    {
        _contractRepository = contractRepository;
        _orderRepository = orderRepository;
    }

    public async Task<(bool Success, string Message, Contract? Contract)> CreateContractAsync(int orderId, int buyerId, int sellerId)
    {
        // Check if contract already exists
        var exists = await _contractRepository.ExistsByOrderIdAsync(orderId);
        if (exists)
        {
            return (false, "Hợp đồng đã tồn tại cho đơn hàng này", null);
        }

        // Get order info
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            return (false, "Đơn hàng không tồn tại", null);
        }

        // Generate contract number
        string contractNumber = await _contractRepository.GenerateContractNumberAsync(orderId);

        var contract = new Contract
        {
            OrderId = orderId,
            BuyerId = buyerId,
            SellerId = sellerId,
            ContractNumber = contractNumber,
            TotalAmount = order.TotalAmount,
            Status = "pending"
        };

        var created = await _contractRepository.CreateAsync(contract);

        return (true, "Tạo hợp đồng thành công", created);
    }

    public async Task<(bool Success, string Message, Contract? Contract)> CreateContractFromOrderAsync(int orderId)
    {
        // Check if contract already exists
        var existing = await _contractRepository.GetByOrderIdAsync(orderId);
        if (existing != null)
        {
            return (true, "Hợp đồng đã tồn tại", existing);
        }

        // Get order with buyer and seller info
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);

        if (order == null)
        {
            return (false, "Đơn hàng không tồn tại", null);
        }

        // Generate contract number
        string contractNumber = await _contractRepository.GenerateContractNumberAsync(orderId);

        var contract = new Contract
        {
            OrderId = orderId,
            BuyerId = order.BuyerId ?? 0,
            SellerId = order.SellerId ?? 0,
            ContractNumber = contractNumber,
            TotalAmount = order.TotalAmount,
            Status = "pending"
        };

        var created = await _contractRepository.CreateAsync(contract);

        return (true, "Tạo hợp đồng thành công", created);
    }

    public async Task<Contract?> GetContractByIdAsync(int contractId)
    {
        return await _contractRepository.GetByIdAsync(contractId);
    }

    public async Task<Contract?> GetContractByOrderIdAsync(int orderId)
    {
        return await _contractRepository.GetByOrderIdAsync(orderId);
    }

    public async Task<(bool Success, string Message)> BuyerConfirmAsync(int contractId, int userId, string? ipAddress)
    {
        var contract = await GetContractByIdAsync(contractId);
        if (contract == null)
        {
            return (false, "Hợp đồng không tồn tại");
        }

        if (contract.BuyerId != userId)
        {
            return (false, "Bạn không có quyền xác nhận hợp đồng này");
        }

        if (contract.BuyerConfirmed)
        {
            return (false, "Bạn đã xác nhận hợp đồng này rồi");
        }

        contract.BuyerConfirmed = true;
        contract.BuyerConfirmedAt = DateTime.Now;

        // Check if both parties confirmed
        if (contract.SellerConfirmed)
        {
            contract.Status = "confirmed";
        }

        await _contractRepository.UpdateAsync(contract);

        // Add confirmation log
        var confirmation = new ContractConfirmation
        {
            ContractId = contractId,
            UserId = userId,
            UserRole = "buyer",
            Action = "confirmed",
            IpAddress = ipAddress
        };
        await _contractRepository.AddConfirmationAsync(confirmation);

        return (true, "Xác nhận hợp đồng thành công");
    }

    public async Task<(bool Success, string Message)> SellerConfirmAsync(int contractId, int userId, string? ipAddress)
    {
        var contract = await GetContractByIdAsync(contractId);
        if (contract == null)
        {
            return (false, "Hợp đồng không tồn tại");
        }

        if (contract.SellerId != userId)
        {
            return (false, "Bạn không có quyền xác nhận hợp đồng này");
        }

        if (contract.SellerConfirmed)
        {
            return (false, "Bạn đã xác nhận hợp đồng này rồi");
        }

        contract.SellerConfirmed = true;
        contract.SellerConfirmedAt = DateTime.Now;

        // Check if both parties confirmed
        if (contract.BuyerConfirmed)
        {
            contract.Status = "confirmed";
        }

        await _contractRepository.UpdateAsync(contract);

        // Add confirmation log
        var confirmation = new ContractConfirmation
        {
            ContractId = contractId,
            UserId = userId,
            UserRole = "seller",
            Action = "confirmed",
            IpAddress = ipAddress
        };
        await _contractRepository.AddConfirmationAsync(confirmation);

        return (true, "Xác nhận hợp đồng thành công");
    }

    public async Task<(bool Success, string Message)> AdminApproveAsync(int contractId, int adminId, string? ipAddress)
    {
        var contract = await GetContractByIdAsync(contractId);
        if (contract == null)
        {
            return (false, "Hợp đồng không tồn tại");
        }

        if (contract.Status != "confirmed")
        {
            return (false, "Hợp đồng chưa được cả 2 bên xác nhận");
        }

        if (contract.AdminApproved)
        {
            return (false, "Hợp đồng đã được duyệt rồi");
        }

        contract.AdminApproved = true;
        contract.AdminApprovedAt = DateTime.Now;
        contract.AdminApprovedBy = adminId;
        contract.Status = "approved";

        await _contractRepository.UpdateAsync(contract);

        // Add confirmation log
        var confirmation = new ContractConfirmation
        {
            ContractId = contractId,
            UserId = adminId,
            UserRole = "admin",
            Action = "approved",
            IpAddress = ipAddress
        };
        await _contractRepository.AddConfirmationAsync(confirmation);

        // Update order status to ready for delivery
        var order = await _orderRepository.GetByIdAsync(contract.OrderId);
        if (order != null)
        {
            order.Status = "confirmed"; // Ready for shipping/handover
            await _orderRepository.UpdateAsync(order);
        }

        return (true, "Duyệt hợp đồng thành công. Đơn hàng đã sẵn sàng để bàn giao xe.");
    }

    public async Task<(bool Success, string Message)> AdminRejectAsync(int contractId, int adminId, string reason, string? ipAddress)
    {
        var contract = await GetContractByIdAsync(contractId);
        if (contract == null)
        {
            return (false, "Hợp đồng không tồn tại");
        }

        contract.Status = "rejected";
        contract.RejectionReason = reason;

        await _contractRepository.UpdateAsync(contract);

        // Add confirmation log
        var confirmation = new ContractConfirmation
        {
            ContractId = contractId,
            UserId = adminId,
            UserRole = "admin",
            Action = "rejected",
            Note = reason,
            IpAddress = ipAddress
        };
        await _contractRepository.AddConfirmationAsync(confirmation);

        return (true, "Từ chối hợp đồng thành công");
    }

    public async Task<List<Contract>> GetPendingContractsAsync()
    {
        var contracts = await _contractRepository.GetPendingContractsAsync();
        return contracts.ToList();
    }

    public async Task<List<Contract>> GetUserContractsAsync(int userId)
    {
        var contracts = await _contractRepository.GetByUserIdAsync(userId);
        return contracts.ToList();
    }

    public async Task<bool> IsContractApprovedAsync(int orderId)
    {
        var contract = await _contractRepository.GetByOrderIdAsync(orderId);
        
        return contract != null && contract.Status == "approved" && contract.AdminApproved;
    }
    
    // DTO methods
    public async Task<List<ContractListDto>> GetPendingContractsDtoAsync()
    {
        var contracts = await _contractRepository.GetPendingContractsAsync();
        return contracts.Select(MapToContractListDto).ToList();
    }

    public async Task<List<ContractListDto>> GetAllContractsAsync()
    {
        var contracts = await _contractRepository.GetAllAsync();
        return contracts.Select(MapToContractListDto).ToList();
    }

    public async Task<List<ContractListDto>> GetUserContractsDtoAsync(int userId)
    {
        var contracts = await _contractRepository.GetByUserIdAsync(userId);
        return contracts.Select(MapToContractListDto).ToList();
    }
    
    public async Task<ContractDetailsDto?> GetContractDetailsByIdAsync(int contractId)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId);
        if (contract == null)
            return null;
            
        return MapToContractDetailsDto(contract);
    }
    
    public async Task<SimpleContractDto?> GetContractByOrderIdDtoAsync(int orderId)
    {
        var contract = await _contractRepository.GetByOrderIdAsync(orderId);
        if (contract == null)
            return null;
            
        return new SimpleContractDto
        {
            Id = contract.Id,
            Status = contract.Status,
            CreatedAt = contract.CreatedAt,
            UpdatedAt = contract.UpdatedAt
        };
    }
    
    private ContractListDto MapToContractListDto(Contract contract)
    {
        return new ContractListDto
        {
            Id = contract.Id,
            OrderId = contract.OrderId,
            BuyerId = contract.BuyerId,
            BuyerName = contract.Buyer?.FullName ?? "Unknown",
            SellerId = contract.SellerId,
            SellerName = contract.Seller?.FullName ?? "Unknown",
            OrderAmount = contract.TotalAmount,
            Status = contract.Status,
            CreatedAt = contract.CreatedAt,
            UpdatedAt = contract.UpdatedAt,
            BuyerConfirmed = contract.BuyerConfirmed,
            SellerConfirmed = contract.SellerConfirmed,
            AdminApproved = contract.AdminApproved
        };
    }
    
    private ContractDetailsDto MapToContractDetailsDto(Contract contract)
    {
        return new ContractDetailsDto
        {
            Id = contract.Id,
            OrderId = contract.OrderId,
            BuyerId = contract.BuyerId,
            BuyerName = contract.Buyer?.FullName ?? "Unknown",
            BuyerEmail = contract.Buyer?.Email,
            BuyerPhone = contract.Buyer?.Phone,
            BuyerAddress = contract.Buyer?.Address,
            SellerId = contract.SellerId,
            SellerName = contract.Seller?.FullName ?? "Unknown",
            SellerEmail = contract.Seller?.Email,
            SellerPhone = contract.Seller?.Phone,
            SellerAddress = contract.Seller?.Address,
            OrderAmount = contract.TotalAmount,
            Status = contract.Status,
            CreatedAt = contract.CreatedAt,
            UpdatedAt = contract.UpdatedAt,
            BuyerConfirmed = contract.BuyerConfirmed,
            SellerConfirmed = contract.SellerConfirmed,
            AdminApproved = contract.AdminApproved,
            BuyerConfirmedAt = contract.BuyerConfirmedAt,
            SellerConfirmedAt = contract.SellerConfirmedAt,
            AdminApprovedAt = contract.AdminApprovedAt,
            AdminApprovedBy = contract.AdminApprovedBy?.ToString(),
            RejectionReason = contract.RejectionReason,
            // These properties don't exist in the Contract entity, setting to null/default
            RejectedAt = null,
            RejectedBy = null,
            BuyerIpAddress = null,
            SellerIpAddress = null,
            AdminIpAddress = null
        };
    }
}
