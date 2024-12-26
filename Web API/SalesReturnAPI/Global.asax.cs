using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SalesReturnAPI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            int ret = 0;
            string LogPath;
            DateTime dtFlTm;

            LogPath =  "";

            ret = Convert.ToInt32(ConfigurationManager.AppSettings["DebugDays"]);
            ret = ret == 0 ? -10 : ret * -1;
            dtFlTm = DateTime.Now.AddDays(ret);
            LogPath = Server.MapPath("") + "\\Log\\";

            if (Directory.Exists(LogPath) == false)
                Directory.CreateDirectory(LogPath);

            // Log Debug Files
            string[] sLogs = Directory.GetFiles(LogPath, "Debug*");
            foreach (string sfile in sLogs)
            {
                FileInfo fi = new FileInfo(sfile);
                if (fi.CreationTime <= dtFlTm)
                    fi.Delete();
            }
        }
    }
}
