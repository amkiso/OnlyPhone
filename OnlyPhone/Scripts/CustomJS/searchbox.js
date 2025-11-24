document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.getElementById('headerSearchInput');
    const suggestionsBox = document.getElementById('searchResultsDropdown');
    let debounceTimer;

    // Hàm định dạng tiền tệ VNĐ
    const formatCurrency = (amount) => {
        return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
    };

    if (searchInput && suggestionsBox) {

        // Sự kiện khi gõ phím
        searchInput.addEventListener('input', function (e) {
            const term = e.target.value.trim();

            // Xóa timer cũ (Debounce)
            clearTimeout(debounceTimer);

            if (term.length < 2) {
                suggestionsBox.classList.remove('active');
                suggestionsBox.innerHTML = '';
                return;
            }

            // Đợi 3s sau khi ngừng gõ mới gọi API
            debounceTimer = setTimeout(() => {
                fetchSuggestions(term);
            }, 3000);
        });

        // Ẩn dropdown khi click ra ngoài
        document.addEventListener('click', function (e) {
            if (!searchInput.contains(e.target) && !suggestionsBox.contains(e.target)) {
                suggestionsBox.classList.remove('active');
            }
        });

        // Hàm gọi API
        function fetchSuggestions(term) {
            // Gọi Action QuickSearch trong SearchController
            fetch(`/Search/QuickSearch?term=${encodeURIComponent(term)}`)
                .then(response => response.json())
                .then(data => {
                    suggestionsBox.innerHTML = '';

                    if (data.success && data.results.length > 0) {
                        data.results.forEach(product => {
                            // Xây dựng đường dẫn ảnh (giả định folder ảnh nằm ở /Content/Images/Products/)
                            // Cần điều chỉnh path này khớp với thực tế project của bạn
                            const imgPath = product.image ? `/Content/Pic/Images/${product.image}` : '/Content/Pic/Images/no-image.jpg';

                            // Link tới trang chi tiết (bạn cần thay Controller/Action cho đúng)
                            const productLink = `/Product/Detail/${product.id}`;

                            const itemHtml = `
                                <a href="${productLink}" class="suggestion-item">
                                    <img src="${imgPath}" alt="${product.name}" class="suggestion-img">
                                    <div class="suggestion-info">
                                        <span class="suggestion-name">${product.name}</span>
                                        <div class="suggestion-price">
                                            ${formatCurrency(product.price)}
                                            ${product.originalPrice > product.price ? `<del>${formatCurrency(product.originalPrice)}</del>` : ''}
                                        </div>
                                    </div>
                                </a>
                            `;
                            suggestionsBox.insertAdjacentHTML('beforeend', itemHtml);
                        });

                        // Thêm nút "Xem tất cả" ở cuối
                        const viewAllLink = `/Search/SearchResult?q=${encodeURIComponent(term)}`;
                        suggestionsBox.insertAdjacentHTML('beforeend', `
                            <a href="${viewAllLink}" class="suggestion-item" style="justify-content: center; color: #3e5cd0; font-weight: bold;">
                                Xem tất cả kết quả cho "${term}"
                            </a>
                        `);

                        suggestionsBox.classList.add('active');
                    } else {
                        suggestionsBox.innerHTML = `<div class="no-results-suggestion">Không tìm thấy sản phẩm nào khớp với "${term}"</div>`;
                        suggestionsBox.classList.add('active');
                    }
                })
                .catch(err => {
                    console.error('Lỗi tìm kiếm:', err);
                });
        }
    }
});
function addToCart(productId) {
    // Hiệu ứng visual đơn giản để người dùng biết đã bấm
    const btn = event.currentTarget;
    const originalContent = btn.innerHTML;

    // Đổi icon thành loading
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i>';
    btn.disabled = true;

    // Gọi API thêm vào giỏ hàng
    // Lưu ý: Đường dẫn '/Cart/AddToCart' cần khớp với Controller của bạn
    fetch('/Product/AddToCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            // Nếu project có AntiForgeryToken thì thêm vào đây
        },
        body: JSON.stringify({ productId: productId, quantity: 1 })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Thông báo thành công (có thể dùng alert hoặc toast notification đẹp hơn)
                alert("Đã thêm sản phẩm vào giỏ hàng!");

                // Cập nhật số lượng trên icon giỏ hàng header (nếu có hàm này)
                if (typeof updateCartCount === 'function') {
                    updateCartCount(data.cartCount);
                }
            } else {
                alert(data.message || "Có lỗi xảy ra, vui lòng thử lại.");
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert("Lỗi kết nối server.");
        })
        .finally(() => {
            // Trả lại trạng thái nút ban đầu
            btn.innerHTML = originalContent;
            btn.disabled = false;
        });
}