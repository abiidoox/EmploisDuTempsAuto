using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

public partial class Affichage_planning_P : System.Web.UI.Page
{
    private DataTable dt_PlanningTemp;
    private DataTable dt_PlanningOrigine;

    private SqlConnection connection;
    private SqlCommand command;
    private SqlDataReader reader;

    public void TraitementVacataire(int Type, DataView dv_Planning, string[] priorite, DataTable dt_locauxEtablissement, DataTable dt_locauxLP, decimal MAX_HEURES_LOCAL, string filtreCDS)
    {
        for (int vac_type = 1; vac_type <= 2; vac_type++)
        {
            string filtre = "";

            if (vac_type == 1)
                filtre = "TypeEmploi = 'EXTERNE2'" + filtreCDS;
            else
                filtre = "TypeEmploi = 'EXTERNE1'" + filtreCDS;

            dv_Planning.RowFilter = filtre;

            if (dv_Planning.Count > 0)
            {
                for (int prio = 0; prio <= priorite.Length - 1; prio++)
                {
                    for (int i_local = 0; i_local <= dt_locauxEtablissement.Rows.Count - 1; i_local++)
                    {
                        dv_Planning.RowFilter = "";

                        if (Type == 1)
                            dv_Planning.RowFilter = filtre + " AND LP" + priorite[prio][0] + "F = LP" + priorite[prio][2] + "G and LP" + priorite[prio][0] + "F= " + dt_locauxEtablissement.Rows[i_local][0].ToString();
                        else
                            dv_Planning.RowFilter = filtre + " AND " + dt_PlanningOrigine.Columns[Int32.Parse(priorite[prio])].ColumnName + "= " + dt_locauxEtablissement.Rows[i_local][0].ToString();

                        if (dv_Planning.Count > 0)
                        {
                            dv_Planning.Sort = "MH desc";
                            dt_locauxLP.Rows.Clear();
                            RemplirParUneCopie(dt_locauxLP, dv_Planning);
                            decimal somme_MH_deja_affecter = 0M;
                            dt_PlanningTemp.DefaultView.RowFilter = "Local =" + dt_locauxEtablissement.Rows[i_local][0].ToString() + filtreCDS;
                            somme_MH_deja_affecter = CalculerMhDeja(dt_PlanningTemp.DefaultView);
                            decimal somme_MH_deja_Vac = 0M;

                            if (vac_type == 1)
                                dt_PlanningTemp.DefaultView.RowFilter = dt_PlanningTemp.DefaultView.RowFilter + " and TypeEmploi = 'EXTERNE2' ";
                            else
                                dt_PlanningTemp.DefaultView.RowFilter = dt_PlanningTemp.DefaultView.RowFilter + " and TypeEmploi = 'EXTERNE1' ";

                            somme_MH_deja_Vac = CalculerMhDeja(dt_PlanningTemp.DefaultView);
                            dt_PlanningTemp.DefaultView.RowFilter = "";

                            if (MAX_HEURES_LOCAL - somme_MH_deja_affecter > 0 && 10 - somme_MH_deja_Vac > 0)
                            {
                                decimal somme_MH_nouveau = 0M;
                                somme_MH_nouveau = CalculerNouveauMh(dt_locauxLP);

                                if ((somme_MH_nouveau <= MAX_HEURES_LOCAL - somme_MH_deja_affecter) && somme_MH_nouveau > 0)
                                {
                                    if ((somme_MH_nouveau <= 10 - somme_MH_deja_Vac))
                                    {
                                        AjouterAuPlanningTemp(dt_locauxLP, dt_locauxEtablissement.Rows[i_local][0].ToString());
                                        DeleteView(dv_Planning);
                                    }
                                    else if (somme_MH_nouveau > 10 - somme_MH_deja_Vac)
                                    {
                                        DataTable dt_RemplirLocal = new DataTable();
                                        dt_RemplirLocal = dt_locauxLP.Copy();

                                        for (int i = 0; i <= dt_RemplirLocal.Rows.Count - 1; i++)
                                            dt_RemplirLocal.Rows[i]["MH"] = 0;

                                        ConditionMAX_HEURE_lOCAL(10, somme_MH_deja_Vac, dt_locauxLP, dt_RemplirLocal);
                                        AjouterAuPlanningTemp(dt_RemplirLocal, dt_locauxEtablissement.Rows[i_local][0].ToString());
                                        DeleteView(dv_Planning);
                                        AjouterAuPlanningOrigine(dt_locauxLP);
                                    }
                                }
                                else if (somme_MH_nouveau > MAX_HEURES_LOCAL - somme_MH_deja_affecter)
                                {
                                    DataTable dt_RemplirLocal = new DataTable();
                                    dt_RemplirLocal = dt_locauxLP.Copy();

                                    for (int i = 0; i <= dt_RemplirLocal.Rows.Count - 1; i++)
                                        dt_RemplirLocal.Rows[i]["MH"] = 0;

                                    ConditionVacataire(MAX_HEURES_LOCAL, somme_MH_deja_affecter, somme_MH_deja_Vac, dt_locauxLP, dt_RemplirLocal);
                                    AjouterAuPlanningTemp(dt_RemplirLocal, dt_locauxEtablissement.Rows[i_local][0].ToString());
                                    DeleteView(dv_Planning);
                                    AjouterAuPlanningOrigine(dt_locauxLP);
                                }
                            }
                        }
                    }
                }
            }
        }

        dv_Planning.RowFilter = "";
    }
    public void TraitementPermanent(int Type, DataView dv_Planning, string[] priorite, DataTable dt_locauxEtablissement, DataTable dt_locauxLP, decimal MAX_HEURES_LOCAL, string filtreCDS)
    {
        string filtreType = "TypeEmploi <>  'EXTERNE1' and TypeEmploi not like 'EXTERNE2'" + filtreCDS;
        dv_Planning.RowFilter = filtreType;

        if (dv_Planning.Count > 0)
        {
            for (int prio = 0; prio <= priorite.Length - 1; prio++)
            {
                string filtrePriorite = "";

                if (Type == 1)
                    filtrePriorite = "LP" + priorite[prio][0] + "F = LP" + priorite[prio][2] + "G";
                else
                    filtrePriorite = " 1=1";

                dv_Planning.RowFilter = filtreType + " and " + filtrePriorite;

                if (dv_Planning.Count > 0)
                {
                    for (int i_local = 0; i_local <= dt_locauxEtablissement.Rows.Count - 1; i_local++)
                    {
                        string filtreLocal = "";

                        if (Type == 1)
                            filtreLocal = "LP" + priorite[prio][0] + "F= " + dt_locauxEtablissement.Rows[i_local][0].ToString();
                        else
                            filtreLocal = dt_PlanningOrigine.Columns[Int32.Parse(priorite[prio])].ColumnName + " = " + dt_locauxEtablissement.Rows[i_local][0].ToString();

                        dv_Planning.RowFilter = filtreType + " and " + filtrePriorite + " and " + filtreLocal;

                        if (dv_Planning.Count > 0)
                        {
                            dv_Planning.Sort = "MH desc";
                            dt_locauxLP.Rows.Clear();
                            RemplirParUneCopie(dt_locauxLP, dv_Planning);
                            decimal somme_MH_deja_affecter = 0M;
                            dt_PlanningTemp.DefaultView.RowFilter = "Local =" + dt_locauxEtablissement.Rows[i_local][0].ToString() + " " + filtreCDS;
                            somme_MH_deja_affecter = CalculerMhDeja(dt_PlanningTemp.DefaultView);
                            dt_PlanningTemp.DefaultView.RowFilter = "";

                            if (MAX_HEURES_LOCAL - somme_MH_deja_affecter > 0)
                            {
                                decimal somme_MH_nouveau = 0M;
                                somme_MH_nouveau = CalculerNouveauMh(dt_locauxLP);

                                if ((somme_MH_nouveau <= MAX_HEURES_LOCAL - somme_MH_deja_affecter) && somme_MH_nouveau > 0)
                                {
                                    AjouterAuPlanningTemp(dt_locauxLP, dt_locauxEtablissement.Rows[i_local][0].ToString());
                                    dv_Planning.Sort = "";
                                    DeleteView(dv_Planning);
                                }
                                else if (somme_MH_nouveau > MAX_HEURES_LOCAL - somme_MH_deja_affecter)
                                {
                                    DataTable dt_RemplirLocal = new DataTable();
                                    dt_RemplirLocal = dt_locauxLP.Copy();

                                    for (int i = 0; i <= dt_RemplirLocal.Rows.Count - 1; i++)
                                        dt_RemplirLocal.Rows[i]["MH"] = 0;

                                    ConditionMAX_HEURE_lOCAL(MAX_HEURES_LOCAL, somme_MH_deja_affecter, dt_locauxLP, dt_RemplirLocal);
                                    AjouterAuPlanningTemp(dt_RemplirLocal, dt_locauxEtablissement.Rows[i_local][0].ToString());
                                    DeleteView(dv_Planning);
                                    AjouterAuPlanningOrigine(dt_locauxLP);
                                }
                            }
                        }
                    }
                }
            }
        }

        dv_Planning.RowFilter = "";
    }
    public void ConditionVacataire(decimal MAX_HEURES_LOCAL, decimal somme_MH_deja_affecter, decimal somme_MH_deja_Vac, DataTable dt_locauxLP, DataTable dt_RemplirLocal)
    {
        decimal MH_Affecter = 0M;

        while (0 < MAX_HEURES_LOCAL - somme_MH_deja_affecter - MH_Affecter && 10 - somme_MH_deja_Vac - MH_Affecter > 0)
        {
            bool pas_de_change = true;
            int i = 0;

            while (i < dt_locauxLP.Rows.Count && 10 - somme_MH_deja_Vac - MH_Affecter > 0 && 0 < MAX_HEURES_LOCAL - somme_MH_deja_affecter - MH_Affecter)
            {
                if (decimal.Parse(dt_locauxLP.Rows[i]["MH"].ToString()) >= decimal.Parse(dt_RemplirLocal.Rows[i]["MH"].ToString()) && decimal.Parse(dt_locauxLP.Rows[i]["MH"].ToString()) > 0)
                {
                    if ((decimal.Parse(dt_locauxLP.Rows[i]["MH"].ToString()) * 10) % 25 != 0)
                    {
                        dt_RemplirLocal.Rows[i]["MH"] = decimal.Parse(dt_RemplirLocal.Rows[i]["MH"].ToString()) + 2;
                        pas_de_change = !true;
                        dt_locauxLP.Rows[i]["MH"] = decimal.Parse(dt_locauxLP.Rows[i]["MH"].ToString()) - 2;
                    }
                    else if ((decimal.Parse(dt_RemplirLocal.Rows[i]["MH"].ToString()) * 10) % 25 == 0)
                    {
                        dt_RemplirLocal.Rows[i]["MH"] = decimal.Parse(dt_RemplirLocal.Rows[i]["MH"].ToString()) + 2.5M;
                        pas_de_change = !true;
                        dt_locauxLP.Rows[i]["MH"] = decimal.Parse(dt_locauxLP.Rows[i]["MH"].ToString()) - 2.5M;
                    }

                    MH_Affecter += 2.5M;
                }

                i += 1;
            }

            if (pas_de_change)
                break;
        }
    }

