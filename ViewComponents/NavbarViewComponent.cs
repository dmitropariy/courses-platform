using courses_platform.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace courses_platform.ViewComponents
{
    public class NavbarViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public NavbarViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new NavbarViewModel();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var claimsUser = User as ClaimsPrincipal;

                var externalId = claimsUser?.FindFirst("sub")?.Value
                    ?? claimsUser?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(externalId))
                {
                    var localUser = await _db.AppUsers
                        .FirstOrDefaultAsync(u => u.ExternalUserId == externalId);

                    if (localUser != null)
                    {
                        model.LocalUserId = localUser.Id;
                        model.Role = claimsUser?.FindFirst(ClaimTypes.Role)?.Value;
                        model.UserName = claimsUser?.Identity?.Name;
                    }
                }
            }

            return View(model);
        }
    }

    public class NavbarViewModel
    {
        public int? LocalUserId { get; set; }
        public string? Role { get; set; }
        public string? UserName { get; set; }
    }
}
