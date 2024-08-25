using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Checod_Africa.Models;
using Payroll_Library;
using System.Web.Script.Serialization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Checod_Africa.Controllers
{
    public class StaffController : Controller
    {
        // GET: Staff
        staff staff = new staff ();
        Payroll_Functions payroll = new Payroll_Functions ();
       DB_Interface db = new DB_Interface ();
        public ActionResult Index()
        {
           
            return View();
        }
        
        public ActionResult Refresh()
        {
            List<staff> all_staff = staff.Get_All_Staff();

            return PartialView("_Stafflist", all_staff);
        }
        public ActionResult DetailsPopup(string id)
        {
            staff newStaff =  new staff(id);
            return PartialView("_Details", newStaff);
        }

        // Action to create staff using a popup
        public ActionResult CreatePopup(string id = "")
        {
            staff staff = new staff(id=="" ? "": id); // Create a new staff object to be used as the model for the partial view
            staff.allsettings.Get_Settings();
            List<string> banks = staff.GetAllBanks(staff.allsettings.paystack_key);
             // Optionally, you can pass these lists to the view using ViewBag or a model
            ViewBag.Banks = new SelectList(banks, staff.bank );
            return PartialView("_Create", staff);

        }


        // Action to handle staff creation form submission
        [HttpPost]
        public JsonResult CreatePopup(FormCollection form)
        {

            // Get the values from the form collection
            if (!string.IsNullOrEmpty(form["staff_id"]) &&
!string.IsNullOrEmpty(form["staff_name"]) &&
!string.IsNullOrEmpty(form["designation"]) &&
!string.IsNullOrEmpty(form["tin"]) &&
!string.IsNullOrEmpty(form["salary"]) &&
double.TryParse(form["salary"], out double salaryValue) &&
!string.IsNullOrEmpty(form["isadmin"]) &&
bool.TryParse(form["isadmin"], out bool isadminValue) &&
!string.IsNullOrEmpty(form["account_no"]) &&
!string.IsNullOrEmpty(form["bank"]) &&
!string.IsNullOrEmpty(form["email"]) &&
!string.IsNullOrEmpty(form["phone"]))
            {
                staff.staff_id = form["staff_id"];
                staff.staff_name = form["staff_name"];
                staff.designation = form["designation"];
                staff.tin = form["tin"];
                staff.salary = Convert.ToDouble(salaryValue.ToString().Replace(",", ""));
                staff.isadmin = isadminValue;
                staff.account_no = form["account_no"];
                staff.bank = form["bank"];
                staff.email = form["email"];
                staff.phone = form["phone"];

                // Proceed with processing since all fields are valid
           
            string commaAllowances = form["selectedAllowances"];

                List<string> allowances = new List<string>();
                if (!string.IsNullOrEmpty(commaAllowances))
                {
                    allowances = commaAllowances.Split(',').ToList();
                }

                List<string> pay_rec = staff.add_recipient();

                if (pay_rec[0].ToString() == "failure")
                {
                    return Json(new { status = false, message = "Error in bank details!" });
                }
                else
                {
                    if (!payroll.Check_Staff_exists(staff.staff_id))
                    {
                        staff.Save_Staff(staff.staff_id, staff.staff_name, staff.designation, staff.salary, staff.isadmin, staff.account_no, staff.bank, staff.email, staff.phone, staff.tin);
                        db.Non_Query("update staff set rec_code = '" + pay_rec[1].ToString() + "' where staffno = '" + staff.staff_id + "'");

                    }
                    else
                    {
                        staff.Edit_Staff(staff.staff_id, staff.staff_name, staff.designation, staff.salary, staff.isadmin, staff.account_no, staff.bank, staff.email, staff.phone, staff.tin);
                        db.Non_Query("update staff set rec_code = '" + pay_rec[1].ToString() + "' where staffno = '" + staff.staff_id + "'");
                    }
                    staff.Add_Allowances(allowances);
                    if (staff.isadmin == true) staff.Make_Admin(staff.staff_id);

                    return Json(new { status = true });
                }



               
            }
            else
            {
                return Json(new { status = false, message = "Please fill all fields!" });
            }

        }
        public ActionResult payslip()
        {
            return View();
        }
        [HttpPost]
        public JsonResult ResolveBankAccount(string bankCode, string accountNumber)
        {
            using (var httpClient = new HttpClient())
            {
                Settings settings = new Settings();
                settings.Get_Settings();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.paystack_key );

                var url = $"https://api.paystack.co/bank/resolve?account_number={accountNumber}&bank_code={settings.GetBankCodeByBankName(bankCode, settings.paystack_key )}";

                var response = httpClient.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    var jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);

                    return Json(new { status = true, message = Convert.ToString(jsonResponse.data.account_name) }); 
                }
                else
                {
                    // Handle the error response
                    return Json(new { status = false, message = "Error retrieving account details" });
                }
            }
        }

        // Action to handle staff deletion form submission

        public ActionResult DeleteStaff(string id)
        {
            payroll.Delete_Staff(id);
            // Redirect to the staff index page after successful deletion
            return RedirectToAction("Index");
        }
    }

}