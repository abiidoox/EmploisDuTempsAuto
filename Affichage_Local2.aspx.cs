using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

public partial class Affichage_Local2 : System.Web.UI.Page
{
    private SqlConnection cn = new SqlConnection("Data Source=DESKTOP-0HNH2FK\\DESKTOP;Initial Catalog=db_gestion_achats3;Integrated Security=True");
    private SqlCommand cmd;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        { DropDownListmois0.SelectedValue = DateTime.Now.Month.ToString();

        }
        else
            MAJCalendrier();
    }
    protected void MAJCalendrier()
    {
        try
        {

            //TitreCalendrierLBL.InnerHtml = "E.T. " + DropDownList1.SelectedItem.Text + " du mois " + DropDownListmois0.SelectedItem.Text;

            int groupe =int.Parse(DropDownList1.SelectedValue.ToString());
            string af = "2021/2022";

            // Dim af As String = Session("anneeformation").ToString()
            string mois = DropDownListmois0.SelectedValue.ToString();
            if ((!(string.IsNullOrEmpty(mois)) ))
            {
                LUNDI1.Text = getGroupeLocalPargroupe(groupe, af, mois, "Lundi", "08:30", "11:00");
                LUNDI2.Text = getGroupeLocalPargroupe(groupe, af, mois, "Lundi", "11:00", "13:30");
                LUNDI3.Text = getGroupeLocalPargroupe(groupe, af, mois, "Lundi", "13:30", "16:00");
                LUNDI4.Text = getGroupeLocalPargroupe(groupe, af, mois, "Lundi", "16:00", "18:30");


                MARDI1.Text = getGroupeLocalPargroupe(groupe, af, mois, "Mardi", "08:30", "11:00");
                MARDI2.Text = getGroupeLocalPargroupe(groupe, af, mois, "Mardi", "11:00", "13:30");
                MARDI3.Text = getGroupeLocalPargroupe(groupe, af, mois, "Mardi", "13:30", "16:00");
                MARDI4.Text = getGroupeLocalPargroupe(groupe, af, mois, "Mardi", "16:00", "18:30");

                MERCREDI1.Text = getGroupeLocalPargroupe(groupe, af, mois, "Mercredi", "08:30", "11:00");
                MERCREDI2.Text = getGroupeLocalPargroupe(groupe, af, mois, "Mercredi", "11:00", "13:30");
                MERCREDI3.Text = getGroupeLocalPargroupe(groupe, af, mois, "Mercredi", "13:30", "16:00");
                MERCREDI4.Text = getGroupeLocalPargroupe(groupe, af, mois, "Mercredi", "16:00", "18:30");

                JEUDI1.Text = getGroupeLocalPargroupe(groupe, af, mois, "Jeudi", "08:30", "11:00");
                JEUDI2.Text = getGroupeLocalPargroupe(groupe, af, mois, "Jeudi", "11:00", "13:30");
                JEUDI3.Text = getGroupeLocalPargroupe(groupe, af, mois, "Jeudi", "13:30", "16:00");
                JEUDI4.Text = getGroupeLocalPargroupe(groupe, af, mois, "Jeudi", "16:00", "18:30");

                VENDREDI1.Text = getGroupeLocalPargroupe(groupe, af, mois, "Vendredi", "08:30", "10:30");
                VENDREDI2.Text = getGroupeLocalPargroupe(groupe, af, mois, "Vendredi", "10:30", "12:30");
                VENDREDI3.Text = getGroupeLocalPargroupe(groupe, af, mois, "Vendredi", "14:30", "16:30");
                VENDREDI4.Text = getGroupeLocalPargroupe(groupe, af, mois, "Vendredi", "16:30", "18:30");

                SAMEDI1.Text = getGroupeLocalPargroupe(groupe, af, mois, "Samedi", "08:30", "11:00");
                SAMEDI2.Text = getGroupeLocalPargroupe(groupe, af, mois, "Samedi", "11:00", "13:30");
                SAMEDI3.Text = getGroupeLocalPargroupe(groupe, af, mois, "Samedi", "13:30", "16:00");
                SAMEDI4.Text = getGroupeLocalPargroupe(groupe, af, mois, "Samedi", "16:00", "18:30");
            }
        }
        catch (Exception ex)
        {
            this.Label1.Visible = true;
            this.Label1.Text = ex.Message;
        }
    }
    protected string getGroupeLocalPargroupe(int groupe, string af, string mois, string jour, string heurDebut, string heurFin)
    {
        if ((cn.State == System.Data.ConnectionState.Closed))
            cn.Open();

        cmd = new SqlCommand("[getGroupeLocalPargroupe]", cn);
        cmd.Parameters.Clear();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@groupe", groupe);
        cmd.Parameters.AddWithValue("@mois", mois);
        cmd.Parameters.AddWithValue("@af", af);

        cmd.Parameters.AddWithValue("@jour", jour);
        cmd.Parameters.AddWithValue("@heurDebut", heurDebut);
        cmd.Parameters.AddWithValue("@heurFin", heurFin);
        SqlParameter p2 = new SqlParameter("@mass", SqlDbType.VarChar);
        p2.Direction = ParameterDirection.ReturnValue;
        cmd.Parameters.Add(p2);

        cmd.ExecuteNonQuery();
        return p2.Value.ToString();
    }
    protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
    {
        //DropDownList1.Items.Add("choisir un groupe");
    }
    protected void DropDownList1_DataBound(object sender, EventArgs e)
    {
        DropDownList1.Items.Insert(0, new ListItem("-- Veuillez Sélectionner un groupe --", "&nbsp;"));

    }

   
}