// /wwwroot/js/test-ui.js
document.addEventListener('DOMContentLoaded', () => {
  const form = document.getElementById('testForm');
  if (!form) return;

  // --- Progress bar ---
  const bar = document.getElementById('test-progress-bar');
  const total = Number(bar?.dataset.totalQuestions || 0);

  function updateProgress() {
    // đếm số câu đã chọn (mỗi câu có 1 radio được chọn)
    const checked = form.querySelectorAll('input[type="radio"][name*=".SelectedAnswer"]:checked').length;
    const pct = total > 0 ? Math.round((checked / total) * 100) : 0;
    if (bar) {
      bar.style.width = pct + '%';
      bar.setAttribute('aria-valuenow', String(checked));
    }
  }

  form.addEventListener('change', (e) => {
    if ((e.target instanceof HTMLInputElement) && e.target.type === 'radio') {
      updateProgress();
    }
  });

  // --- Anti-forgery token (đã có @Html.AntiForgeryToken trong form) ---
  function getAntiForgeryToken() {
    const el = form.querySelector('input[name="__RequestVerificationToken"]');
    return el ? el.value : '';
  }

  // --- Thu thập answers và submit JSON ---
  form.addEventListener('submit', async (e) => {
    e.preventDefault();

    // Gom dữ liệu bằng FormData theo đúng tên input "answers[i].*"
    const fd = new FormData(form);
    const answers = [];

    for (const [key, value] of fd.entries()) {
        const m = key.match(/^answers\[(\d+)\]\.(QuestionId|SelectedAnswer)$/);
      if (!m) continue;
      const idx = parseInt(m[1], 10);
      const field = m[2];

      answers[idx] = answers[idx] || {};
      if (field === 'QuestionId') {
        answers[idx].QuestionId = parseInt(String(value), 10);
      } else if (field === 'SelectedAnswer') {
        // value của radio đang là index (0..n) -> gửi dạng string như DTO định nghĩa
          answers[idx].SelectedIndex = String(value);
      }
    }

    // Nếu còn câu chưa chọn thì báo
      const answered = answers.filter(a => a && a.SelectedIndex !== undefined).length;
    if (total && answered < total) {
      const ok = confirm(`Bạn mới chọn ${answered}/${total} câu. Bạn vẫn muốn nộp bài?`);
      if (!ok) return;
    }

    const token = getAntiForgeryToken();

    try {
      const res = await fetch('/Test/Submit', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'RequestVerificationToken': token
        },
        body: JSON.stringify(answers)
      });

      if (!res.ok) {
        const txt = await res.text();
        console.error('Submit failed', res.status, txt);
        alert('Lỗi gửi bài kiểm tra. Vui lòng thử lại.');
        return;
      }

      // Server trả về HTML của View "Result" -> thay vào body
      const html = await res.text();
      document.open();
      document.write(html);
      document.close();
    } catch (err) {
      console.error(err);
      alert('Có lỗi mạng khi gửi bài. Vui lòng thử lại.');
    }
  });

  // Khởi tạo tiến trình lần đầu
  updateProgress();
});
