using dacn_dtgplx.Services;
using Microsoft.AspNetCore.Mvc;

namespace dacn_dtgplx.Controllers
{
    [Route("search")]
    public class SearchController : Controller
    {
        [HttpGet("features")]
        public IActionResult SearchFeatures(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new List<object>());

            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;

            var result = FeatureService.All
                .Where(f =>
                    f.Roles.Contains(roleId) &&
                    (f.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                     f.Description.Contains(q, StringComparison.OrdinalIgnoreCase))
                )
                .Select(f => new {
                    f.Title,
                    f.Description,
                    f.Url
                })
                .Take(10)
                .ToList();

            return Json(result);
        }
    }
}
