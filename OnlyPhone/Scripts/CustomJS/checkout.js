// ========================
// CHECKOUT PAGE SCRIPT
// ========================

document.addEventListener('DOMContentLoaded', function () {
    console.log('🛒 Checkout page loaded');

    // Load provinces for datalist
    loadProvinces();

    // Add event listeners for method options
    setupMethodSelectionListeners();

    // Setup form submission
    setupFormSubmission();

    // Initial calculation
    updateTotalAmount();
});

// ========================
// LOAD PROVINCES
// ========================
function loadProvinces() {
    fetch('/Checkout/GetProvinces', {
        method: 'GET',
        headers: {
            'Accept': 'application/json'
        }
    })
    .then(response => response.json())
    .then(provinces => {
        const datalist = document.getElementById('provinceList');
        if (datalist) {
            provinces.forEach(province => {
                const option = document.createElement('option');
                option.value = province;
                datalist.appendChild(option);
            });
        }
    })
    .catch(error => {
        console.error('Error loading provinces:', error);
    });
}

// ========================
// METHOD SELECTION
// ========================
function setupMethodSelectionListeners() {
    // Payment and shipping method selection
    const methodOptions = document.querySelectorAll('.method-option');
    
    methodOptions.forEach(option => {
        option.addEventListener('click', function() {
            const input = this.querySelector('input[type="radio"]');
            if (input) {
                input.checked = true;
                
                // Update UI
                const allOptions = this.closest('.method-options').querySelectorAll('.method-option');
                allOptions.forEach(opt => opt.classList.remove('selected'));
                this.classList.add('selected');
                
                // Update total if shipping method changed
                if (input.name === 'SelectedShippingMethod') {
                    updateShippingFee();
                }
            }
        });
    });
}

// ========================
// UPDATE SHIPPING FEE
// ========================
function updateShippingFee() {
    const selectedShipping = document.querySelector('input[name="SelectedShippingMethod"]:checked');
    
    if (selectedShipping) {
        const fee = parseFloat(selectedShipping.getAttribute('data-fee')) || 0;
        document.getElementById('shippingFee').textContent = formatCurrency(fee) + 'đ';
        
        console.log('💰 Shipping fee updated:', fee);
        updateTotalAmount();
    }
}

// ========================
// VOUCHER FUNCTIONS
// ========================
function applyVoucher() {
    const voucherCode = document.getElementById('voucherCode').value.trim();
    
    if (!voucherCode) {
        showNotification('warning', 'Vui lòng nhập mã voucher');
        return;
    }

    const subTotalText = document.getElementById('subTotal').textContent;
    const subTotal = parseFloat(subTotalText.replace(/[^\d]/g, ''));

    console.log('🎟️ Applying voucher:', voucherCode);

    fetch('/Checkout/ValidateVoucher', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        body: JSON.stringify({
            voucherCode: voucherCode,
            orderValue: subTotal
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification('success', data.message);
            document.getElementById('selectedVoucherId').value = data.voucherId;
            document.getElementById('discount').textContent = '-' + data.discountText;
            updateTotalAmount();
            
            // Highlight voucher in list
            highlightVoucher(data.voucherId);
        } else {
            showNotification('error', data.message);
        }
    })
    .catch(error => {
        console.error('Error validating voucher:', error);
        showNotification('error', 'Lỗi kiểm tra mã voucher');
    });
}

function selectVoucher(voucherId, voucherCode) {
    document.getElementById('voucherCode').value = voucherCode;
    document.getElementById('selectedVoucherId').value = voucherId;
    
    // Highlight selected voucher
    highlightVoucher(voucherId);
    
    // Apply voucher
    applyVoucher();
}

function highlightVoucher(voucherId) {
    const vouchers = document.querySelectorAll('.voucher-item');
    vouchers.forEach(v => v.classList.remove('selected'));
    
    const selectedVoucher = Array.from(vouchers).find(v => 
        v.getAttribute('onclick')?.includes(voucherId.toString())
    );
    
    if (selectedVoucher) {
        selectedVoucher.classList.add('selected');
    }
}

// ========================
// CALCULATE TOTAL
// ========================
function updateTotalAmount() {
    const subTotalText = document.getElementById('subTotal').textContent;
    const shippingFeeText = document.getElementById('shippingFee').textContent;
    const discountText = document.getElementById('discount').textContent;

    const subTotal = parseFloat(subTotalText.replace(/[^\d]/g, ''));
    const shippingFee = parseFloat(shippingFeeText.replace(/[^\d]/g, ''));
    const discount = parseFloat(discountText.replace(/[^\d]/g, ''));

    const total = subTotal + shippingFee - discount;

    document.getElementById('totalAmount').textContent = formatCurrency(total) + 'đ';

    console.log('💵 Total updated:', {
        subTotal,
        shippingFee,
        discount,
        total
    });
}

