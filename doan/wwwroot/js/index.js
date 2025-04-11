// Toggle Menu Mobile
function toggleMenu() {
    document.getElementById("menuDropdown").classList.toggle("hidden");
}

// Scroll lên đầu trang
function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// Hiệu ứng Sakura rơi
window.addEventListener('DOMContentLoaded', function () {
    new Sakura('body', {
        fallSpeed: 1,
        className: 'sakura'
    });
});

// Hiệu ứng typing gõ chữ
const text = "Hành Trình Học Tiếng Nhật N5 → N3 Dễ Dàng & Hiệu Quả";
let i = 0;

function typeEffect() {
    if (i < text.length) {
        document.querySelector('.typing').innerHTML += text.charAt(i);
        i++;
        setTimeout(typeEffect, 50);
    }
}

// Khi load xong trang
window.onload = function () {
    typeEffect();

    const loader = document.getElementById('loader');
    if (loader) {
        loader.style.display = 'none';
    }
}
