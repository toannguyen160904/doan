$(document).ready(function () {
    var levelFilter = $('#levelFilter');
    var lessonFilter = $('#lessonFilter');
    var grammarContainer = $('#grammarListContainer');

    // --- HÀM LỌC CHÍNH ---
    function filterGrammar() {
        var levelId = levelFilter.val();
        var lessonId = lessonFilter.val();

        // Hiệu ứng loading
        grammarContainer.html('<div class="text-center p-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>');

        // Gọi Partial View
        $.get('/Admin/Grammars/_GrammarListPartial', { levelId: levelId, lessonId: lessonId }, function (data) {
            grammarContainer.html(data);
        });
    }

    // Khi thay đổi cấp độ
    levelFilter.on('change', function () {
        var selectedLevelId = $(this).val();
        lessonFilter.prop('disabled', true).html('<option value="0">-- Đang tải... --</option>');

        if (selectedLevelId && selectedLevelId !== "0") {
            $.getJSON(`/Admin/Grammars/GetByLevel/${selectedLevelId}`, function (data) {
                lessonFilter.html('<option value="0" selected>-- Tất cả Bài học --</option>');
                if (data && data.length > 0) {
                    $.each(data, function (index, item) {
                        lessonFilter.append($('<option>', {
                            value: item.id,
                            text: item.name
                        }));
                    });
                }
                lessonFilter.prop('disabled', false);
            });
        } else {
            lessonFilter.html('<option value="0" selected>-- Tất cả Bài học --</option>');
            lessonFilter.prop('disabled', false);
        }

        filterGrammar();
    });

    // Khi thay đổi bài học
    lessonFilter.on('change', function () {
        filterGrammar();
    });

    // Reset bộ lọc
    $('#resetFilter').on('click', function () {
        levelFilter.val('0');
        lessonFilter.html('<option value="0" selected>-- Tất cả Bài học --</option>');
        filterGrammar();
    });
});
