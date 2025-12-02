// ========================
// COMBINED HEADER SCRIPT
// ========================

document.addEventListener('DOMContentLoaded', () => {
    console.log('ombined Header Script Loaded');

    // Detect device type
    const isMobile = window.innerWidth <= 768;
    const isTouch = 'ontouchstart' in window || navigator.maxTouchPoints > 0;

    // ========================
    // MOBILE MENU NAVIGATION
    // ========================
    const mobileMenuToggle = document.getElementById('mobileMenuToggle');
    const mobileMenuClose = document.getElementById('mobileMenuClose');
    const headerNav = document.getElementById('headerNav');
    const mobileMenuOverlay = document.getElementById('mobileMenuOverlay');

    function openMobileMenu() {
        if (headerNav && mobileMenuOverlay) {
            headerNav.classList.add('active');
            mobileMenuOverlay.classList.add('active');
            document.body.style.overflow = 'hidden';
            console.log('📱 Mobile menu opened');
        }
    }

    function closeMobileMenu() {
        if (headerNav && mobileMenuOverlay) {
            headerNav.classList.remove('active');
            mobileMenuOverlay.classList.remove('active');
            document.body.style.overflow = '';
            // Close all active submenus
            document.querySelectorAll('.menu-item.active').forEach(item => {
                item.classList.remove('active');
            });
            console.log('📱 Mobile menu closed');
        }
    }

    if (mobileMenuToggle) {
        mobileMenuToggle.addEventListener('click', openMobileMenu);
    }

    if (mobileMenuClose) {
        mobileMenuClose.addEventListener('click', closeMobileMenu);
    }

    if (mobileMenuOverlay) {
        mobileMenuOverlay.addEventListener('click', closeMobileMenu);
    }

    // ========================
    // SUBMENU TOGGLE (Mobile)
    // ========================
    document.querySelectorAll('.submenu-toggle').forEach(toggle => {
        toggle.addEventListener('click', function (e) {
            if (window.innerWidth < 992) {
                e.preventDefault();
                const menuItem = this.closest('.menu-item');

                // Close other submenus at same level
                const siblings = Array.from(menuItem.parentElement.children).filter(
                    child => child !== menuItem && child.classList.contains('menu-item')
                );
                siblings.forEach(sibling => {
                    sibling.classList.remove('active');
                });

                // Toggle current submenu
                menuItem.classList.toggle('active');
            }
        });
    });

    // ========================
    // USER MENU TOGGLE
    // ========================
    const avatarToggle = document.getElementById('user-avatar-toggle');
    const userMenu = document.getElementById('user-menu');
    const userMenuWrapper = document.querySelector('.user-menu-wrapper');

    if (avatarToggle && userMenu) {
        const toggleUserMenu = (e) => {
            e.preventDefault();
            e.stopPropagation();

            console.log('👤 Avatar clicked');

            const isActive = userMenu.classList.contains('active');

            // Close notification and cart menus
            closeMenu('notification-menu');
            closeMenu('cart-menu');
            removeMobileOverlay();

            // Toggle user menu
            if (!isActive) {
                userMenu.classList.add('active');
                if (userMenuWrapper) {
                    userMenuWrapper.classList.add('active');
                }
                console.log('✅ User menu opened');

                // Add overlay for mobile (tablet range)
                if (window.innerWidth < 992 && window.innerWidth >= 768) {
                    if (mobileMenuOverlay) {
                        mobileMenuOverlay.classList.add('active');
                    }
                    document.body.style.overflow = 'hidden';
                }
            } else {
                userMenu.classList.remove('active');
                if (userMenuWrapper) {
                    userMenuWrapper.classList.remove('active');
                }
                if (mobileMenuOverlay) {
                    mobileMenuOverlay.classList.remove('active');
                }
                document.body.style.overflow = '';
                console.log('❌ User menu closed');
            }
        };

        avatarToggle.addEventListener('click', toggleUserMenu);
        if (isTouch) {
            avatarToggle.addEventListener('touchstart', toggleUserMenu, { passive: false });
        }

        console.log('✅ User menu toggle initialized');
    } else {
        console.warn('⚠️ User menu elements not found');
    }

    // ========================
    // NOTIFICATION MENU TOGGLE
    // ========================
    const notificationToggle = document.getElementById('notification-toggle');
    const notificationMenu = document.getElementById('notification-menu');

    if (notificationToggle && notificationMenu) {
        const toggleNotificationMenu = (e) => {
            e.preventDefault();
            e.stopPropagation();

            console.log('🔔 Notification icon clicked');

            const isActive = notificationMenu.classList.contains('active');

            // Close other menus
            closeMenu('user-menu');
            closeMenu('cart-menu');
            if (userMenuWrapper) {
                userMenuWrapper.classList.remove('active');
            }

            // Toggle notification menu
            if (!isActive) {
                notificationMenu.classList.add('active');
                console.log('✅ Notification menu opened');

                // Add mobile overlay
                if (window.innerWidth <= 768) {
                    addMobileOverlay();
                }
            } else {
                notificationMenu.classList.remove('active');
                removeMobileOverlay();
                console.log('❌ Notification menu closed');
            }
        };

        notificationToggle.addEventListener('click', toggleNotificationMenu);
        if (isTouch) {
            notificationToggle.addEventListener('touchstart', toggleNotificationMenu, { passive: false });
        }

        console.log('✅ Notification menu toggle initialized');
    } else {
        console.warn('⚠️ Notification menu elements not found');
    }

    // ========================
    // CART MENU TOGGLE
    // ========================
    const cartToggle = document.getElementById('cart-toggle');
    const cartMenu = document.getElementById('cart-menu');

    if (cartToggle && cartMenu) {
        const toggleCartMenu = (e) => {
            e.preventDefault();
            e.stopPropagation();

            console.log('🛒 Cart icon clicked');

            const isActive = cartMenu.classList.contains('active');

            // Close other menus
            closeMenu('user-menu');
            closeMenu('notification-menu');
            if (userMenuWrapper) {
                userMenuWrapper.classList.remove('active');
            }
            removeMobileOverlay();

            // Toggle cart menu
            if (!isActive) {
                cartMenu.classList.add('active');
                console.log('✅ Cart menu opened');

                // Add mobile overlay
                if (window.innerWidth <= 768) {
                    addMobileOverlay();
                }
            } else {
                cartMenu.classList.remove('active');
                removeMobileOverlay();
                console.log('❌ Cart menu closed');
            }
        };

        cartToggle.addEventListener('click', toggleCartMenu);
        if (isTouch) {
            cartToggle.addEventListener('touchstart', toggleCartMenu, { passive: false });
        }

        console.log('✅ Cart menu toggle initialized');
    } else {
        console.warn('⚠️ Cart menu elements not found');
    }

    // ========================
    // CLOSE MENUS ON OUTSIDE CLICK
    // ========================
    document.addEventListener('click', (e) => {
        const userMenuWrapperEl = document.querySelector('.user-menu-wrapper');
        const notificationContainer = document.querySelector('.notification-container');
        const cartContainer = document.querySelector('.cart-container');

        // Check if click is outside user menu
        if (userMenuWrapperEl && !userMenuWrapperEl.contains(e.target)) {
            closeMenu('user-menu');
            if (userMenuWrapperEl) {
                userMenuWrapperEl.classList.remove('active');
            }
            // Only remove overlay if on mobile menu overlay range
            if (window.innerWidth < 992 && window.innerWidth >= 768) {
                if (mobileMenuOverlay && !headerNav.classList.contains('active')) {
                    mobileMenuOverlay.classList.remove('active');
                    document.body.style.overflow = '';
                }
            }
        }

        // Check if click is outside notification menu
        if (notificationContainer && !notificationContainer.contains(e.target)) {
            closeMenu('notification-menu');
            removeMobileOverlay();
        }

        // Check if click is outside cart menu
        if (cartContainer && !cartContainer.contains(e.target)) {
            closeMenu('cart-menu');
            removeMobileOverlay();
        }
    });

    // Handle touch events for mobile
    if (isTouch) {
        document.addEventListener('touchstart', (e) => {
            const userMenuWrapperEl = document.querySelector('.user-menu-wrapper');
            const notificationContainer = document.querySelector('.notification-container');
            const cartContainer = document.querySelector('.cart-container');

            if (userMenuWrapperEl && !userMenuWrapperEl.contains(e.target)) {
                closeMenu('user-menu');
                if (userMenuWrapperEl) {
                    userMenuWrapperEl.classList.remove('active');
                }
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

    // ========================
    // PREVENT MENU CLOSE ON INSIDE CLICK
    // ========================
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

    // ========================
    // WINDOW RESIZE HANDLER
    // ========================
    let resizeTimer;
    window.addEventListener('resize', () => {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(() => {
            if (window.innerWidth >= 992) {
                closeMobileMenu();
                if (userMenuWrapper) {
                    userMenuWrapper.classList.remove('active');
                }
            }
            closeAllMenus();
            removeMobileOverlay();
        }, 250);
    });

    // ========================
    // NOTIFICATION MARK AS READ
    // ========================
    const notificationItems = document.querySelectorAll('.notification-item');
    notificationItems.forEach(item => {
        const markAsRead = function () {
            if (this.classList.contains('unread')) {
                const notificationId = this.getAttribute('data-id');
                if (notificationId) {
                    markNotificationAsRead(notificationId);
                    this.classList.remove('unread');
                    this.classList.add('read');
                    updateNotificationBadge();
                }
            }
        };

        if (!isTouch) {
            item.addEventListener('mouseenter', markAsRead);
        } else {
            item.addEventListener('touchstart', markAsRead, { passive: true });
        }
    });

    // ========================
    // CART INITIALIZATION
    // ========================
    updateCartTotal();

    // ========================
    // ESC KEY & BACK BUTTON
    // ========================
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            closeNotificationDetail();
            closeAllMenus();
            closeMobileMenu();
        }
    });

    window.addEventListener('popstate', () => {
        closeAllMenus();
        closeMobileMenu();
    });

    console.log('✅ Combined Header Script fully loaded');
});

// ========================
// HELPER FUNCTIONS
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

    const userMenuWrapper = document.querySelector('.user-menu-wrapper');
    if (userMenuWrapper) {
        userMenuWrapper.classList.remove('active');
    }

    removeMobileOverlay();
}

