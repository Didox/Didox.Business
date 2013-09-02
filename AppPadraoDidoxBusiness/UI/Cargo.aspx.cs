using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Didox.Business;
using Didox.DataBase.Generics;

public partial class _Cargo : System.Web.UI.Page 
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request["idCargo"] != null)
            {
                GetCargo(int.Parse(Request["idCargo"]));
                return;
            }

            GetCargos();
        }
    }

    protected void btnSalvar_Click(object sender, EventArgs e)
    {
        var Cargo = new Cargo();
        try
        {
            if (txtId.Text != "")
            {
                Cargo.IDCargo = int.Parse(txtId.Text);
                Cargo.Get();
            }

            Cargo.Descricao = txtDescricao.Text;

            Cargo.Save();
            GetCargo((int)Cargo.IDCargo);

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

    private void GetCargos()
    {
        dvSalvarCargo.Visible = false;
        dvListarCargos.Visible = true;
        
        gvCargos.DataSource = new Cargo().Find();
        gvCargos.DataBind();
    }

    private void GetCargo(int idCargo)
    {
        dvSalvarCargo.Visible = true;
        dvListarCargos.Visible = false;

        var Cargo = new Cargo();
        Cargo.IDCargo = idCargo;
        Cargo.Get();

        txtId.Text = Cargo.IDCargo.ToString();
        txtDescricao.Text = Cargo.Descricao.ToString();

    }

    protected void btnVoltar_Click(object sender, EventArgs e)
    {
        GetCargos();
    }

    protected void btnIncluir_Click(object sender, EventArgs e)
    {
        dvSalvarCargo.Visible = true;
        dvListarCargos.Visible = false;

        txtId.Text = "";
        txtDescricao.Text = string.Empty;

    }

	protected void DeleteCargo(int idCargo)
    {
        try
        {
			var Cargo = new Cargo();
			Cargo.IDCargo = idCargo;
			Cargo.Delete();
			GetCargos();
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
    
    protected void gvCargos_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Editar")
            GetCargo(int.Parse(e.CommandArgument.ToString()));
        else if (e.CommandName == "Excluir")
			DeleteCargo(int.Parse(e.CommandArgument.ToString()));
    }

    protected void gvCargos_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvCargos.PageIndex = e.NewPageIndex;
        GetCargos();
    }
}
