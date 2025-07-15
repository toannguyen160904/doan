using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models.DTO
{
    public class CreatePostDto
    {
        public int BaiHocId { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }

        public string? UserId { get; set; }
    }
}