function addMobileOverlay() {
    // Remove existing overlay if any
    removeMobileOverlay();

    const overlay = document.createElement('div');
    overlay.id = 'mobile-menu-overlay-dynamic';
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
    const overlay = document.getElementById('mobile-menu-overlay-dynamic');
    if (overlay) {
        overlay.remove();
    }

    // Only restore scroll if mobile menu is also closed
    const mobileMenuOverlay = document.getElementById('mobileMenuOverlay');
    const headerNav = document.getElementById('headerNav');
    if ((!mobileMenuOverlay || !mobileMenuOverlay.classList.contains('active')) &&
        (!headerNav || !headerNav.classList.contains('active'))) {
        document.body.style.overflow = '';
    }
}

// ========================
// NOTIFICATION FUNCTIONS
// ========================
function showNotificationDetail(notificationId) {
    console.log('📖 Opening notification detail:', notificationId);

    fetch(`/Account/GetNotificationDetail?id=${notificationId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            const modalContent = document.getElementById('notification-detail-content');
            if (modalContent) {
                modalContent.innerHTML = `
                    <h2>${data.Title || 'Thông báo'}</h2>
                    <div style="color: #6b7280; font-size: 0.85rem; margin-bottom: 1.5rem;">
                        <i class="far fa-clock"></i> ${formatDate(data.CreatedDate)}
                    </div>
                    <p>${data.Content || ''}</p>
                `;
            }

            const overlay = document.getElementById('notification-detail-overlay');
            if (overlay) {
                overlay.classList.add('active');
                document.body.style.overflow = 'hidden';
            }

            markNotificationAsRead(notificationId);
        })
        .catch(error => {
            console.error('Error loading notification:', error);
            alert('Không thể tải thông báo. Vui lòng thử lại!');
        });
}

function closeNotificationDetail() {
    const overlay = document.getElementById('notification-detail-overlay');
    if (overlay) {
        overlay.classList.remove('active');

        // Check if any menu is open
        const isAnyMenuOpen =
            document.getElementById('notification-menu')?.classList.contains('active') ||
            document.getElementById('cart-menu')?.classList.contains('active') ||
            document.getElementById('user-menu')?.classList.contains('active') ||
            document.getElementById('headerNav')?.classList.contains('active');

        if (!isAnyMenuOpen) {
            document.body.style.overflow = '';
        }
    }
}

function markNotificationAsRead(notificationId) {
    if (!notificationId) return;

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
    const unreadItems = document.querySelectorAll('.notification-item.unread').length;
    const badge = document.querySelector('#notification-toggle .notify-count');
    const notificationCount = document.querySelector('.notification-count');

    console.log(`🔔 Updating badge: ${unreadItems} unread`);

    if (unreadItems > 0) {
        if (badge) {
            badge.textContent = unreadItems > 99 ? '99+' : unreadItems;
            badge.style.display = '';
        }
        if (notificationCount) {
            notificationCount.textContent = `${unreadItems} chưa đọc`;
        }
    } else {
        if (badge) {
            badge.style.display = 'none';
        }
        if (notificationCount) {
            notificationCount.textContent = '0 chưa đọc';
        }
    }
}

// ========================
// CART FUNCTIONS
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

    if (selectAllCheckbox) {
        allCheckboxes.forEach(checkbox => {
            checkbox.checked = selectAllCheckbox.checked;
        });

        updateCartTotal();
    }
}

function updateCartTotal() {
    const checkedBoxes = document.querySelectorAll('.cart-checkbox:checked');
    let total = 0;

    checkedBoxes.forEach(checkbox => {
        const price = parseFloat(checkbox.getAttribute('data-price'));
        if (!isNaN(price)) {
            total += price;
        }
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

    console.log(`🛒 Cart total updated: ${formatCurrency(total)} đ`);
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
        if (cartItem) {
            const productId = cartItem.getAttribute('data-id');
            if (productId) {
                selectedProducts.push(productId);
            }
        }
    });

    // Redirect to checkout page
    if (selectedProducts.length > 0) {
        console.log('💳 Proceeding to checkout with products:', selectedProducts);
        window.location.href = `/Checkout/Index?products=${selectedProducts.join(',')}`;
    } else {
        alert('Không thể lấy thông tin sản phẩm. Vui lòng thử lại!');
    }
}

// ========================
// UTILITY FUNCTIONS
// ========================
function formatCurrency(amount) {
    return amount.toLocaleString('vi-VN');
}

function formatDate(dateString) {
    if (!dateString) return '';

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