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
        private readonly IModelState<ShoppingCartViewModel> _viewModel;

        public ShoppingCartController(IAsyncRepository repository, IModelState<ShoppingCartViewModel> viewModel)
        {
            _repository = repository;
            _viewModel = viewModel;
        }

        public async Task<IActionResult> Index(Guid id)
        {
            if (id.Equals(Guid.Empty)) 
            {
                return RedirectToAction("Index", new { id = await CreateShoppingCartSession() });
            }

            var tito = await _viewModel.GetCurrentState(id);
            Console.WriteLine(tito.Items.FirstOrDefault());

            var viewModel = new ShoppingCartViewModel { Id = id };
            return View(viewModel);
        }

        [HttpPost("[controller]/sessions")]
        public async Task<IActionResult> GenerateNewSession()
        {
            return RedirectToAction("Index", new { id = await CreateShoppingCartSession() });
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(Guid id)
        {
            var aggregate = await _repository.GetById<ShoppingCart>(id);
            aggregate.AddItem(Guid.NewGuid());
            await _repository.Save(aggregate);

            return RedirectToAction("Index", new { id = id });
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