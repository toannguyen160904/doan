document.addEventListener("DOMContentLoaded", function () {
    const levelSelect = document.getElementById("levelSelect");
    const lessonSelect = document.getElementById("lessonSelect");
    const lessonIdInput = document.getElementById("LessonId");

    if (levelSelect) {
        levelSelect.addEventListener("change", function () {
            const levelId = this.value;
            fetch(`/api/BaihocApi/GetByLevel/${levelId}`)
                .then(response => response.json())
                .then(data => {
                    lessonSelect.innerHTML = '<option value="">-- Chọn bài học --</option>';
                    data.forEach(lesson => {
                        const option = document.createElement("option");
                        option.value = lesson.id;
                        option.textContent = lesson.name;
                        lessonSelect.appendChild(option);
                    });
                    lessonIdInput.value = "";
                })
                .catch(error => console.error("Lỗi khi tải bài học:", error));
        });
    }

    if (lessonSelect) {
        lessonSelect.addEventListener("change", function () {
            lessonIdInput.value = this.value;
        });
    }
});
