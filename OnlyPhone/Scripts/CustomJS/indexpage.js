// JavaScript cho trang Index

// Hàm thêm sản phẩm vào giỏ hàng
function addToCart(productId) {
    $.ajax({
        url: '/Home/AddToCart',
        type: 'POST',
        data: { productId: productId, quantity: 1 },
        success: function (response) {
            if (response.success) {
                // Hiển thị thông báo thành công
                showNotification('success', response.message);

                // Cập nhật số lượng giỏ hàng trên header
                if (response.cartCount !== undefined) {
                    updateCartCount(response.cartCount);
                }
            } else {
                // Hiển thị thông báo lỗi
                showNotification('error', response.message);
            }
        },
        error: function () {
            showNotification('error', 'Đã xảy ra lỗi khi thêm sản phẩm vào giỏ hàng');
        }
    });
}

// Hàm hiển thị thông báo
function showNotification(type, message) {
    // Sử dụng thư viện toast notification hoặc alert đơn giản
    // Ví dụ dùng alert đơn giản:
    alert(message);

    // Hoặc có thể dùng toast như sau (cần thêm thư viện):
    // toastr[type](message);
}

// Hàm cập nhật số lượng giỏ hàng
function updateCartCount(count) {
    $('.cart-count').text(count > 99 ? '99+' : count);
}

// Auto play carousel
document.addEventListener('DOMContentLoaded', function () {
    var carouselElement = document.getElementById('heroCarousel');
    if (carouselElement) {
        var carousel = new bootstrap.Carousel(carouselElement, {
            interval: 5000,
            wrap: true,
            pause: 'hover'
        });
    }
});

// Lazy loading cho hình ảnh (tùy chọn)
document.addEventListener('DOMContentLoaded', function () {
    const images = document.querySelectorAll('img[data-src]');

    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.add('loaded');
                observer.unobserve(img);
            }
        });
    });

    images.forEach(img => imageObserver.observe(img));
});