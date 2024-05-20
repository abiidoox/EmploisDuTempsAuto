<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Affichage_Local1.aspx.cs" Inherits="Affichage_Local1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Affichage Emploie du Temps</title>
    <link href="source/css/bootstrap.css" rel="stylesheet" />
    <script src="source/js/bootstrap.js"></script>
    <style>
       
    </style>
</head>
<body style="background-color: #f0f0f0">
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

        <div class="container mt-5" style="max-width: 1200px !important; text-align: center; justify-content: center">
            <div class="row justify-content-center mt-5 ">

                           <div class="col-8 mt-5  mb-1 bg-white p-5 rounded shadow border" style="width: 80% ">




                    <div class="col-12 mt-3 exercice-sec pt-2 m-1" style="display: inline">
                        <div class="row">
                            <div class="col">
                                <asp:Label ID="Label1" runat="server"
                                    Style="font-weight: 700; color: #FF3300" Text="Label" Visible="False"></asp:Label>
                                <br />
                                <span class="col-form-label h5" style="">Mois:</span> &nbsp;
    <asp:DropDownList ID="DropDownListmois0" runat="server" AutoPostBack="True"
        Height="40px" Width="250px" Font-Bold="True">
        <asp:ListItem Value="0">-- Sélectionner Un mois--</asp:ListItem>
        <asp:ListItem Value="9">Septembre</asp:ListItem>
        <asp:ListItem Value="10">Octobre</asp:ListItem>
        <asp:ListItem Value="11">Novembre</asp:ListItem>
        <asp:ListItem Value="12">Décembre</asp:ListItem>
        <asp:ListItem Value="1">Janvier</asp:ListItem>
        <asp:ListItem Value="2">Février</asp:ListItem>
        <asp:ListItem Value="3">Mars</asp:ListItem>
        <asp:ListItem Value="4">Avril</asp:ListItem>
        <asp:ListItem Value="5">Mai</asp:ListItem>
        <asp:ListItem Value="6">Juin</asp:ListItem>
        <asp:ListItem Value="7">Juillet</asp:ListItem>
        <asp:ListItem Value="8">Aout</asp:ListItem>
    </asp:DropDownList>
                                &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="col-form-label h5" style="">Formateur :</span>
                                <asp:DropDownList ID="DropDownList1" runat="server" AutoPostBack="True"
                                    DataSourceID="SqlDataSource2" DataTextField="Nom" DataValueField="Matricule"
                                    Height="47px" Width="357px" Font-Bold="True">
                                </asp:DropDownList>

                                <asp:SqlDataSource ID="SqlDataSource2" runat="server"
                                    ConnectionString="<%$ ConnectionStrings:db_gestion_achats3ConnectionString %>"
                                    SelectCommand="SELECT DISTINCT Formateurs.Nom, Formateurs.Matricule FROM Groupes INNER JOIN ModuleFormateurGroupe INNER JOIN Formateurs ON ModuleFormateurGroupe.Formateur = Formateurs.Matricule ON Groupes.idGroupe = ModuleFormateurGroupe.Groupe WHERE (Formateurs.statut &lt;&gt; 'PA') AND (Formateurs.etat = 1) AND (Formateurs.Matricule NOT LIKE '%tuteur%') AND (Groupes.AnneeFormation = '2021/2022') AND (Groupes.etablissement = 'p200') ORDER BY Formateurs.Nom"></asp:SqlDataSource>


                                <br />
                                <br />
                                <div id="ZoneCalendrier">
                                    <style>
                                        #customers {
                                            font-family: Arial, Helvetica, sans-serif;
                                            border-collapse: collapse;
                                        }

                                            #customers td, #customers th {
                                                border: 1px solid #ddd;
                                                padding: 8px;
                                                width: 150px;
                                            }

                                            #customers tr:nth-child(even) {
                                                background-color: #f2f2f2;
                                            }

                                            #customers tr:hover {
                                                background-color: #ddd;
                                            }

                                            #customers th {
                                                padding-top: 12px;
                                                padding-bottom: 12px;
                                                text-align: left;
                                                background-color: #04AA6D;
                                                color: white;
                                            }
                                    </style>
                                    <table id="customers" class="table">
                                        <tr>
                                            <th></th>
                                            <th style="">Séance 1</th>
                                            <th style="">Séance 2</th>
                                            <th>Séance 3</th>
                                            <th>Séance 4</th>


                                        </tr>
                                        <tr>
                                            <td>LUNDI</td>
                                            <td>
                                                <asp:Label ID="LUNDI1" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="LUNDI2" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="LUNDI3" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="LUNDI4" runat="server" Text=""></asp:Label></td>

                                        </tr>
                                        <tr>
                                            <td>MARDI</td>
                                            <td>
                                                <asp:Label ID="MARDI1" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="MARDI2" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="MARDI3" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="MARDI4" runat="server" Text=""></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td>MERCREDI</td>
                                            <td>
                                                <asp:Label ID="MERCREDI1" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="MERCREDI2" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="MERCREDI3" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="MERCREDI4" runat="server" Text=""></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td>JEUDI</td>
                                            <td>
                                                <asp:Label ID="JEUDI1" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="JEUDI2" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="JEUDI3" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="JEUDI4" runat="server" Text=""></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td>VENDREDI</td>
                                            <td>
                                                <asp:Label ID="VENDREDI1" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="VENDREDI2" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="VENDREDI3" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="VENDREDI4" runat="server" Text=""></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td>SAMEDI</td>
                                            <td>
                                                <asp:Label ID="SAMEDI1" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="SAMEDI2" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="SAMEDI3" runat="server" Text=""></asp:Label></td>
                                            <td>
                                                <asp:Label ID="SAMEDI4" runat="server" Text=""></asp:Label></td>
                                        </tr>

                                    </table>
                                </div>
                                <script type="text/javascript">
                                    function ImprimerCalendrier() {
                                        var divContents = document.getElementById("ZoneCalendrier").innerHTML;
                                        var printWindow = window.open('', '', 'height=600,width=800');
                                        printWindow.document.write('<html><head>');
                                        printWindow.document.write('</head><body >');
                                        printWindow.document.write(divContents);
                                        printWindow.document.write('</body></html>');
                                        printWindow.document.close();
                                        printWindow.print();
                                    }
                                </script>
                                <input type="button" onclick="ImprimerCalendrier()" class="btn" style="background-color: #04AA6D; color: white" value="imprimer" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
