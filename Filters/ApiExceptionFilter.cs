using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using POE_RaceDay_MVC.Models;
using POE_RaceDay_MVC.Services;

namespace POE_RaceDay_MVC.Filters
{
    // Last line of defence: any ApiException an action did not handle is
    // turned into a friendly message instead of a crash page.
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ISessionUser _user;
        private readonly ITempDataDictionaryFactory _tempDataFactory;
        private readonly IModelMetadataProvider _metadata;

        public ApiExceptionFilter(
            ISessionUser user,
            ITempDataDictionaryFactory tempDataFactory,
            IModelMetadataProvider metadata)
        {
            _user = user;
            _tempDataFactory = tempDataFactory;
            _metadata = metadata;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is not ApiException ex) return;

            var tempData = _tempDataFactory.GetTempData(context.HttpContext);

            // Token expired or rejected: end the session and ask to log in.
            if (ex.StatusCode == 401)
            {
                _user.SignOut();
                tempData["Warning"] =
                    "Your session has expired. Please log in again.";

                context.Result = new RedirectToActionResult(
                    "Login", "Account", null);
                context.ExceptionHandled = true;
                return;
            }

            if (ex.StatusCode == 403)
            {
                context.Result = new RedirectToActionResult(
                    "AccessDenied", "Account", null);
                context.ExceptionHandled = true;
                return;
            }

            // A form post that failed: go home with an explanation.
            if (HttpMethods.IsPost(context.HttpContext.Request.Method))
            {
                tempData["Error"] = ex.Message;

                context.Result = new RedirectToActionResult(
                    "Index", "Home", null);
                context.ExceptionHandled = true;
                return;
            }

            // A page that could not load: show the friendly error page.
            context.Result = new ViewResult
            {
                ViewName = "Error",
                StatusCode = ex.StatusCode >= 400 ? ex.StatusCode : 500,
                ViewData = new ViewDataDictionary<ErrorViewModel>(
                    _metadata, context.ModelState)
                {
                    Model = new ErrorViewModel
                    {
                        StatusCode = ex.StatusCode,
                        Title = ex.StatusCode == 404
                            ? "We couldn't find that page"
                            : "Something went off course",
                        Message = ex.Message
                    }
                }
            };

            context.ExceptionHandled = true;
        }
    }
}
