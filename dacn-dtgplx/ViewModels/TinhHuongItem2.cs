namespace dacn_dtgplx.ViewModels
{
    public class TinhHuongItem2
    {
        public int IdThMp { get; set; }

        public string TieuDe { get; set; } = "";

        /// <summary>
        /// URL video của tình huống (đã normalize, bắt đầu bằng /videos/...)
        /// </summary>
        public string VideoUrl { get; set; } = "";

        /// <summary>
        /// Thời gian bắt đầu – kết thúc tình huống trong VIDEO (giây)
        /// </summary>
        public double TgBatDau { get; set; }
        public double TgKetThuc { get; set; }

        /// <summary>
        /// Vùng tính điểm (giây) – PHẢI cùng đơn vị với video.currentTime
        /// </summary>
        public double ScoreStartSec { get; set; }
        public double ScoreEndSec { get; set; }

        public double Duration =>
            Math.Max(0, ScoreEndSec - ScoreStartSec);

        /// <summary>
        /// 5 mốc điểm (5 → 1)
        /// </summary>
        public List<MocDiemItem> Mocs { get; set; } = new();
        public string? HintImageUrl { get; set; }
    }
}
