using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;

namespace DataDashboardWeb
{
    public class NonWwwRule : IRule
    {
        public void ApplyRule(RewriteContext context)
        {
            var req = context.HttpContext.Request;
            var currentHost = req.Host;
            if (currentHost.Host.StartsWith("www."))
            {
                var newHost = new HostString(currentHost.Host.Substring(4));
                var newUrl = new StringBuilder().Append("https://").Append(newHost).Append(req.PathBase).Append(req.Path).Append(req.QueryString);
                
                context.HttpContext.Response.StatusCode = StatusCodes.Status301MovedPermanently;
                context.HttpContext.Response.Redirect(newUrl.ToString(), true);
                context.Result = RuleResult.EndResponse;
            }
        }
    }
}
