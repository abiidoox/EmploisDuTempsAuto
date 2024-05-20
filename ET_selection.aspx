<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ET_selection.aspx.cs" Inherits="ET_selection" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head2" runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Planning</title>
    <link href="source/css/bootstrap.css" rel="stylesheet" />
    <script src="source/js/bootstrap.js">
    </script>
     <style>
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


    <div class="container" style="max-width: 1200px !important;">
        <div class="row justify-content-center mt-4">
            <div class="col-8 mt-5  mb-1 bg-white p-5 rounded shadow border" style="width: 80% ">
                <h5 style="font-weight: bold; font-size: 23px;">Choisir le mode :</h5>
                <label class="" style="width: 60px; height: 5px; position: absolute; margin-top: -4px;background-color:#04AA6D"></label>

                <div class="col-12 mt-3 exercice-sec pt-2 m-1" style="display: inline">
                    <div class="row mt-4">
                        <div class="col-sm-6" style="">
                            <div class="card" >
                                <div class="card-body">
                                    <h5 class="card-title">Présentiel</h5>
                                    <p class="card-text">Générer l'emploie du Temps (P)</p>
                                    <a href="Generer_ET_P.aspx" class="btn btn-btn">Entrer →</a>
                                </div>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <div class="card" >
                                <div class="card-body">
                                    <h5 class="card-title">Formation à distance</h5>
                                    <p class="card-text">Générer l'emploie du Temps (FAD)</p>
                                    <a href="Generer_ET_FAD.aspx" class="btn btn-btn">Entrer →</a>
                                </div>
                            </div>
                        </div>
                       
                    </div>
                </div>
            </div>


        </div>

    </div>
    
    




</body></html>