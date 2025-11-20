
$(document).ready(function () {
    let currentStep = 1;
    let otpTimer;
    let otpExpireTime = 300; // 5 phút

    // Validate Email Format
    function isValidEmail(email) {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    }

    // Check Password Strength
    function checkPasswordStrength(password) {
        let strength = 0;
        const strengthFill = $('#strengthFill');
        const strengthText = $('#strengthText');

        if (password.length >= 8) strength++;
        if (password.match(/[a-z]/) && password.match(/[A-Z]/)) strength++;
        if (password.match(/\d/)) strength++;
        if (password.match(/[^a-zA-Z\d]/)) strength++;

        strengthFill.removeClass('weak medium strong');

        if (password.length === 0) {
            strengthFill.css('width', '0');
            strengthText.text('');
        } else if (strength <= 1) {
            strengthFill.addClass('weak');
            strengthText.text('Yếu - Nên có ít nhất 8 ký tự, chữ hoa, chữ thường và số');
        } else if (strength <= 3) {
            strengthFill.addClass('medium');
            strengthText.text('Trung bình - Thêm ký tự đặc biệt để mạnh hơn');
        } else {
            strengthFill.addClass('strong');
            strengthText.text('Mạnh - Mật khẩu an toàn');
        }

        return strength;
    }

    // Show Error
    function showError(fieldName, message) {
        $(`#${fieldName}`).addClass('error').removeClass('success');
        $(`#${fieldName}Error`).text(message).addClass('show');
        $(`#${fieldName}Success`).removeClass('show');
    }

    // Clear Error
    function clearError(fieldName) {
        $(`#${fieldName}`).removeClass('error');
        $(`#${fieldName}Error`).text('').removeClass('show');
    }

    // Show Success
    function showSuccess(fieldName, message) {
        $(`#${fieldName}`).addClass('success').removeClass('error');
        $(`#${fieldName}Success`).text(message).addClass('show');
        $(`#${fieldName}Error`).removeClass('show');
    }

    // Show Alert
    function showAlert(message, type) {
        const alertClass = type === 'success' ? 'alert-success' :
            type === 'error' ? 'alert-error' : 'alert-info';
        const icon = type === 'success' ? 'check-circle' :
            type === 'error' ? 'exclamation-circle' : 'info-circle';

        const alert = `<div class="alert ${alertClass}">
                    <i class="fas fa-${icon}"></i> ${message}
                </div>`;

        $('#alertContainer').html(alert);
        setTimeout(() => $('#alertContainer').empty(), 5000);
    }

    // Change Step
    function goToStep(step) {
        $('.form-section').removeClass('active');
        $(`#step${step}`).addClass('active');

        $('.step').removeClass('active completed');
        for (let i = 1; i < step; i++) {
            $(`.step[data-step="${i}"]`).addClass('completed');
        }
        $(`.step[data-step="${step}"]`).addClass('active');

        currentStep = step;
    }

    // Password Strength Check
    $('#password').on('input', function () {
        checkPasswordStrength($(this).val());
        clearError('password');
    });

    // Confirm Password Match
    $('#confirmPassword').on('input', function () {
        const password = $('#password').val();
        const confirmPassword = $(this).val();

        clearError('confirmPassword');
        if (confirmPassword && password === confirmPassword) {
            showSuccess('confirmPassword', 'Mật khẩu khớp');
        }
    });

    // Email validation on input
    $('#email').on('blur', function () {
        const email = $(this).val().trim();
        if (email && isValidEmail(email)) {
            showSuccess('email', 'Email hợp lệ');
        }
    });

    // Step 1: Send OTP
    $('#btnSendOTP').click(function () {
        const username = $('#username').val().trim();
        const email = $('#email').val().trim();
        const password = $('#password').val();
        const confirmPassword = $('#confirmPassword').val();
        let isValid = true;

        // Validate Username
        if (!username) {
            showError('username', 'Vui lòng nhập tên đăng nhập');
            isValid = false;
        } else if (username.length < 3) {
            showError('username', 'Tên đăng nhập phải có ít nhất 3 ký tự');
            isValid = false;
        } else {
            clearError('username');
        }

        // Validate Email
        if (!email) {
            showError('email', 'Vui lòng nhập email');
            isValid = false;
        } else if (!isValidEmail(email)) {
            showError('email', 'Email không hợp lệ');
            isValid = false;
        } else {
            clearError('email');
        }

        // Validate Password
        if (!password) {
            showError('password', 'Vui lòng nhập mật khẩu');
            isValid = false;
        } else if (password.length < 6) {
            showError('password', 'Mật khẩu phải có ít nhất 6 ký tự');
            isValid = false;
        } else {
            clearError('password');
        }

        // Validate Confirm Password
        if (!confirmPassword) {
            showError('confirmPassword', 'Vui lòng xác nhận mật khẩu');
            isValid = false;
        } else if (password !== confirmPassword) {
            showError('confirmPassword', 'Mật khẩu không khớp');
            isValid = false;
        } else {
            clearError('confirmPassword');
        }

        if (isValid) {
            $(this).prop('disabled', true).text('Đang gửi...');

            // Send OTP via AJAX
            $.ajax({
                url: '/Account/SendOTP',
                type: 'POST',
                data: {
                    email: email,
                    username: username,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    if (response.success) {
                        $('#emailDisplay').text(email);
                        goToStep(2);
                        startOTPTimer();
                        showAlert('Mã OTP đã được gửi đến email của bạn', 'success');
                    } else {
                        showAlert(response.message || 'Có lỗi xảy ra', 'error');
                    }
                },
                error: function () {
                    showAlert('Không thể gửi OTP. Vui lòng thử lại', 'error');
                },
                complete: function () {
                    $('#btnSendOTP').prop('disabled', false).text('Tiếp tục');
                }
            });
        }
    });

    // OTP Input Handler
    $('.otp-input').on('input', function () {
        const value = $(this).val();
        if (value && /^\d$/.test(value)) {
            const nextInput = $(this).next('.otp-input');
            if (nextInput.length) {
                nextInput.focus();
            }
            clearError('otp');
        }
    });

    $('.otp-input').on('keydown', function (e) {
        if (e.key === 'Backspace' && !$(this).val()) {
            const prevInput = $(this).prev('.otp-input');
            if (prevInput.length) {
                prevInput.focus();
            }
        }
    });

    // Start OTP Timer
    function startOTPTimer() {
        let timeLeft = otpExpireTime;
        $('#resendOTP').addClass('disabled');

        otpTimer = setInterval(function () {
            timeLeft--;
            const minutes = Math.floor(timeLeft / 60);
            const seconds = timeLeft % 60;
            $('#countdown').text(`${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`);

            if (timeLeft <= 0) {
                clearInterval(otpTimer);
                $('#otpTimer').addClass('expired').find('strong').text('Mã OTP đã hết hạn');
                $('#resendOTP').removeClass('disabled');
            }
        }, 1000);
    }

    // Resend OTP
    $('#resendOTP').click(function (e) {
        e.preventDefault();
        if ($(this).hasClass('disabled')) return;

        const email = $('#email').val();

        $.ajax({
            url: '/Account/SendOTP',
            type: 'POST',
            data: {
                email: email,
                username: $('#username').val(),
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    showAlert('Mã OTP mới đã được gửi', 'success');
                    clearInterval(otpTimer);
                    $('#otpTimer').removeClass('expired');
                    startOTPTimer();
                    $('.otp-input').val('').first().focus();
                } else {
                    showAlert(response.message || 'Có lỗi xảy ra', 'error');
                }
            }
        });
    });

    // Verify OTP
    $('#btnVerifyOTP').click(function () {
        let otp = '';
        $('.otp-input').each(function () {
            otp += $(this).val();
        });

        if (otp.length !== 6) {
            showError('otp', 'Vui lòng nhập đủ 6 số');
            $('.otp-input').addClass('error');
            return;
        }

        $(this).prop('disabled', true).text('Đang xác thực...');

        // Verify OTP via AJAX
        $.ajax({
            url: '/Account/VerifyOTP',
            type: 'POST',
            data: {
                email: $('#email').val(),
                otpCode: otp,
                username: $('#username').val(),
                password: $('#password').val(),
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    clearInterval(otpTimer);
                    goToStep(3);
                    showAlert('Đăng ký thành công! Đang chuyển hướng...', 'success');

                    // Redirect to login after 2 seconds
                    setTimeout(function () {
                        window.location.href = '/Account/Login';
                    }, 2000);
                } else {
                    showError('otp', response.message || 'Mã OTP không đúng');
                    $('.otp-input').addClass('error');
                }
            },
            error: function () {
                showAlert('Có lỗi xảy ra. Vui lòng thử lại', 'error');
            },
            complete: function () {
                $('#btnVerifyOTP').prop('disabled', false).text('Xác thực');
            }
        });
    });

    // Back to Step 1
    $('#btnBackStep1').click(function () {
        clearInterval(otpTimer);
        goToStep(1);
        $('.otp-input').val('').removeClass('error');
    });

    // Clear field errors on input
    $('#username, #email').on('input', function () {
        clearError($(this).attr('id'));
    });
});
