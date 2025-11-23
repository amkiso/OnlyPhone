// ========================
// INDEX PAGE SCRIPT - FETCH API VERSION
// ========================

// Hàm thêm sản phẩm vào giỏ hàng
function addToCart(productId) {
    // Validate productId
    if (!productId || productId <= 0) {
        showNotification('error', 'Sản phẩm không hợp lệ');
        return;
    }

    // Lấy button từ event (nếu có)
    const button = event?.target?.closest('.btn-add-cart');
    let originalHTML = '';

    if (button) {
        originalHTML = button.innerHTML;
        button.disabled = true;
        button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang thêm...';
    }

    console.log('🛒 Adding product to cart:', productId);

    fetch('/Product/AddToCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        body: JSON.stringify({
            productId: productId,
            quantity: 1
        })
    })
        .then(response => {
            console.log('📡 Response status:', response.status);

            // Kiểm tra response status
            if (!response.ok) {
                if (response.status === 401 || response.status === 403) {
                    throw new Error('UNAUTHORIZED');
                }
                throw new Error(`HTTP ${response.status}`);
            }

            return response.json();
        })
        .then(data => {
            console.log('✅ Server response:', data);

            if (data.success) {
                // Hiển thị thông báo thành công
                showNotification('success', data.message || 'Đã thêm sản phẩm vào giỏ hàng');

                // Cập nhật số lượng giỏ hàng
                if (data.cartCount !== undefined) {
                    updateCartBadge(data.cartCount);
                } else {
                    updateCartBadge(); // Tự động fetch số lượng
                }

                // Animation success cho button
                if (button) {
                    button.classList.add('btn-success-flash');
                    setTimeout(() => {
                        button.classList.remove('btn-success-flash');
                    }, 1000);
                }
            } else {
                // Kiểm tra nếu cần đăng nhập
                if (data.requireLogin) {
                    const message = data.message || 'Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng';

                    if (confirm(`${message}\n\nChuyển đến trang đăng nhập?`)) {
                        const returnUrl = encodeURIComponent(window.location.pathname);
                        window.location.href = `/Account/Login?returnUrl=${returnUrl}`;
                    } else {
                        showNotification('warning', message);
                    }
                } else {
                    // Hiển thị thông báo lỗi
                    showNotification('error', data.message || 'Không thể thêm sản phẩm vào giỏ hàng');
                }
            }
        })
        .catch(error => {
            console.error('❌ Error:', error);

            // Xử lý các loại lỗi
            if (error.message === 'UNAUTHORIZED') {
                if (confirm('Vui lòng đăng nhập để tiếp tục.\n\nChuyển đến trang đăng nhập?')) {
                    const returnUrl = encodeURIComponent(window.location.pathname);
                    window.location.href = `/Account/Login?returnUrl=${returnUrl}`;
                }
            } else if (error.message.includes('HTTP')) {
                showNotification('error', 'Lỗi server. Vui lòng thử lại sau!');
            } else {
                showNotification('error', 'Có lỗi xảy ra. Vui lòng thử lại!');
            }
        })
        .finally(() => {
            // Restore button state
            if (button) {
                button.disabled = false;
                button.innerHTML = originalHTML;
            }
        });
}

// Hàm cập nhật badge giỏ hàng
function updateCartBadge(cartCount) {
    console.log('🔢 Updating cart badge');

    // Nếu có cartCount từ server
    if (cartCount !== undefined) {
        updateCartCount(cartCount);
        return;
    }

    // Nếu không có, fetch từ server
    fetch('/Product/GetCartCount', {
        method: 'GET',
        headers: {
            'Accept': 'application/json'
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success && data.cartCount !== undefined) {
                updateCartCount(data.cartCount);
            }
        })
        .catch(error => {
            console.error('❌ Error fetching cart count:', error);
        });
}

