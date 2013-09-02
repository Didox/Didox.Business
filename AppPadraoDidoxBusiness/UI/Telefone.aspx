<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Telefone.aspx.cs" Inherits="_Telefone" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Telefone</title>
</head>
<body>
    <form id="form1" runat="server">
 <h1>
        Telefones</h1>
    <div id="dvSalvarTelefone" visible="false" runat="server">
        <fieldset>
            <legend>Editar</legend>
            <asp:TextBox ID="txtId" runat="server" Width="45px" Visible="False"></asp:TextBox>
            <div id="dvFields">
				<div class="dvRoll">  <div class="dvColl">    DDI:  </div>  <div class="dvColl">    <asp:TextBox ID="txtDDI" runat="server"></asp:TextBox><br />  </div></div><div class="dvRoll">  <div class="dvColl">    DDD:  </div>  <div class="dvColl">    <asp:TextBox ID="txtDDD" runat="server"></asp:TextBox><br />  </div></div><div class="dvRoll">  <div class="dvColl">    Numero:  </div>  <div class="dvColl">    <asp:TextBox ID="txtNumero" runat="server"></asp:TextBox><br />  </div></div><div class="dvRoll">  <div class="dvColl">    IDTipoTelefone:  </div>  <div class="dvColl">    <asp:TextBox ID="txtIDTipoTelefone" runat="server"></asp:TextBox><br />  </div></div>				
            </div>
            <asp:Button ID="btnSalvar" runat="server" Text="Salvar" OnClick="btnSalvar_Click" />
            <asp:Button ID="btnVoltar" runat="server" Text="Voltar" OnClick="btnVoltar_Click" /><br />
        </fieldset>
    </div>
    <div id="dvListarTelefones" runat="server">
        <fieldset>
            <legend>Listar</legend>
            <asp:Button runat="server" ID="btnIncluir" Text="Incluir" OnClick="btnIncluir_Click" />
            <asp:GridView ID="gvTelefones" runat="server" Width="465px" AutoGenerateColumns="False"
                    EmptyDataText="Nenhum registro encontrado." AllowPaging="true" PageSize="20"
                OnRowCommand="gvTelefones_RowCommand"
                OnPageIndexChanging="gvTelefones_PageIndexChanging">
                <Columns>
                    <asp:TemplateField HeaderText="DDI">  <ItemTemplate>      <%# DataBinder.Eval(Container.DataItem, "DDI") %>  </ItemTemplate></asp:TemplateField><asp:TemplateField HeaderText="DDD">  <ItemTemplate>      <%# DataBinder.Eval(Container.DataItem, "DDD") %>  </ItemTemplate></asp:TemplateField><asp:TemplateField HeaderText="Numero">  <ItemTemplate>      <%# DataBinder.Eval(Container.DataItem, "Numero") %>  </ItemTemplate></asp:TemplateField><asp:TemplateField HeaderText="IDTipoTelefone">  <ItemTemplate>      <%# DataBinder.Eval(Container.DataItem, "IDTipoTelefone") %>  </ItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" ID="lnkAlterar" CommandName="Editar" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "IDTelefone") %>'>Editar</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" ID="lnkExcluir" CommandName="Excluir" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "IDTelefone") %>' OnClientClick="if(confirm('Confirma exclusao?')){ return true; }else{ return false; }" >Excluir</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </fieldset>
    </div>
    </form>
</body>
</html>
