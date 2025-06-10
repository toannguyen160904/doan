// File: wwwroot/js/lesson-details.js

// Đảm bảo code chỉ chạy sau khi toàn bộ trang đã được tải xong
$(document).ready(function () {

    // Bắt sự kiện submit của form có id là 'comment-form'
    $('#comment-form').on('submit', function (e) {

        // Ngăn chặn hành vi mặc định của form là tải lại trang
        e.preventDefault();

        var form = $(this);
        var url = form.attr('action');
        var formData = form.serialize(); // Thu thập dữ liệu từ các input trong form
        var submitButton = $('#submit-comment-btn');

        // Vô hiệu hóa nút gửi và hiển thị spinner để người dùng biết đang xử lý
        submitButton.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Gửi...');

        // Gửi yêu cầu AJAX đến server
        $.ajax({
            type: 'POST',
            url: url,
            data: formData,
            success: function (response) {
                // Hàm này sẽ chạy khi server trả về kết quả thành công (status 200 OK)
                if (response.success) {
                    // Tạo một chuỗi HTML cho bình luận mới dựa trên dữ liệu server trả về
                    var newCommentHtml = `
                        <div class="col">
                            <div class="card h-100 discussion-card animate__animated animate__fadeIn">
                                <div class="card-body">
                                    <div class="d-flex justify-content-between align-items-start">
                                        <h5 class="card-title h6 fw-bold text-primary mb-1">${response.post.tieuDe}</h5>
                                    </div>
                                    <p class="card-text small">${response.post.noiDung}</p>
                                </div>
                                <div class="card-footer bg-transparent border-top-0 pt-0 text-end">
                                    <small class="text-muted">
                                        <strong>${response.post.userName}</strong> - ${response.post.createdAt}
                                    </small>
                                </div>
                            </div>
                        </div>
                    `;

                    // Ẩn thông báo "Chưa có bài viết" nếu nó đang hiển thị
                    $('#no-comments-alert').hide();

                    // Nếu danh sách bình luận chưa tồn tại, tạo nó trước
                    if ($('#discussion-list').length === 0) {
                        $('#discussion-container').prepend('<div class="row row-cols-1 row-cols-lg-2 g-3" id="discussion-list"></div>');
                    }

                    // Thêm bình luận mới vào đầu danh sách
                    $('#discussion-list').prepend(newCommentHtml);

                    // Xóa nội dung trong ô textarea để người dùng có thể viết tiếp
                    $('#comment-content').val('');
                } else {
                    // Hiển thị lỗi từ server nếu có
                    alert(response.message || 'Đã xảy ra lỗi, vui lòng thử lại.');
                }
            },
            error: function (xhr, status, error) {
                // Hàm này sẽ chạy khi có lỗi kết nối hoặc server trả về lỗi (4xx, 5xx)
                if (xhr.status === 401) {
                    alert('Bạn cần đăng nhập để bình luận.');
                } else if (xhr.status === 400) {
                    alert('Nội dung bình luận không hợp lệ.');
                }
                else {
                    alert('Không thể gửi bình luận. Vui lòng kiểm tra lại kết nối mạng.');
                }
            },
            complete: function () {
                // Hàm này luôn chạy sau khi success hoặc error kết thúc
                // Kích hoạt lại nút gửi để người dùng có thể tiếp tục thao tác
                submitButton.prop('disabled', false).html('<i class="fas fa-paper-plane"></i> Gửi');
            }
        });
    });
});