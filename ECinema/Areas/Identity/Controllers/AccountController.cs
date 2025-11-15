using ECinema.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Threading.Tasks;


namespace ECinema.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public IRepository<ApplicationUserOTP> _applicationUserOTPRepository;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, IRepository<ApplicationUserOTP> ApplicationUserOTPRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _applicationUserOTPRepository = ApplicationUserOTPRepository;
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");

        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            var user = new ApplicationUser()
            {
                UserName = registerVM.UserName,
                Email = registerVM.Email,
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName
            };
            var result = await _userManager.CreateAsync(user, registerVM.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Code);
                }
                return View(registerVM);
            }

            // send confirmation mail

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmMail), "Account", new { area = "Identity", token, userId = user.Id }, Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email!, "ECinema - Confirm Your Email!"
                , $"<h1>Confirm Your Email By Clicking <a href='{link}'>Here</a></h1>");

            await _userManager.AddToRoleAsync(user, SD.CUSTOMER_ROLE);

            return RedirectToAction("Login");
        }

        public async Task<IActionResult> ConfirmMail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                TempData["error-notification"] = "invalid user";

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                TempData["error-notification"] = "invalid token";

            TempData["success-notification"] = "Email Confirmed Successfully";

            return RedirectToAction("Login");

        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return View(loginVM);

            var user = await _userManager.FindByNameAsync(loginVM.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(loginVM.UserNameOrEmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Name / Email OR Password");
                return View(loginVM);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    ModelState.AddModelError(string.Empty, "Too many attemps, try again after 5 min");
                else if (!user.EmailConfirmed)
                    ModelState.AddModelError(string.Empty, "Please Confirm Your Email First!!");
                else
                    ModelState.AddModelError(string.Empty, "Invalid User Name / Email OR Password");

                return View(loginVM);
            }

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM resendEmailConfirmationVM)
        {
            if (!ModelState.IsValid)
                return View(resendEmailConfirmationVM);

            var user = await _userManager.FindByNameAsync(resendEmailConfirmationVM.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(resendEmailConfirmationVM.UserNameOrEmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Name / Email");
                return View(resendEmailConfirmationVM);
            }

            if (user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Already Confirmed!!");
                return View(resendEmailConfirmationVM);
            }

            // Send Confirmation Mail
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", token, userId = user.Id }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email!, "Ecommerce 519 - Resend Confirm Your Email!"
                , $"<h1>Confirm Your Email By Clicking <a href='{link}'>Here</a></h1>");

            return RedirectToAction("Login");
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswordVM)
        {
            if (!ModelState.IsValid)
                return View(forgetPasswordVM);

            var user = await _userManager.FindByNameAsync(forgetPasswordVM.UserNameOREmail) ?? await _userManager.FindByEmailAsync(forgetPasswordVM.UserNameOREmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Name / Email");
                return View(forgetPasswordVM);
            }

            var userOTPs = await _applicationUserOTPRepository.GetAllAsync(e => e.ApplicationUserId == user.Id);

            var totalOTPs = userOTPs.Count(e => (DateTime.UtcNow - e.CreateAt).TotalHours < 24);

            if (totalOTPs > 3)
            {
                ModelState.AddModelError(string.Empty, "Too Many Attemps");
                return View(forgetPasswordVM);
            }

            var otp = new Random().Next(1000, 9999).ToString(); // 1000 - 9999

            await _applicationUserOTPRepository.AddAsync(new()
            {
                Id = Guid.NewGuid().ToString(),
                ApplicationUserId = user.Id,
                CreateAt = DateTime.UtcNow,
                IsValid = true,
                OTP = otp,
                ValidTo = DateTime.UtcNow.AddDays(1),
            });
            await _applicationUserOTPRepository.CommitAsync();

            await _emailSender.SendEmailAsync(user.Email!, "Ecommerce 519 - Reset Your Password"
                , $"<h1>Use This OTP: {otp} To Reset Your Account. Don't share it.</h1>");

            return RedirectToAction("ValidateOTP", new { userId = user.Id });
        }

        public IActionResult ValidateOTP(string userId)
        {
            return View(new ValidateOTPVM
            {
                ApplicationUserId = userId
            });
        }

        [HttpPost]
        public async Task<IActionResult> ValidateOTP(ValidateOTPVM validateOTPVM)
        {
            var result = await _applicationUserOTPRepository.GetOneAsync(e => e.ApplicationUserId == validateOTPVM.ApplicationUserId && e.OTP == validateOTPVM.OTP && e.IsValid);

            if (result is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid OTP");
                return RedirectToAction(nameof(ValidateOTP), new { userId = validateOTPVM.ApplicationUserId });
            }

            return RedirectToAction("NewPassword", new { userId = validateOTPVM.ApplicationUserId });
        }

        public IActionResult NewPassword(string userId)
        {
            return View(new NewPasswordVM
            {
                ApplicationUserId = userId
            });
        }

        [HttpPost]
        public async Task<IActionResult> NewPassword(NewPasswordVM newPasswordVM)
        {
            var user = await _userManager.FindByIdAsync(newPasswordVM.ApplicationUserId);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid ");
                return View(newPasswordVM);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, newPasswordVM.Password);


            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Code);
                }

                return View(newPasswordVM);
            }

            return RedirectToAction("Login");
        }


    }
}
