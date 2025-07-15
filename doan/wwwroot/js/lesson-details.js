document.addEventListener('DOMContentLoaded', async function () {
    const userId = document.getElementById('currentUserId')?.value;
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    // ========== PHẦN 1: Trạng thái học từ vựng ==========
    let learnedIds = [];
    try {
        const res = await fetch(`/api/LearningApi/LearnedVocabIds/${userId}`);
        if (res.ok) {
            learnedIds = await res.json();
        }
    } catch (error) {
        console.error("Không thể tải danh sách từ đã học:", error);
    }

    document.querySelectorAll('.btn-mark-learned').forEach(btn => {
        const vocabId = parseInt(btn.dataset.vocabId);
        const isLearned = learnedIds.includes(vocabId);
        updateButtonStyle(btn, isLearned);

        btn.addEventListener('click', async function () {
            const isCurrentlyLearned = btn.textContent.includes('Chưa học');
            const apiUrl = isCurrentlyLearned ? '/api/LearningApi/MarkUnlearned' : '/api/LearningApi/MarkLearned';

            const response = await fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({
                    VocabId: vocabId,
                    UserId: userId
                })
            });

            if (response.ok) {
                updateButtonStyle(btn, !isCurrentlyLearned);
            } else {
                alert("Có lỗi khi cập nhật trạng thái.");
            }
        });
    });

    function updateButtonStyle(button, isLearned) {
        if (isLearned) {
            button.textContent = '🔁 Chưa học';
            button.classList.remove('btn-outline-success');
            button.classList.add('btn-warning');
        } else {
            button.textContent = '✅ Đã học';
            button.classList.remove('btn-warning');
            button.classList.add('btn-outline-success');
        }
    }

    // ========== PHẦN 2: Bình luận ==========
    $('#comment-form').on('submit', function (e) {
        e.preventDefault();

        const baiHocId = parseInt($('input[name="BaiHocId"]').val());
        const noiDung = $('#comment-content').val().trim();
        const submitButton = $('#submit-comment-btn');

        if (!noiDung) {
            alert("Vui lòng nhập nội dung bình luận.");
            return;
        }

        submitButton.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Gửi...');

        $.ajax({
            type: 'POST',
            url: '/Lesson/DangBai',
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': token
            },
            data: JSON.stringify({
                BaiHocId: baiHocId,
                TieuDe: "Bình luận bài học",
                NoiDung: noiDung,
                UserId: userId
            }),
            success: function (response) {
                if (response.success) {
                    const post = response.post; // Lấy thông tin bình luận mới từ server

                    // Tạo HTML cho bình luận mới (SỬ DỤNG THÔNG TIN TỪ SERVER)
                    const newCommentHtml = `
            <div class="col discussion-item-${post.id}">
                <div class="card h-100 discussion-card animate__animated animate__fadeIn">
                    <div class="card-body">
                        <h5 class="card-title h6 fw-bold text-primary mb-1">${post.tieuDe}</h5>
                        <p class="card-text small">${post.noiDung}</p>
                    </div>
                    <div class="card-footer bg-transparent border-top-0 pt-0 text-end">
                        <small class="text-muted">
                            <strong>${post.userName}</strong> - ${new Date(post.createdAt).toLocaleString()}
                        </small>
                    </div>
                </div>
            </div>`;

                    // Ẩn thông báo "Chưa có bình luận nào"
                    $('#no-comments-alert').hide();

                    // Thêm bình luận mới vào danh sách (SAU KHI NHẬN PHẢN HỒI TỪ SERVER)
                    $('#discussion-list').prepend(newCommentHtml);

                    // Xóa nội dung textarea
                    $('#comment-content').val('');

                } else {
                    alert(response.message || 'Đã xảy ra lỗi, vui lòng thử lại.');
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
                submitButton.prop('disabled', false).html('<i class="fas fa-paper-plane"></i> Gửi');
            }
        });
    });
});
