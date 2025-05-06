document.addEventListener("DOMContentLoaded", function () {
    let params = new URLSearchParams(window.location.search);
    let level = params.get("level") || localStorage.getItem("selectedLevel") || "N5";

    changeLevel(level);
    sortLessons();

    document.getElementById("searchInput").addEventListener("input", searchLesson);
});

function changeLevel(level) {
    history.pushState({}, "", "?level=" + level);
    localStorage.setItem("selectedLevel", level);

    document.getElementById("loadingIcon").classList.remove("hidden");

    setTimeout(() => {
        document.querySelectorAll(".level-section").forEach(section => {
            section.style.display = section.getAttribute('data-level') === level ? "block" : "none";
        });

        document.querySelectorAll(".grid-item").forEach(item => {
            item.style.display = "none";
            item.classList.remove('animate-fadeInUp');
        });

        document.querySelectorAll(`.level-${level}`).forEach((item, index) => {
            item.style.display = "block";
            item.style.animationDelay = (index * 0.1) + 's';
            item.classList.add('animate-fadeInUp');
        });

        document.querySelectorAll(".level-btn").forEach(btn => btn.classList.remove("bg-blue-700"));
        document.querySelector(`button[onclick="changeLevel('${level}')"]`).classList.add("bg-blue-700");

        typeLevelTitle(level);
        document.getElementById("loadingIcon").classList.add("hidden");
    }, 300);
}

function typeLevelTitle(level) {
    let text = level + " - Danh sách bài học";
    let i = 0;
    const target = document.querySelector(`.level-section[data-level="${level}"] .typing-title`);
    target.innerHTML = '';

    function typing() {
        if (i < text.length) {
            target.innerHTML += text.charAt(i);
            i++;
            setTimeout(typing, 50);
        }
    }
    typing();
}

function sortLessons() {
    document.querySelectorAll(".level-section").forEach(section => {
        let lessons = Array.from(section.querySelectorAll(".grid-item"));
        lessons.sort((a, b) => {
            let nameA = a.querySelector(".lesson-title").textContent.trim().toLowerCase();
            let nameB = b.querySelector(".lesson-title").textContent.trim().toLowerCase();
            return nameA.localeCompare(nameB);
        });
        lessons.forEach(lesson => {
            section.querySelector(".grid").appendChild(lesson);
        });
    });
}

function searchLesson() {
    let keyword = document.getElementById("searchInput").value.toLowerCase();
    let currentLevel = localStorage.getItem("selectedLevel") || "N5";

    document.querySelectorAll(".grid-item").forEach(item => {
        if (item.classList.contains("level-" + currentLevel)) {
            let title = item.querySelector(".lesson-title").textContent.toLowerCase();
            item.style.display = title.includes(keyword) ? "block" : "none";
        }
    });
}

function clearSearch() {
    document.getElementById("searchInput").value = "";
    changeLevel(localStorage.getItem("selectedLevel") || "N5");
}
