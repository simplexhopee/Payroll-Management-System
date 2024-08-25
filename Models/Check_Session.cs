using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Checod_Africa.Controllers;
using System.Net.Http;

public class UserSessionFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        if ((filterContext.HttpContext.Session["user"]) == null && !(filterContext.Controller is HomeController) && !(filterContext.HttpContext.Request.Url.AbsolutePath.StartsWith("/schedule") && !string.IsNullOrEmpty(filterContext.HttpContext.Request.Url.Query )))
        {
            
             
                filterContext.Result = new ViewResult
                {
                    ViewName = "Error500", // Your custom error view name

                };
          
           
        }
    }
}
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class ExcludeUserSessionFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var skipFilter = filterContext.ActionDescriptor.IsDefined(typeof(ExcludeUserSessionFilterAttribute), inherit: true)
                         || filterContext.Controller.GetType().IsDefined(typeof(ExcludeUserSessionFilterAttribute), inherit: true);

       
    }
}
