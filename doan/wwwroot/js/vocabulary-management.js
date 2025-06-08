$(document).ready(function () {
    var levelFilter = $('#levelFilter');
    var lessonFilter = $('#lessonFilter');
    var container = $('#vocabularyListContainer');

    function filterVocab() {
        var levelId = levelFilter.val();
        var lessonId = lessonFilter.val();

        container.html('<div class="text-center p-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>');

        $.get('/Admin/Vocabularies/_VocabularyListPartial', { levelId: levelId, lessonId: lessonId }, function (data) {
            container.html(data);
        });
    }

    levelFilter.on('change', function () {
        var levelId = $(this).val();

        lessonFilter.prop('disabled', true).html('<option value="0">-- Đang tải... --</option>');

        if (levelId !== "0") {
            $.getJSON(`/Admin/Vocabularies/GetLessonsByLevel/${levelId}`, function (data) {
                lessonFilter.html('<option value="0">-- Tất cả Bài học --</option>');
                $.each(data, function (i, item) {
                    lessonFilter.append($('<option>', {
                        value: item.id,
                        text: item.name
                    }));
                });
                lessonFilter.prop('disabled', false);
            });
        } else {
            lessonFilter.html('<option value="0">-- Tất cả Bài học --</option>');
            lessonFilter.prop('disabled', false);
        }

        filterVocab();
    });

    lessonFilter.on('change', function () {
        filterVocab();
    });

    $('#resetFilter').on('click', function () {
        levelFilter.val('0');
        lessonFilter.html('<option value="0" selected>-- Tất cả Bài học --</option>');
        filterVocab();
    });
});
