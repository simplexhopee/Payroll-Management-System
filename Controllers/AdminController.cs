using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Checod_Africa.Models;
using Payroll_Library;

namespace Checod_Africa.Controllers
{
    public class AdminController : Controller
    {
        admin admin = new admin();
        Payroll_Functions payroll = new Payroll_Functions();
        DB_Interface db = new DB_Interface();
        // GET: Admin
        public ActionResult Index()
        {
            List<admin> all_admin = admin.Get_All_Admin();
            return View(all_admin);
        }

        [HttpPost]
        public JsonResult Save(FormCollection form)
        {
            if (form["adminid"] == null)
            {
                return Json(new { status = false, message = "Field cannot be blank" });
            }
            else
            {
                if (admin.Add_Admin(form["adminid"]) == true)
                    return Json(new { status = true });
                else
                    return Json(new { status = false, message = "Id exists" });
            }
            
        }
        [HttpPost]
        public JsonResult Delete(FormCollection form)
        {
            admin.Remove_Admin(form["adminid"]);
            // Redirect to the staff index page after successful deletion
            return Json(new { status = true});
        }
    }
}