using System.Threading.Tasks;

namespace Vykut.to.Services.ViewRender
{
    // https://stackoverflow.com/a/57888901
    interface IViewRenderService
    {
        /// <summary>
        /// Renders a .cshtml view into a HTML string
        /// </summary>
        /// <param name="viewName">Name of rendered view</param>
        /// <param name="model">Model for view data binding</param>
        /// <returns>HTML of view</returns>
        Task<string> RenderToStringAsync(string viewName, object model);
    }
}
