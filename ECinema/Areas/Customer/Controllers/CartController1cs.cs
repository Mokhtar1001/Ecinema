using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Threading.Tasks;

namespace ECinema.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IMovieRepository _productRepository;

        public CartController(UserManager<ApplicationUser> userManager, IRepository<Cart> cartRepository, IRepository<Promotion> promotionRepository, IMovieRepository productRepository)
        {
            _userManager = userManager;
            _cartRepository = cartRepository;
            _promotionRepository = promotionRepository;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index(string code)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var cart = await _cartRepository.GetAllAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Movie, e => e.ApplicationUser]);

            var promotion = await _promotionRepository.GetOneAsync(e => e.Code == code && e.IsValid);

            if (promotion is not null)
            {
                var result = cart.FirstOrDefault(e => e.MovieId == promotion.MovieId);

                if (result is not null)
                    result.Price -= result.Movie.Price * (promotion.Discount / 100);

                await _cartRepository.CommitAsync();
            }

            return View(cart);
        }

        public async Task<IActionResult> AddToCart(int count, int productId, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var productInDb = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.MovieId == productId);

            if (productInDb is not null)
            {
                productInDb.Count += count;
                await _cartRepository.CommitAsync(cancellationToken);

                TempData["success-notification"] = "Update Movie Count to cart successfully";

                return RedirectToAction("Index", "Home");
            }

            await _cartRepository.AddAsync(new()
            {
                MovieId = productId,
                Count = count,
                ApplicationUserId = user.Id,
                Price = (await _productRepository.GetOneAsync(e => e.Id == productId)!).Price
            }, cancellationToken: cancellationToken);
            await _cartRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Add Movie to cart successfully";

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> IncrementMovie(int productId, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var product = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.MovieId == productId);

            if (product is null) return NotFound();

            product.Count += 1;
            await _cartRepository.CommitAsync(cancellationToken);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DecrementMovie(int productId, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var product = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.MovieId == productId);

            if (product is null) return NotFound();

            if (productId <= 1)
                _cartRepository.Delet(product);
            else
                product.Count -= 1;

            await _cartRepository.CommitAsync(cancellationToken);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteMovie(int productId, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var product = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.MovieId == productId);

            if (product is null) return NotFound();

            _cartRepository.Delet(product);
            await _cartRepository.CommitAsync(cancellationToken);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Pay()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var cart = await _cartRepository.GetAllAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Movie]);

            if (cart is null) return NotFound();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/customer/checkout/success",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/customer/checkout/cancel",
            };

            //foreach (var item in cart)
            //{
            //    options.LineItems.Add(new SessionLineItemOptions
            //    {
            //        PriceData = new SessionLineItemPriceDataOptions
            //        {
            //            Currency = "egp",
            //            MovieData = new SessionLineItemPriceDataMovieDataOptions
            //            {
            //                Name = item.Movie.Name,
            //                Description = item.Movie.Description,
            //            },
            //            UnitAmount = (long)item.Price * 100,
            //        },
            //        Quantity = item.Count,
            //    });
            //}

            var service = new SessionService();
            var session = service.Create(options);
            return Redirect(session.Url);
        }
    }
}
