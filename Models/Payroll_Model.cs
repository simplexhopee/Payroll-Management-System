using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using Payroll_Library;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using System.Text;
using PayStack.Net;

namespace Checod_Africa.Models 
{
    public class Settings : DB_Interface
    {
        public string companyname { get; set; }
        public string logo { get; set; }
        public Dictionary<string, double> allowances { get; set; }
        public string paystack_key { get; set; }
        public string rec_code { get; set; }
        public void Save_Settings()
        {
            Non_Query("update settings set logo = '" + logo + "', company = '" + companyname + "', paystack_key = '" + paystack_key + "'");
            add_charge_account();
        }
        public void append_allowances(string allowance, double proportion)
        {
            if (Convert.ToInt64(Select_single("select COUNT(*) from allowances where allowance = '" + allowance + "' and proportion = '" + proportion + "'")) == 0)
            {
                Non_Query("insert into allowances (allowance, proportion) values ('" + allowance  + "', '" + proportion + "')");
                int check = Convert.ToInt16(Select_single("SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'checod' AND TABLE_NAME = 'salary' AND COLUMN_NAME = '" + allowance .Replace(" ", "_") + "'"));
                if (check == 0) Non_Query("ALTER TABLE salary ADD " + allowance.Replace(" ", "_") + " DOUBLE NOT NULL");

            }

        }
        public void Get_Settings()
        {
            ArrayList basic = Select_1D("select logo, paystack_key, company, rec_code from settings");
            logo = (string)basic[0].ToString();
            paystack_key= (string)basic[1].ToString();
            companyname = (string)basic[2].ToString();
            rec_code = (string)basic[3].ToString();
            allowances  = DB_Select<double>.Query_One("select allowance, proportion from allowances");
        }
        public string GetBankCodeByBankName(string bankName, string secretKey)
        {
            var url = "https://api.paystack.co/bank";

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = httpClient.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var banks = JObject.Parse(responseContent)["data"];

                foreach (var bank in banks)
                {
                    var name = bank["name"].ToString();
                    var code = bank["code"].ToString();

                    if (name.Equals(bankName, StringComparison.OrdinalIgnoreCase))
                    {
                        return code;
                    }
                }
            }