    public void AjouterAuPlanningOrigine(DataTable dt)
    {
        for (int i = 0; i <= dt.Rows.Count - 1; i++)
        {
            if (dt.Rows[i]["MH"].ToString() != "0")
                dt_PlanningOrigine.Rows.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString(), dt.Rows[i][2].ToString(), dt.Rows[i][3].ToString(), dt.Rows[i][4].ToString(), dt.Rows[i][5].ToString(), dt.Rows[i][6].ToString(), dt.Rows[i][7].ToString(), dt.Rows[i][8].ToString(), dt.Rows[i][9].ToString(), dt.Rows[i][10].ToString(), dt.Rows[i][11].ToString(), dt.Rows[i][12].ToString(), dt.Rows[i][13].ToString(), dt.Rows[i][14].ToString());
        }
    }

    public void DeleteView(DataView dv)
    {
        dv.Sort = "";

        while (0 < dv.Count)
            dv.Delete(0);
    }

    public void ConditionMAX_HEURE_lOCAL(decimal MAX_HEURES_LOCAL, decimal somme_MH_deja_affecter, DataTable dt_locauxLP, DataTable dt_RemplirLocal)
    {
        decimal MH_Affecter = 0M;

        while (MAX_HEURES_LOCAL - somme_MH_deja_affecter - MH_Affecter > 0)
        {
            bool pas_de_change = true;
            int i = 0;

            while (i < dt_locauxLP.Rows.Count && MAX_HEURES_LOCAL - somme_MH_deja_affecter - MH_Affecter > 0)
            {
                if (decimal.Parse(dt_locauxLP.Rows[i]["MH"].ToString()) >= decimal.Parse(dt_RemplirLocal.Rows[i]["MH"].ToString()) && decimal.Parse(dt_locauxLP.Rows[i]["MH"].ToString()) > 0)
                {
                    if ((decimal.Parse(dt_locauxLP.Rows[i]["MH"].ToString()) * 10) % 25 != 0)
                    {
                        dt_RemplirLocal.Rows[i]["MH"] = decimal.Parse(dt_RemplirLocal.Rows[i]["MH"].ToString()) + 2;
                        dt_locauxLP.Rows[i]["MH"] = decimal.Parse(dt_locauxLP.Rows[i]["MH"].ToString()) - 2;
                        pas_de_change = !true;
                    }
                    else if ((decimal.Parse(dt_RemplirLocal.Rows[i]["MH"].ToString()) * 10) % 25 == 0)
                    {
                        dt_RemplirLocal.Rows[i]["MH"] = decimal.Parse(dt_RemplirLocal.Rows[i]["MH"].ToString()) + 2.5M;
                        pas_de_change = !true;
                        dt_locauxLP.Rows[i]["MH"] = decimal.Parse(dt_locauxLP.Rows[i]["MH"].ToString()) - 2.5M;
                    }

                    MH_Affecter += 2.5M;
                }

                i += 1;
            }

            if (pas_de_change)
                break;
        }
    }
    public void AjouterAuPlanningTemp(DataTable dt, string Local)
    {
        for (int i = 0; i <= dt.Rows.Count - 1; i++)
        {
            if (dt.Rows[i]["MH"].ToString() != "0")
                dt_PlanningTemp.Rows.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString(), dt.Rows[i][3].ToString(), dt.Rows[i][7].ToString(), dt.Rows[i][8].ToString(), dt.Rows[i][9].ToString(), dt.Rows[i][10].ToString(), Local, dt.Rows[i][14].ToString());
        }
    }

    public decimal CalculerNouveauMh(DataTable dt)
    {
        decimal somme_MH_nouveau = 0;

        for (int i = 0; i <= dt.Rows.Count - 1; i++)
            somme_MH_nouveau += decimal.Parse(dt.Rows[i]["MH"].ToString());

        return somme_MH_nouveau;
    }

    public decimal CalculerMhDeja(DataView dv)
    {
        decimal somme_MH_deja_affecter = 0;

        for (int i = 0; i <= dv.Count - 1; i++)
        {
            decimal MH = decimal.Parse(dv[i]["MH"].ToString());
            somme_MH_deja_affecter += MH;

            if (MH % 2 == 0 && MH % 10 != 0)
            {
                if (MH <= 8)
                    somme_MH_deja_affecter += MH / 4;
                else
                    somme_MH_deja_affecter += (MH % 10) / 4;
            }
        }

        return somme_MH_deja_affecter;
    }

    public void RemplirParUneCopie(DataTable dt, DataView dv)
    {
        for (int i = 0; i <= dv.Count - 1; i++)
            dt.Rows.Add(dv[i][0].ToString(), dv[i][1].ToString(), dv[i][2].ToString(), dv[i][3].ToString(), dv[i][4].ToString(), dv[i][5].ToString(), dv[i][6].ToString(), dv[i][7].ToString(), dv[i][8].ToString(), dv[i][9].ToString(), dv[i][10].ToString(), dv[i][11].ToString(), dv[i][12].ToString(), dv[i][13].ToString(), dv[i][14].ToString());
    }




    protected void Page_Load(object sender, EventArgs e)
    {
        connection = new SqlConnection("Data Source=DESKTOP-0HNH2FK\\DESKTOP;Initial Catalog=db_gestion_achats3;Integrated Security=True");
        command = new SqlCommand();
        command.Connection = connection;
       if ((Page.IsPostBack == false))
        cbx_Mois.SelectedIndex = DateTime.Now.Month - 1;
    
    }
    protected void Button1_Click1(object sender, EventArgs e)
    {
        try
        {
        Session["anneeformation"] = "2021/2022";
        Session["etablissement"] = "p200";
        connection.Open();
        command.CommandText = string.Format("SELECT p.[Formateur] as 'Matricule',f.Nom as 'Formateur', MH_Hebdo,IsNull(Upper(Type_Emploi), 'P') as 'TypeEmploi', lpf.LP1 as LP1F, IsNull(lpf.LP2,-1) as LP2F, IsNull(lpf.LP3,-1) as LP3F,( select idGroupe from Groupes g where p.Groupe = g.nom and p.Annee = g.Annee and p.CodeFiliere = g.codefiliere and p.AnneeFormation = g.AnneeFormation and p.Etablissement = g.etablissement) as 'Groupe', p.CodeFiliere as 'Filiere', p.Groupe as 'Nom' ,p.Annee as 'Annee', lpg.Lp1 AS LP1G, IsNull( lpg.LP2,-1) as LP2G, IsNull(lpg.LP3,-1) as LP3G,[Mois{1}] as 'MH' FROM [db_gestion_achats3].[dbo].[PlanningAnnueleFormateur] p  inner join Formateurs f on p.formateur = f.matricule inner join LocalPriorite_Formateur lpf on (p.Etablissement = lpf.etablissement and p.formateur = lpf.Formateur), LocalPriorite_Groupe lpg WHERE p.AnneeFormation='{0}' and mois{1}>0 and p.Etablissement='{2}'and Upper(p.Formateur) not like 'VAC %' and typeformation='P'  and  ( select idGroupe from Groupes g where p.Groupe = g.nom and p.Annee = g.Annee and p.CodeFiliere = g.codefiliere and p.AnneeFormation = g.AnneeFormation and p.Etablissement = g.etablissement) = lpg.Groupe", Session["anneeformation"].ToString(), cbx_Mois.SelectedIndex + 1, Session["etablissement"].ToString());
        
        reader = command.ExecuteReader();
        dt_PlanningOrigine = new DataTable();
        dt_PlanningOrigine.Load(reader);
        reader.Close();
        connection.Close();
        dt_PlanningTemp = new DataTable();
        dt_PlanningTemp.Columns.Add("Matricule");
        dt_PlanningTemp.Columns.Add("Formateur");
        dt_PlanningTemp.Columns.Add("TypeEmploi");
        dt_PlanningTemp.Columns.Add("Groupe");
        dt_PlanningTemp.Columns.Add("Filiere");
        dt_PlanningTemp.Columns.Add("Nom");
        dt_PlanningTemp.Columns.Add("Annee");
        dt_PlanningTemp.Columns.Add("Local");
        dt_PlanningTemp.Columns.Add("MH");
        dgv_PlanningOrigine.DataSource = dt_PlanningOrigine;
        dgv_PlanningAlgo.DataSource = dt_PlanningTemp;
        dgv_PlanningOrigine.DataBind();
        dgv_PlanningAlgo.DataBind();

        double somme = 0;

        for (int i = 0; i <= dt_PlanningOrigine.Rows.Count - 1; i++)
            somme += double.Parse(dt_PlanningOrigine.Rows[i]["MH"].ToString());

        //Somme_Planning_Origine.Text = "Somme Planning Origine: " + somme;
        //Somme_AlgoPlanning.Text = "Somme AlgoPlanning: 0";
        if (dgv_PlanningOrigine.Rows.Count > 0)
            Button2.Visible = true;

        Session["dt_PlanningOrigine"] = dt_PlanningOrigine;
        Session["dt_PlanningTemp"] = dt_PlanningTemp;
    }   catch(Exception ex)
        {
            Label2.Visible = true;
            Label2.Text = ex.Message;
        }
    }
    protected void Button2_Click(object sender, EventArgs e)
    {
        try{
        Session["anneeformation"] = "2021/2022";
        Session["etablissement"] = "p200";

        dt_PlanningTemp =(DataTable) Session["dt_PlanningTemp"];
        dt_PlanningOrigine =(DataTable) Session["dt_PlanningOrigine"];
        const decimal MAX_HEURES_LOCAL = 60;
        DataView dv_Planning;
        dv_Planning = new DataView();
        // dv_Planning = Session("dt_PlanningOrigine").DefaultView

        dv_Planning = dt_PlanningOrigine.DefaultView;
        DataTable dt_locauxLP;
        dt_locauxLP = new DataTable();
        dt_locauxLP = dt_PlanningOrigine.Copy();
        // dt_locauxLP = dt_PlanningOrigine.Copy()

        command.CommandText = string.Format("select id from Local WHERE Etablissement='{0}' ", Session["etablissement"].ToString());
        connection.Open();
        reader = command.ExecuteReader();
        DataTable dt_locauxEtablissement = new DataTable();
        dt_locauxEtablissement.Load(reader);
        reader.Close();
        connection.Close();
        string[] priorite = new string[9] { "1,1", "1,2", "2,1", "1,3", "2,2", "3,1", "2,3", "3,2", "3,3" };
        string[] rest_priorite = new string[6] { "4", "11", "5", "12", "6", "13" };
        TraitementPermanent(1, dv_Planning, priorite, dt_locauxEtablissement, dt_locauxLP, MAX_HEURES_LOCAL, "AND Filiere not like '%RCDS%'");
        TraitementVacataire(1, dv_Planning, priorite, dt_locauxEtablissement, dt_locauxLP, MAX_HEURES_LOCAL, "AND Filiere not like '%RCDS%'");
        TraitementPermanent(2, dv_Planning, rest_priorite, dt_locauxEtablissement, dt_locauxLP, MAX_HEURES_LOCAL, "AND Filiere not like '%RCDS%'");
        TraitementVacataire(2, dv_Planning, rest_priorite, dt_locauxEtablissement, dt_locauxLP, MAX_HEURES_LOCAL, "AND Filiere not like '%RCDS%'");
        TraitementPermanent(1, dv_Planning, priorite, dt_locauxEtablissement, dt_locauxLP, 10, "AND Filiere like '%RCDS%'");
        TraitementPermanent(2, dv_Planning, rest_priorite, dt_locauxEtablissement, dt_locauxLP, 10, "AND Filiere  like '%RCDS%'");
        command.CommandText = string.Format("select id from Local WHERE Etablissement='{0}' AND AccessibleParTous = 1 order by IsNull(OrdrePriorite, id) asc", Session["etablissement"].ToString());
        connection.Open();
        reader = command.ExecuteReader();
        dt_locauxEtablissement = new DataTable();
        dt_locauxEtablissement.Load(reader);
        reader.Close();
        connection.Close();
        dv_Planning.RowFilter = "";
        dv_Planning.RowFilter = "TypeEmploi <>  'EXTERNE1' and TypeEmploi not like 'EXTERNE2'";

        if (0 < dv_Planning.Count)
        {
            for (int i = 0; i <= dv_Planning.Count - 1; i++)
            {
                int i_local = 0;

                while (i_local < dt_locauxEtablissement.Rows.Count && double.Parse(dv_Planning[i]["MH"].ToString()) > 0)
                {
                    DataTable row_dv_Planning = new DataTable();
                    row_dv_Planning = dt_PlanningOrigine.Copy();
                    row_dv_Planning.Rows.Clear();
                    row_dv_Planning.Rows.Add(dv_Planning[i][0].ToString(), dv_Planning[i][1].ToString(), dv_Planning[i][2].ToString(), dv_Planning[i][3].ToString(), dv_Planning[i][4].ToString(), dv_Planning[i][5].ToString(), dv_Planning[i][6].ToString(), dv_Planning[i][7].ToString(), dv_Planning[i][8].ToString(), dv_Planning[i][9].ToString(), dv_Planning[i][10].ToString(), dv_Planning[i][11].ToString(), dv_Planning[i][12].ToString(), dv_Planning[i][13].ToString(), dv_Planning[i][14].ToString());
                    dt_locauxLP.Rows.Clear();
                    RemplirParUneCopie(dt_locauxLP, row_dv_Planning.DefaultView);
                    decimal somme_MH_deja_affecter = 0M;
                    dt_PlanningTemp.DefaultView.RowFilter = "Local = " + dt_locauxEtablissement.Rows[i_local][0].ToString();
                    somme_MH_deja_affecter = CalculerMhDeja(dt_PlanningTemp.DefaultView);
                    dt_PlanningTemp.DefaultView.RowFilter = "";

                    if (MAX_HEURES_LOCAL - somme_MH_deja_affecter > 0)
                    {
                        decimal somme_MH_nouveau = 0M;
                        somme_MH_nouveau = CalculerNouveauMh(dt_locauxLP);

                        if ((somme_MH_nouveau <= MAX_HEURES_LOCAL - somme_MH_deja_affecter) && somme_MH_nouveau > 0)
                        {
                            AjouterAuPlanningTemp(dt_locauxLP, dt_locauxEtablissement.Rows[i_local][0].ToString());
                            dv_Planning[i]["MH"] = 0;
                        }
                        else if (somme_MH_nouveau > MAX_HEURES_LOCAL - somme_MH_deja_affecter)
                        {
                            DataTable dt_RemplirLocal = new DataTable();
                            dt_RemplirLocal = dt_locauxLP.Copy();
                            dt_RemplirLocal.Rows[0]["MH"] = 0;
                            ConditionMAX_HEURE_lOCAL(MAX_HEURES_LOCAL, somme_MH_deja_affecter, dt_locauxLP, dt_RemplirLocal);
                            AjouterAuPlanningTemp(dt_RemplirLocal, dt_locauxEtablissement.Rows[i_local][0].ToString());
                            dv_Planning[i]["MH"] = 0;
                            AjouterAuPlanningOrigine(dt_locauxLP);
                        }
                    }

                    i_local += 1;
                }
            }
        }

        dv_Planning.RowFilter = "";
        dv_Planning.RowFilter = "TypeEmploi =  'EXTERNE1' or TypeEmploi  like 'EXTERNE2'";

        if (0 < dv_Planning.Count)
        {
            for (int vac_type = 1; vac_type <= 2; vac_type++)
            {
                dv_Planning.RowFilter = "";

                if (vac_type == 1)
                    dv_Planning.RowFilter = dv_Planning.RowFilter + "  TypeEmploi = 'EXTERNE2' ";
                else
                    dv_Planning.RowFilter = dv_Planning.RowFilter + "  TypeEmploi = 'EXTERNE1' ";

                if (dv_Planning.Count > 0)
                {
                    for (int i = 0; i <= dv_Planning.Count - 1; i++)
                    {
                        int i_local = 0;

                        while (i_local < dt_locauxEtablissement.Rows.Count && double.Parse(dv_Planning[i]["MH"].ToString()) > 0)
                        {
                            dt_locauxLP.Rows.Clear();
                            dt_locauxLP.Rows.Add(dv_Planning[i][0].ToString(), dv_Planning[i][1].ToString(), dv_Planning[i][2].ToString(), dv_Planning[i][3].ToString(), dv_Planning[i][4].ToString(), dv_Planning[i][5].ToString(), dv_Planning[i][6].ToString(), dv_Planning[i][7].ToString(), dv_Planning[i][8].ToString(), dv_Planning[i][9].ToString(), dv_Planning[i][10].ToString(), dv_Planning[i][11].ToString(), dv_Planning[i][12].ToString(), dv_Planning[i][13].ToString(), dv_Planning[i][14].ToString());
                            decimal somme_MH_deja_affecter = 0M;
                            dt_PlanningTemp.DefaultView.RowFilter = "Local =" + dt_locauxEtablissement.Rows[i_local][0].ToString();
                            somme_MH_deja_affecter = CalculerMhDeja(dt_PlanningTemp.DefaultView);
                            decimal somme_MH_deja_Vac = 0M;

                            if (vac_type == 1)
                                dt_PlanningTemp.DefaultView.RowFilter = dt_PlanningTemp.DefaultView.RowFilter + " and TypeEmploi = 'EXTERNE2' ";
                            else
                                dt_PlanningTemp.DefaultView.RowFilter = dt_PlanningTemp.DefaultView.RowFilter + " and TypeEmploi = 'EXTERNE1' ";

                            somme_MH_deja_Vac = CalculerMhDeja(dt_PlanningTemp.DefaultView);
                            dt_PlanningTemp.DefaultView.RowFilter = "";

                            if (MAX_HEURES_LOCAL - somme_MH_deja_affecter > 0 && 10 - somme_MH_deja_Vac > 0)
                            {
                                decimal somme_MH = 0M;
                                somme_MH += decimal.Parse(dt_locauxLP.Rows[0]["MH"].ToString());

                                if ((somme_MH <= MAX_HEURES_LOCAL - somme_MH_deja_affecter) && somme_MH > 0)
                                {
                                    if ((somme_MH <= 10 - somme_MH_deja_Vac))
                                    {
                                        AjouterAuPlanningTemp(dt_locauxLP, dt_locauxEtablissement.Rows[i_local][0].ToString());
                                        dv_Planning[i]["MH"] = 0;
                                    }
                                    else if (somme_MH > 10 - somme_MH_deja_Vac)
                                    {
                                        DataTable dt_RemplirLocal = new DataTable();
                                        dt_RemplirLocal = dt_locauxLP.Copy();
                                        dt_RemplirLocal.Rows[0]["MH"] = 0;
                                        ConditionMAX_HEURE_lOCAL(10, somme_MH_deja_affecter, dt_locauxLP, dt_RemplirLocal);
                                        AjouterAuPlanningTemp(dt_RemplirLocal, dt_locauxEtablissement.Rows[i_local][0].ToString());
                                        dv_Planning[i]["MH"] = 0;
                                        AjouterAuPlanningOrigine(dt_locauxLP);
                                    }
                                }
                                else if (somme_MH > MAX_HEURES_LOCAL - somme_MH_deja_affecter)
                                {
                                    DataTable dt_RemplirLocal = new DataTable();
                                    dt_RemplirLocal = dt_locauxLP.Copy();
                                    dt_RemplirLocal.Rows[0]["MH"] = 0;
                                    ConditionVacataire(MAX_HEURES_LOCAL, somme_MH_deja_affecter, somme_MH_deja_Vac, dt_locauxLP, dt_RemplirLocal);
                                    AjouterAuPlanningTemp(dt_RemplirLocal, dt_locauxEtablissement.Rows[i_local][0].ToString());
                                    dv_Planning[i]["MH"] = 0;
                                    AjouterAuPlanningOrigine(dt_locauxLP);
                                }
                            }

                            i_local += 1;
                        }
                    }
                }
            }
        }

        dv_Planning.Sort = "";
        dv_Planning.RowFilter = "";
        int del = 0;

        while (del < dv_Planning.Count)
        {
            if (dv_Planning[del]["MH"].ToString() == "0")
                dv_Planning.Delete(del);
            else
                del += 1;
        }
        dgv_PlanningOrigine.DataSource = dt_PlanningOrigine;
        dgv_PlanningOrigine.DataBind();
        double somme = 0;

        for (int i = 0; i <= dgv_PlanningOrigine.Rows.Count - 1; i++)
            somme += double.Parse(dgv_PlanningOrigine.Rows[i].Cells[14].Text);

       // Somme_Planning_Origine.Text = "Somme Planning Origine: " + (somme);
        somme = 0;

        for (int i = 0; i <= dt_PlanningTemp.Rows.Count - 1; i++)
            somme += double.Parse(dt_PlanningTemp.Rows[i]["MH"].ToString());

       // Somme_AlgoPlanning.Text = "Somme AlgoPlanning: " + somme;
        dv_Planning.Sort = "";
        dv_Planning.RowFilter = "";



        dgv_PlanningAlgo.DataSource = dt_PlanningTemp;
        dgv_PlanningAlgo.DataBind();

        if (dgv_PlanningAlgo.Rows.Count > 0)
        { Button3.Visible = true;
        Button2.Enabled = false;        
        }


    } catch(Exception ex)
        {
            Label2.Visible = true;
            Label2.Text = ex.Message;
        }
    }
    protected void Button3_Click(object sender, EventArgs e)
    {
        try
        {
            Session["anneeformation"] = "2021/2022";
            Session["etablissement"] = "p200";
            dt_PlanningTemp = (DataTable)Session["dt_PlanningTemp"];
            connection.Open();
            command.CommandText = String.Format("delete AlgoPlanning  WHERE AnneeFormation='{0}' and Mois = {1} and Etablissement='{2}' and type_formation='P' ", Session["anneeformation"].ToString(), cbx_Mois.SelectedIndex + 1, Session["etablissement"].ToString());
            command.ExecuteNonQuery();
            string insert_requet = string.Format("insert into AlgoPlanning values ( '{0}', '{1}', {2},", Session["etablissement"].ToString(), Session["anneeformation"].ToString(), cbx_Mois.SelectedIndex + 1);
            // dgv_PlanningAlgo.AllowUserToAddRows = False

            for (int i = 0; i <= dgv_PlanningAlgo.Rows.Count - 1; i++)
            {
                string formateur = dgv_PlanningAlgo.Rows[i].Cells[0].Text.ToString();
                string groupe = dgv_PlanningAlgo.Rows[i].Cells[3].Text.ToString();
                string local = dgv_PlanningAlgo.Rows[i].Cells[7].Text.ToString();
                string MH = dgv_PlanningAlgo.Rows[i].Cells[8].Text.ToString();
                if (MH.Contains(","))
                    MH = MH.Replace(",", ".");
                string insertion = insert_requet + string.Format(" '{0}' , {1} , {2} , {3} ,'{4}')", formateur, groupe, local, MH, "P");
                
                command.CommandText = insertion;
                command.ExecuteNonQuery();
            }
            dt_PlanningTemp.Rows.Clear();

            connection.Close();
            dgv_PlanningAlgo.DataSource = dt_PlanningTemp;
            dgv_PlanningAlgo.DataBind();
            Label2.Visible = true;
            Label2.Text = "Tous les données sont enregistrés";
            Label2.ForeColor = System.Drawing.Color.Green;

            Button3.Enabled = false;
        }
        catch (Exception ex)
        {
            Label2.Visible = true;
            Label2.Text = ex.Message;
        }
    }
}