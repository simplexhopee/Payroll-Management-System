using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Checod_Africa.Models;
using Payroll_Library;
using System.IO;

namespace Checod_Africa.Controllers
{
    public class SettingsController : Controller
    {
        // GET: Settings
        Payroll_Functions payroll = new Payroll_Functions();
        Settings settings = new Settings();
        public ActionResult Index()
        {
            settings.Get_Settings();
            return View(settings);
        }
        [HttpPost]
        public ActionResult SaveSettings(HttpPostedFileBase logo)
        {
            if (ModelState.IsValid)
            {
               
                // Assign values from the form elements to the settings properties
                settings.companyname = Request.Form["companyname"];
                settings.paystack_key = Request.Form["paystack_key"];

                // Save the uploaded logo
                if (logo != null && logo.ContentLength > 0)
                {
                    settings.logo = SaveLogo(logo); // Save the uploaded logo and update the settings
                }
                 settings.Save_Settings();
                // Create a dictionary to hold the allowances from the form
               Dictionary<string, double> data = new Dictionary<string, double>();
                foreach (string key in Request.Form)
                {
                    if (key.StartsWith("allowances[") && key.EndsWith("].Name"))
                    {
                        string index = key.Substring(key.IndexOf('[') + 1, key.IndexOf(']') - key.IndexOf('[') - 1);
                        string allowanceName = Request.Form["allowances[" + index + "].Name"];
                        double allowanceProportion;
                        if (double.TryParse(Request.Form["allowances[" + index + "].Proportion"], out allowanceProportion))
                        {
                            settings.append_allowances (allowanceName, allowanceProportion);
                            data.Add (allowanceName , allowanceProportion);
                        }
                    }
                }
                settings.Get_Settings();
                
                foreach (string key in settings.allowances.Keys)
                {
                    if ((!data.ContainsKey(key)) || ((data.ContainsKey(key) && settings.allowances[key] != data[key])))
                    {
                        DB_Interface db = new DB_Interface();
                        db.Non_Query("delete from allowances where allowance = '" + key + "'");
                        db.Non_Query("delete from staff_allowances where allowance = '" + Convert.ToInt64(db.Select_single("select id from allowances where allowance = '" + key + "'")) + "'");
                    }
                }
              
                // Call the Save_Settings method to save the settings
               

                return RedirectToAction("Index");
            }

            return View("Index");
        }


        private string SaveLogo(HttpPostedFileBase logo)
        {
            // Implement logic to save the uploaded logo to a directory and return the filename or path
            if(logo != null && logo.ContentLength > 0)
            {
                // Get the file name without any path information
                var fileName = Path.GetFileName(logo.FileName);

                // Combine the file name with the destination directory path
                var destinationPath = Path.Combine(Server.MapPath("~/Content/img/"), fileName);

                // Save the file to the destination path
                logo.SaveAs(destinationPath);

                // Return the relative path to the saved file (if you want to store this path in the database, for example)
                return "~/Content/img/" + fileName;
            }

            // If there was no logo uploaded, return null or an empty string
            return null;
        }
    }
}