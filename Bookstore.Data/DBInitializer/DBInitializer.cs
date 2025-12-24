using BookStore.DataAccess.Data;
using BookStore.Models;
using BookStore.Utilites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStore.DataAccess.DBInitializer
{
    public class DBInitializer : IDBInitializer
    {
        private readonly UserManager<IdentityUser> _usermanager;
        private readonly RoleManager<IdentityRole> _rolemanager;
        private readonly ApplicationDbContext _db;

        public DBInitializer(UserManager<IdentityUser> usermanager,RoleManager<IdentityRole> rolemanager,ApplicationDbContext db)
        {
            _usermanager = usermanager;
            _rolemanager = rolemanager;
            _db = db;
        }
        public void Initialize()
        {
            try
            {
               if(_db.Database.GetPendingMigrations().Count()>0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception e){ }

            if (!_rolemanager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                 _rolemanager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                 _rolemanager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                 _rolemanager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                 _rolemanager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

                _usermanager.CreateAsync(new ApplicationUser
                {
                    Name = "Admin",
                    Email = "Admin@gmail.com",
                    UserName = "Admin@gmail.com",
                    City = "Cairo",
                    StreetAddress = "10 El Nozha Street",
                    State = "Cairo",
                    PhoneNumber = "01010101010",
                    PostalCode = "11111",
                },"Admin123@").GetAwaiter().GetResult();

                var user = _db.ApplicationUsers.FirstOrDefault(u=>u.Email == "Admin@gmail.com");
                _usermanager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }
            return;
        }
    }
}
