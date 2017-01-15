using System;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Core;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IAsyncRepository _repository;
        private readonly IModelState<ShoppingCartViewModel> _viewModelState;
        private readonly ProductsCache _cache;

        public ShoppingCartController(IAsyncRepository repository, IModelState<ShoppingCartViewModel> viewModelState, ProductsCache cache)
        {
            _repository = repository;
            _viewModelState = viewModelState;
            _cache = cache;
        }

        public async Task<IActionResult> Index(Guid id)
        {
            if (id.Equals(Guid.Empty)) 
            {
                return RedirectToAction("Index", new { id = await CreateShoppingCartSession() });
            }

            var viewModel = await _viewModelState.GetCurrentState(id);
            return View(viewModel);
        }

        [HttpPost("[controller]/sessions")]
        public async Task<IActionResult> GenerateNewSession()
        {
            return RedirectToAction("Index", new { id = await CreateShoppingCartSession() });
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(Guid id, Guid itemId)
        {
            var aggregate = await _repository.GetById<ShoppingCart>(id);
            aggregate.AddItem(itemId);
            await _repository.Save(aggregate);

            return RedirectToAction("Index", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(Guid id, Guid itemId)
        {
            var aggregate = await _repository.GetById<ShoppingCart>(id);
            aggregate.RemoveItem(itemId);
            await _repository.Save(aggregate);

            return RedirectToAction("Index", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> RefreshItem(Guid id, Guid itemId, int quantity)
        {
            var aggregate = await _repository.GetById<ShoppingCart>(id);
            aggregate.ChangeItemQuantity(itemId, quantity);
            await _repository.Save(aggregate);

            return RedirectToAction("Index", new { id = id });
        }

        [HttpGet("/products")]
        public JsonResult GetProductList() 
        {
            return Json(_cache.GetProductList());
        }

        private async Task<Guid> CreateShoppingCartSession()
        {
            var id = Guid.NewGuid();
            var shoppingCart = new ShoppingCart(id);
            await _repository.Save(shoppingCart);
            return id;
        }

    }
}