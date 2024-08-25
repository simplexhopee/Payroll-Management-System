using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Checod_Africa
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                name: "Login",
                url: "Home/Login",
                defaults: new { controller = "Home", action = "Login", id = UrlParameter.Optional }
            );
            routes.MapRoute(
               name: "Home/Change",
               url: "Home/Change",
               defaults: new { controller = "Home", action = "Change", id = UrlParameter.Optional }
           );
            routes.MapRoute(
                name: "Settings",
                url: "Settings",
                defaults: new { controller = "Settings", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
               name: "SaveSettings",
               url: "Settings",
               defaults: new { controller = "Settings", action = "SaveSettings", id = UrlParameter.Optional }
           );
            routes.MapRoute(
              name: "Staff",
              url: "Staff",
              defaults: new { controller = "Staff", action = "Index", id = UrlParameter.Optional }
          );
            routes.MapRoute(
             name: "Staff/Create",
             url: "Staff/CreatePopup",
             defaults: new { controller = "Staff", action = "CreatePopup", id = UrlParameter.Optional }
         );
            routes.MapRoute(
             name: "Staff/Details",
             url: "Staff/ShowDetails/{id}",
             defaults: new { controller = "Staff", action = "DetailsPopup", id = UrlParameter.Optional }
         );
            routes.MapRoute(
             name: "Staff/DeleteStaff",
             url: "Staff/DeleteStaff/{id}",
             defaults: new { controller = "Staff", action = "DeleteStaff", id = UrlParameter.Optional }
         );
            routes.MapRoute(
            name: "Staff/payslip",
            url: "Staff/payslip",
            defaults: new { controller = "Staff", action = "payslip", id = UrlParameter.Optional }
        );
            routes.MapRoute(
            name: "Staff/Refresh",
            url: "Staff/Refresh",
            defaults: new { controller = "Staff", action = "Refresh", id = UrlParameter.Optional }
        );
            routes.MapRoute(
          name: "Staff/ResolveBankAccount",
          url: "Staff/ResolveBankAccount",
          defaults: new { controller = "Staff", action = "ResolveBankAccount", id = UrlParameter.Optional }
      );
            routes.MapRoute(
            name: "Schedule",
            url: "Schedule",
            defaults: new { controller = "Schedule", action = "Index", id = UrlParameter.Optional }
        );
            routes.MapRoute(
           name: "Schedule/To_Pdf",
           url: "Schedule/To_Pdf",
           defaults: new { controller = "Schedule", action = "To_Pdf", id = UrlParameter.Optional }
       );
            routes.MapRoute(
            name: "Change",
            url: "Schedule",
            defaults: new { controller = "Schedule", action = "Change", id = UrlParameter.Optional }
        );
            routes.MapRoute(
            name: "Schedule/PaySalary",
            url: "Schedule/PaySalary/{id}",
            defaults: new { controller = "Schedule", action = "PaySalary", id = UrlParameter.Optional }
        );
            routes.MapRoute(
            name: "Schedule/Resend_Email",
            url: "Schedule/Resend_Email",
            defaults: new { controller = "Schedule", action = "Resend_Email", id = UrlParameter.Optional }
        );
            routes.MapRoute(
            name: "Schedule/Payroll",
            url: "Schedule/Payroll",
            defaults: new { controller = "Schedule", action = "Payroll", id = UrlParameter.Optional }
        );
            routes.MapRoute(
            name: "Schedule/payslip",
            url: "Schedule/payslip",
            defaults: new { controller = "Schedule", action = "payslip", id = UrlParameter.Optional }
        );
            routes.MapRoute(
            name: "Schedule/View_Payroll",
            url: "Schedule/View_Payroll",
            defaults: new { controller = "Schedule", action = "View_Payroll", id = UrlParameter.Optional }
        );
            routes.MapRoute(
           name: "Admin",
           url: "Admin",
           defaults: new { controller = "Admin", action = "Index", id = UrlParameter.Optional }
       );
            routes.MapRoute(
           name: "Admin/Save",
           url: "Admin/Save",
           defaults: new { controller = "Admin", action = "Save", id = UrlParameter.Optional }
       );
            routes.MapRoute(
          name: "Admin/Delete",
          url: "Admin/Delete",
          defaults: new { controller = "Admin", action = "Delete", id = UrlParameter.Optional }
      );
            routes.MapRoute(
         name: "schedule/CreatePopup",
         url: "schedule/CreatePopup",
         defaults: new { controller = "schedule", action = "CreatePopup", id = UrlParameter.Optional }
     );
            routes.MapRoute(
                     name: "schedule/Refresh",
                     url: "schedule/Refresh",
                     defaults: new { controller = "schedule", action = "Refresh", id = UrlParameter.Optional }
                 );
            routes.MapRoute(
         name: "schedule/Edit_Staff",
         url: "schedule/Edit_Staff",
         defaults: new { controller = "schedule", action = "Edit_Staff", id = UrlParameter.Optional }
     );



        }
    }
}
