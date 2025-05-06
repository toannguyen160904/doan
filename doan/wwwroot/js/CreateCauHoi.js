document.addEventListener("DOMContentLoaded", function () {
    let questionIndex = 0;

    // Add new question
    document.getElementById("addQuestionButton").addEventListener("click", function () {
        const questionContainer = document.getElementById("questionContainer");
        const newQuestion = document.createElement("div");
        newQuestion.className = "question-card";
        newQuestion.setAttribute("data-question-index", questionIndex);

        newQuestion.innerHTML = `
            <div class="question-header">
                <div class="question-content">
                    <input type="text" 
                           class="question-input"
                           name="Questions[${questionIndex}].NoiDung" 
                           placeholder="Câu hỏi không có tiêu đề"
                           required />
                    <div class="required-text">* Bắt buộc</div>
                </div>
                <div class="question-actions">
                    <button type="button" class="btn btn-outline-danger btn-sm delete-question" title="Xóa câu hỏi">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </div>

            <div class="answers-section" id="answersContainer_${questionIndex}" data-question-index="${questionIndex}">
                <!-- Answers will be added here -->
            </div>

            <div class="answer-controls">
                <button type="button" 
                        class="add-answer-btn" 
                        data-question-index="${questionIndex}">
                    <i class="bi bi-plus-circle"></i>
                    Thêm câu trả lời
                </button>
            </div>
        `;

        questionContainer.appendChild(newQuestion);

        // Add first two answers automatically
        addAnswer(questionIndex);
        addAnswer(questionIndex);

        // Focus on the new question input
        newQuestion.querySelector('.question-input').focus();

        questionIndex++;
    });

    // Add new answer
    function addAnswer(questionIndex) {
        const answersContainer = document.getElementById(`answersContainer_${questionIndex}`);
        const answerIndex = answersContainer.children.length;

        const newAnswer = document.createElement("div");
        newAnswer.className = "answer-item";
        newAnswer.innerHTML = `
            <div class="answer-content">
                <div class="answer-checkbox-wrapper">
                    <input type="checkbox"
                           class="form-check-input answer-checkbox"
                           name="Questions[${questionIndex}].CauTraLois[${answerIndex}].IsCorrect"
                           value="true" />
                </div>
                <div class="answer-input-wrapper">
                    <input type="text" 
                           class="answer-input"
                           name="Questions[${questionIndex}].CauTraLois[${answerIndex}].NoiDung"
                           placeholder="Câu trả lời ${answerIndex + 1}"
                           required />
                </div>
                <div class="answer-actions">
                    <button type="button" class="delete-answer-btn" title="Xóa câu trả lời">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </div>
            </div>
        `;

        answersContainer.appendChild(newAnswer);

        // Add visual feedback when checkbox is checked
        const checkbox = newAnswer.querySelector('.answer-checkbox');
        checkbox.addEventListener('change', function() {
            const answerItem = this.closest('.answer-item');
            if (this.checked) {
                answerItem.classList.add('correct-answer');
            } else {
                answerItem.classList.remove('correct-answer');
            }
        });

        // Focus on the new answer input
        newAnswer.querySelector('.answer-input').focus();
    }

    // Event delegation for adding answers
    document.addEventListener("click", function (e) {
        if (e.target.closest('.add-answer-btn')) {
            const button = e.target.closest('.add-answer-btn');
            const questionIndex = button.getAttribute("data-question-index");
            addAnswer(questionIndex);
        }
    });

    // Event delegation for deleting questions and answers
    document.addEventListener("click", function (e) {
        // Delete question
        if (e.target.closest('.delete-question')) {
            const questionCard = e.target.closest('.question-card');
            if (document.querySelectorAll('.question-card').length > 1) {
                questionCard.remove();
                updateQuestionNumbers();
            } else {
                alert('Phải có ít nhất một câu hỏi trong quiz!');
            }
        }

        // Delete answer
        if (e.target.closest('.delete-answer-btn')) {
            const answerItem = e.target.closest('.answer-item');
            const answersContainer = answerItem.closest('.answers-section');
            
            if (answersContainer.children.length > 2) {
                answerItem.remove();
                updateAnswerIndices(answersContainer);
            } else {
                alert('Mỗi câu hỏi phải có ít nhất hai câu trả lời!');
            }
        }
    });

    // Update question numbers after deletion
    function updateQuestionNumbers() {
        document.querySelectorAll('.question-card').forEach((card, index) => {
            // Update question index attribute
            card.setAttribute('data-question-index', index);
            
            // Update question input name
            const questionInput = card.querySelector('.question-input');
            questionInput.name = `Questions[${index}].NoiDung`;
            
            // Update answers container id
            const answersContainer = card.querySelector('.answers-section');
            answersContainer.id = `answersContainer_${index}`;
            answersContainer.setAttribute('data-question-index', index);
            
            // Update add answer button
            const addAnswerBtn = card.querySelector('.add-answer-btn');
            addAnswerBtn.setAttribute('data-question-index', index);
            
            updateAnswerIndices(answersContainer);
        });
    }

    // Update answer indices after deletion
    function updateAnswerIndices(container) {
        const questionIndex = container.getAttribute('data-question-index');
        const answers = container.querySelectorAll('.answer-item');
        
        answers.forEach((answer, index) => {
            const textInput = answer.querySelector('.answer-input');
            const checkbox = answer.querySelector('.answer-checkbox');
            
            textInput.name = `Questions[${questionIndex}].CauTraLois[${index}].NoiDung`;
            checkbox.name = `Questions[${questionIndex}].CauTraLois[${index}].IsCorrect`;
            textInput.placeholder = `Câu trả lời ${index + 1}`;
        });
    }

    // Form submission
    document.getElementById('quizForm').addEventListener('submit', function(e) {
        e.preventDefault();

        // Validate questions and answers
        const questions = document.querySelectorAll('.question-card');
        let isValid = true;

        questions.forEach((question, index) => {
            // Check if at least one answer is marked as correct
            const hasCorrectAnswer = question.querySelector('.answer-checkbox:checked');
            if (!hasCorrectAnswer) {
                alert(`Vui lòng chọn ít nhất một câu trả lời đúng cho câu hỏi ${index + 1}`);
                isValid = false;
                return;
            }

            // Check if question has content
            const questionInput = question.querySelector('.question-input');
            if (!questionInput.value.trim()) {
                alert(`Vui lòng nhập nội dung cho câu hỏi ${index + 1}`);
                isValid = false;
                questionInput.focus();
                return;
            }

            // Check if all answers have content
            const answerInputs = question.querySelectorAll('.answer-input');
            answerInputs.forEach((input, answerIndex) => {
                if (!input.value.trim()) {
                    alert(`Vui lòng nhập nội dung cho câu trả lời ${answerIndex + 1} của câu hỏi ${index + 1}`);
                    isValid = false;
                    input.focus();
                    return;
                }
            });
        });

        if (!isValid) return;

        // Handle form submission
        const formData = new FormData(this);
        
        // Ensure unchecked checkboxes are included as false
        questions.forEach((question, qIndex) => {
            const answers = question.querySelectorAll('.answer-item');
            answers.forEach((answer, aIndex) => {
                const checkbox = answer.querySelector('.answer-checkbox');
                if (!checkbox.checked) {
                    formData.append(`Questions[${qIndex}].CauTraLois[${aIndex}].IsCorrect`, "false");
                }
            });
        });

        // Submit the form
        const hiddenForm = document.createElement('form');
        hiddenForm.method = 'post';
        hiddenForm.action = this.action;
        
        for (const [key, value] of formData) {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = key;
            input.value = value;
            hiddenForm.appendChild(input);
        }
        
        document.body.appendChild(hiddenForm);
        hiddenForm.submit();
    });

    // Add first question automatically when page loads
    document.getElementById('addQuestionButton').click();
});






