using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Zata.Dynamic;

namespace WebUI
{
    public partial class Test1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("~/Test2.aspx");
        }


        [CachedMethodAttribute]
        public string Test(
            string a, string b, string c)
        {
            return string.Format("<a>{0}</a><b>{1}</b><c>{2}</c>", a, b, c);
        }

        protected override void OnError(EventArgs e)
        {
            
            base.OnError(e);
        }
    }
}