// SignalR Notification Client
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

// Start connection
async function startConnection() {
    try {
        await connection.start();
        console.log("SignalR Connected");
        
        // Join admin group if user is admin
        const userRole = document.querySelector('meta[name="user-role"]')?.content;
        if (userRole === 'admin') {
            await connection.invoke("JoinAdminGroup");
            console.log("Joined Admin Group");
        }
    } catch (err) {
        console.error("SignalR Connection Error:", err);
        setTimeout(startConnection, 5000);
    }
}

// Receive notification
connection.on("ReceiveNotification", (message, type, link) => {
    showNotification(message, type, link);
    playNotificationSound();
    updateNotificationBadge();
});

// Receive new product broadcast
connection.on("NewProductAvailable", (product) => {
    console.log("New product available:", product);

    // Show notification
    showNotification(product.message, "info", `/Products/Details?id=${product.productId}`);

    // Nếu là trang duyệt sản phẩm admin thì reload hoặc thêm vào bảng
    if (window.location.pathname.includes("/Admin/Products/Pending")) {
        // Cách 1: Reload lại trang (đơn giản, chắc chắn)
        location.reload();

        // Cách 2: (Tốt hơn) Gọi AJAX để lấy sản phẩm mới và thêm vào bảng mà không reload
        // TODO: Viết hàm addProductToAdminTable(product) để thêm sản phẩm vào bảng duyệt
        // addProductToAdminTable(product);
    }

    // Add product to homepage nếu đang ở homepage
    if (window.location.pathname === '/' || window.location.pathname === '/Index') {
        addProductToHomepage(product);
    }
});

// Receive product removed broadcast
connection.on("ProductRemoved", (data) => {
    console.log("Product removed:", data);
    
    // Remove product from page immediately
    removeProductFromPage(data.productId);
    
    // Show notification (optional - only if user is viewing that product)
    if (window.location.pathname.includes(`/Products/Details`) && 
        window.location.search.includes(`id=${data.productId}`)) {
        showNotification("Sản phẩm này đã bị gỡ bởi admin", "error", "/Products");
        setTimeout(() => {
            window.location.href = "/Products";
        }, 2000);
    }
});

