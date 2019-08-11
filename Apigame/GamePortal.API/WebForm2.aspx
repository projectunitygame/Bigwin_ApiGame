<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm2.aspx.cs" Inherits="GamePortal.API.WebForm2" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:TextBox ID="TextBox1" runat="server" Height="46px" Width="1078px"></asp:TextBox>
            <br />
            <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Change" />
&nbsp;<asp:Button ID="Button3" runat="server" OnClick="Button3_Click" Text="Change2" />
            <asp:Button ID="Button4" runat="server" Text="Encrypt" OnClick="Button4_Click" />
&nbsp;<asp:Button ID="Button5" runat="server" OnClick="Button5_Click" Text="Encrypt2" />
&nbsp;<asp:Button ID="Button2" runat="server" OnClick="Button2_Click" Text="Clear" />
            <br />
            <br />
            <asp:Label ID="lblDecrypt" runat="server" Text="DecryptData:"></asp:Label>
            <asp:Label ID="lblMsg1" runat="server"></asp:Label>
            <br />
            <asp:Label ID="lblEncrypt" runat="server" Text="EncryptFix:"></asp:Label>
            <asp:TextBox ID="TextBox2" runat="server" Width="926px"></asp:TextBox>
            <br />
            Encrypt Data:<asp:TextBox ID="TextBox3" runat="server" Width="894px"></asp:TextBox>
        </div>
    </form>
</body>
</html>
