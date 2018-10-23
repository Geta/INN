using System;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using EPiServer.Web.Routing;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Inn;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using INN.Core.Infrastructure;
using INN.Core.Models;
using INN.Core.Services.Address;
using INN.Core.Services.Auth;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    public class CheckoutController : PageController<CheckoutPage>
    {
        private readonly ICurrencyService _currencyService;
        private readonly ControllerExceptionHandler _controllerExceptionHandler;
        private readonly CheckoutViewModelFactory _checkoutViewModelFactory;
        private readonly OrderSummaryViewModelFactory _orderSummaryViewModelFactory;
        private readonly IOrderRepository _orderRepository;
        private readonly ICartService _cartService;
        private readonly IRecommendationService _recommendationService;
        private ICart _cart;
        private readonly CheckoutService _checkoutService;
        private readonly CustomerContextFacade _customerContextFacade;
        private readonly IInnLoginService _innLoginService;
        private readonly IInnAddressService _innAddressService;
        private readonly IInnUserTokenCache _innUserTokenCache;

        public CheckoutController(
            ICurrencyService currencyService,
            ControllerExceptionHandler controllerExceptionHandler,
            IOrderRepository orderRepository,
            CheckoutViewModelFactory checkoutViewModelFactory,
            ICartService cartService,
            OrderSummaryViewModelFactory orderSummaryViewModelFactory,
            IRecommendationService recommendationService,
            CustomerContextFacade customerContextFacade,
            IInnLoginService innLoginService,
            IInnAddressService innAddressService,
            IInnUserTokenCache innUserTokenCache,
            CheckoutService checkoutService)
        {
            _currencyService = currencyService;
            _controllerExceptionHandler = controllerExceptionHandler;
            _orderRepository = orderRepository;
            _checkoutViewModelFactory = checkoutViewModelFactory;
            _cartService = cartService;
            _orderSummaryViewModelFactory = orderSummaryViewModelFactory;
            _recommendationService = recommendationService;
            _checkoutService = checkoutService;
            _customerContextFacade = customerContextFacade;
            _innLoginService = innLoginService;
            _innAddressService = innAddressService;
            _innUserTokenCache = innUserTokenCache;
        }

        [HttpGet]
        public async Task<ActionResult> RedirectResult(string userticket = "", string returnToUrl = "")
        {
            await _innLoginService.HandleRedirectResult(_innUserTokenCache, userticket);

            return Redirect(returnToUrl);
        }

        [HttpGet]
        [OutputCache(Duration = 0, NoStore = true)]
        public async Task<ActionResult> Index(CheckoutPage currentPage)
        {
            if (CartIsNullOrEmpty())
            {
                return View("EmptyCart");
            }

            var handleUserTokenUrl = Url.Action("RedirectResult", "Checkout", new { }, Request.Url?.Scheme ?? "http");
            var returnToThisUrl = Request.Url?.AbsolutePath ?? "/en/checkout";
            var innStatusResult = await _innLoginService.GetLoginStatus(
                _innUserTokenCache,
                handleUserTokenUrl,
                returnToThisUrl);

            if (innStatusResult.LoginStatus == InnLoginStatus.Unknown)
            {
                // We have to check inn login status, this will:
                // Redirect to INN to check login status
                // Redirect back to the RedirectResult action
                // Which, in turn, redirects back to the current action
                return Redirect(innStatusResult.Url);
            }

            var viewModel = CreateCheckoutViewModel(currentPage);
            viewModel.InnViewModel = await GetInnViewModel(innStatusResult);

            Cart.Currency = _currencyService.GetCurrentCurrency();

            _checkoutService.UpdateShippingAddresses(Cart, viewModel);
            _checkoutService.UpdateShippingMethods(Cart, viewModel.Shipments);

            _cartService.ApplyDiscounts(Cart);
            _orderRepository.Save(Cart);

            await _recommendationService.TrackCheckoutAsync(HttpContext);

            _checkoutService.ProcessPaymentCancel(viewModel, TempData, ControllerContext);

            return View(viewModel.ViewName, viewModel);
        }

        private async Task<InnViewModel> GetInnViewModel(InnStatusResult innStatusResult)
        {
            var innViewModel = new InnViewModel {InnStatusResult = innStatusResult};
            switch (innStatusResult.LoginStatus)
            {
                case InnLoginStatus.NotLoggedIn:
                    // Show log in button
                    break;
                case InnLoginStatus.NeedConsent:
                    // Show consent button
                    break;
                case InnLoginStatus.LoggedIn:
                    if (_customerContextFacade.CurrentContact.CurrentContact != null)
                    {
                        // Sync address for logged in user
                        await _innAddressService.SyncAddresses(_customerContextFacade.CurrentContact.CurrentContact,
                            innStatusResult.UserToken.UserTokenId);
                    }
                    else
                    {
                        // Show dropdown? for non anonymous user
                        innViewModel.Addresses =
                            await _innAddressService.GetUserAddresses(innStatusResult.UserToken.UserTokenId);
                        innViewModel.PreselectedAddress =
                            await _innAddressService.GetDefaultDeliveryAddress(innStatusResult.UserToken.UserTokenId);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return innViewModel;
        }

        [HttpGet]
        public ActionResult SingleShipment(CheckoutPage currentPage)
        {
            if (!CartIsNullOrEmpty())
            {
                _cartService.MergeShipments(Cart);
                _orderRepository.Save(Cart);
            }

            return RedirectToAction("Index", new { node = currentPage.ContentLink });
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult ChangeAddress(UpdateAddressViewModel addressViewModel)
        {
            ModelState.Clear();
            var viewModel = CreateCheckoutViewModel(addressViewModel.CurrentPage);
            _checkoutService.CheckoutAddressHandling.ChangeAddress(viewModel, addressViewModel);

            _checkoutService.UpdateShippingAddresses(Cart, viewModel);

            _orderRepository.Save(Cart);

            var addressViewName = addressViewModel.ShippingAddressIndex > -1 ? "SingleShippingAddress" : "BillingAddress";

            return PartialView(addressViewName, viewModel);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult OrderSummary()
        {
            var viewModel = _orderSummaryViewModelFactory.CreateOrderSummaryViewModel(Cart);
            return PartialView(viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult AddCouponCode(CheckoutPage currentPage, string couponCode)
        {
            if (_cartService.AddCouponCode(Cart, couponCode))
            {
                _orderRepository.Save(Cart);
            }
            var viewModel = CreateCheckoutViewModel(currentPage);
            return View(viewModel.ViewName, viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult RemoveCouponCode(CheckoutPage currentPage, string couponCode)
        {
            _cartService.RemoveCouponCode(Cart, couponCode);
            _orderRepository.Save(Cart);
            var viewModel = CreateCheckoutViewModel(currentPage);
            return View(viewModel.ViewName, viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult Purchase(CheckoutViewModel viewModel, IPaymentMethod paymentMethod)
        {
            if (CartIsNullOrEmpty())
            {
                return Redirect(Url.ContentUrl(ContentReference.StartPage));
            }

            viewModel.Payment = paymentMethod;

            viewModel.IsAuthenticated = User.Identity.IsAuthenticated;

            _checkoutService.CheckoutAddressHandling.UpdateUserAddresses(viewModel);

            if (!_checkoutService.ValidateOrder(ModelState, viewModel, _cartService.ValidateCart(Cart)))
            {
                return View(viewModel);
            }

            if (!paymentMethod.ValidateData())
            {
                return View(viewModel);
            }

            _checkoutService.UpdateShippingAddresses(Cart, viewModel);

            _checkoutService.CreateAndAddPaymentToCart(Cart, viewModel);

            var purchaseOrder = _checkoutService.PlaceOrder(Cart, ModelState, viewModel);
            if (!string.IsNullOrEmpty(viewModel.RedirectUrl))
            {
                return Redirect(viewModel.RedirectUrl);
            }

            if (purchaseOrder == null)
            {
                return View(viewModel);
            }

            var confirmationSentSuccessfully = _checkoutService.SendConfirmation(viewModel, purchaseOrder);

            return Redirect(_checkoutService.BuildRedirectionUrl(viewModel, purchaseOrder, confirmationSentSuccessfully));
        }

        public ActionResult OnPurchaseException(ExceptionContext filterContext)
        {
            var currentPage = filterContext.RequestContext.GetRoutedData<CheckoutPage>();
            if (currentPage == null)
            {
                return new EmptyResult();
            }

            var viewModel = CreateCheckoutViewModel(currentPage);
            ModelState.AddModelError("Purchase", filterContext.Exception.Message);

            return View(viewModel.ViewName, viewModel);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            _controllerExceptionHandler.HandleRequestValidationException(filterContext, "purchase", OnPurchaseException);
        }

        private ViewResult View(CheckoutViewModel checkoutViewModel)
        {
            return View(checkoutViewModel.ViewName, CreateCheckoutViewModel(checkoutViewModel.CurrentPage, checkoutViewModel.Payment));
        }

        private CheckoutViewModel CreateCheckoutViewModel(CheckoutPage currentPage, IPaymentMethod paymentMethod = null)
        {
            return _checkoutViewModelFactory.CreateCheckoutViewModel(Cart, currentPage, paymentMethod);
        }

        private ICart Cart => _cart ?? (_cart = _cartService.LoadCart(_cartService.DefaultCartName));

        private bool CartIsNullOrEmpty()
        {
            return Cart == null || !Cart.GetAllLineItems().Any();
        }
    }
}
