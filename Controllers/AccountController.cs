using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using courses_platform.Services;
using System.Security.Claims;
using courses_platform.Models;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet;
using static courses_platform.Services.UserApiService;

namespace courses_platform.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserApiService _userApiService;
        private readonly CloudinaryService _cloudinary;

        public AccountController(ApplicationDbContext context, UserApiService userApiService, CloudinaryService cloudinary)
        {
            _context = context;
            _userApiService = userApiService;
            _cloudinary = cloudinary;
        }

        [Authorize]
        public async Task<IActionResult> ProfileStudent(int id)
        {
            var localUser = await _context.AppUsers.Include(u => u.StudentCourses)
                .ThenInclude(sc => sc.Course)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (localUser == null) return NotFound();

            var profile = await _userApiService.GetUserProfileAsync(localUser.ExternalUserId);
            if (profile == null) return View("Error");

            var currentExternalId = User.FindFirst("sub")?.Value;
            bool canEdit = currentExternalId == localUser.ExternalUserId;

            var courses = localUser.StudentCourses?
                .Where(sc => sc.Course != null)
                .Select(sc => new StudentCourseViewModel
                {
                    CourseId = sc.Course!.CourseId,
                    Title = sc.Course.Title,
                    Description = sc.Course.Description,
                    CompletedCount = sc.Course.CompletedCount,
                    IsCompleted = sc.IsCompleted,
                    CompletedTime = sc.CompletedTime
                }).ToList() ?? new List<StudentCourseViewModel>();

            var model = new StudentProfileViewModel
            {
                Profile = profile,
                Courses = courses,
                CanEdit = canEdit
            };

            return View(model);
        }


        [Authorize]
        public async Task<IActionResult> ProfileProfessor(int id)
        {
            var localUser = await _context.AppUsers.Include(u => u.ProfessorCourses)
                .ThenInclude(pc => pc.Course)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (localUser == null) return NotFound();

            var profile = await _userApiService.GetUserProfileAsync(localUser.ExternalUserId);
            if (profile == null) return View("Error");

            var currentExternalId = User.FindFirst("sub")?.Value;
            bool canEdit = currentExternalId == localUser.ExternalUserId;

            var courses = localUser.ProfessorCourses?
                .Where(pc => pc.Course != null)
                .Select(pc => new ProfessorCourseViewModel
                {
                    CourseId = pc.Course!.CourseId,
                    Title = pc.Course.Title,
                    Description = pc.Course.Description,
                    CompletedCount = pc.Course.CompletedCount
                }).ToList() ?? new List<ProfessorCourseViewModel>();

            var model = new ProfessorProfileViewModel
            {
                Profile = profile,
                Courses = courses,
                CanEdit = canEdit
            };

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> EditProfile(string id)
        {
            var localUser = await _context.AppUsers.FirstOrDefaultAsync(u => u.ExternalUserId == id);
            if (localUser == null) return NotFound();

            var currentExternalId = User.FindFirst("sub")?.Value;
            if (currentExternalId != id)
                return Forbid();

            var profile = await _userApiService.GetUserProfileAsync(id);
            if (profile == null) return View("Error");

            return View(profile);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditProfile(string id, UserProfileUpdateModel model)
        {
            var localUser = await _context.AppUsers.FirstOrDefaultAsync(u => u.ExternalUserId == id);
            if (localUser == null) return NotFound();

            var currentExternalId = User.FindFirst("sub")?.Value;
            if (currentExternalId != id)
                return Forbid();

            var updatedProfile = await _userApiService.UpdateUserProfileAsync(id, model);
            if (updatedProfile == null) return View("Error");

            return RedirectToAction("ProfileStudent", new { localUser.Id });
        }

        public IActionResult ProfileLogin(string role)
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = "/",
                Parameters = { ["role"] = role }
            };
            return Challenge(props, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            var callbackUrl = Url.Action("Index", "Home", values: null, protocol: Request.Scheme);
            return SignOut(
                new AuthenticationProperties
                {
                    RedirectUri = callbackUrl
                },
                OpenIdConnectDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
