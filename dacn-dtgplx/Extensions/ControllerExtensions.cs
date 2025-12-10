using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace dacn_dtgplx.Extensions
{
    public static class ControllerExtensions
    {
        public static async Task<string> RenderViewAsync<TModel>(
            this Controller controller,
            string viewName,
            TModel model,
            bool partial = false)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.RouteData.Values["action"].ToString();

            controller.ViewData.Model = model;

            using var writer = new StringWriter();
            IViewEngine engine = controller.HttpContext.RequestServices
                .GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

            var view = engine.FindView(controller.ControllerContext, viewName, !partial).View;

            ViewContext viewContext = new(
                controller.ControllerContext,
                view,
                controller.ViewData,
                controller.TempData,
                writer,
                new HtmlHelperOptions()
            );

            await view.RenderAsync(viewContext);
            return writer.GetStringBuilder().ToString();
        }
    }
}