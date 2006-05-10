<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Sooda.Web.Tests._Default" %>
<%@ Import Namespace="Sooda.UnitTests.BaseObjects" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Button ID="Button1" runat="server" Text="Change something" OnClick="Button1_Click" />
        <asp:Button ID="Button2" runat="server" Text="Commit" OnClick="Button2_Click" />
        <asp:Button ID="Button3" runat="server" OnClick="Button3_Click" Text="Button" />
        <asp:gridview ID="Gridview1" runat="server" Height="195px" Width="251px" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField HeaderText="Id" DataField="ContactId" />
                <asp:BoundField HeaderText="Name" DataField="Name" />
                
                <asp:TemplateField>
                    <HeaderTemplate>
                        PrimaryGroup.Name
                    </HeaderTemplate>
                    <ItemTemplate><%# Eval("PrimaryGroup.Name") %></ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:gridview>
        <asp:Label ID="serialized" runat="server"></asp:Label>
    </form>
</body>
</html>
