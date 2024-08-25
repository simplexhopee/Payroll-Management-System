using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Checod_Africa.Models;
using System.Web.Script.Serialization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Payroll_Library;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Templating;
using RazorDinkToPdf;
using DinkToPdf;


namespace Checod_Africa.Controllers
{
    public class ScheduleController : Controller
    {
        staff staff = new staff();
        Payroll_Functions payroll = new Payroll_Functions();
        DB_Interface db = new DB_Interface();

        schedule schedule = new schedule();
        // GET: Schedule
        public ActionResult Index()
        {
            if (Request.QueryString.ToString() != "")
            {
                
                string sid = Request.QueryString["sno"];
                
                
                Download(sid);
            }
            List<string> months = schedule.GetMonths();
            List<int> years = schedule.GetYears();

            // Optionally, you can pass these lists to the view using ViewBag or a model
            ViewBag.Months = new SelectList(months, schedule.month);
            ViewBag.Years = new SelectList(years, schedule.year);

            return View(schedule );
        }

        [HttpPost]
        public ActionResult Refresh(FormCollection form)
        {

            string month = form["selectedMonth"];
            string year = form["selectedYear"];
          
            payroll.Delete_Schedule(month, year);
            schedule.Create_Schedule(month, year);

            List<string> months = schedule.GetMonths();
            List<int> years = schedule.GetYears();
            schedule.allsettings.Get_Settings();
            schedule.Get_All_Staff();

            // Optionally, you can pass these lists to the view using ViewBag or a model
            ViewBag.Months = new SelectList(months, schedule.month);
            ViewBag.Years = new SelectList(years, schedule.year);
            return PartialView("_Schedule", schedule);
        }

