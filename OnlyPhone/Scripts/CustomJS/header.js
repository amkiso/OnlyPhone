
// Mobile Menu Toggle
const mobileMenuToggle = document.getElementById('mobileMenuToggle');
const mobileMenuClose = document.getElementById('mobileMenuClose');
const headerNav = document.getElementById('headerNav');
const mobileMenuOverlay = document.getElementById('mobileMenuOverlay');

function openMobileMenu() {
    headerNav.classList.add('active');
    mobileMenuOverlay.classList.add('active');
    document.body.style.overflow = 'hidden';
}
//Xử lý khi người dùng bấm nút x ở góc phải
function closeMobileMenu() {
    headerNav.classList.remove('active');
    mobileMenuOverlay.classList.remove('active');
    document.body.style.overflow = '';
    // Close all active submenus
    document.querySelectorAll('.menu-item.active').forEach(item => {
        item.classList.remove('active');
    });
}
//Các sự kiện bấm nút
mobileMenuToggle.addEventListener('click', openMobileMenu);
mobileMenuClose.addEventListener('click', closeMobileMenu);
mobileMenuOverlay.addEventListener('click', closeMobileMenu);

// Mobile Submenu Toggle button( hoạt động cả submenu cấp 1 và cấp 2)
document.querySelectorAll('.submenu-toggle').forEach(toggle => {
    toggle.addEventListener('click', function (e) {
        if (window.innerWidth < 992) {
            e.preventDefault();
            const menuItem = this.closest('.menu-item');

            // đóng các submenu khác
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

// Mobile User Menu Toggle
const userMenuWrapper = document.querySelector('.user-menu-wrapper');
const userActionItem = userMenuWrapper.querySelector('.action-item');

userActionItem.addEventListener('click', function (e) {
    if (window.innerWidth < 992) {
        e.preventDefault();
        userMenuWrapper.classList.toggle('active');
        mobileMenuOverlay.classList.add('active');
        document.body.style.overflow = 'hidden';
    }
});

// Đóng menu khi người dùng click ra ngoài 
mobileMenuOverlay.addEventListener('click', function () {
    userMenuWrapper.classList.remove('active');
    closeMobileMenu();
});

// đóng giao diện mobile khi thay đổi kích thước màn hình thành trên laptop/pc
window.addEventListener('resize', function () {
    if (window.innerWidth >= 992) {
        closeMobileMenu();
        userMenuWrapper.classList.remove('active');
    }
});
