    // Toggle password visibility
    function togglePassword(inputId) {
        const input = document.getElementById(inputId);
    const button = input.parentElement.querySelector('.btn-toggle-password');
    const icon = button.querySelector('i');

    if (input.type === 'password') {
        input.type = 'text';
    icon.classList.remove('fa-eye');
    icon.classList.add('fa-eye-slash');
        } else {
        input.type = 'password';
    icon.classList.remove('fa-eye-slash');
    icon.classList.add('fa-eye');
        }
    }

    // Password strength checker
    const newPasswordInput = document.getElementById('newPassword');
    const strengthBar = document.getElementById('passwordStrength');
    const strengthBarFill = document.getElementById('strengthBarFill');
    const strengthText = document.getElementById('strengthText');

    newPasswordInput.addEventListener('input', function() {
        const password = this.value;

    if (password.length === 0) {
        strengthBar.style.display = 'none';
    return;
        }

    strengthBar.style.display = 'block';

    let strength = 0;
    let text = '';
    let color = '';

        // Length
        if (password.length >= 6) strength += 20;
        if (password.length >= 8) strength += 20;

    // Contains lowercase
    if (/[a-z]/.test(password)) strength += 20;

    // Contains uppercase
    if (/[A-Z]/.test(password)) strength += 20;

    // Contains numbers
    if (/[0-9]/.test(password)) strength += 10;

    // Contains special characters
    if (/[^a-zA-Z0-9]/.test(password)) strength += 10;

    if (strength < 40) {
        text = 'Yếu';
    color = '#ef4444';
        } else if (strength < 60) {
        text = 'Trung bình';
    color = '#f59e0b';
        } else if (strength < 80) {
        text = 'Khá';
    color = '#3b82f6';
        } else {
        text = 'Mạnh';
    color = '#10b981';
        }

    strengthBarFill.style.width = strength + '%';
    strengthBarFill.style.backgroundColor = color;
    strengthText.textContent = text;
    strengthText.style.color = color;
    });

    // Form validation
    document.getElementById('changePasswordForm').addEventListener('submit', function(e) {
        const currentPassword = document.getElementById('currentPassword').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;

    if (!currentPassword) {
        e.preventDefault();
    alert('Vui lòng nhập mật khẩu hiện tại');
    return false;
        }

    if (!newPassword) {
        e.preventDefault();
    alert('Vui lòng nhập mật khẩu mới');
    return false;
        }

    if (newPassword.length < 6) {
        e.preventDefault();
    alert('Mật khẩu mới phải có ít nhất 6 ký tự');
    return false;
        }

    if (newPassword !== confirmPassword) {
        e.preventDefault();
    alert('Mật khẩu xác nhận không khớp');
    return false;
        }

    if (currentPassword === newPassword) {
        e.preventDefault();
    alert('Mật khẩu mới phải khác mật khẩu hiện tại');
    return false;
        }

    // Show loading
    const submitBtn = document.getElementById('submitBtn');
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';

    return true;
    });

    // Auto-hide alerts
    setTimeout(() => {
        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(alert => {
        alert.style.opacity = '0';
            setTimeout(() => alert.remove(), 300);
        });
    }, 5000);
