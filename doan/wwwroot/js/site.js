//document.addEventListener("DOMContentLoaded", function () {
//    document.querySelectorAll(".edit-btn").forEach((btn) => {
//        btn.addEventListener("click", function () {
//            let row = this.closest("tr");
//            let inputRow = row.nextElementSibling;
//            let inputs = inputRow.querySelectorAll(".edit-input");
//            let textCells = row.querySelectorAll(".text-cell");

//            // Ẩn tất cả các hàng nhập trước đó
//            document.querySelectorAll(".input-row").forEach(row => {
//                row.style.display = "none";
//            });

//            // Nếu hàng nhập đang ẩn, thì hiển thị và điền giá trị cũ vào input
//            if (inputRow.style.display === "none" || inputRow.style.display === "") {
//                inputRow.style.display = "table-row";
//                inputs.forEach((input, i) => {
//                    input.value = textCells[i].textContent.trim();
//                });
//            } else {
//                inputRow.style.display = "none";
//            }
//        });
//    });

//    document.querySelectorAll(".save-btn").forEach((btn) => {
//        btn.addEventListener("click", function () {
//            let inputRow = this.closest("tr");
//            let row = inputRow.previousElementSibling;
//            let inputs = inputRow.querySelectorAll(".edit-input");
//            let textCells = row.querySelectorAll(".text-cell");

//            // Cập nhật dữ liệu ngay lập tức
//            inputs.forEach((input, i) => {
//                textCells[i].textContent = input.value;
//            });

//            // Ẩn dòng nhập liệu
//            inputRow.style.display = "none";

//            // Gửi dữ liệu cập nhật đến server
//            let id = inputRow.querySelector(".edit-id").value;

//            let tuVung = inputs[0].value;
//            let phatAm = inputs[1].value;
//            let amHan = inputs[2].value;
//            let hanTu = inputs[3].value;
//            let nghia = inputs[4].value;

//            $.ajax({
//                url: '/Home/UpdateVocabulary',
//                type: 'POST',
//                data: {
//                    Id: id,
//                    TuVung: tuVung,
//                    PhatAm: phatAm,
//                    AmHan: amHan,
//                    HanTu: hanTu,
//                    Nghia: nghia
//                },
//                success: function (response) {
//                    if (response.success) {
//                        alert('Cập nhật thành công!');
//                    } else {
//                        alert('Cập nhật thất bại!');
//                    }
//                },
//                error: function (xhr, status, error) {
//                    console.log(xhr.responseText);
//                    alert('Lỗi khi gửi yêu cầu đến server!');
//                }
//            });
//        });
//    });
//});