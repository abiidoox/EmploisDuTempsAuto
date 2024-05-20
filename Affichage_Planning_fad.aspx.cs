
using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

partial class Affichage_Planning_fad : System.Web.UI.Page
{
    private DataTable dt_PlanningTemp;
    private DataTable dt_PlanningOrigine;

    private SqlConnection connection;
    private SqlCommand command;
    private SqlDataReader reader;


    



   

    protected void Page_Load(object sender, EventArgs e)
    {
        connection = new SqlConnection("Data Source=DESKTOP-0HNH2FK\\DESKTOP;Initial Catalog=db_gestion_achats3;Integrated Security=True");
        command = new SqlCommand();
        command.Connection = connection;

        if ((Page.IsPostBack == false))
            cbx_Mois.SelectedIndex = DateTime.Now.Month - 1;
    }

    protected void cbx_Mois_SelectedIndexChanged(object sender, EventArgs e)
    {
    }
    protected void Button1_Click1(object sender, EventArgs e)
    {
        try
        {
            // Dim dt_PlanningTemp As DataTable

            Session["anneeformation"] = "2021/2022";
            Session["etablissement"] = "p200";
            connection.Open();

            string mois = (cbx_Mois.SelectedIndex + 1).ToString();
            string etablissement = Session["etablissement"].ToString();
            string annee_formation = Session["Anneeformation"].ToString();
            command.CommandText = string.Format("SELECT p.[Formateur] as 'Matricule',f.Nom as 'Formateur',IsNull(Upper(Type_Emploi), 'P') as 'TypeEmploi',( select idGroupe from Groupes g where p.Groupe = g.nom and p.Annee = g.Annee and p.CodeFiliere = g.codefiliere and p.AnneeFormation = g.AnneeFormation and p.Etablissement = g.etablissement) as 'Groupe', p.CodeFiliere as 'Filiere', p.Groupe as 'Nom' ,p.Annee as 'Annee', mois{0} as 'MH' FROM [db_gestion_achats3].[dbo].[PlanningAnnueleFormateur] p  inner join Formateurs f on p.formateur = f.matricule WHERE p.AnneeFormation='{1}' and mois{0}>0 and p.Etablissement='{2}'and Upper(p.Formateur) not like 'VAC %'  and typeFormation='fad'", mois, annee_formation, etablissement);
            reader = command.ExecuteReader();
            dt_PlanningOrigine = new DataTable();
            dt_PlanningOrigine.Load(reader);
            reader.Close();
            connection.Close();

            double somme = 0;

            for (int i = 0; i <= dt_PlanningOrigine.Rows.Count - 1; i++)
                somme += double.Parse(dt_PlanningOrigine.Rows[i]["MH"].ToString());

            dgv_PlanningOrigine.DataSource = dt_PlanningOrigine;
            dgv_PlanningOrigine.DataBind();

            if (dgv_PlanningOrigine.Rows.Count > 0)
                Button3.Visible = true;
            Session["dt_PlanningOrigine"] = dt_PlanningOrigine;
            Session["dt_PlanningTemp"] = dt_PlanningTemp;
        }
        catch(Exception ex)
        {
            Label2.Visible = true;
            Label2.Text = ex.Message;
        }
    }
    protected void Button3_Click(object sender, EventArgs e)
    {
        try{
        Session["anneeformation"] = "2021/2022";
        Session["etablissement"] = "p200";
        dt_PlanningOrigine = (DataTable)Session["dt_PlanningOrigine"];
        connection.Open();
        command.CommandText = string.Format("delete AlgoPlanning  WHERE AnneeFormation='{0}' and Mois = {1} and Etablissement='{2}' AND type_formation='FAD' ", Session["anneeformation"].ToString(), cbx_Mois.SelectedIndex + 1, Session["etablissement"].ToString());
        command.ExecuteNonQuery();

        connection.Close();

        int local;

        // dt_locauxLP = dt_PlanningOrigine.Copy()

        command.CommandText = string.Format("select id from Local WHERE Etablissement='{0}' and type='fad'", Session["etablissement"].ToString());
        connection.Open();
        local = int.Parse(command.ExecuteScalar().ToString());


        string insert_requet = string.Format("insert into AlgoPlanning values ( '{0}', '{1}', {2},", Session["etablissement"].ToString(), Page.Session["anneeformation"].ToString(), cbx_Mois.SelectedIndex + 1);
        // dgv_PlanningAlgo.AllowUserToAddRows = False
        string localtype = "FAD";
        for (int i = 0; i <= dgv_PlanningOrigine.Rows.Count - 1; i++)
        {
            string formateur = dgv_PlanningOrigine.Rows[i].Cells[0].Text.ToString();
            string groupe = dgv_PlanningOrigine.Rows[i].Cells[3].Text.ToString();
            // Dim local As String = dgv_PlanningOrigine.Rows(i).Cells(7).Text.ToString()
            string MH = dgv_PlanningOrigine.Rows[i].Cells[7].Text.ToString();
            if (MH.Contains(","))
                MH = MH.Replace(",", ".");
            string insertion = insert_requet + string.Format(" '{0}' , {1} , {2} , {3},'{4}' )", formateur, groupe, local, MH, localtype);
            command.CommandText = insertion;
            command.ExecuteNonQuery();
            dt_PlanningOrigine.Rows[i].Delete();
        }
        
        dgv_PlanningOrigine.DataSource = dt_PlanningOrigine;
        dgv_PlanningOrigine.DataBind();
        Label2.Visible = true;
        Label2.Text = "Tous les données sont enregistrés";
        Label2.ForeColor = System.Drawing.Color.Green;
        connection.Close();
          }
        catch(Exception ex)
        {
            Label2.Visible = true;
            Label2.Text = ex.Message;
        }
    }
}
