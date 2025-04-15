let currentIndex = 0;
let flashcards = [];

document.addEventListener("DOMContentLoaded", () => {
    flashcards = Array.from(document.querySelectorAll(".flashcard"));
    flashcards.forEach(card => {
        card.style.opacity = 0;
        card.style.transition = "opacity 0.5s ease";
    });
    showCard(currentIndex, true);
});

function showCard(index, immediate = false) {
    flashcards.forEach((card, i) => {
        if (i === index) {
            card.style.display = "block";
            if (immediate) {
                card.style.opacity = 1;
            } else {
                requestAnimationFrame(() => {
                    card.style.opacity = 1;
                });
            }
        } else {
            card.style.opacity = 0;
            setTimeout(() => {
                card.style.display = "none";
                card.classList.remove("flip");
            }, 500); // chờ opacity ẩn xong mới ẩn hẳn
        }
    });
}

function nextCard() {
    if (currentIndex < flashcards.length - 1) {
        currentIndex++;
        showCard(currentIndex);
    } else {
        alert("🎉 Bạn đã xem hết các thẻ!");
    }
}

function prevCard() {
    if (currentIndex > 0) {
        currentIndex--;
        showCard(currentIndex);
    } else {
        alert("⛔ Đây là thẻ đầu tiên rồi!");
    }
}

function flipCard(cardElement) {
    cardElement.classList.toggle("flip");
}
