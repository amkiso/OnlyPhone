// Header User Data JavaScript
document.addEventListener('DOMContentLoaded', () => {
    console.log('🔍 Subheader Script Loaded');

    // ========================
    // Menu Toggle Functions
    // ========================

    // Avatar Menu Toggle
    const avatarToggle = document.getElementById('user-avatar-toggle');
    const userMenu = document.getElementById('user-menu');

    console.log('Avatar Toggle:', avatarToggle);
    console.log('User Menu:', userMenu);

    if (avatarToggle && userMenu) {
        // Remove existing event listeners
        const newAvatarToggle = avatarToggle.cloneNode(true);
        avatarToggle.parentNode.replaceChild(newAvatarToggle, avatarToggle);

        newAvatarToggle.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();

            console.log('🖱️ Avatar clicked');
            console.log('Menu classes before:', userMenu.className);

            userMenu.classList.toggle('active');

            console.log('Menu classes after:', userMenu.className);
            console.log('Has active class?', userMenu.classList.contains('active'));

            // Close other menus
            closeMenu('notification-menu');
            closeMenu('cart-menu');
        });

        console.log('✅ User menu toggle initialized');
    } else {
        console.error('❌ User menu elements not found');
    }

    // Notification Menu Toggle
    const notificationToggle = document.getElementById('notification-toggle');
    const notificationMenu = document.getElementById('notification-menu');

    if (notificationToggle && notificationMenu) {
        notificationToggle.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            notificationMenu.classList.toggle('active');
            // Close other menus
            closeMenu('user-menu');
            closeMenu('cart-menu');
        });
        console.log('✅ Notification menu toggle initialized');
    }

    // Cart Menu Toggle
    const cartToggle = document.getElementById('cart-toggle');
    const cartMenu = document.getElementById('cart-menu');

    if (cartToggle && cartMenu) {
        cartToggle.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            cartMenu.classList.toggle('active');
            // Close other menus
            closeMenu('user-menu');
            closeMenu('notification-menu');
        });
        console.log('✅ Cart menu toggle initialized');
    }

    // Close menus when clicking outside
    document.addEventListener('click', (e) => {
        // Check if click is outside user menu
        const userMenuWrapper = document.querySelector('.user-menu-wrapper');
        if (userMenuWrapper && !userMenuWrapper.contains(e.target)) {
            closeMenu('user-menu');
        }

        // Check if click is outside notification menu
        if (!e.target.closest('.notification-container')) {
            closeMenu('notification-menu');
        }

        // Check if click is outside cart menu
        if (!e.target.closest('.cart-container')) {
            closeMenu('cart-menu');
        }
    });

    // Close menus on resize
    window.addEventListener('resize', () => {
        closeMenu('user-menu');
        closeMenu('notification-menu');
        closeMenu('cart-menu');
    });

    // ========================
    // Notification Functions
    // ========================

    // Mark notification as read on hover
    const notificationItems = document.querySelectorAll('.notification-item');
    notificationItems.forEach(item => {
        item.addEventListener('mouseenter', function () {
            if (this.classList.contains('unread')) {
                const notificationId = this.getAttribute('data-id');
                markNotificationAsRead(notificationId);
                this.classList.remove('unread');
                updateNotificationBadge();
            }
        });
    });

    // ========================
    // Cart Functions
    // ========================

    // Initialize cart total on load
    updateCartTotal();
});

// ========================
// Helper Functions
// ========================

function closeMenu(menuId) {
    const menu = document.getElementById(menuId);
    if (menu && menu.classList.contains('active')) {
        console.log(`🔒 Closing menu: ${menuId}`);
        menu.classList.remove('active');
    }
}

// ========================
// Notification Functions
// ========================

function showNotificationDetail(notificationId) {
    // Get notification details via AJAX
    fetch(`/Account/GetNotificationDetail?id=${notificationId}`)
        .then(response => response.json())
        .then(data => {
            const modalContent = document.getElementById('notification-detail-content');
            modalContent.innerHTML = `
                <h2>${data.Title}</h2>
                <div style="color: #6b7280; font-size: 0.85rem; margin-bottom: 1.5rem;">
                    <i class="far fa-clock"></i> ${formatDate(data.CreatedDate)}
                </div>
                <p>${data.Content}</p>
            `;

            const overlay = document.getElementById('notification-detail-overlay');
            overlay.classList.add('active');

            // Mark as read
            markNotificationAsRead(notificationId);
        })
        .catch(error => {
            console.error('Error loading notification:', error);
            alert('Không thể tải thông báo. Vui lòng thử lại!');
        });
}

