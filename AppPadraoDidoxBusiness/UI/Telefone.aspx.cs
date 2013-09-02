using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Didox.Business;
using Didox.DataBase.Generics;

public partial class _Telefone : System.Web.UI.Page 
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request["idTelefone"] != null)
            {
                GetTelefone(int.Parse(Request["idTelefone"]));
                return;
            }

            GetTelefones();
        }
    }

    protected void btnSalvar_Click(object sender, EventArgs e)
    {
        var Telefone = new Telefone();
        try
        {
            if (txtId.Text != "")
            {
                Telefone.IDTelefone = int.Parse(txtId.Text);
                Telefone.Get();
            }

            Telefone.DDI = int.Parse(txtDDI.Text);
Telefone.DDD = int.Parse(txtDDD.Text);
Telefone.Numero = int.Parse(txtNumero.Text);
Telefone.IDTipoTelefone = int.Parse(txtIDTipoTelefone.Text);

            Telefone.Save();
            GetTelefone((int)Telefone.IDTelefone);

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

    private void GetTelefones()
    {
        dvSalvarTelefone.Visible = false;
        dvListarTelefones.Visible = true;
        
        gvTelefones.DataSource = new Telefone().Find();
        gvTelefones.DataBind();
    }

    private void GetTelefone(int idTelefone)
    {
        dvSalvarTelefone.Visible = true;
        dvListarTelefones.Visible = false;

        var Telefone = new Telefone();
        Telefone.IDTelefone = idTelefone;
        Telefone.Get();

        txtId.Text = Telefone.IDTelefone.ToString();
        txtDDI.Text = Telefone.DDI.ToString();
txtDDD.Text = Telefone.DDD.ToString();
txtNumero.Text = Telefone.Numero.ToString();
txtIDTipoTelefone.Text = Telefone.IDTipoTelefone.ToString();

    }

    protected void btnVoltar_Click(object sender, EventArgs e)
    {
        GetTelefones();
    }

    protected void btnIncluir_Click(object sender, EventArgs e)
    {
        dvSalvarTelefone.Visible = true;
        dvListarTelefones.Visible = false;

        txtId.Text = "";
        txtDDI.Text = string.Empty;
txtDDD.Text = string.Empty;
txtNumero.Text = string.Empty;
txtIDTipoTelefone.Text = string.Empty;

    }

	protected void DeleteTelefone(int idTelefone)
    {
        try
        {
			var Telefone = new Telefone();
			Telefone.IDTelefone = idTelefone;
			Telefone.Delete();
			GetTelefones();
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
    
    protected void gvTelefones_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Editar")
            GetTelefone(int.Parse(e.CommandArgument.ToString()));
        else if (e.CommandName == "Excluir")
			DeleteTelefone(int.Parse(e.CommandArgument.ToString()));
    }

    protected void gvTelefones_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvTelefones.PageIndex = e.NewPageIndex;
        GetTelefones();
    }
}
