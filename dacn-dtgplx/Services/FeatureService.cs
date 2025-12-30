using dacn_dtgplx.ViewModels;

namespace dacn_dtgplx.Services
{
    public static class FeatureService
    {
        private static FeatureItem F(
            string title, 
            string desc, 
            string url, 
            params int[] roles
        ) => new()
        {
            Title = title,
            Description = desc,
            Url = url,
            Roles = roles.ToList()
        };

        public static List<FeatureItem> All = new()
        {
            F("Trang chủ","Trang chính của hệ thống","/",0,2,3),
            F("Hồ sơ của tôi","Quản lý hồ sơ học viên","/HoSoThiSinh/MyProfile",2),
            F("Danh sách khóa học","Xem các khóa học đang mở","/KhoaHoc",0,2,3),
            F("Thời khóa biểu","Xem lịch học","/ThoiKhoaBieu",2),
            F("Thuê xe tập lái","Các xe tập lái trong trung tâm","/ThueXe",0,2,3),
            F("Thêm hồ sơ mới","Thêm hồ sơ mới","/HoSoThiSinh/Create",2),
            F("Tài liệu & Ôn tập","Tài liệu & Ôn tập","/Hoc",0,2,3),
            F("Chatbot","Chatbot chuyên về GPLX","/ChatBot",0,1,2,3)
            ,//admin
            F("Khóa học", "Danh sách khóa học", "/admin/courses", 1),
            F("Thêm khóa học", "Thêm khóa học vào danh sách", "/admin/courses/create", 1),
            F("Lịch dạy", "Quản lý lịch dạy", "/LichDayGv", 1),
            F("Câu hỏi trắc nghiệm", "Danh sách câu hỏi trắc nghiệm", "/AdminTheoryQuestions", 1),
            F("Bộ đề trắc nghiệm", "Bộ đề chuẩn theo cấu trúc của bộ", "/AdminExamSets", 1),
            F("Bài làm trắc nghiệm", "Các bài làm của các học viên", "/AdminResults", 1),
            F("Tình huống mô phỏng", "Danh sách tình huống mô phỏng", "/AdminSimulationQuestions", 1),
            F("Bộ đề mô phỏng", "Bộ đề chuẩn theo cấu trúc của bộ", "/AdminSimulationExamSets", 1),
            F("Bài làm mô phỏng", "Các bài làm của các học viên", "/AdminSimulationResults", 1),
            F("Biển báo", "Quản lý các biển báo", "/AdminSigns", 1),
            F("Flashcard", "Quản lý Flashcard biển báo", "/AdminFlashCards", 1),
            F("Xe tập lái", "Danh sách xe tập lái", "/AdminVehicles", 1),
            F("Scan phiếu thuê xe", "Quét mã QR phiếu thuê xe", "/AdminVehicles/Scan", 1),
            F("Hồ sơ học viên", "Danh sách hồ sơ học viên", "/AdminProfiles", 1),
            F("Gửi thông báo", "Danh sách thông báo và gửi thông báo đến người dùng", "/AdminNotifications", 1),
            F("Phản hồi", "Danh sách phản hồi từ người dùng", "/AdminFeedbacks", 1),
            F("Báo cáo thông kê", "Các thống kê, xuất file báo cáo", "/AdminReport/Index", 1),
            F("Người dùng", "Quản lý người dùng", "/admin/users", 1),
            F("Hóa đơn", "Hóa đơn thanh toán khóa học và thuê xe", "/AdminPayments", 1),
        };
    }
}