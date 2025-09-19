using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WSM.Helpers
{
    public class CaptchaRequiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var captchaVerified = context.HttpContext.Session.GetString("CaptchaVerified");
            if (captchaVerified != "true")
            {
                context.Result = new RedirectToActionResult("Both", "Home", null);
            }
        }
    }
}
