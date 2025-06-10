// File: wwwroot/js/quiz-ui.js

// Script này chỉ chạy sau khi toàn bộ cấu trúc HTML của trang đã được tải
document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('quizForm');
    const progressBar = document.getElementById('quiz-progress-bar');

    // Nếu không tìm thấy các thành phần cần thiết, dừng lại để tránh lỗi
    if (!form || !progressBar) {
        return;
    }

    // Lấy tổng số câu hỏi từ data attribute trên thẻ HTML
    const totalQuestions = parseInt(progressBar.dataset.totalQuestions, 10);

    // Nếu không có câu hỏi nào, cũng dừng lại
    if (totalQuestions === 0) {
        return;
    }

    // Hàm để tính toán và cập nhật thanh tiến trình
    function updateProgress() {
        // Đếm số câu hỏi đã được trả lời (số radio button đã được check)
        const answeredQuestions = form.querySelectorAll('input[type="radio"]:checked').length;

        // Tính toán phần trăm hoàn thành
        const percentage = (answeredQuestions / totalQuestions) * 100;

        // Cập nhật lại chiều rộng và các thuộc tính aria của thanh tiến trình
        progressBar.style.width = percentage + '%';
        progressBar.setAttribute('aria-valuenow', answeredQuestions);
    }

    // Gắn sự kiện: mỗi khi người dùng thay đổi lựa chọn trong form, gọi hàm updateProgress
    form.addEventListener('change', updateProgress);

    // Gọi hàm một lần ngay khi tải trang để hiển thị tiến trình ban đầu
    // (rất hữu ích khi kết hợp với việc lưu câu trả lời)
    updateProgress();
});