function closeNotificationDetail() {
    const overlay = document.getElementById('notification-detail-overlay');
    overlay.classList.remove('active');
}

function markNotificationAsRead(notificationId) {
    // Send AJAX request to mark notification as read
    fetch('/Account/MarkNotificationAsRead', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ notificationId: notificationId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                updateNotificationBadge();
            }
        })
        .catch(error => console.error('Error marking notification as read:', error));
}

function updateNotificationBadge() {
    // Update notification count
    const unreadItems = document.querySelectorAll('.notification-item.unread').length;
    const badge = document.querySelector('#notification-toggle .notify-count');
    const notificationCount = document.querySelector('.notification-count');

    if (unreadItems > 0) {
        if (badge) {
            badge.textContent = unreadItems > 99 ? '99+' : unreadItems;
        }
        if (notificationCount) {
            notificationCount.textContent = `${unreadItems} chưa đọc`;
        }
    } else {
        if (badge) {
            badge.remove();
        }
        if (notificationCount) {
            notificationCount.textContent = '0 chưa đọc';
        }
    }
}

// ========================
// Cart Functions
// ========================

function toggleCartItem(element) {
    const checkbox = element.querySelector('.cart-checkbox');
    if (checkbox && event.target !== checkbox) {
        checkbox.checked = !checkbox.checked;
        updateCartTotal();
    }
}

function toggleAllCartItems() {
    const selectAllCheckbox = document.getElementById('select-all-cart');
    const allCheckboxes = document.querySelectorAll('.cart-checkbox');

    allCheckboxes.forEach(checkbox => {
        checkbox.checked = selectAllCheckbox.checked;
    });

    updateCartTotal();
}

function updateCartTotal() {
    const checkedBoxes = document.querySelectorAll('.cart-checkbox:checked');
    let total = 0;

    checkedBoxes.forEach(checkbox => {
        const price = parseFloat(checkbox.getAttribute('data-price'));
        total += price;
    });

    const totalElement = document.getElementById('cart-total-price');
    if (totalElement) {
        totalElement.textContent = formatCurrency(total) + ' đ';
    }

    // Update select all checkbox state
    const selectAllCheckbox = document.getElementById('select-all-cart');
    const allCheckboxes = document.querySelectorAll('.cart-checkbox');
    if (selectAllCheckbox && allCheckboxes.length > 0) {
        selectAllCheckbox.checked = checkedBoxes.length === allCheckboxes.length;
    }
}

function proceedToCheckout() {
    const checkedBoxes = document.querySelectorAll('.cart-checkbox:checked');

    if (checkedBoxes.length === 0) {
        alert('Vui lòng chọn ít nhất một sản phẩm để thanh toán!');
        return;
    }

    // Get selected product IDs
    const selectedProducts = [];
    checkedBoxes.forEach(checkbox => {
        const cartItem = checkbox.closest('.cart-item-row');
        const productId = cartItem.getAttribute('data-id');
        selectedProducts.push(productId);
    });

    // Redirect to checkout page with selected products
    window.location.href = `/Checkout/Index?products=${selectedProducts.join(',')}`;
}

// ========================
// Utility Functions
// ========================

function formatCurrency(amount) {
    return amount.toLocaleString('vi-VN');
}

function formatDate(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diffInSeconds = Math.floor((now - date) / 1000);

    if (diffInSeconds < 60) {
        return 'Vừa xong';
    } else if (diffInSeconds < 3600) {
        const minutes = Math.floor(diffInSeconds / 60);
        return `${minutes} phút trước`;
    } else if (diffInSeconds < 86400) {
        const hours = Math.floor(diffInSeconds / 3600);
        return `${hours} giờ trước`;
    } else if (diffInSeconds < 604800) {
        const days = Math.floor(diffInSeconds / 86400);
        return `${days} ngày trước`;
    } else {
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        return `${day}/${month}/${year}`;
    }
}

// Close modal with ESC key
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        closeNotificationDetail();
    }
});

console.log('✅ Subheader Script fully loaded');