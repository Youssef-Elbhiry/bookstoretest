using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecomerce_mvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
        
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }



        #region api
        public IActionResult GetAllUsers()
        {
            var users = _db.ApplicationUsers.Include(u=>u.company);
            var roles = _db.Roles.ToList(); 
            var useroles = _db.UserRoles.ToList();
            foreach (var user in users)
            {
                var roleid = useroles.FirstOrDefault(r => r.UserId == user.Id)?.RoleId;

                user.role = roles.FirstOrDefault(r => r.Id == roleid)?.Name;
                
            }
            return Json(new { data = users });
        }
        [HttpPost]
        public IActionResult LockUnLock([FromBody]string id) { 
        
        var userfromdb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (userfromdb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }
            if (userfromdb.LockoutEnd != null && userfromdb.LockoutEnd > DateTime.Now)
            {
                //user is locked and we need to unlock them
                userfromdb.LockoutEnd = DateTime.Now;
            }
            else
            {
                userfromdb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _db.SaveChanges();
            return Json(new { success = true, message = "Operation Successful" });

        }
        #endregion

    }
}
