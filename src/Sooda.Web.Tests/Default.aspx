<%@ Page Language="C#" AutoEventWireup="true" Inherits="Sooda.Web.Tests._Default" %>
<%@ Import Namespace="Sooda.UnitTests.BaseObjects" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Button ID="Button1" runat="server" Text="Button" OnClick="Button1_Click" />
        <asp:gridview ID="Gridview1" runat="server" Height="163px" Width="214px" AutoGenerateColumns="false">
            <Columns>
                
                <asp:TemplateField>
                    <HeaderTemplate>ID</HeaderTemplate>
                    <ItemTemplate><%# Eval("ContactId") %></ItemTemplate>
                </asp:TemplateField>
                
                <asp:TemplateField>
                    <HeaderTemplate>
                        Name
                    </HeaderTemplate>
                    <ItemTemplate><%# Eval("Name") %></ItemTemplate>
                </asp:TemplateField>
                
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
