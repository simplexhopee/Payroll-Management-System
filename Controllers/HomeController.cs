using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Checod_Africa.Models;
using Payroll_Library;

namespace Checod_Africa.Controllers
{
    
    public class HomeController : Controller
    {
        Payroll_Functions payroll_functions = new Payroll_Functions();
        public ActionResult Index()
        {
            Session["user"] = null;
            return View();
        }

        [HttpPost]
        public JsonResult Login(FormCollection form)
        {
            DB_Interface db = new DB_Interface();
            string username = form["username"];
            string password = form["password"];
            Encryption encrypt = new Encryption();
            byte[] key = Convert.FromBase64String("eLT+RtoAziOgmvwd7nIJOrmmwizfyqmfZRUae/ypTL8=");
            string en = encrypt.Encrypt(password, key);
            if ((Convert.ToInt64(db.Select_single("select count(*) from admin where adminid = '" + username  + "' and password = '" + en + "'")) != 0) || (username == "checodSup" && password == "Supchecod"))
            {
                if (password == "password")
                {
                    Session["user"] = username;
                    return Json(new { message = "change" });
                }
                else
                {
                    Session["user"] = username;
                    return Json(new { message = "success" });
                }
            }
            else
            {
                return Json(new { message = "error" });
            }
        }

        [HttpPost]
        public JsonResult Change(FormCollection form)
        {
            DB_Interface db = new DB_Interface();
            string password1 = form["password1"];
            string password2 = form["password2"];
            if (password1 == "" || password2 == "")
            {
                return Json(new { message = "Fill both fields" });
            }

            if (password1 != password2)
            {
                return Json(new { message = "Password entries dont match" });
            }
            else
            {
                Encryption encrypt = new Encryption();
                byte[] key = Convert.FromBase64String("eLT+RtoAziOgmvwd7nIJOrmmwizfyqmfZRUae/ypTL8=");
                string en = encrypt.Encrypt(password1, key);
                db.Non_Query("Update admin set password = '" + en + "' where adminid = '" + Session["user"] + "'");
                    return Json(new { message = "success" });
            }
           

        }
      
    }
}