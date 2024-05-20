<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Generer_ET_FAD.aspx.cs" Inherits="Generer_ET_FAD" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>
    <link href="source/css/bootstrap.css" rel="stylesheet" />
    <script src="source/js/bootstrap.js">
    </script>
     <style>
    .chkboxlist td 
{
    font-size:large;
}
    #Button1
    {
        text-align:center;
    }
      .btn-btn {
              background: linear-gradient(to right, #0575e6, #021b79); /* W3C, IE 10+/ Edge, Firefox 16+, Chrome 26+, Opera 12+, Safari 7+ */
              color:white;
                background: linear-gradient(to right, #1488cc, #2b32b2); /* W3C, IE 10+/ Edge, Firefox 16+, Chrome 26+, Opera 12+, Safari 7+ */
                border:none;
                background:#04AA6D;
        }

</style>
</head>
<body class="" style="background-color: #f0f0f0">

    <form id="form1" runat="server">
        <nav class="bg-white shadow-sm" >
        <div class="container ">
            <div class="row justify-content-center ">
                <div class="col-12 nav justify-content-center p-3">
                    <a href="Default.aspx" style="color: #1d1c1c !important; text-decoration: none">
                        <h3>Emploi Du Temps Automatique </h3>
                    </a>
                </div>
            </div>
        </div>
    </nav>
        <div class="container-fluid mt-5 justify-content-center" style="text-align: center">
            <div class="row justify-content-center mt-5 ">

            <div class="col-8 mt-5  mb-1 bg-white p-5 rounded shadow border" style="width: 80% ">
               


                    <div class="row ">
                        <div class="col-12 mt-3 exercice-sec pt-2 m-1" style="display: inline">
                            <div class="row">
                                <div class="col">
                                    <asp:Label ID="Label2" runat="server" Visible="False" ForeColor="Red"></asp:Label>
                                </div>
                            </div>
                            <div class="row mt-5">

                                <div class="col-6">


                                    <asp:Label ID="Label1" runat="server" Text="Label">MOIS: </asp:Label>


                                    <asp:DropDownList ID="cbx_Mois" runat="server" Font-Bold="True" Height="40px" Width="75%">
                                        <asp:ListItem Selected="True">Janvier</asp:ListItem>
                                        <asp:ListItem>fevrier</asp:ListItem>
                                        <asp:ListItem>Mars</asp:ListItem>
                                        <asp:ListItem>Avril</asp:ListItem>
                                        <asp:ListItem>Mai</asp:ListItem>
                                        <asp:ListItem>Juin</asp:ListItem>
                                        <asp:ListItem>Juillet</asp:ListItem>
                                        <asp:ListItem>Aout</asp:ListItem>
                                        <asp:ListItem>Septombre</asp:ListItem>
                                        <asp:ListItem>Octobre</asp:ListItem>
                                        <asp:ListItem>Novembre</asp:ListItem>
                                        <asp:ListItem>Décembre</asp:ListItem>


                                    </asp:DropDownList>
                                </div>




                                <div class="col-3">
                                    <asp:Button ID="Button1" runat="server" Text="Afficher planning" CssClass="btn btn-btn" OnClick="Button1_Click1" />
                                </div>

                                <div class="col-3">
                                    <asp:Button ID="Button3" runat="server" Text="Générer l'emploi du temps" Visible="false" CssClass="btn btn-btn" OnClick="Button3_Click" />
                                </div>
                            </div>

                            <div class="row mt-5">
                                <div class="col-12">
                                    <asp:GridView ID="dgv_PlanningOrigine" runat="server" CssClass="table" AllowPaging="false"
                                        CellPadding="4" ForeColor="#333333" GridLines="None">
                                        <AlternatingRowStyle BackColor="White" />
                                        <EditRowStyle BackColor="#2461BF" />
                                        <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                                        <HeaderStyle BackColor="#04AA6D" Font-Bold="True" ForeColor="White" />
                                        <PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Center" />
                                        <RowStyle BackColor="#EFF3FB" />
                                        <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
                                        <SortedAscendingCellStyle BackColor="#F5F7FB" />
                                        <SortedAscendingHeaderStyle BackColor="#6D95E1" />
                                        <SortedDescendingCellStyle BackColor="#E9EBEF" />
                                        <SortedDescendingHeaderStyle BackColor="#4870BE" />
                                    </asp:GridView>

                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </form>
</body>
</html>