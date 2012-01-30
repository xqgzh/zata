using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

namespace WebUI
{
    public partial class Test2 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Zata.Dynamic.Web.HttpMethodBuilder.CurrentAction.IsRenderView = true;

            Response.ContentType = "text/html";

            Response.ContentEncoding = Encoding.UTF8;
            Response.AddHeader("X-Auth", "1234567");

            Response.Write("<h1>Test</h1>");

        }
    }
}