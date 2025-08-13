// ====== Filter & Search helpers ======
function setFilter(filterValue) {
    // Tạo form GET động, giữ nguyên searchTerm hiện tại (nếu có)
    const form = document.createElement('form');
    form.method = 'get';
    form.action = (window.tuDienIndexUrl ?? '/TuDien'); // fallback nếu không có Url.Action

    const filterInput = document.createElement('input');
    filterInput.type = 'hidden';
    filterInput.name = 'filter';
    filterInput.value = filterValue;
    form.appendChild(filterInput);

    const searchInput = document.querySelector('input[name="searchTerm"]');
    if (searchInput && searchInput.value) {
        const hiddenSearch = document.createElement('input');
        hiddenSearch.type = 'hidden';
        hiddenSearch.name = 'searchTerm';
        hiddenSearch.value = searchInput.value;
        form.appendChild(hiddenSearch);
    }

    document.body.appendChild(form);
    form.submit();
}

// ====== UX helpers ======
function copyText(text) {
    if (!text) return;
    navigator.clipboard.writeText(text)
        .then(() => toast('Đã sao chép!'))
        .catch(() => alert('Không thể sao chép, vui lòng thử lại.'));
}

function notImplemented() {
    toast('Tính năng đang phát triển 👷');
}

function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// Mini toast (không cần lib)
function toast(message = 'Thành công', ms = 1400) {
    const el = document.createElement('div');
    el.textContent = message;
    el.style.position = 'fixed';
    el.style.left = '50%';
    el.style.top = '20px';
    el.style.transform = 'translateX(-50%)';
    el.style.background = '#111827';
    el.style.color = '#fff';
    el.style.padding = '10px 14px';
    el.style.borderRadius = '10px';
    el.style.fontSize = '14px';
    el.style.boxShadow = '0 8px 24px rgba(0,0,0,.15)';
    el.style.zIndex = 9999;
    document.body.appendChild(el);
    setTimeout(() => el.remove(), ms);
}

// ====== Optional: gắn Url.Action vào window (đặt trong View nếu muốn) ======
// Trong View, bạn có thể thêm dòng sau (trước khi import file JS) để chính xác URL:
// <script>window.tuDienIndexUrl='@Url.Action("Index","TuDien")'</script>