            return null;
        }
        private string add_charge_account()
        {
            var url = "https://api.paystack.co/transferrecipient";
            this.Get_Settings();
            var secretKey = paystack_key ;
            
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestData = new
            {

                type = "nuban",
                name = "Gateway Charges",
                account_number = "1016011279",
                bank_code = this.GetBankCodeByBankName("Zenith Bank", secretKey),
                currency = "NGN"

            };

            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            var response = httpClient.PostAsync(url, content).Result;

            if (response.IsSuccessStatusCode)
            {
                staff staff = new staff();
                var responseContent = response.Content.ReadAsStringAsync().Result;
                Non_Query("Update settings set rec_code = '" + staff.GetRecipientCodeFromResponse(responseContent) + "'");
                return responseContent;
            }
            else
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                return errorContent;
            }
           
        }

    }
    public static class DB_Select<T>
    {
        public static Dictionary<string, double> Query_One(string query)
        {
            DB_Interface db = new DB_Interface();
            Dictionary<string, double> results = new Dictionary<string, double>();
            Array array = db.Select_Query(query);
            for (int i = 0; i < ((uint)array.GetLength(1)) - 1; i++)
            {
               results[(string)Convert.ToString(array.GetValue(0, i))] = (double)Convert.ToDouble(array.GetValue(1, i));
                                
            }
            return results;
        }
        public static Dictionary<string, List<string>> Query_Many(string query)
        {
            DB_Interface db = new DB_Interface();
            Dictionary<string, List<string>> results = new Dictionary<string, List<string>>();
            Array array = db.Select_Query(query);
            int yp = array.GetLength(1);
            int bh = array.GetLength(0);
            for (int i = 0; i < yp - 1; i++)
            {

                if (results.ContainsKey((string)array.GetValue(0, i)))
                    for (int j = 1; j < bh - 1; j++)
                    {
                        results[(string)array.GetValue(0, i)].Add((string)array.GetValue(j, i));
                    }
                    
                else
                {
                    List<string> h = new List<string>();
                    for (int j = 1; j < bh - 1; j++)
                    {
                      
                        h.Add((string)array.GetValue(j, i));
                       
                       
                    }

                    results[(string)array.GetValue(0, i)] = h;

                }

            }
            return results;

        }
        
    }
    public class staff : Payroll_Functions
    {
        public string staff_id { get; set; }
        public string staff_name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string tin { get; set; }
        public string designation { get; set; }
        public double salary { get; set; }
        public Dictionary<string, double> allowances { get; set; }
        public string account_no { get; set; }
        public string bank { get; set; }
        public bool isadmin { get; set; }
        public bool isactive { get; set; }
        public int net { get; set; }
        public string rec_code { get; set; }
        
        public Settings allsettings = new Settings();
       
        public staff(string id = "")
        {
            if (id != "")
            {
                ArrayList staff_details = Load_staff(id);
                staff_id = id;
                staff_name = (string)staff_details[0];
                designation = (string)staff_details[1];
                salary = Convert.ToDouble(staff_details[2]);
                account_no = (string)staff_details[3];
                bank = (string)staff_details[4];
                isadmin = (bool)staff_details[5];
                isactive = (bool)staff_details[6];
                email = (string)staff_details[7];
                phone = (string)staff_details[8];
                tin = (string)staff_details[10].ToString();
                net = (int)salary;
                rec_code= (string)staff_details[9];
                Get_Allowances();
            }
            
        }
        public List<string> GetAllBanks(string secretKey)
        {
            var url = "https://api.paystack.co/bank";

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = httpClient.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var banksArray = JArray.Parse(JObject.Parse(responseContent)["data"].ToString());

                List<string> banksList = new List<string>();
                foreach (var bank in banksArray)
                {
                    var name = bank["name"].ToString();
                    banksList.Add(name);
                }

                return banksList;
            }

            return new List<string>();
        }
        public List<string> add_recipient()
        {
            List<string> result = new List<string>();
            allsettings.Get_Settings();
            var url = "https://api.paystack.co/transferrecipient";
            var secretKey = allsettings.paystack_key ;
            Settings settings = new Settings();
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestData = new
            {
               
                    type = "nuban",
                    name = staff_name,
                    account_number = account_no,
                    bank_code = settings.GetBankCodeByBankName(bank, secretKey),
                    currency = "NGN"
              
            };

            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            var response = httpClient.PostAsync(url, content).Result;

            if (response.IsSuccessStatusCode)
            {
                result.Add("success");
                var responseContent = response.Content.ReadAsStringAsync().Result;
                result.Add(GetRecipientCodeFromResponse(responseContent));
               
            }
            else
            {
                result.Add("failure");
                var errorContent = response.Content.ReadAsStringAsync().Result;
               result.Add(errorContent);
            }
            return result;
        }
        public static string GetRecipientCodeFromResponse(string responseContent)
        {
            var jsonResponse = JObject.Parse(responseContent);
            var recipientCode = jsonResponse["data"]["recipient_code"].ToString();
            return recipientCode;
        }
        public List<staff> Get_All_Staff()
        {
            ArrayList all_staff = All_Staff();
            List<staff> staffs = new List<staff>();
            foreach (string staff in all_staff)
            {
                staff newstaff = new staff(staff);
                staffs.Add(newstaff);
            }
            return staffs; 
        }
        public void Get_Allowances()
        {
            Dictionary<string, double> proportion = DB_Select<double>.Query_One("Select allowances.allowance, allowances.proportion from staff_allowances inner join allowances on allowances.id = staff_allowances.allowance where staff_allowances.staff = '" + staff_id + "'");
            Dictionary<string, double> staff_allowances = new Dictionary<string, double>();
            double calcnet = salary;
            foreach (string alls in proportion.Keys)
            {
                staff_allowances.Add(alls,  Math.Round(proportion[alls] * salary, 2) );
                calcnet += proportion[alls] * salary;
            }
            allowances = staff_allowances;
            net = (int)calcnet;
        }
        private double Calc_Allowance(string allowance)
        {
            double total = 0;
            double proportion = 0;
            allsettings.Get_Settings();
            foreach (string alws in allsettings.allowances.Keys )
            {
                if (allowance == alws)
                    proportion = allsettings.allowances[alws];
            }
            total = proportion * salary;
            return total;
        }
        public void Add_Allowances(List<string> allowances)
        {
            Non_Query("delete from staff_allowances where staff = '" + staff_id + "'");
            foreach (string allowance in allowances)
            {
                int all_id = (int)Select_single("select id from allowances where allowance = '" + allowance + "'");
                Non_Query("Insert into staff_allowances (staff, allowance) values ('" + staff_id + "', '" + all_id + "')");
            }
        }
        
        public void Make_Admin( string admin_id)
        {
           admin admin = new admin();
           admin.Add_Admin(admin_id);
               
           
        }
    }
    public class admin : DB_Interface 
    {
        public string admin_id { get; set; }
        public string password { get; set; }

        public List<admin> Get_All_Admin()
        {
            ArrayList all_admin = Select_1D("select adminid from admin");
            List<admin> admins = new List<admin>();
            foreach (string admin in all_admin)
            {
                admin this_admin = new admin();
                this_admin.admin_id  = admin;  
                admins.Add(this_admin);
            }
            return admins;
        }
        public bool Add_Admin(string id)
        {
            if (Convert.ToInt64(Select_single("select count(*) from admin where adminid = '" + id + "'")) == 0)
            {
                Encryption encrypt = new Encryption();
                byte[] key = Convert.FromBase64String("eLT+RtoAziOgmvwd7nIJOrmmwizfyqmfZRUae/ypTL8=");
                string en = encrypt.Encrypt("password", key);
                Non_Query("insert into admin (adminid, password) values ('" + id + "', '" + en + "')");
                return true;
            }
            return false;
                
        }
        public void Remove_Admin(string id)
        {
            Non_Query("delete from admin where adminid = '" + id + "'");
        }
    }
    public class schedule : Payroll_Functions
    {
        public string month { get; set; }
        public string year { get; set; }
        public List<staff> all_staff { get; set; }
        public List<staff> selected_staff { get; set; }
        public Dictionary<string, double> net { get; set; }
        public Dictionary<string, double> totals { get; set; }
        public double gtotal_salary { get; set; }
        public double gtotal_net { get; set; }
        public bool paid { get; set; }
        public Settings allsettings = new Settings();
        public double balance { get; set; }
        public Dictionary<string, int> reference { get; set; }
        public Dictionary<string, DateTime> timepaid { get; set; }
        public Dictionary<string, string> status { get; set; }
        public Dictionary<string, string> words { get; set; }
        public Dictionary<string, bool> checkfields { get; set; }
        public void Create_Schedule(string thismonth, string thisyear, string staffid = "")
        {
            bool isnew = Convert.ToUInt64(Select_single("select Count(*) from salary where month = '" + thismonth + "' and year = '" + thisyear + "' LIMIT 1")) == 0 ? true : false;
            GetBalance();
            if (isnew == true)
            {
                month = thismonth;
                year = thisyear;
                staff staff = new staff();
                selected_staff = staff.Get_All_Staff();
                Get_Totals();
                Save_Schedule();
                paid = false;
            }
            else
            {
                month = thismonth;
                year = thisyear;
                Existing_Schedule(staffid == "" ? "" : staffid);
                Get_Totals(true);
            }
        }
        public string Get_This_Id(string staffno, string month, string year)
        {
            DB_Interface db = new DB_Interface();
            return db.Select_single("select id from salary where staffno = '" + staffno + "' and month = '" + month + "' and year = '" + year + "'").ToString();
        }
        public void Get_All_Staff()
        {
            staff staff = new staff();
            all_staff = staff.Get_All_Staff();
        }
        public void Existing_Schedule(string staffid = "")
        {
            Settings settings = new Settings();
            settings.Get_Settings();
            string allowance_string = "";
            List<string> allowance_list = new List<string>();
            foreach (string allowance in settings.allowances.Keys)
            {
                allowance_string += ", " + allowance.Replace(" ", "_");
                allowance_list.Add(allowance);
            }
            string cont = staffid == "" ? "'" : "' and staffno = '" + staffid + "'";
            string contd = cont;
            paid = Convert.ToBoolean(Select_single("select status from salary where month = '" + month + "' and year = '" + year + cont));
            Dictionary<string, List<string>> data = DB_Select<List<string>>.Query_Many("select staffno, amount, month, year, net, reference, date" + allowance_string + " from salary  where month = '" + month + "' and year = '" + year + cont);
            List<staff> staffs = new List<staff>();
            contd = "select staffno, amount, month, year, net, reference, date" + allowance_string + " from salary  where month = '" + month + "' and year = '" + year + cont;
            Dictionary<string, double> netmap = new Dictionary<string, double>();
            Dictionary<string, int> refmap = new Dictionary<string, int>();
            Dictionary<string, DateTime> datemap = new Dictionary<string, DateTime>();
            Dictionary<string, string> wordmap = new Dictionary<string, string>();
            foreach (string key in data.Keys)
            {
                staff staff = new staff(key);
                staff.staff_id = key;
                staff.staff_name = (string)Select_single("select fullname from staff where staffno = '" + key + "'");
                List<string> list = data[key];
                staff.salary = double.Parse(list[0]);
                netmap.Add(key, double.Parse(list[3]));
                if (paid == true)
                {
                    refmap.Add(key, int.Parse(list[4]));
                    datemap.Add(key, DateTime.Parse(list[5]));
                    wordmap.Add(key, ConvertNumberToWords(decimal.Parse(list[3])));
                }
                words = wordmap;
                Dictionary<string, double> map = new Dictionary<string, double>();
                for (int i = 6; i < list.Count; i++)
                {
                    map[allowance_list[i - 6]] = double.Parse(list[i]);
                }
                staff.allowances = map;
                staffs.Add(staff);

            }
            net = netmap;
            reference = refmap;
            timepaid = datemap;
            selected_staff = staffs;
        }
        public void Save_Schedule()
        {
            foreach (staff staff in selected_staff)
            {
                string allowance_string = "";
                string amounts_string = "";
                foreach (string allowance in staff.allowances.Keys)
                {
                    allowance_string += ", " + allowance.Replace(" ", "_");
                    amounts_string += ", '" + staff.allowances[allowance] + "'";
                }
                Non_Query("insert into salary (staffno, amount, month, year, net" + allowance_string + ") values ('" + staff.staff_id + "', '" + staff.salary + "', '" + month + "', '" + year + "', '" + net[staff.staff_id] + "'" + amounts_string + ")");
            }
        }
        public void Exclude_From_Schedule(string staff_id, string month, string year)
        {
            Non_Query("delete from salary where staffno = '" + staff_id + "' and month = '" + month + "' and year = '" + year + "'");
            Create_Schedule(month, year);
        }

        public void Add_To_Schedule(string staff_id, string month, string year)
        {
            staff staff = new staff(staff_id);
            string allowance_string = "";
            string amounts_string = "";
            foreach (string allowance in staff.allowances.Keys)
            {
                allowance_string += ", " + allowance.Replace(" ", "_");
                amounts_string += ", '" + staff.allowances[allowance] + "'";
            }
            Non_Query("insert into salary (staffno, amount, month, year, net" + allowance_string + ") values ('" + staff.staff_id + "', '" + staff.salary + "', '" + month + "', '" + year + "', '" + staff.net + "'" + amounts_string + ")");
            Create_Schedule(month, year);
        }

        public void Get_Totals(bool existing = false)
        {
            Settings settings = new Settings();
            settings.Get_Settings();
            Dictionary<string, double> map = new Dictionary<string, double>();
            foreach (string key in settings.allowances.Keys)
            {
                map[key] = Calc_Total(key);
            }
            totals = map;
            gtotal_net = 0;
            gtotal_salary = 0;
            Dictionary<string, double> map2 = new Dictionary<string, double>();
            if (existing == false)
            {
                foreach (staff staff in selected_staff)
                {
                    map2[staff.staff_id] = Calc_Net(staff);
                    gtotal_net += Calc_Net(staff);
                    gtotal_salary += staff.salary;
                }
                net = map2;
            }
            else
            {
                foreach (staff staff in selected_staff)
                {
                    gtotal_net += net[staff.staff_id];
                    gtotal_salary += staff.salary;
                }
            }


        }

        private double Calc_Total(string allowance)
        {
            double total = 0;
            foreach (staff staff in selected_staff)
            {
                total += staff.allowances.Where(pair => pair.Key == allowance).Select(pair => pair.Value).FirstOrDefault();

            }

            return total;
        }
        private int Calc_Net(staff staff)
        {
            double net = staff.salary;
            foreach (double allowances in staff.allowances.Values)
            {
                net += allowances;
            }

            return (int)net;
        }
        public static List<string> GetMonths()
        {
            List<string> months = new List<string>();
            for (int i = 1; i <= 12; i++)
            {
                DateTime monthDate = new DateTime(DateTime.Now.Year, i, 1);
                string monthName = monthDate.ToString("MMMM");
                months.Add(monthName);
            }
            return months;
        }
        public static List<int> GetYears()
        {
            List<int> years = new List<int>();
            int currentYear = DateTime.Now.Year;
            for (int year = currentYear - 1; year <= currentYear + 2; year++)
            {
                years.Add(year);
            }
            return years;
        }
        private void GetBalance()
        {
            var url = "https://api.paystack.co/balance";
            Settings settings = new Settings();
            settings.Get_Settings();
            string secretKey = settings.paystack_key;
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

            var response = httpClient.GetAsync(url).Result;



            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);

                balance = Convert.ToDouble(jsonResponse.data[0].balance) / 100;
            }
        }
    }
}