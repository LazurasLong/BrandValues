using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrandValues.Utils
{
    public static class IpAddress
    {
        public static string GetIp()
        {

            string visitorsIPAddr = string.Empty;
            if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            {
                visitorsIPAddr = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            }
            else if (HttpContext.Current.Request.UserHostAddress.Length != 0)
            {
                visitorsIPAddr = HttpContext.Current.Request.UserHostAddress;
            }
            return visitorsIPAddr;

        }

        public static bool CheckIp()
        {
            if (Utils.IpAddress.GetIp() == "194.69.198.225")
            {
                return true;
            }

            if (Utils.IpAddress.GetIp() == "194.69.198.226")
            {
                return true;
            }

            if (Utils.IpAddress.GetIp() == "194.69.198.227")
            {
                return true;
            }

            if (Utils.IpAddress.GetIp() == "194.69.198.228")
            {
                return true;
            }

            return false;
        }

    }
}