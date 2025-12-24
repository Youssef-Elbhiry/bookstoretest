using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecomerce_mvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult UpSert(int? id)
        {
            var company = new Company();
            if (id == null || id == 0)
            { 
            }
            else
            {
                 company = _unitOfWork.Company.Get(c=>c.Id==id);
               
            }
            return View(company);
        }

        [HttpPost]
        public IActionResult UpSert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0 )
                {

                    _unitOfWork.Company.Add(company);
                    TempData["success"] = "Company Added Successfuly";
                }
                else
                { 
                    _unitOfWork.Company.Update(company);
                    TempData["success"] = "Company Updated Successfuly";


                }
                _unitOfWork.Save();
                return RedirectToAction("Index");

            }
            return View(company);
        }



        #region api
        [HttpGet]
        public IActionResult GetAll()
        {
            var companies = _unitOfWork.Company.GetAll();
            return Json(new { data= companies } );
        }

        [HttpDelete]
        public IActionResult Delete(int id) { 
        
         var company = _unitOfWork.Company.Get(c=>c.Id==id);
            if (company != null) { 
            _unitOfWork.Company.Delete(company);
                _unitOfWork.Save();
                return Json(new {success=true ,message="Company Deleted Successfuly"});
            }
            return Json(new { success = false, message = "Company not found" });
        }
        #endregion
    }
}
