using dacn_dtgplx.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/xetaplai")]
[ApiController]
public class ApiXeController : ControllerBase
{
    private readonly DtGplxContext _context;

    public ApiXeController(DtGplxContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetXe(int id)
    {
        var xe = await _context.XeTapLais.FirstOrDefaultAsync(x => x.XeTapLaiId == id);

        if (xe == null)
            return NotFound();

        return Ok(new
        {
            id = xe.XeTapLaiId,
            loaiXe = xe.LoaiXe,
            bienSo = xe.BienSo,
            anhXe = xe.AnhXe,
            giaThueTheoGio = xe.GiaThueTheoGio
        });
    }
}