//<input type="hidden" name="Questions[${questionIndex}].CauTraLois[0].IsCorrect" value="false" />
//<input type="checkbox" name="Questions[${questionIndex}].CauTraLois[0].IsCorrect" class="form-check-input me-2" value="true" />

 //<input type="hidden" name="Questions[0].CauTraLois[0].IsCorrect" value="false" />
 //               <input type="checkbox" name="Questions[0].CauTraLois[0].IsCorrect" value="true" />





//document.addEventListener("DOMContentLoaded", function () {
//    let questionIndex = 0;

//    // Thêm câu hỏi mới
//    document.getElementById("addQuestionButton").addEventListener("click", function () {
//        const questionContainer = document.getElementById("questionContainer");
//        const newQuestion = document.createElement("div");
//        newQuestion.className = "question-item mb-4";
//        newQuestion.setAttribute("data-question-index", questionIndex);

//        newQuestion.innerHTML = `
//            <div class="mb-3">
//                <label for="question_${questionIndex}" class="form-label">Nội dung câu hỏi</label>
//                <input type="text" class="form-control" name="Questions[${questionIndex}].NoiDung" placeholder="Nhập nội dung câu hỏi" required />
//            </div>
//          <div id="answersContainer_${questionIndex}" class="answers-container">
//                <div class="answer-item d-flex align-items-center mb-2">
//                    <input type="text" name="Questions[${questionIndex}].CauTraLois[0].NoiDung" class="form-control me-2" placeholder="Nhập câu trả lời" required />
//                    <input type="hidden" name="Questions[${questionIndex}].CauTraLois[0].IsCorrect" value="false" />
//                    <input type="checkbox" name="Questions[${questionIndex}].CauTraLois[0].IsCorrect" value="true" class="form-check-input me-2" />
//                    <button type="button" class="btn btn-danger btn-sm remove-answer">
//                        <i class="bi bi-x"></i>
//                    </button>
//                </div>
//            </div>