        [HttpPost]
        public ActionResult  Edit_Staff(FormCollection form)
        {
            string month = form["selectedMonth"];
            string year = form["selectedYear"];
            string staff = form["staff_id"];
           schedule.Exclude_From_Schedule(staff,month,year);
            schedule.Add_To_Schedule(staff,month,year);
            schedule.Create_Schedule(month, year);

            List<string> months = schedule.GetMonths();
            List<int> years = schedule.GetYears();
            schedule.allsettings.Get_Settings();
            schedule.Get_All_Staff();

            // Optionally, you can pass these lists to the view using ViewBag or a model
            ViewBag.Months = new SelectList(months, schedule.month);
            ViewBag.Years = new SelectList(years, schedule.year);
            return PartialView("_Schedule", schedule);
        }
        [HttpPost]
        public ActionResult Change(FormCollection form)
        {
            string month = form["month"];
            string year = form["year"];
            
            string staffId = form["staffId"];
            bool isChecked = form["isChecked"] == "true"; // Convert to boolean

            if (isChecked == true)
                schedule.Add_To_Schedule(staffId, month, year);
            else
                schedule.Exclude_From_Schedule(staffId, month, year);
            
            List<string> months = schedule.GetMonths();
            List<int> years = schedule.GetYears();
            schedule.allsettings.Get_Settings();
            schedule.Get_All_Staff();
            // Optionally, you can pass these lists to the view using ViewBag or a model
            ViewBag.Months = new SelectList(months, schedule.month);
            ViewBag.Years = new SelectList(years, schedule.year);
            return PartialView("_Schedule", schedule);
        }
        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            try
            {
                string month = form["selectedMonth"];
                string year = form["selectedYear"];
                
                schedule.Create_Schedule(month, year);

                List<string> months = schedule.GetMonths();
                List<int> years = schedule.GetYears();
                schedule.allsettings.Get_Settings();
                schedule.Get_All_Staff();

                // Optionally, you can pass these lists to the view using ViewBag or a model
                ViewBag.Months = new SelectList(months, schedule.month);
                ViewBag.Years = new SelectList(years, schedule.year);
                db.Non_Query("Insert into error Values ('here')");
                return PartialView("_Schedule", schedule);
            }
            catch (Exception ex)
            {
                db.Non_Query("Insert into error Values ('" + ex.ToString() + "')");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Resend_Email(FormCollection form)
        {
            try
            {
                string month = form["selectedMonth"];
                string year = form["selectedYear"];
                string staff_id = form["staff_id"];
                if (!string.IsNullOrEmpty(staff_id)) 
                {
                    schedule.Create_Schedule(month, year, staff_id);
                }
                else
                {
                    schedule.Create_Schedule(month, year);
                }
                schedule.allsettings.Get_Settings();
                Email(schedule);
                return Json(new { status = true });
            }
            catch(Exception ex)
            {
                return Json(new { status = false, message = ex.ToString() });
            }
        }


                [HttpPost]
        public JsonResult  PaySalary(FormCollection form)
        {
          try
            {
                string month = form["selectedMonth"];
                string year = form["selectedYear"];

                schedule.Create_Schedule(month, year);

                if ((schedule.gtotal_net + (schedule.gtotal_net * 0.0025)) < schedule.balance)
                {
                    var url = "https://api.paystack.co/transfer/bulk";
                    schedule.allsettings.Get_Settings();
                    var secretKey = schedule.allsettings.paystack_key; // Replace with your actual secret key


                    var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var transfers = new List<object>();
                    List<string> refs = GenerateUniqueReference(schedule.selected_staff.Count + 1);
                    int i = 0;
                    DB_Interface db = new DB_Interface();
                    Date_Convert dt = new Date_Convert ();
                    List<staff> paid_staff = new List<staff>();
                    foreach (var staff in schedule.selected_staff)
                    {
                        if (staff.rec_code != null)
                        {
                            paid_staff.Add(staff);
                            db.Non_Query("update salary set reference = '" + refs[i] + "', date = '" + dt.ToSQL(DateTime.Now) + "' where staffno = '" + staff.staff_id + "' and month = '" + month + "' and year = '" + year + "'");
                            int am = (int)schedule.net[staff.staff_id] ;
                            var transfer = new
                            {
                                amount = am * 100,
                                reference = refs[i],
                                reason = month + "-" + year + " SALARY",
                                recipient = staff.rec_code
                                
                            };

                            transfers.Add(transfer);
                            i++;
                        }
                        
                    }
                    Settings settings = new Settings();
                    settings.Get_Settings();
                    int ch = (int)(schedule.gtotal_net * 0.0025);
                    var charges = new
                    {
                        amount = ch * 100,
                        reference = refs[i],
                        reason = month + "-" + year + " SALARY CHARGES",
                        recipient = settings.rec_code 
                    };
                    if (charges.amount >= 10000) transfers.Add(charges);



                    var requestData = new
                    {
                        currency = "NGN",
                        source = "balance",
                        transfers
                    };

                    var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                    string contentString = content.ReadAsStringAsync().Result;

                    var response = httpClient.PostAsync(url, content).Result;


                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);

                    // Check if the 'status' field is true
                    if (responseObject.status == true)
                    {
                        
                        foreach (var staff in paid_staff )
                        {
                            db.Non_Query("update salary set status = '1'  where month = '" + month + "' and year = '" + year + "' and staffno = '" + staff.staff_id + "'");
                        }
                        try
                        {
                            
                            schedule.Create_Schedule(month, year);
                            schedule.allsettings.Get_Settings();
                            Email(schedule);
                        }
                        catch(Exception ex)
                        {
                            return Json(new { status = false, message = ex.ToString() });
                        }
                            
                        
                       
                        return Json(new { status = true });
                        // Do something when status is true
                    }
                    else
                    {
                        return Json(new { status = false, message = responseContent.ToString() });
                        // Do something when status is false
                    }
                   

                }
                else
                {
                    return Json(new { status = false, message = "Insufficient Funds. Please Topup."   });
                }

            }
            catch(Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }



        }

        
        private static List<string> GenerateUniqueReference(int no)
        {
            Random random = new Random();
            List<string> refs = new List<string>(); 
            for (int i = 0; i < no; i++)
            {
               int randomNumber = random.Next(100000, 999999);
                refs.Add(randomNumber.ToString());
            }
           

            return refs;
        }
        // Initiate the payment with Paystack
        static string DisableOTP(string secretKey)
        {
            var url = "https://api.paystack.co/transfer/disable_otp";
            
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = httpClient.PostAsync(url, null).Result;

        
                var responseContent = response.Content.ReadAsStringAsync().Result;
            return responseContent;
        }
       
