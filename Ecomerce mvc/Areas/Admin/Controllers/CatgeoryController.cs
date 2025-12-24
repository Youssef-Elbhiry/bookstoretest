using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Utilites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecomerce_mvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CatgeoryController : Controller
    {
      
        private IUnitOfWork _Unitofwork;
        public CatgeoryController(IUnitOfWork Unitofwork)
        {
            _Unitofwork = Unitofwork;
        }
        public IActionResult Index()
        {
            var catgories = _Unitofwork.Category.GetAll();
            return View(catgories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category catgeory)
        {
            if (ModelState.IsValid)
            {

                _Unitofwork.Category.Add(catgeory);
                _Unitofwork.Save();
                   TempData["Success"] = "Category Created Successfuly";
                return RedirectToAction("Index");

            }
            return View(catgeory);
        }

        public IActionResult Edit(int? id)
        {
            if (id is null || id == 0)
            {
                return NotFound();
            }

            Category? category = _Unitofwork.Category.Get(c=>c.Id==id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category catgeory)
        {
            if (ModelState.IsValid)
            {

                _Unitofwork.Category.Update(catgeory);
                _Unitofwork.Save();
                return RedirectToAction("Index");

            }
            return View(catgeory);
        }
        public IActionResult Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            Category? category = _Unitofwork.Category.Get(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost]
        public IActionResult Delete( Category category)
        {
            _Unitofwork.Category.Delete(category);
            _Unitofwork.Save();
             TempData["Success"] = "Category deleted successfuly";
            return RedirectToAction("Index");
        }
    }
}
