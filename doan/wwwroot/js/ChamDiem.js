document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("quizForm");

    form.addEventListener("submit", function (e) {
        e.preventDefault();

        const formData = new FormData(form);
        const userAnswers = {};

        for (const [key, value] of formData.entries()) {
            if (key.startsWith("userAnswers[")) {
                const questionId = key.match(/\d+/)[0]; // lấy ID trong [ID]
                userAnswers[questionId] = parseInt(value);
            }
        }

        const quizId = document.getElementById("quizId").value;
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        fetch("/Quiz/ChamDiem", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": token
            },
            body: JSON.stringify({
                quizId: quizId,
                userAnswers: userAnswers
            })
        })
            .then(res => res.json())
            .then(data => {
                if (data.error) {
                    document.getElementById("scoreResult").textContent = "❌ " + data.error;
                } else {
                    document.getElementById("scoreResult").textContent =
                        `Kết quả: ${data.soCauDung}/${data.tongSoCau} câu đúng.`;
                }
            })
            .catch(err => {
                document.getElementById("scoreResult").textContent = "❌ Có lỗi xảy ra.";
                console.error(err);
            });
    });
});
