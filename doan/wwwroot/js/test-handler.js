document.addEventListener('DOMContentLoaded', function () {
    // Tìm form bằng ID.
    const form = document.getElementById('testForm');

    // Nếu không tìm thấy form, dừng thực thi để tránh lỗi.
    if (!form) {
        console.error('Lỗi: Không tìm thấy form có id="testForm".');
        return;
    }

    const storageKey = 'userTestAnswers';

    // 1. Hàm tải lại các câu trả lời đã lưu khi trang được load
    function loadAnswers() {
        const savedAnswers = localStorage.getItem(storageKey);
        if (savedAnswers) {
            const answers = JSON.parse(savedAnswers);

            Object.keys(answers).forEach(questionIndex => {
                const selectedValue = answers[questionIndex];

                // Escape các ký tự đặc biệt trong value để querySelector hoạt động đúng
                const escapedValue = CSS.escape(selectedValue);
                const radioButton = form.querySelector(`input[name='answers[${questionIndex}].SelectedAnswer'][value="${escapedValue}"]`);

                if (radioButton) {
                    radioButton.checked = true;
                }
            });
        }
    }

    // 2. Hàm lưu câu trả lời mỗi khi người dùng chọn
    function saveAnswer(event) {
        if (event.target.type === 'radio' && event.target.name.startsWith('answers[')) {
            const savedAnswers = localStorage.getItem(storageKey);
            const answers = savedAnswers ? JSON.parse(savedAnswers) : {};

            const name = event.target.name;
            const match = name.match(/\[(\d+)\]/);

            if (match) {
                const questionIndex = match[1];
                answers[questionIndex] = event.target.value;

                localStorage.setItem(storageKey, JSON.stringify(answers));
            }
        }
    }

    // 3. Hàm xóa dữ liệu đã lưu khi nộp bài
    function clearSavedAnswers() {
        localStorage.removeItem(storageKey);
    }

    // Gắn các sự kiện
    loadAnswers(); // Chạy ngay khi tải trang xong để khôi phục bài làm
    form.addEventListener('change', saveAnswer); // Lưu khi có thay đổi
    form.addEventListener('submit', clearSavedAnswers); // Xóa khi nộp bài
});