//            <button type="button" class="btn btn-secondary btn-sm addAnswerButton" data-question-index="${questionIndex}">
//                Thêm câu trả lời
//            </button>
//        `;

//        questionContainer.appendChild(newQuestion);
//        questionIndex++;
//    });

//    // Thêm câu trả lời mới
//    document.addEventListener("click", function (e) {
//        if (e.target && e.target.classList.contains("addAnswerButton")) {
//            const questionIndex = e.target.getAttribute("data-question-index");
//            const answersContainer = document.getElementById(`answersContainer_${questionIndex}`);
//            const answerIndex = answersContainer.children.length;

//            const newAnswer = document.createElement("div");
//            newAnswer.className = "answer-item d-flex align-items-center mb-2";
//            newAnswer.innerHTML = `
//                <input type="text" name="Questions[${questionIndex}].CauTraLois[${answerIndex}].NoiDung" class="form-control me-2" placeholder="Nhập câu trả lời" required />
//                <input type="checkbox" name="Questions[${questionIndex}].CauTraLois[${answerIndex}].IsCorrect" class="form-check-input me-2" />
//                <button type="button" class="btn btn-danger btn-sm remove-answer">
//                    <i class="bi bi-x"></i>
//                </button>
//            `;

//            answersContainer.appendChild(newAnswer);
//        }
//    });

//    // Xóa câu trả lời
//    document.addEventListener("click", function (e) {
//        if (e.target && e.target.classList.contains("remove-answer")) {
//            e.target.closest(".answer-item").remove();
//        }
//    });
//});


//function capNhatDapAnDung(checkbox) {
//    const cauTraLoiId = checkbox.getAttribute("data-id");
//    const isCorrect = checkbox.checked;

//    if (!cauTraLoiId) return;

//    fetch('/Quiz/CapNhatDapAnDung', {
//        method: 'POST',
//        headers: {
//            'Content-Type': 'application/json',
//            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
//        },
//        body: JSON.stringify({ cauTraLoiId, isCorrect })
//    })
//        .then(response => {
//            if (response.ok) {
//                console.log("Cập nhật thành công");
//            } else {
//                console.error("Lỗi khi cập nhật");
//            }
//        });
//}