// ========================
// FORM SUBMISSION
// ========================
function setupFormSubmission() {
    const form = document.getElementById('checkoutForm');
    
    if (!form) return;

    form.addEventListener('submit', function(e) {
        e.preventDefault();
        
        console.log('📝 Submitting checkout form');

        // Validate form
        if (!validateCheckoutForm()) {
            return;
        }

        // Disable submit button
        const submitBtn = document.getElementById('btnPlaceOrder');
        const originalText = submitBtn.innerHTML;
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';

        // Get form data
        const formData = new FormData(form);
        const products = document.getElementById('selectedProducts').value;

        // Convert FormData to object
        const data = {};
        formData.forEach((value, key) => {
            data[key] = value;
        });

        // Add products
        data.products = products;

        console.log('📦 Order data:', data);

        // Submit order
        fetch('/Checkout/ProcessOrder', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: new URLSearchParams(data)
        })
        .then(response => response.json())
        .then(result => {
            console.log('✅ Order result:', result);

            if (result.success) {
                showNotification('success', result.message);
                
                // Redirect to confirmation page
                setTimeout(() => {
                    window.location.href = result.redirectUrl;
                }, 1000);
            } else {
                showNotification('error', result.message);
                
                // Re-enable button
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;

                // Check if need login
                if (result.requireLogin) {
                    setTimeout(() => {
                        window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.href);
                    }, 2000);
                }
            }
        })
        .catch(error => {
            console.error('❌ Order error:', error);
            showNotification('error', 'Đã xảy ra lỗi khi đặt hàng. Vui lòng thử lại!');
            
            // Re-enable button
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalText;
        });
    });
}

// ========================
// FORM VALIDATION
// ========================
function validateCheckoutForm() {
    const recipientName = document.querySelector('input[name="UserInfo.RecipientName"]').value.trim();
    const phoneNumber = document.querySelector('input[name="UserInfo.PhoneNumber"]').value.trim();
    const province = document.querySelector('input[name="UserInfo.Province"]').value.trim();
    const district = document.querySelector('input[name="UserInfo.District"]').value.trim();
    const addressDetail = document.querySelector('textarea[name="UserInfo.AddressDetail"]').value.trim();

    // Validate recipient name
    if (!recipientName) {
        showNotification('error', 'Vui lòng nhập họ tên người nhận');
        return false;
    }

    // Validate phone number
    const phoneRegex = /^(0[3|5|7|8|9])+([0-9]{8})$/;
    if (!phoneNumber) {
        showNotification('error', 'Vui lòng nhập số điện thoại');
        return false;
    }
    if (!phoneRegex.test(phoneNumber)) {
        showNotification('error', 'Số điện thoại không hợp lệ (vd: 0912345678)');
        return false;
    }

    // Validate address
    if (!province) {
        showNotification('error', 'Vui lòng chọn Tỉnh/Thành phố');
        return false;
    }
    if (!district) {
        showNotification('error', 'Vui lòng nhập Quận/Huyện');
        return false;
    }
    if (!addressDetail) {
        showNotification('error', 'Vui lòng nhập địa chỉ chi tiết');
        return false;
    }

    // Validate payment method
    const paymentMethod = document.querySelector('input[name="SelectedPaymentMethod"]:checked');
    if (!paymentMethod) {
        showNotification('error', 'Vui lòng chọn phương thức thanh toán');
        return false;
    }

    // Validate shipping method
    const shippingMethod = document.querySelector('input[name="SelectedShippingMethod"]:checked');
    if (!shippingMethod) {
        showNotification('error', 'Vui lòng chọn phương thức vận chuyển');
        return false;
    }

    return true;
}

// ========================
// NOTIFICATION FUNCTION
// ========================
function showNotification(type, message) {
    console.log(`📢 Notification [${type}]:`, message);

    // Check if toastr exists
    if (typeof toastr !== 'undefined') {
        toastr.options = {
            closeButton: true,
            progressBar: true,
            positionClass: 'toast-top-right',
            timeOut: 3000
        };
        toastr[type](message);
        return;
    }

    // Fallback: Custom notification
    showCustomNotification(type, message);
}

function showCustomNotification(type, message) {
    // Remove old notification
    const oldNotification = document.querySelector('.custom-notification');
    if (oldNotification) {
        oldNotification.remove();
    }

    const icons = {
        success: '<i class="fas fa-check-circle"></i>',
        error: '<i class="fas fa-times-circle"></i>',
        warning: '<i class="fas fa-exclamation-triangle"></i>',
        info: '<i class="fas fa-info-circle"></i>'
    };

    const colors = {
        success: '#10b981',
        error: '#ef4444',
        warning: '#f59e0b',
        info: '#3b82f6'
    };

    const notification = document.createElement('div');
    notification.className = 'custom-notification';
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        min-width: 300px;
        max-width: 400px;
        background: white;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        z-index: 10000;
        border-left: 4px solid ${colors[type]};
        animation: slideInRight 0.3s ease;
    `;

    notification.innerHTML = `
        <div style="display: flex; align-items: center; gap: 12px; padding: 15px 20px;">
            <span style="font-size: 20px; color: ${colors[type]};">${icons[type]}</span>
            <span style="flex: 1; color: #1f2937; font-size: 14px;">${message}</span>
            <button onclick="this.parentElement.parentElement.remove()" 
                    style="background: none; border: none; color: #9ca3af; cursor: pointer; font-size: 20px; padding: 0;">
                ×
            </button>
        </div>
    `;

    document.body.appendChild(notification);

    // Auto remove
    setTimeout(() => {
        if (notification.parentElement) {
            notification.style.animation = 'slideInRight 0.3s ease reverse';
            setTimeout(() => notification.remove(), 300);
        }
    }, 3000);
}

// ========================
// UTILITY FUNCTIONS
// ========================
function formatCurrency(amount) {
    return amount.toLocaleString('vi-VN');
}

// Add animation keyframes
(function() {
    if (!document.getElementById('checkout-animations')) {
        const style = document.createElement('style');
        style.id = 'checkout-animations';
        style.textContent = `
            @keyframes slideInRight {
                from {
                    transform: translateX(100%);
                    opacity: 0;
                }
                to {
                    transform: translateX(0);
                    opacity: 1;
                }
            }
        `;
        document.head.appendChild(style);
    }
})();

console.log('✅ Checkout script loaded');