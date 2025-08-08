document.addEventListener("DOMContentLoaded", async function () {
    // ====== PHẦN 1: Level, Sort, Search ======
    let params = new URLSearchParams(window.location.search);
    let level = params.get("level") || localStorage.getItem("selectedLevel") || "N5";

    changeLevel(level);
    sortLessons();

    document.getElementById("searchInput").addEventListener("input", searchLesson);

    // ====== PHẦN 2: Học từ vựng ======
    const userId = document.getElementById('currentUserId')?.value;
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    let learnedIds = [];
    try {
        const res = await fetch('/Learning/LearnedVocabIds', { credentials: 'same-origin' });
        if (res.ok) learnedIds = await res.json();
    } catch (err) {
        console.error('Không thể tải danh sách từ đã học:', err);
    }

    document.querySelectorAll('.btn-mark-learned').forEach(btn => {
        const vocabId = parseInt(btn.dataset.vocabId, 10);
        const isLearned = learnedIds.includes(vocabId);
        updateButtonStyle(btn, isLearned);

        btn.addEventListener('click', async () => {
            const currentlyLearned = btn.dataset.learned === 'true';
            const url = currentlyLearned ? '/Learning/MarkUnlearned' : '/Learning/MarkLearned';

            const resp = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ vocabId })
            });

            if (!resp.ok) {
                const text = await resp.text();
                alert(`Lỗi ${resp.status}: ${text || 'Có lỗi khi cập nhật trạng thái.'}`);
                return;
            }

            updateButtonStyle(btn, !currentlyLearned);
        });
    });

    function updateButtonStyle(button, learned) {
        button.dataset.learned = String(learned);
        if (learned) {
            button.textContent = '🔁 Chưa học';
            button.classList.remove('btn-outline-success');
            button.classList.add('btn-warning');
        } else {
            button.textContent = '✅ Đã học';
            button.classList.remove('btn-warning');
            button.classList.add('btn-outline-success');
        }
    }

    // ====== PHẦN 3: Gửi bình luận ======
    $('#comment-form').on('submit', function (e) {
        e.preventDefault();

        const baiHocId = parseInt($('input[name="BaiHocId"]').val());
        const noiDung = $('#comment-content').val().trim();
        const submitButton = $('#submit-comment-btn');

        if (!noiDung) {
            alert("Vui lòng nhập nội dung bình luận.");
            return;
        }

        submitButton.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> Gửi...');

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
            success: function (response) {
                if (response.success) {
                    const post = response.post;
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
                    $('#no-comments-alert').hide();
                    $('#discussion-list').prepend(newCommentHtml);
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
                    alert('Không thể gửi bình luận.');
                }
            },
            complete: function () {
                submitButton.prop('disabled', false).html('<i class="fas fa-paper-plane"></i> Gửi');
            }
        });
    });
});

// ====== Các hàm ban đầu ======
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
