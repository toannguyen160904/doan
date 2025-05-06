document.addEventListener("DOMContentLoaded", function () {
    const levelSelect = document.getElementById("levelSelect");
    const lessonSelect = document.getElementById("lessonSelect");
    const lessonIdInput = document.getElementById("LessonId");
    const currentLevelId = levelSelect.getAttribute("data-current-level-id");
    const currentLessonId = lessonIdInput.getAttribute("data-current-lesson-id");

    if (currentLevelId) {
        levelSelect.value = currentLevelId;
        loadLessonsByLevel(currentLevelId, currentLessonId);
    }

    levelSelect.addEventListener("change", function () {
        loadLessonsByLevel(this.value, "");
    });

    lessonSelect.addEventListener("change", function () {
        lessonIdInput.value = this.value;
    });

    function loadLessonsByLevel(levelId, selectedLessonId) {
        if (levelId) {
            fetch(`/api/BaihocApi/GetByLevel/${levelId}`)
                .then(res => res.json())
                .then(data => {
                    lessonSelect.innerHTML = '<option value="">-- Chọn bài học --</option>';
                    data.forEach(lesson => {
                        const option = document.createElement("option");
                        option.value = lesson.id;
                        option.textContent = lesson.name;
                        if (lesson.id == selectedLessonId) option.selected = true;
                        lessonSelect.appendChild(option);
                    });
                    lessonIdInput.value = selectedLessonId;
                })
                .catch(error => console.error("Lỗi khi load bài học:", error));
        }
    }
});
