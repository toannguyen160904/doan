// File: wwwroot/js/quiz-handler.js

document.addEventListener('DOMContentLoaded', function () {
    // Tìm form và ID của bài quiz
    const quizForm = document.getElementById('quizForm');
    const quizIdInput = document.getElementById('quizIdForJs');

    // Nếu không tìm thấy các thành phần cần thiết, dừng lại để tránh lỗi
    if (!quizForm || !quizIdInput) {
        // console.error('Lỗi: Thiếu form#quizForm hoặc input#quizIdForJs trên trang.');
        return;
    }

    const quizId = quizIdInput.value;
    // Tạo một key duy nhất cho mỗi bài quiz để không bị lẫn lộn dữ liệu
    const storageKey = `quizAnswers_${quizId}`;

    // 1. Hàm tải lại các câu trả lời đã lưu khi trang được load
    function loadAnswers() {
        const savedData = localStorage.getItem(storageKey);
        if (savedData) {
            const answers = JSON.parse(savedData);

            // Duyệt qua các câu hỏi đã được trả lời
            Object.keys(answers).forEach(questionId => {
                const answerId = answers[questionId];

                // Tìm đúng radio button dựa trên questionId và answerId
                const radioButton = quizForm.querySelector(`input[name='userAnswers[${questionId}]'][value='${answerId}']`);

                if (radioButton) {
                    radioButton.checked = true;
                }
            });
        }
    }

    // 2. Hàm lưu câu trả lời mỗi khi người dùng chọn một đáp án
    function saveAnswer(event) {
        // Chỉ xử lý khi người dùng click vào một radio button
        if (event.target.type === 'radio' && event.target.name.startsWith('userAnswers[')) {
            const savedData = localStorage.getItem(storageKey);
            const answers = savedData ? JSON.parse(savedData) : {};

            const name = event.target.name;
            const value = event.target.value; // Đây là AnswerId

            // Dùng regex để lấy QuestionId từ thuộc tính name
            const match = name.match(/\[(\d+)\]/);
            if (match) {
                const questionId = match[1];
                // Lưu cặp { questionId: answerId }
                answers[questionId] = value;

                // Cập nhật lại localStorage
                localStorage.setItem(storageKey, JSON.stringify(answers));
            }
        }
    }

    // 3. Hàm xóa dữ liệu đã lưu khi người dùng nộp bài
    function clearSavedAnswers() {
        localStorage.removeItem(storageKey);
    }

    // Gắn các sự kiện vào form
    loadAnswers(); // Chạy ngay khi tải trang xong
    quizForm.addEventListener('change', saveAnswer); // Lưu khi có thay đổi
    quizForm.addEventListener('submit', clearSavedAnswers); // Xóa khi nộp bài
});// File: wwwroot/js/quiz-handler.js

document.addEventListener('DOMContentLoaded', function () {
    // Tìm form và ID của bài quiz
    const quizForm = document.getElementById('quizForm');
    const quizIdInput = document.getElementById('quizIdForJs');

    // Nếu không tìm thấy các thành phần cần thiết, dừng lại để tránh lỗi
    if (!quizForm || !quizIdInput) {
        // console.error('Lỗi: Thiếu form#quizForm hoặc input#quizIdForJs trên trang.');
        return;
    }

    const quizId = quizIdInput.value;
    // Tạo một key duy nhất cho mỗi bài quiz để không bị lẫn lộn dữ liệu
    const storageKey = `quizAnswers_${quizId}`;

    // 1. Hàm tải lại các câu trả lời đã lưu khi trang được load
    function loadAnswers() {
        const savedData = localStorage.getItem(storageKey);
        if (savedData) {
            const answers = JSON.parse(savedData);

            // Duyệt qua các câu hỏi đã được trả lời
            Object.keys(answers).forEach(questionId => {
                const answerId = answers[questionId];

                // Tìm đúng radio button dựa trên questionId và answerId
                const radioButton = quizForm.querySelector(`input[name='userAnswers[${questionId}]'][value='${answerId}']`);

                if (radioButton) {
                    radioButton.checked = true;
                }
            });
        }
    }

    // 2. Hàm lưu câu trả lời mỗi khi người dùng chọn một đáp án
    function saveAnswer(event) {
        // Chỉ xử lý khi người dùng click vào một radio button
        if (event.target.type === 'radio' && event.target.name.startsWith('userAnswers[')) {
            const savedData = localStorage.getItem(storageKey);
            const answers = savedData ? JSON.parse(savedData) : {};

            const name = event.target.name;
            const value = event.target.value; // Đây là AnswerId

            // Dùng regex để lấy QuestionId từ thuộc tính name
            const match = name.match(/\[(\d+)\]/);
            if (match) {
                const questionId = match[1];
                // Lưu cặp { questionId: answerId }
                answers[questionId] = value;

                // Cập nhật lại localStorage
                localStorage.setItem(storageKey, JSON.stringify(answers));
            }
        }
    }

    // 3. Hàm xóa dữ liệu đã lưu khi người dùng nộp bài
    function clearSavedAnswers() {
        localStorage.removeItem(storageKey);
    }

    // Gắn các sự kiện vào form
    loadAnswers(); // Chạy ngay khi tải trang xong
    quizForm.addEventListener('change', saveAnswer); // Lưu khi có thay đổi
    quizForm.addEventListener('submit', clearSavedAnswers); // Xóa khi nộp bài
});