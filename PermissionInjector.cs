using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Html.PermissionInjector
{
    public class PermissionsInjector
    {
        private readonly IList<Permission> _permissionList;

        /// <summary>
        /// A class for injecting code into the the application that will serve to hide HTML elements that
        /// a given user does not have access to view or interact with.
        /// </summary>
        /// <param name="permissionList"></param>
        public PermissionsInjector(IList<Permission> permissionList)
        {
            _permissionList = permissionList;
        }

        /// <summary>
        /// Inserts into the page JavaScript surrounded by "script" tags.
        /// JS will hide any of the HTML elements matching the selectors provided during
        /// the class instantiation.
        /// </summary>
        /// <returns>JavaScript to hide elements.</returns>
        public string InjectAsJavaScript(string resourceName = null)
        {
            var sb = new StringBuilder();

            foreach (var permission in FilterPermissions(_permissionList, resourceName))
                sb.Append(GetJavaScriptTextToHideElement(permission));

            if (sb.Length > 0)
            {
                sb.Insert(0, "<script>\n");
                sb.Append("</script>\n");
            }

            return sb.ToString();
        }

        /// <summary>
        /// NOT SUPPORTED - Idea to inject using middleware pipeline directly.
        /// Could also use to remove the element entirely.
        /// </summary>
        public void InjectIntoMiddlewarePipeline(HttpContext context)
        {
            throw new NotImplementedException("Method 'InjectIntoMiddlewarePipeline' not implemented.");
            //TODO - Could add injection to context pipeline instead.
            //However this would mean needlessly parsing the HTML with HTML Agility Pack or similar.
            //This would add unnecessary overhead to each request.
            //app.Use(async (context, next) =>
            //{
            //    var injector = new PermissionsInjector(GetPermissions());

            //});
        }

        /// <summary>
        /// Get only those permissions related to this the current page and or resource.
        /// Also include only those items we are restricting access to.
        /// </summary>
        /// <param name="permissionList">List of permissions.</param>
        /// <param name="resourceName">Name of resource we will restrict access to.</param>
        /// <returns></returns>
        private IList<Permission> FilterPermissions(IList<Permission> permissionList, string resourceName) =>
            permissionList.Where(x => !x.HasAccess && (string.IsNullOrEmpty(resourceName) || x.ResourceName == resourceName))
                .ToList();


        /// <summary>
        /// Generates JS to hide elements matching a given permission's selector.
        /// </summary>
        /// <param name="permission">Information about what HTML element to hide.</param>
        /// <returns>JavaScript to hide all matching elements.</returns>
        private string GetJavaScriptTextToHideElement(Permission permission)
        {
            //TODO: Could update to C# 8+ Switch Lambda Syntax
            var js = string.Empty;

            switch (permission.IdentifiedBy)
            {
                case IdentifiedBy.Id:
                    js = $"document.getElementById('{permission.Identifier}').style.visibility = 'hidden';\n";
                    break;
                case IdentifiedBy.ClassName:
                    js = $"Array.prototype.forEach.call(document.getElementsByClassName('{permission.Identifier}'), element => {{element.style.visibility = 'hidden'}});\n";
                    break;
                case IdentifiedBy.Selector:
                    js = $"Array.prototype.forEach.call(document.querySelectorAll('{permission.Identifier}'), element => {{element.style.visibility = 'hidden'}});\n";
                    break;
                case IdentifiedBy.Name:
                    js = $"Array.prototype.forEach.call(document.getElementsByName('{permission.Identifier}'), element => {{element.style.visibility = 'hidden'}});\n";
                    break;
            }

            return js;
        }
    }
}
