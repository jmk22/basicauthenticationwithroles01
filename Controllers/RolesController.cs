using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Http.Internal;
using BasicAuthentication.Models;
using Microsoft.Data.Entity;
using Microsoft.AspNet.Mvc.Rendering;

namespace BasicAuthentication.Controllers
{
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public RolesController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext db, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var roles = _db.Roles.ToList();
            return View(roles);
        }

        // GET: /Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Roles/Create
        [HttpPost]
        public IActionResult Create(string rolename)
        {
            try
            {
                _db.Roles.Add(new IdentityRole()
                {
                    Name = rolename
                });
                _db.SaveChanges();
                ViewBag.ResultMessage = "Role created successfully !";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return View();
            }
        }
        public IActionResult Delete(string roleName)
        {
            var thisRole = _db.Roles.Where(r => r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            _db.Roles.Remove(thisRole);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        //
        // GET: /Roles/Edit/5
        public ActionResult Edit(string roleName)
        {
            var thisRole = _db.Roles.Where(r => r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            return View(thisRole);
        }

        //
        // POST: /Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IdentityRole role)
        {
            try
            {
                _db.Entry(role).State = EntityState.Modified;
                _db.SaveChanges();
               
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public IActionResult ManageUserRoles()
        {
            //prepopulate roles for the view dropdown
            var rolesList = _db.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            var usersList = _db.Users.OrderBy(u => u.UserName).ToList().Select(uu => new SelectListItem { Value = uu.UserName.ToString(), Text = uu.UserName }).ToList();
            ViewBag.Roles = rolesList;
            ViewBag.Users = usersList;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoleAddToUser(string UserName, string RoleName)
        {
            ApplicationUser user = _db.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            await _userManager.AddToRoleAsync(user, RoleName);
            ViewBag.ResultMessage = "Role created successfully!";

            //prepopulate roles for the view dropdown
            var rolesList = _db.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            var usersList = _db.Users.OrderBy(u => u.UserName).ToList().Select(uu => new SelectListItem { Value = uu.UserName.ToString(), Text = uu.UserName }).ToList();

            ViewBag.Roles = rolesList;
            ViewBag.Users = usersList;

            return View("ManageUserRoles");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetRoles(string userName)
        {
            Console.WriteLine("Hello!");
            if (!string.IsNullOrWhiteSpace(userName))
            {
                Console.WriteLine("Hello from inside the if statement!");

                ApplicationUser user = _db.Users.Where(u => u.UserName.Equals(userName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                var account = new AccountController(_userManager, _signInManager, _db);

                ViewBag.RolesForThisUser = await account._userManager.GetRolesAsync(user);
                foreach(string role in ViewBag.RolesForThisUser)
                {
                    Console.WriteLine(role);
                }

                //prepopulate roles for the view dropdown
                var rolesList = _db.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
                var usersList = _db.Users.OrderBy(u => u.UserName).ToList().Select(uu => new SelectListItem { Value = uu.UserName.ToString(), Text = uu.UserName }).ToList();

                ViewBag.Roles = rolesList;
                ViewBag.Users = usersList;
            }
            return View("ManageUserRoles");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteRoleForUser(string UserName, string RoleName)
        {
            var account = new AccountController(_userManager, _signInManager, _db);
            ApplicationUser user = _db.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            bool result = await account._userManager.IsInRoleAsync(user, RoleName);
            if (result)
            {
                await account._userManager.RemoveFromRoleAsync(user, RoleName);
                ViewBag.ResultMessage = "Role removed from this user successfully!";
            }
            else
            {
                ViewBag.ResultMessage = "This user doesn't belong to the selected role.";
            }
            //prepopulate roles for the view dropdown
            var rolesList = _db.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            var usersList = _db.Users.OrderBy(u => u.UserName).ToList().Select(uu => new SelectListItem { Value = uu.UserName.ToString(), Text = uu.UserName }).ToList();

            ViewBag.Roles = rolesList;
            ViewBag.Users = usersList;

            return View("ManageUserRoles");
        }
    }
}
