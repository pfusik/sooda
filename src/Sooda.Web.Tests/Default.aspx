<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Sooda.Web.Tests._Default" %>
<%@ Import Namespace="Sooda.UnitTests.BaseObjects" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
<style type="text/css">
body
{
    font-family: Tahoma;
}
</style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Button ID="Button1" runat="server" Text="Change something" OnClick="Button1_Click" />
        <asp:Button ID="Button2" runat="server" Text="Commit" OnClick="Button2_Click" />
        <asp:Button ID="Button3" runat="server" OnClick="Button3_Click" Text="Button" />
        <asp:gridview Font-Size="13px" ID="Gridview1" runat="server" Height="195px" Width="251px" AutoGenerateColumns="False" CellPadding="4" ForeColor="#333333" AllowPaging="True" DataSourceID="contacts">
            <Columns>
                <asp:CommandField ShowSelectButton="True" />
                <asp:BoundField DataField="ContactId" HeaderText="ContactId" SortExpression="ContactId" />
                <asp:BoundField DataField="PersistentValue" HeaderText="PersistentValue" SortExpression="PersistentValue" />
                <asp:CheckBoxField DataField="DisableTriggers" HeaderText="DisableTriggers" SortExpression="DisableTriggers" />
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:CheckBoxField DataField="Active" HeaderText="Active" SortExpression="Active" />
            </Columns>
            <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
            <RowStyle BackColor="#E3EAEB" />
            <EditRowStyle BackColor="#7C6F57" />
            <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
            <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
            <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
            <AlternatingRowStyle BackColor="White" />
        </asp:gridview>
        <asp:ObjectDataSource ID="contacts" runat="server" SelectMethod="GetAllObjects" TypeName="Sooda.UnitTests.BaseObjects.Contact">
        </asp:ObjectDataSource>
        <asp:Label ID="serialized" runat="server"></asp:Label>
    </form>
</body>
</html>