// Show notification toast
function showNotification(message, type, link) {
    // Create toast element
    const toastHtml = `
        <div class="toast align-items-center text-white bg-${getBootstrapColor(type)} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="bi bi-${getIcon(type)} me-2"></i>
                    ${message}
                    ${link ? `<a href="${link}" class="text-white ms-2"><i class="bi bi-arrow-right"></i></a>` : ''}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;
    
    // Add to toast container
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'toast-container position-fixed top-0 end-0 p-3';
        container.style.zIndex = '9999';
        document.body.appendChild(container);
    }
    
    container.insertAdjacentHTML('beforeend', toastHtml);
    
    // Show toast
    const toastElement = container.lastElementChild;
    const toast = new bootstrap.Toast(toastElement, { delay: 5000 });
    toast.show();
    
    // Remove after hidden
    toastElement.addEventListener('hidden.bs.toast', () => {
        toastElement.remove();
    });
    
    // Add to notification list
    addToNotificationList(message, type, link);
}

// Get Bootstrap color class
function getBootstrapColor(type) {
    switch(type) {
        case 'success': return 'success';
        case 'error': return 'danger';
        case 'warning': return 'warning';
        case 'info': return 'info';
        default: return 'primary';
    }
}

// Get icon
function getIcon(type) {
    switch(type) {
        case 'success': return 'check-circle-fill';
        case 'error': return 'x-circle-fill';
        case 'warning': return 'exclamation-triangle-fill';
        case 'info': return 'info-circle-fill';
        default: return 'bell-fill';
    }
}

// Play notification sound
function playNotificationSound() {
    const audio = new Audio('/sounds/notification.mp3');
    audio.volume = 0.3;
    audio.play().catch(err => console.log('Sound play failed:', err));
}

// Update notification badge
function updateNotificationBadge() {
    const badge = document.getElementById('notification-badge');
    if (badge) {
        const currentCount = parseInt(badge.textContent) || 0;
        badge.textContent = currentCount + 1;
        badge.style.display = 'inline-block';
    }
}

// Add to notification list
function addToNotificationList(message, type, link) {
    const notificationList = document.getElementById('notification-list');
    if (notificationList) {
        const now = new Date();
        const timeStr = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
        
        const itemHtml = `
            <a href="${link || '#'}" class="list-group-item list-group-item-action notification-item" data-read="false">
                <div class="d-flex w-100 justify-content-between">
                    <h6 class="mb-1">
                        <i class="bi bi-${getIcon(type)} text-${getBootstrapColor(type)} me-2"></i>
                        ${message}
                    </h6>
                    <small class="text-muted">${timeStr}</small>
                </div>
            </a>
        `;
        
        notificationList.insertAdjacentHTML('afterbegin', itemHtml);
        
        // Limit to 50 notifications
        const items = notificationList.querySelectorAll('.notification-item');
        if (items.length > 50) {
            items[items.length - 1].remove();
        }
    }
}

// Clear all notifications
function clearAllNotifications() {
    const notificationList = document.getElementById('notification-list');
    if (notificationList) {
        notificationList.innerHTML = '<div class="text-center text-muted p-3">Không có thông báo</div>';
    }
    
    const badge = document.getElementById('notification-badge');
    if (badge) {
        badge.style.display = 'none';
        badge.textContent = '0';
    }
}

// Mark notification as read
function markAsRead(element) {
    element.setAttribute('data-read', 'true');
    element.classList.add('opacity-50');
    
    // Update badge count
    const unreadCount = document.querySelectorAll('.notification-item[data-read="false"]').length;
    const badge = document.getElementById('notification-badge');
    if (badge) {
        if (unreadCount > 0) {
            badge.textContent = unreadCount;
        } else {
            badge.style.display = 'none';
        }
    }
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    startConnection();
    
    // Add click handler for notification items
    document.addEventListener('click', (e) => {
        const notificationItem = e.target.closest('.notification-item');
        if (notificationItem && notificationItem.getAttribute('data-read') === 'false') {
            markAsRead(notificationItem);
        }
    });
});

// Reconnect on disconnect
connection.onreconnecting(() => {
    console.log("SignalR Reconnecting...");
});

connection.onreconnected(() => {
    console.log("SignalR Reconnected");
});

connection.onclose(() => {
    console.log("SignalR Disconnected");
    setTimeout(startConnection, 5000);
});

// Add product to homepage dynamically
function addProductToHomepage(product) {
    const productGrid = document.querySelector('.product-grid, .row.g-4, #product-list');
    
    if (!productGrid) {
        console.log('Product grid not found on this page');
        return;
    }
    
    // Create product card HTML
    const productCard = `
        <div class="col-md-4 col-lg-3 product-item" data-product-id="${product.productId}" style="animation: fadeInUp 0.5s;">
            <div class="card h-100 shadow-sm">
                <img src="${product.imageUrl}" class="card-img-top" alt="${product.productName}" style="height: 200px; object-fit: cover;">
                <div class="card-body">
                    <h5 class="card-title">${product.productName}</h5>
                    <p class="card-text text-danger fw-bold">${product.price.toLocaleString('vi-VN')} đ</p>
                    <span class="badge bg-success mb-2">
                        <i class="bi bi-star-fill"></i> MỚI
                    </span>
                    <a href="/Products/Details?id=${product.productId}" class="btn btn-primary w-100">
                        <i class="bi bi-eye"></i> Xem chi tiết
                    </a>
                </div>
            </div>
        </div>
    `;
    
    // Add to beginning of product grid with animation
    productGrid.insertAdjacentHTML('afterbegin', productCard);
    
    // Add CSS animation if not exists
    if (!document.getElementById('product-animation-style')) {
        const style = document.createElement('style');
        style.id = 'product-animation-style';
        style.textContent = `
            @keyframes fadeInUp {
                from {
                    opacity: 0;
                    transform: translateY(20px);
                }
                to {
                    opacity: 1;
                    transform: translateY(0);
                }
            }
        `;
        document.head.appendChild(style);
    }
    
    console.log('Product added to homepage:', product.productName);
}

// Remove product from page dynamically
function removeProductFromPage(productId) {
    // Find product card by data attribute or ID
    const productCard = document.querySelector(`[data-product-id="${productId}"]`);
    
    if (productCard) {
        // Add fade out animation
        productCard.style.animation = 'fadeOut 0.5s';
        
        // Remove after animation
        setTimeout(() => {
            productCard.remove();
            console.log('Product removed from page:', productId);
        }, 500);
        
        // Add CSS animation if not exists
        if (!document.getElementById('product-removal-animation')) {
            const style = document.createElement('style');
            style.id = 'product-removal-animation';
            style.textContent = `
                @keyframes fadeOut {
                    from {
                        opacity: 1;
                        transform: scale(1);
                    }
                    to {
                        opacity: 0;
                        transform: scale(0.8);
                    }
                }
            `;
            document.head.appendChild(style);
        }
    } else {
        console.log('Product card not found on this page:', productId);
    }
}
