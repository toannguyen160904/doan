using System;
using System.Collections.Generic;
using SharedModels.Models;

namespace SharedModels.Models.ViewModels
{
    public class LessonDetailsViewModel
    {
        // Thông tin bài học
        public Baihoc Lesson { get; set; }

        // Flashcards liên quan đến từ vựng hoặc ngữ pháp
        public List<flashcards> Flashcards { get; set; } = new();

        // Tất cả bài viết trong diễn đàn của bài học
        public List<Diendanmodel> Diendan { get; set; } = new();

        // Danh sách từ vựng theo trang
        public List<Vocabulary> PagedVocabularies { get; set; } = new();
        public int CurrentVocabularyPage { get; set; }
        public int TotalVocabularyPages { get; set; }

        // Danh sách bình luận theo trang
        public List<Diendanmodel> PagedComments { get; set; } = new();
        public int CurrentCommentPage { get; set; }
        public int TotalCommentPages { get; set; }

        // Danh sách ID từ vựng đã học của user
        public HashSet<int> LearnedVocabularyIds { get; set; } = new();
    }
}