        [HttpPost]
        public ActionResult Comfirm(FormCollection form)
        {
            string staffid = form["staff"];
            string month = form["selectedMonth"];
            string year = form["selectedYear"];
            schedule.allsettings.Get_Settings();
            var secretKey = schedule.allsettings.paystack_key;
            schedule.Create_Schedule( month, year, staffid );
            Dictionary<string, string> data = new Dictionary<string, string>();
            data[staffid]  = VerifyTransferAndGetStatus(schedule.reference[staffid].ToString(), secretKey);
            schedule.status = data;
            return PartialView("_comfirmPayment", schedule);
        }
        static string VerifyTransferAndGetStatus(string reference, string secretKey)
        {
            var url = "https://api.paystack.co/transfer/verify/" + reference;

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);
            
            var response = httpClient.GetAsync(url).Result;


            
                if (response.IsSuccessStatusCode)
                {
                    
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);

              string   status = jsonResponse.data.status;
               
                    return status;
                }
                else
                {
                var responseContent = response.Content.ReadAsStringAsync().Result;

                return "Unknown";
            }
           
        }
		
		[HttpPost]
		public void To_Pdf(FormCollection form)
		{
            Dictionary<string, bool> remainingCheckfields = Session["Checkfields"] as Dictionary<string, bool>;
           
            string month = form["selectedMonth"];
            string year = form["selectedYear"];
            
            schedule.Create_Schedule(month, year);
            schedule.allsettings.Get_Settings();
            var secretKey = schedule.allsettings.paystack_key;
            schedule.checkfields = remainingCheckfields;
            string cshtmlContent = System.IO.File.ReadAllText("C:\\inetpub\\DS Sites\\Checod Africa\\Views\\Schedule\\pay-pdf.cshtml");
            string renderedHtml = Engine.Razor.RunCompile(cshtmlContent, "templateKey" + GenerateUniqueReference(1)[0], typeof(schedule), schedule);

            var converter = new BasicConverter(new PdfTools());

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
        PaperSize = PaperKind.A4, // Or any other size you want
        Orientation = Orientation.Portrait,
    },
                Objects = {
        new ObjectSettings()
        {
            HtmlContent = renderedHtml, // The HTML content you've rendered
        }
    }
            };

            byte[] pdfBytes = converter.Convert(doc);

            try
            {
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "attachment; filename=payroll.pdf");

                // Write the PDF bytes to the response stream
                Response.BinaryWrite(pdfBytes);
            }
            finally
            {
                // Ensure that pdfBytes is properly disposed
                if (pdfBytes != null)
                {
                    pdfBytes = null;
                }
            }

        }

        static void Email(schedule schedule)
        {
            DB_Interface db = new DB_Interface();
            string apiUrl = "https://api.elasticemail.com/v2/email/send";
            string apiKey = "7E7633738ED6D89D9AA1CDB5AA4E79F9E2E32732ED87515D7DDD8AB1D0B0F7EDA822377ED276D8AA6AEB7E2FF04E3ED0";
            
            foreach (var staff in schedule.selected_staff )
            {
               
                string en = db.Select_single("select reference from salary where staffno = '" + staff.staff_id + "' and month = '" + schedule.month + "' and year = '" + schedule.year + "'").ToString();
                
                string downloadLink = $"https://checod.digitalschooltech.com/schedule?sno=" + en ;

                string requestData = $"apikey={apiKey}&subject=Salary%20Payment&from=checod@digitalschooltech.com&fromName=Checod%20Africa&replyTo=checod@digitalschooltech.com&to={staff.email}&template=Checod&merge_url={downloadLink}&merge_month={schedule.month}&merge_year={schedule.year}";

                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    string response = client.UploadString(apiUrl, requestData);
                    string p = response;
                }
            }
           

        }
        public void Download(string id)
        {
            DB_Interface db = new DB_Interface();

            string month = db.Select_single("select month from salary where reference = '" + id + "'").ToString();
            string year = db.Select_single("select year from salary where reference = '" + id + "'").ToString();
            string staff = db.Select_single("select staffno from salary where reference = '" + id + "'").ToString();
            schedule schedule = new schedule();
            schedule.Create_Schedule(month, year, staff);
			schedule.allsettings.Get_Settings();
            string cshtmlContent = System.IO.File.ReadAllText("C:\\inetpub\\DS Sites\\Checod Africa\\Views\\Schedule\\payslip.cshtml");
                string renderedHtml = Engine.Razor.RunCompile(cshtmlContent, "templateKey" + GenerateUniqueReference(1)[0], typeof(schedule), schedule);

            var converter = new BasicConverter(new PdfTools());

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
        PaperSize = PaperKind.A4, // Or any other size you want
        Orientation = Orientation.Portrait,
    },
                Objects = {
        new ObjectSettings()
        {
            HtmlContent = renderedHtml, // The HTML content you've rendered
        }
    }
            };

            byte[] pdfBytes = converter.Convert(doc);
            try
            {
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "attachment; filename=payslip.pdf");

                // Write the PDF bytes to the response stream
                Response.BinaryWrite(pdfBytes);
            }
            finally
            {
                // Ensure that pdfBytes is properly disposed
                if (pdfBytes != null)
                {
                    pdfBytes = null;
                }
            }


        }
        public ActionResult View_Payroll()
        {
            string month = Session["month"] as string;
            string year = Session["year"] as string;
            Dictionary<string, bool> remainingCheckfields = Session["Checkfields"] as Dictionary<string, bool>;

            // Store remainingCheckfields in a ViewBag property
            ViewBag.Checkfields = remainingCheckfields;
            schedule.Create_Schedule(month, year);

           
            schedule.allsettings.Get_Settings();

            // Optionally, you can pass these lists to the view using ViewBag or a model
           
            return View("Payroll", schedule);
        }
       
            [HttpPost]
            public ActionResult Payroll(FormCollection checkfields)
            {
                // Retrieve values from the requestData dictionary
                Session["month"] = checkfields["month"].ToString();
            Session["year"] = checkfields["year"].ToString();

            // Remove month and year entries from the dictionary
            Dictionary<string, bool> remainingCheckfields = new Dictionary<string, bool>();

            foreach (var key in checkfields.AllKeys)
            {
                // Skip "month" and "year" keys
                if (key != "month" && key != "year")
                {
                    bool isChecked = checkfields[key] == "true"; // Convert the string value to a bool
                    remainingCheckfields[key] = isChecked;
                }
            }

            // Store the remaining checkfields dictionary in a session variable
            Session["Checkfields"] = remainingCheckfields;

            // Return a response
            return Json(new { status = true });
            }

            public ActionResult CreatePopup(string id = "")
        {
            staff staff = new staff(id == "" ? "" : id); // Create a new staff object to be used as the model for the partial view
            staff.allsettings.Get_Settings();
            List<string> banks = staff.GetAllBanks(staff.allsettings.paystack_key);
            // Optionally, you can pass these lists to the view using ViewBag or a model
            ViewBag.Banks = new SelectList(banks, staff.bank);
            return PartialView("_SCreate", staff);
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


                    return Json(new { status = true });
                }




            }
            else
            {
                
                return Json(new { status = false, message = "Please fill all fields!" });
            }

        }

    }

    // Callback URL to handle Paystack payment callback (can be set in Paystack Dashboard)


}