// Header User Data JavaScript - Fixed for Mobile
document.addEventListener('DOMContentLoaded', () => {
    console.log('🔍 Subheader Script Loaded');

    // Detect if mobile device
    const isMobile = window.innerWidth <= 768;
    const isTouch = 'ontouchstart' in window || navigator.maxTouchPoints > 0;

    // ========================
    // Menu Toggle Functions
    // ========================

    // Avatar Menu Toggle
    const avatarToggle = document.getElementById('user-avatar-toggle');
    const userMenu = document.getElementById('user-menu');

    if (avatarToggle && userMenu) {
        // Remove existing event listeners
        const newAvatarToggle = avatarToggle.cloneNode(true);
        avatarToggle.parentNode.replaceChild(newAvatarToggle, avatarToggle);

        // Use both click and touch events for better mobile support
        const toggleUserMenu = (e) => {
            e.preventDefault();
            e.stopPropagation();

            console.log('🖱️ Avatar clicked/touched');

            const isActive = userMenu.classList.contains('active');

            // Close all menus first
            closeAllMenus();

            // Toggle current menu
            if (!isActive) {
                userMenu.classList.add('active');
                console.log('✅ User menu opened');
            } else {
                console.log('❌ User menu closed');
            }
        };

        newAvatarToggle.addEventListener('click', toggleUserMenu);
        if (isTouch) {
            newAvatarToggle.addEventListener('touchstart', toggleUserMenu, { passive: false });
        }

        console.log('✅ User menu toggle initialized');
    } else {
        console.error('❌ User menu elements not found');
    }

    // Notification Menu Toggle
    const notificationToggle = document.getElementById('notification-toggle');
    const notificationMenu = document.getElementById('notification-menu');

    if (notificationToggle && notificationMenu) {
        const toggleNotificationMenu = (e) => {
            e.preventDefault();
            e.stopPropagation();

            const isActive = notificationMenu.classList.contains('active');

            // Close all menus first
            closeAllMenus();

            // Toggle current menu
            if (!isActive) {
                notificationMenu.classList.add('active');

                // Add body overlay for mobile
                if (isMobile) {
                    addMobileOverlay('notification-menu');
                }
            }
        };

        notificationToggle.addEventListener('click', toggleNotificationMenu);
        if (isTouch) {
            notificationToggle.addEventListener('touchstart', toggleNotificationMenu, { passive: false });
        }

        console.log('✅ Notification menu toggle initialized');
    }

    // Cart Menu Toggle
    const cartToggle = document.getElementById('cart-toggle');
    const cartMenu = document.getElementById('cart-menu');

    if (cartToggle && cartMenu) {
        const toggleCartMenu = (e) => {
            e.preventDefault();
            e.stopPropagation();

            const isActive = cartMenu.classList.contains('active');

            // Close all menus first
            closeAllMenus();

            // Toggle current menu
            if (!isActive) {
                cartMenu.classList.add('active');

                // Add body overlay for mobile
                if (isMobile) {
                    addMobileOverlay('cart-menu');
                }
            }
        };

        cartToggle.addEventListener('click', toggleCartMenu);
        if (isTouch) {
            cartToggle.addEventListener('touchstart', toggleCartMenu, { passive: false });
        }

        console.log('✅ Cart menu toggle initialized');
    }

    // Close menus when clicking outside
    document.addEventListener('click', (e) => {
        // Check if click is outside all menus
        const userMenuWrapper = document.querySelector('.user-menu-wrapper');
        const notificationContainer = document.querySelector('.notification-container');
        const cartContainer = document.querySelector('.cart-container');

        if (userMenuWrapper && !userMenuWrapper.contains(e.target)) {
            closeMenu('user-menu');
        }

        if (notificationContainer && !notificationContainer.contains(e.target)) {
            closeMenu('notification-menu');
            removeMobileOverlay();
        }

        if (cartContainer && !cartContainer.contains(e.target)) {
            closeMenu('cart-menu');
            removeMobileOverlay();
        }
    });

    // Handle touch events for mobile
    if (isTouch) {
        document.addEventListener('touchstart', (e) => {
            const userMenuWrapper = document.querySelector('.user-menu-wrapper');
            const notificationContainer = document.querySelector('.notification-container');
            const cartContainer = document.querySelector('.cart-container');

            if (userMenuWrapper && !userMenuWrapper.contains(e.target)) {
                closeMenu('user-menu');
            }

            if (notificationContainer && !notificationContainer.contains(e.target)) {
                closeMenu('notification-menu');
                removeMobileOverlay();
            }

            if (cartContainer && !cartContainer.contains(e.target)) {
                closeMenu('cart-menu');
                removeMobileOverlay();
            }
        });
    }

    // Close menus on resize
    let resizeTimer;
    window.addEventListener('resize', () => {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(() => {
            closeAllMenus();
            removeMobileOverlay();
        }, 250);
    });

    // ========================
    // Notification Functions
    // ========================

    // Mark notification as read on hover (desktop) or tap (mobile)
    const notificationItems = document.querySelectorAll('.notification-item');
    notificationItems.forEach(item => {
        const markAsRead = function () {
            if (this.classList.contains('unread')) {
                const notificationId = this.getAttribute('data-id');
                markNotificationAsRead(notificationId);
                this.classList.remove('unread');
                updateNotificationBadge();
            }
        };

        if (!isTouch) {
            item.addEventListener('mouseenter', markAsRead);
        } else {
            item.addEventListener('touchstart', markAsRead, { passive: true });
        }
    });

    // ========================
    // Cart Functions
    // ========================

    // Initialize cart total on load
    updateCartTotal();

    // Prevent menu close when interacting with items
    if (notificationMenu) {
        notificationMenu.addEventListener('click', (e) => {
            e.stopPropagation();
        });
        if (isTouch) {
            notificationMenu.addEventListener('touchstart', (e) => {
                e.stopPropagation();
            }, { passive: true });
        }
    }

    if (cartMenu) {
        cartMenu.addEventListener('click', (e) => {
            e.stopPropagation();
        });
        if (isTouch) {
            cartMenu.addEventListener('touchstart', (e) => {
                e.stopPropagation();
            }, { passive: true });
        }
    }

    if (userMenu) {
        userMenu.addEventListener('click', (e) => {
            e.stopPropagation();
        });
        if (isTouch) {
            userMenu.addEventListener('touchstart', (e) => {
                e.stopPropagation();
            }, { passive: true });
        }
    }
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

function closeAllMenus() {
    closeMenu('user-menu');
    closeMenu('notification-menu');
    closeMenu('cart-menu');
    removeMobileOverlay();
}

// Add mobile overlay for bottom sheet menus
function addMobileOverlay(menuId) {
    // Remove existing overlay if any
    removeMobileOverlay();

    const overlay = document.createElement('div');
    overlay.id = 'mobile-menu-overlay';
    overlay.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0, 0, 0, 0.5);
        z-index: 1099;
        animation: fadeIn 0.3s ease;
    `;

    overlay.addEventListener('click', () => {
        closeAllMenus();
    });

    document.body.appendChild(overlay);
    document.body.style.overflow = 'hidden';
}

function removeMobileOverlay() {
    const overlay = document.getElementById('mobile-menu-overlay');
    if (overlay) {
        overlay.remove();
    }
    document.body.style.overflow = '';
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
            document.body.style.overflow = 'hidden';

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
    document.body.style.overflow = '';
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
        closeAllMenus();
    }
});

// Handle back button on mobile
window.addEventListener('popstate', () => {
    closeAllMenus();
});

console.log('✅ Subheader Script fully loaded');