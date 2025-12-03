using System.Net.ServerSentEvents;

namespace dacn_dtgplx.ViewModels
{
    public class GuiThongBaoViewModel
    {
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public int CurrentUserId { get; set; }

        // Checkbox chọn role
        public List<RoleItem> Roles { get; set; } = new();

        // Checkbox chọn user (tùy chọn)
        public List<UserItem> Users { get; set; } = new();

        // Danh sách role được chọn
        public List<int> SelectedRoleIds { get; set; } = new();

        // Danh sách user được chọn
        public List<int> SelectedUserIds { get; set; } = new();
    }
}
