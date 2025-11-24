
    const initialData = {
        fullName: '@Html.Raw(Model.FullName)',
    phone: '@Html.Raw(Model.PhoneNumber)',
    province: '@Html.Raw(Model.Province ?? "")',
    ward: '@Html.Raw(Model.Ward ?? "")',
    addressDetail: '@Html.Raw(Model.AddressDetail ?? "")',
    avatarSrc: '@Url.Content(Model.AvatarPath)'
    };

    // Danh sách tỉnh/thành phố Việt Nam
    const provinces = [
    "Hà Nội", "TP. Hồ Chí Minh", "Đà Nẵng", "Hải Phòng", "Cần Thơ",
    "An Giang", "Bà Rịa - Vũng Tàu", "Bắc Giang", "Bắc Kạn", "Bạc Liêu",
    "Bắc Ninh", "Bến Tre", "Bình Định", "Bình Dương", "Bình Phước",
    "Bình Thuận", "Cà Mau", "Cao Bằng", "Đắk Lắk", "Đắk Nông",
    "Điện Biên", "Đồng Nai", "Đồng Tháp", "Gia Lai", "Hà Giang",
    "Hà Nam", "Hà Tĩnh", "Hải Dương", "Hậu Giang", "Hòa Bình",
    "Hưng Yên", "Khánh Hòa", "Kiên Giang", "Kon Tum", "Lai Châu",
    "Lâm Đồng", "Lạng Sơn", "Lào Cai", "Long An", "Nam Định",
    "Nghệ An", "Ninh Bình", "Ninh Thuận", "Phú Thọ", "Phú Yên",
    "Quảng Bình", "Quảng Nam", "Quảng Ngãi", "Quảng Ninh", "Quảng Trị",
    "Sóc Trăng", "Sơn La", "Tây Ninh", "Thái Bình", "Thái Nguyên",
    "Thanh Hóa", "Thừa Thiên Huế", "Tiền Giang", "Trà Vinh", "Tuyên Quang",
    "Vĩnh Long", "Vĩnh Phúc", "Yên Bái"
    ];

    const $province = document.getElementById('province');
    const $ward = document.getElementById('ward');
    const $addressDetail = document.getElementById('addressDetail');
    const avatarImg = document.getElementById('avatarImg');
    const avatarInput = document.getElementById('avatarInput');
    const avatarWrap = document.getElementById('avatarWrap');
    const avatarOverlay = document.getElementById('avatarOverlay');
    const changeBtn = document.getElementById('changeBtn');
    const resetBtn = document.getElementById('resetBtn');
    const editBtn = document.getElementById('editBtn');
    const saveBtn = document.getElementById('saveBtn');
    const homebtn = document.getElementById('homebtn');
    let editing = false;
    let originalData = { };


    // Populate provinces
    function populateProvinces() {
        provinces.forEach(p => {
            const opt = document.createElement('option');
            opt.value = p;
            opt.textContent = p;
            $province.appendChild(opt);
        });

    // Set initial value
    if (initialData.province) {
        $province.value = initialData.province;
        }
    }

    // Initialize
    populateProvinces();
    $ward.value = initialData.ward || '';

    function snapshotOriginal() {
        originalData = {
            name: document.getElementById('fullName').value,
            phone: document.getElementById('phone').value,
            province: $province.value,
            ward: $ward.value,
            addressDetail: $addressDetail.value,
            avatarSrc: avatarImg.src
        };
    }

    function setEditable(state) {
        editing = !!state;
    document.getElementById('fullName').readOnly = !editing;
    document.getElementById('phone').readOnly = !editing;
    $province.disabled = !editing;
    $ward.readOnly = !editing;
    $addressDetail.readOnly = !editing;
    changeBtn.disabled = !editing;
    resetBtn.disabled = !editing;
    saveBtn.disabled = !editing;
    editBtn.innerHTML = editing ? '<i class="fas fa-times"></i> Hủy' : '<i class="fas fa-edit"></i> Sửa';

    if (editing) {
        avatarWrap.classList.add('editable');
        } else {
        avatarWrap.classList.remove('editable');
        }
    }

    // Initialize
    snapshotOriginal();
    setEditable(false);

    // Edit/Cancel button
    editBtn.addEventListener('click', () => {
        if (!editing) {
        snapshotOriginal();
    setEditable(true);
        } else {
        // Cancel - restore original values
        document.getElementById('fullName').value = originalData.name;
    document.getElementById('phone').value = originalData.phone;
    $province.value = originalData.province || '';
    $ward.value = originalData.ward || '';
    $addressDetail.value = originalData.addressDetail || '';
    avatarImg.src = originalData.avatarSrc;
    avatarInput.value = '';
    setEditable(false);
        }
    });

    // Avatar change
    avatarInput.addEventListener('change', (e) => {
        if (!editing) return;
    const f = e.target.files && e.target.files[0];
    if (!f) return;

    console.log('File selected:', f.name, f.size, f.type);

        // Validate file size (5MB)
        if (f.size > 5 * 1024 * 1024) {
        alert('Kích thước file không được vượt quá 5MB');
    avatarInput.value = '';
    return;
        }

    // Validate file type
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
    if (!allowedTypes.includes(f.type)) {
        alert('Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif)');
    avatarInput.value = '';
    return;
        }

    const url = URL.createObjectURL(f);
    avatarImg.src = url;
    console.log('Avatar updated with:', url);
    });

    changeBtn.addEventListener('click', (e) => {
        e.preventDefault();
    e.stopPropagation();
    console.log('Change button clicked, editing:', editing);
    if (!editing) {
        console.log('Not in editing mode, ignoring click');
    return;
        }
    console.log('Triggering file input click');
    avatarInput.click();
    });

    resetBtn.addEventListener('click', (e) => {
        e.preventDefault();
    e.stopPropagation();
    console.log('Reset button clicked');
    if (!editing) return;
    avatarImg.src = originalData.avatarSrc;
    avatarInput.value = '';
    console.log('Avatar reset to:', originalData.avatarSrc);
    });

    // Drag & drop avatar
    ['dragenter', 'dragover'].forEach(evt => {
        avatarWrap.addEventListener(evt, e => {
            if (!editing) return;
            e.preventDefault();
            avatarWrap.style.opacity = 0.9;
        });
    });

    ['dragleave', 'drop'].forEach(evt => {
        avatarWrap.addEventListener(evt, e => {
            if (!editing) return;
            e.preventDefault();
            avatarWrap.style.opacity = 1;
        });
    });

    avatarWrap.addEventListener('drop', e => {
        if (!editing) return;
    const f = e.dataTransfer.files && e.dataTransfer.files[0];
    if (!f) return;

        // Validate
        if (f.size > 5 * 1024 * 1024) {
        alert('Kích thước file không được vượt quá 5MB');
    return;
        }

    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
    if (!allowedTypes.includes(f.type)) {
        alert('Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif)');
    return;
        }

    // Create a DataTransfer to set files
    const dataTransfer = new DataTransfer();
    dataTransfer.items.add(f);
    avatarInput.files = dataTransfer.files;

    avatarImg.src = URL.createObjectURL(f);
    });

    // Click on avatar to change (when editing)
    avatarWrap.addEventListener('click', () => {
        if (editing) {
        avatarInput.click();
        }
    });

    // Form validation before submit
    document.getElementById('profileForm').addEventListener('submit', function(e) {
        const fullName = document.getElementById('fullName').value.trim();
    const phone = document.getElementById('phone').value.trim();

    if (!fullName) {
        e.preventDefault();
    alert('Vui lòng nhập họ và tên');
    return false;
        }

    if (!phone) {
        e.preventDefault();
    alert('Vui lòng nhập số điện thoại');
    return false;
        }

    // Validate phone format
    const phoneRegex = /^(0[3|5|7|8|9])+([0-9]{8})$/;
    if (!phoneRegex.test(phone)) {
        e.preventDefault();
    alert('Số điện thoại không hợp lệ (vd: 0912345678)');
    return false;
        }

    // Show loading
    saveBtn.disabled = true;
    saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';

    return true;
    });

    // Auto-hide alerts after 5 seconds
    setTimeout(() => {
        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(alert => {
        alert.style.opacity = '0';
            setTimeout(() => alert.remove(), 300);
        });
    }, 5000);