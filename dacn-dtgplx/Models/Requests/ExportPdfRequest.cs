using dacn_dtgplx.DTOs;
using dacn_dtgplx.Models.Requests;

public class ExportPdfRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Tab { get; set; }

    public string? ConfirmText { get; set; }
    public string? WatermarkText { get; set; }

    public List<ChartImageDto>? Charts { get; set; }
}