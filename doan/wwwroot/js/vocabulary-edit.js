// Đặt tất cả code vào trong một hàm để tránh xung đột với các script khác
(function () {
    // Chạy script sau khi toàn bộ cây DOM đã được tải
    document.addEventListener('DOMContentLoaded', function () {

        // --- Phần 1: Xử lý dropdown động (Level -> Lesson) ---

        const levelSelect = document.getElementById('LevelId');
        const lessonSelect = document.getElementById('LessonId');
        const form = document.getElementById('editForm');

        // Kiểm tra xem các phần tử có tồn tại không để tránh lỗi
        if (!levelSelect || !lessonSelect || !form) {
            console.error('Không tìm thấy các phần tử form cần thiết (LevelId, LessonId, editForm).');
            return;
        }

        // Lấy URL từ thuộc tính data- của form, đây là cách làm an toàn và linh hoạt
        const lessonsUrl = form.dataset.lessonsUrl;
        if (!lessonsUrl) {
            console.error('Không tìm thấy URL để lấy danh sách bài học (data-lessons-url).');
            return;
        }

        levelSelect.addEventListener('change', function () {
            const levelId = this.value;
            lessonSelect.innerHTML = '<option value="">-- Đang tải... --</option>';
            lessonSelect.disabled = true; // Vô hiệu hóa trong khi tải

            if (levelId) {
                // Thay thế placeholder trong URL bằng levelId đã chọn
                const finalUrl = lessonsUrl.replace('0', levelId);

                fetch(finalUrl)
                    .then(response => {
                        if (!response.ok) {
                            throw new Error('Network response was not ok');
                        }
                        return response.json();
                    })
                    .then(data => {
                        lessonSelect.innerHTML = '<option value="">-- Chọn bài học --</option>'; // Reset
                        data.forEach(lesson => {
                            const option = new Option(lesso.title, lesson.id);
                            lessonSelect.add(option);
                        });
                        lessonSelect.disabled = false; // Kích hoạt lại
                    })
                    .catch(error => {
                        console.error('Lỗi khi lấy danh sách bài học:', error);
                        lessonSelect.innerHTML = '<option value="">-- Lỗi tải dữ liệu --</option>';
                    });
            } else {
                lessonSelect.innerHTML = '<option value="">-- Chọn trình độ trước --</option>';
            }
        });

        // --- Phần 2: Xử lý hiệu ứng nút Submit ---

        form.addEventListener('submit', function (event) {
            // Chỉ thực hiện khi form hợp lệ (theo validation của trình duyệt)
            if (form.checkValidity()) {
                const btn = form.querySelector('button[type="submit"]');
                if (btn) {
                    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span> Đang lưu...';
                    btn.disabled = true;
                }
            }
        });
    });
})();