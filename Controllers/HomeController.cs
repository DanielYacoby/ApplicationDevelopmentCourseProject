using ApplicationDevelopmentCourseProject.Data;
using ApplicationDevelopmentCourseProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ApplicationDevelopmentCourseProject.Controllers.BranchesController;
using Microsoft.AspNetCore.Http;

namespace ApplicationDevelopmentCourseProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDevelopmentCourseProjectContext _context;

        public class ProductsAndCategoriesViewModel
        {
            public List<Category> Categories { get; set; }
            public List<Product> Products { get; set; }
            public Category CategoryModel { get; set; }
            public Product ProductModel { get; set; }
        }

        public HomeController(ILogger<HomeController> logger, ApplicationDevelopmentCourseProjectContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            var productsAndCategoriesViewModel = new ProductsAndCategoriesViewModel
            {
                Categories = await _context.Category.ToListAsync(),
                Products = (categoryId == null) ? await _context.Product.ToListAsync() : await _context.Product.Where(p => p.Category.Id == categoryId).ToListAsync(),
            };
            return View(productsAndCategoriesViewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Index(string searchString)
        {
            if (searchString != null)
            {
                var products = from m in _context.Product
                               select m;

                if (!string.IsNullOrEmpty(searchString))
                {
                    products = products.Where(s => s.Name.Contains(searchString));
                }

                var list = new List<Product>(products);
                var productsViewModel = new ProductsAndCategoriesViewModel
                {
                    Categories = await _context.Category.ToListAsync(),
                    Products = new List<Product>(products)
                };

                //if (IsAjaxRequest(Request))
                //    return PartialView(productsViewModel);

                return View(productsViewModel);
            }
            var productsAndCategoriesViewModel = new ProductsAndCategoriesViewModel
            {
                Categories = await _context.Category.ToListAsync(),
                Products = await _context.Product.ToListAsync(),
            };

            return View(productsAndCategoriesViewModel);
        }

        public async Task<IActionResult> SearchJson(string searchString)
        {//Check if works

            var products = from m in _context.Product
                               select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                products = from product in _context.Product
                        where product.Name.Contains(searchString)
                        orderby product.Id descending
                        select product;
                //products = products.Where(s => s.Name.Contains(searchString));
            }

            return Json(await products.ToListAsync());
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            var contactViewModel = new ContactViewModel
            {
                Contacts =  _context.Contact.ToList(),
                Branches =  _context.Branch.ToList()
            };
            return View(contactViewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //public JsonResult searchedProductsList(string searched)
        //{
        //    var products = from m in _context.Product
        //                   select m;

        //    if (!string.IsNullOrEmpty(searched))
        //    {
        //        products = products.Where(s => s.Name.Contains(searched));
        //    }

        //    var productsList = new List<Product>(products);

        //    return Json(productsList, JsonRequestBehavior.AllowGet);

        //    //var json = JsonSerializer.Serialize(productsList);
        //    return json;
        //}
        //public async Task<IActionResult> ViewSearchedProducts(string searchString)
        //{
        //    var products = from m in _context.Product
        //                   select m;

        //    if (!string.IsNullOrEmpty(searchString))
        //    {
        //        products = products.Where(s => s.Name.Contains(searchString));
        //    }

        //    var list = new List<Product>(products);
        //    var productsAndCategoriesViewModel = new ProductsAndCategoriesViewModel
        //    {
        //        Categories = await _context.Category.ToListAsync(),
        //        Products = new List<Product>(products)
        //};

        //    return RedirectToAction("Index", "Home", productsAndCategoriesViewModel);

        //    //if (IsAjaxRequest(Request)) return PartialView(productsAndCategoriesViewModel);
        //    //return View(productsAndCategoriesViewModel);
        //}

        public bool IsAjaxRequest(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Headers != null)
                return !string.IsNullOrEmpty(request.Headers["X-Requested-With"]) &&
                    string.Equals(
                        request.Headers["X-Requested-With"],
                        "XmlHttpRequest",
                        StringComparison.OrdinalIgnoreCase);

            return false;
        }
    }
}
