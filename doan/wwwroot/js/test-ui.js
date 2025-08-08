document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('testForm');
    if (!form) return;

    // Lấy token từ hidden input của AntiForgeryToken
    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    // Ví dụ nếu bạn muốn submit bằng fetch
    form.addEventListener('submit', function (e) {
        e.preventDefault(); // chặn submit mặc định nếu bạn muốn AJAX
        const token = getAntiForgeryToken();

        const formData = new FormData(form);
        const answers = [];

        // Gom dữ liệu từ form vào object
        for (let [key, value] of formData.entries()) {
            if (key.includes("QuestionId")) {
                const index = key.match(/\[(\d+)\]/)[1];
                answers[index] = answers[index] || {};
                answers[index].QuestionId = parseInt(value);
            } else if (key.includes("SelectedAnswer")) {
                const index = key.match(/\[(\d+)\]/)[1];
                answers[index] = answers[index] || {};
                answers[index].SelectedAnswer = value;
            }
        }

        fetch('/Test/Submit', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(answers)
        })
            .then(res => {
                if (res.ok) return res.text();
                throw new Error('Lỗi gửi bài kiểm tra');
            })
            .then(html => {
                document.body.innerHTML = html; // load trang kết quả
            })
            .catch(err => console.error(err));
    });
});
