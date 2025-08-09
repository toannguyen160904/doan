document.addEventListener('DOMContentLoaded', async function () {
    const userId = document.getElementById('currentUserId')?.value;
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const API_BASE = 'https://localhost:7191';

    // ===== PHẦN 1: Trạng thái học từ vựng (chỉ chạy nếu có bảng) =====
    const table = document.getElementById('new-words-table');
    if (table) {
        // Lấy danh sách từ đã học
        let learnedIds = [];
        try {
            const res = await fetch(`${API_BASE}/api/LearningApi/LearnedVocabIds/${userId}`, {
                credentials: 'include'
            });
            if (res.ok) {
                learnedIds = await res.json();
            } else {
                console.warn('Load LearnedVocabIds lỗi:', res.status);
            }
        } catch (error) {
            console.error("Không thể tải danh sách từ đã học:", error);
        }

        // Cập nhật UI
        function updateRowUI(row, learned) {
            const btn = row.querySelector('.btn-mark-learned');
            const badge = row.querySelector('.status-badge');
            if (!btn || !badge) return;

            btn.dataset.learned = learned ? 'true' : 'false';

            if (learned) {
                btn.classList.remove('btn-outline-success');
                btn.classList.add('btn-warning');
                btn.textContent = 'Đánh dấu chưa học';
                badge.className = 'badge bg-success status-badge';
                badge.textContent = '✅ Đã học';
            } else {
                btn.classList.remove('btn-warning');
                btn.classList.add('btn-outline-success');
                btn.textContent = 'Đánh dấu đã học';
                badge.className = 'badge bg-secondary status-badge';
                badge.textContent = '❌ Chưa học';
            }
        }

        // Khởi tạo trạng thái
        table.querySelectorAll('tbody tr').forEach(row => {
            const vocabId = Number(row.querySelector('.btn-mark-learned')?.dataset.vocabId);
            const learned = learnedIds.includes(vocabId);
            updateRowUI(row, learned);
        });

        // Toggle trạng thái
        table.addEventListener('click', async (e) => {
            const btn = e.target.closest('.btn-mark-learned');
            if (!btn) return;

            const vocabId = Number(btn.dataset.vocabId);
            const learned = btn.dataset.learned === 'true';

            const apiUrl = learned
                ? `${API_BASE}/api/LearningApi/MarkUnlearned`
                : `${API_BASE}/api/LearningApi/MarkLearned`;

            try {
                const response = await fetch(apiUrl, {
                    method: 'POST',
                    credentials: 'include',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: JSON.stringify({ VocabId: vocabId, UserId: userId })
                });

                if (response.ok) {
                    const row = btn.closest('tr');
                    updateRowUI(row, !learned);
                } else if (response.status === 401) {
                    alert('⚠️ Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.');
                    window.location.href = '/Account/Login';
                } else {
                    const txt = await response.text().catch(() => '');
                    console.error('Cập nhật trạng thái lỗi:', response.status, txt);
                    alert("Có lỗi khi cập nhật trạng thái.");
                }
            } catch (error) {
                console.error('Lỗi kết nối:', error);
                alert('Không thể kết nối máy chủ.');
            }
        });
    }

    // ===== PHẦN 2: Bình luận (luôn gắn handler, không phụ thuộc bảng) =====
    $('#comment-form').on('submit', function (e) {
        e.preventDefault();

        const baiHocId = parseInt($('input[name="BaiHocId"]').val());
        const noiDung = $('#comment-content').val().trim();
        const submitButton = $('#submit-comment-btn');

        if (!noiDung) {
            alert("Vui lòng nhập nội dung bình luận.");
            return;
        }

        submitButton.prop('disabled', true)
            .html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Gửi...');

        $.ajax({
            type: 'POST',
            url: '/Lesson/DangBai',
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': token },
            data: JSON.stringify({
                BaiHocId: baiHocId,
                TieuDe: "Bình luận bài học",
                NoiDung: noiDung,
                UserId: userId
            }),
            xhrFields: { withCredentials: true },
            success: function (response) {
                if (response.success) {
                    location.reload();
                } else {
                    alert(response.message || 'Không thể gửi bình luận.');
                }
            },
            error: function (xhr) {
                if (xhr.status === 401) {
                    alert('⚠️ Bạn cần đăng nhập để bình luận.');
                } else {
                    console.error("Lỗi chi tiết:", xhr.responseText);
                    alert('Không thể gửi bình luận. Kiểm tra kết nối mạng hoặc thử lại.');
                }
            },
            complete: function () {
                submitButton.prop('disabled', false)
                    .html('<i class="fas fa-paper-plane"></i> Gửi');
            }
        });
    });
});
