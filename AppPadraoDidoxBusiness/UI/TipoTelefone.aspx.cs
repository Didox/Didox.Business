using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Didox.Business;
using Didox.DataBase.Generics;

public partial class _TipoTelefone : System.Web.UI.Page 
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request["idTipoTelefone"] != null)
            {
                GetTipoTelefone(int.Parse(Request["idTipoTelefone"]));
                return;
            }

            GetTipoTelefones();
        }
    }

    protected void btnSalvar_Click(object sender, EventArgs e)
    {
        var TipoTelefone = new TipoTelefone();
        try
        {
            if (txtId.Text != "")
            {
                TipoTelefone.IDTipoTelefone = int.Parse(txtId.Text);
                TipoTelefone.Get();
            }

            TipoTelefone.Nome = txtNome.Text;

            TipoTelefone.Save();
            GetTipoTelefone((int)TipoTelefone.IDTipoTelefone);

            Page.ClientScript.RegisterStartupScript(this.GetType(), "script", "<script>alert('Registro salvo.')</script>");
        }
        catch (Exception err)
        {
            var erroMessage = err.Message;
            erroMessage += ", " + err.StackTrace;
            erroMessage = erroMessage.Replace("\n", " ");
            erroMessage = erroMessage.Replace("'", "");
            erroMessage = erroMessage.Replace("\r", " ");
            Page.ClientScript.RegisterStartupScript(this.GetType(), "script", "<script>alert('" + erroMessage + "')</script>");
        }
    }

    private void GetTipoTelefones()
    {
        dvSalvarTipoTelefone.Visible = false;
        dvListarTipoTelefones.Visible = true;
        
        gvTipoTelefones.DataSource = new TipoTelefone().Find();
        gvTipoTelefones.DataBind();
    }

    private void GetTipoTelefone(int idTipoTelefone)
    {
        dvSalvarTipoTelefone.Visible = true;
        dvListarTipoTelefones.Visible = false;

        var TipoTelefone = new TipoTelefone();
        TipoTelefone.IDTipoTelefone = idTipoTelefone;
        TipoTelefone.Get();

        txtId.Text = TipoTelefone.IDTipoTelefone.ToString();
        txtNome.Text = TipoTelefone.Nome.ToString();

    }

    protected void btnVoltar_Click(object sender, EventArgs e)
    {
        GetTipoTelefones();
    }

    protected void btnIncluir_Click(object sender, EventArgs e)
    {
        dvSalvarTipoTelefone.Visible = true;
        dvListarTipoTelefones.Visible = false;

        txtId.Text = "";
        txtNome.Text = string.Empty;

    }

	protected void DeleteTipoTelefone(int idTipoTelefone)
    {
        try
        {
			var TipoTelefone = new TipoTelefone();
			TipoTelefone.IDTipoTelefone = idTipoTelefone;
			TipoTelefone.Delete();
			GetTipoTelefones();
        }
        catch (Exception err)
        {
            var erroMessage = err.Message;
            erroMessage += ", " + err.StackTrace;
            erroMessage = erroMessage.Replace("\n", " ");
            erroMessage = erroMessage.Replace("'", "");
            erroMessage = erroMessage.Replace("\r", " ");
            Page.ClientScript.RegisterStartupScript(this.GetType(), "script", "<script>alert('" + erroMessage + "')</script>");
        }
    }
    
    protected void gvTipoTelefones_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Editar")
            GetTipoTelefone(int.Parse(e.CommandArgument.ToString()));
        else if (e.CommandName == "Excluir")
			DeleteTipoTelefone(int.Parse(e.CommandArgument.ToString()));
    }

    protected void gvTipoTelefones_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvTipoTelefones.PageIndex = e.NewPageIndex;
        GetTipoTelefones();
    }
}