// Hàm cập nhật số lượng giỏ hàng
function updateCartCount(count) {
    console.log('🔢 Setting cart count to:', count);

    // Cập nhật tất cả cart count badges
    const cartCountElements = document.querySelectorAll('.cart-count');
    cartCountElements.forEach(element => {
        element.textContent = count > 99 ? '99+' : count;

        // Hiển thị/ẩn badge
        if (count > 0) {
            element.style.display = '';
        } else {
            element.style.display = 'none';
        }
    });

    // Cập nhật cart count text
    const cartCountText = document.querySelector('.cart-count-text');
    if (cartCountText) {
        cartCountText.textContent = `${count} sản phẩm`;
    }

    // Pulse animation
    cartCountElements.forEach(element => {
        element.classList.remove('cart-pulse');
        void element.offsetWidth; // Force reflow
        element.classList.add('cart-pulse');
    });
}

// Hàm hiển thị thông báo
function showNotification(type, message) {
    console.log(`📢 Notification [${type}]:`, message);

    // Kiểm tra xem có thư viện Toastr không
    if (typeof toastr !== 'undefined') {
        toastr.options = {
            closeButton: true,
            progressBar: true,
            positionClass: 'toast-top-right',
            timeOut: 3000,
            showMethod: 'fadeIn',
            hideMethod: 'fadeOut'
        };

        toastr[type](message);
        return;
    }

    // Fallback: Custom notification
    showCustomNotification(type, message);
}

// Custom notification (không cần thư viện)
function showCustomNotification(type, message) {
    // Xóa notification cũ
    const oldNotification = document.querySelector('.custom-notification');
    if (oldNotification) {
        oldNotification.remove();
    }

    // Icon mapping
    const icons = {
        success: '<i class="fas fa-check-circle"></i>',
        error: '<i class="fas fa-times-circle"></i>',
        warning: '<i class="fas fa-exclamation-triangle"></i>',
        info: '<i class="fas fa-info-circle"></i>'
    };

    // Tạo notification
    const notification = document.createElement('div');
    notification.className = `custom-notification custom-notification-${type}`;
    notification.innerHTML = `
        <div class="custom-notification-content">
            <span class="custom-notification-icon">${icons[type] || icons.info}</span>
            <span class="custom-notification-message">${message}</span>
            <button class="custom-notification-close" onclick="this.parentElement.parentElement.remove()">
                <i class="fas fa-times"></i>
            </button>
        </div>
    `;

    // Thêm styles nếu chưa có
    if (!document.getElementById('custom-notification-styles')) {
        addNotificationStyles();
    }

    // Thêm vào body
    document.body.appendChild(notification);

    // Auto remove
    setTimeout(() => {
        if (notification.parentElement) {
            notification.style.opacity = '0';
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => notification.remove(), 300);
        }
    }, 3000);
}

// Thêm styles cho notification
function addNotificationStyles() {
    const style = document.createElement('style');
    style.id = 'custom-notification-styles';
    style.textContent = `
        .custom-notification {
            position: fixed;
            top: 20px;
            right: 20px;
            min-width: 300px;
            max-width: 400px;
            background: white;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
            z-index: 10000;
            opacity: 1;
            transform: translateX(0);
            transition: all 0.3s ease;
        }
        
        .custom-notification-success {
            border-left: 4px solid #10b981;
        }
        
        .custom-notification-error {
            border-left: 4px solid #ef4444;
        }
        
        .custom-notification-warning {
            border-left: 4px solid #f59e0b;
        }
        
        .custom-notification-info {
            border-left: 4px solid #3b82f6;
        }
        
        .custom-notification-content {
            display: flex;
            align-items: center;
            gap: 12px;
            padding: 15px 20px;
        }
        
        .custom-notification-icon {
            font-size: 20px;
            flex-shrink: 0;
        }
        
        .custom-notification-success .custom-notification-icon {
            color: #10b981;
        }
        
        .custom-notification-error .custom-notification-icon {
            color: #ef4444;
        }
        
        .custom-notification-warning .custom-notification-icon {
            color: #f59e0b;
        }
        
        .custom-notification-info .custom-notification-icon {
            color: #3b82f6;
        }
        
        .custom-notification-message {
            flex: 1;
            color: #1f2937;
            font-size: 14px;
            line-height: 1.5;
        }
        
        .custom-notification-close {
            background: none;
            border: none;
            color: #9ca3af;
            cursor: pointer;
            padding: 0;
            width: 24px;
            height: 24px;
            flex-shrink: 0;
            font-size: 16px;
            display: flex;
            align-items: center;
            justify-content: center;
            border-radius: 4px;
            transition: all 0.2s ease;
        }
        
        .custom-notification-close:hover {
            color: #1f2937;
            background: #f3f4f6;
        }
        
        @media (max-width: 768px) {
            .custom-notification {
                top: 10px;
                right: 10px;
                left: 10px;
                min-width: auto;
                max-width: none;
            }
        }
        
        /* Button success flash animation */
        .btn-success-flash {
            animation: successFlash 1s ease;
        }
        
        @keyframes successFlash {
            0%, 100% {
                background-color: inherit;
            }
            50% {
                background-color: #10b981;
                color: white;
            }
        }
        
        /* Cart badge pulse animation */
        .cart-pulse {
            animation: cartPulse 0.5s ease;
        }
        
        @keyframes cartPulse {
            0%, 100% {
                transform: scale(1);
            }
            50% {
                transform: scale(1.3);
            }
        }
    `;
    document.head.appendChild(style);
}

// ========================
// CAROUSEL AUTO PLAY
// ========================
document.addEventListener('DOMContentLoaded', function () {
    const carouselElement = document.getElementById('heroCarousel');

    if (carouselElement && typeof bootstrap !== 'undefined') {
        try {
            new bootstrap.Carousel(carouselElement, {
                interval: 5000,
                wrap: true,
                pause: 'hover',
                keyboard: true,
                touch: true
            });
            console.log('🎠 Carousel initialized');
        } catch (error) {
            console.error('❌ Carousel error:', error);
        }
    }
});

// ========================
// LAZY LOADING IMAGES
// ========================
document.addEventListener('DOMContentLoaded', function () {
    const images = document.querySelectorAll('img[data-src]');

    if (images.length === 0) return;

    console.log(`🖼️ Lazy loading ${images.length} images`);

    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    const src = img.dataset.src;

                    if (src) {
                        img.src = src;
                        img.classList.add('loaded');
                        imageObserver.unobserve(img);
                    }
                }
            });
        }, {
            rootMargin: '50px',
            threshold: 0.01
        });

        images.forEach(img => imageObserver.observe(img));
    } else {
        // Fallback
        images.forEach(img => {
            if (img.dataset.src) {
                img.src = img.dataset.src;
                img.classList.add('loaded');
            }
        });
    }
});

// ========================
// AUTO-BIND ADD TO CART BUTTONS
// ========================
document.addEventListener('DOMContentLoaded', function () {
    // Bind tất cả buttons có class .btn-add-cart
    const buttons = document.querySelectorAll('.btn-add-cart');

    buttons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();

            // Lấy productId từ data-product-id hoặc data-id
            const productId = parseInt(
                this.getAttribute('data-product-id') ||
                this.getAttribute('data-id')
            );

            if (productId && productId > 0) {
                addToCart(productId);
            } else {
                console.error('❌ Invalid productId:', productId);
                showNotification('error', 'Sản phẩm không hợp lệ');
            }
        });
    });

    console.log(`🛒 Initialized ${buttons.length} add-to-cart buttons`);
});

// ========================
// LOAD CART COUNT ON PAGE LOAD
// ========================
document.addEventListener('DOMContentLoaded', function () {
    updateCartBadge();
});

console.log('✅ Index page script loaded (Fetch API version)');