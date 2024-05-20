using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

public partial class Generer_ET_P : System.Web.UI.Page
{
    private SqlConnection connection;
    private SqlCommand command;
    private SqlDataReader reader;
    private DataTable dt_Planning;
    private DataTable dt_Formateur;
    private DataTable dt_Groupes;
    
    private DataTable dt_Horraire;

    protected void Page_Load(object sender, EventArgs e)
    {
        connection = new SqlConnection("Data Source=DESKTOP-0HNH2FK\\DESKTOP;Initial Catalog=db_gestion_achats3;Integrated Security=True");
        command = new SqlCommand();
        command.Connection = connection;
        if (Page.IsPostBack == false)
            cbx_Mois.SelectedIndex = DateTime.Now.Month - 1;

        Session["etablissement"] = "P200";
        Session["anneeformation"] = "2021/2022";
    }


    public void DeleteView(DataView dv)
    {
        dv.Sort = "";
        int i = 0;

        while (i < dv.Count)
        {
            if (dv[i]["MH_Mois"].ToString() == "0")
                dv.Delete(i);
            else
                i += 1;
        }
    }
    public void RenouvellerMH(DataView dv)
    {
        for (int i = 0; i <= dv.Count - 1; i++)
            dv[i]["MH"] = dv[i]["MH_Mois"].ToString();
    }


    public List<string> RemplirLocaux(string etablissement, string matricule, string id_groupe)
    {
        List<string> List_LocauxPriorite = new List<string>();
        connection.Open();
        command.CommandText = string.Format(" SELECT lpf.LP1 as LPF1, IsNull(lpg.LP1,-1) AS LPG1, IsNull(lpf.LP2,-1) as LPF2 , IsNull(lpg.LP2,-1) AS LPG2, IsNull(lpf.LP3,-1) as LPF3, IsNull(lpg.LP2,-1) AS LPG3 FROM  LocalPriorite_Formateur lpf , LocalPriorite_Groupe lpg WHERE lpf.Etablissement = '{0}' and lpf.Formateur= '{1}' and lpg.Groupe = {2} ", etablissement, matricule, id_groupe);
        reader = command.ExecuteReader();
        DataTable dt_LocauxPriorite = new DataTable();
        dt_LocauxPriorite.Load(reader);
        reader.Close();
        connection.Close();
        connection.Open();
        command.CommandText = string.Format("select id from Local WHERE Etablissement='{0}' AND AccessibleParTous = 1 order by IsNull(OrdrePriorite, id) asc", etablissement);
        reader = command.ExecuteReader();
        DataTable dt_Locaux = new DataTable();
        dt_Locaux.Load(reader);
        reader.Close();
        connection.Close();

        for (int k = 0; k <= dt_Locaux.Rows.Count - 1; k++)
        {
            if (List_LocauxPriorite.IndexOf(dt_Locaux.Rows[k][0].ToString()) == -1)
                List_LocauxPriorite.Add(dt_Locaux.Rows[k][0].ToString());
        }

        connection.Open();
        command.CommandText = string.Format(" SELECT id       FROM Local WHERE  Etablissement = '{0}' and type <> 'FAD'", etablissement);
        reader = command.ExecuteReader();
        dt_Locaux = new DataTable();
        dt_Locaux.Load(reader);
        reader.Close();
        connection.Close();

        for (int k = 0; k <= dt_Locaux.Rows.Count - 1; k++)
        {
            if (List_LocauxPriorite.IndexOf(dt_Locaux.Rows[k][0].ToString()) == -1)
                List_LocauxPriorite.Add(dt_Locaux.Rows[k][0].ToString());
        }

        return List_LocauxPriorite;
    }

    public bool InsertionSamedi(string local, string matricule, string id_groupe, string mois, string jour, string heure_debut, string heure_fin, string annee_formation)
    {
        int occupation_Form_Gro_Loc_Lundi = -1;
        bool insertion = false;
        connection.Open();
        command.CommandText = string.Format("SELECT count(*)  FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  ([mois]={0} and Upper(jour)='LUNDI' and ( [heurDebut] = '08:30:00' or [heurDebut] =  '11:00:00') and [AF]='{1}' and [formateur] ='{2}'  )", mois, annee_formation, matricule);
        occupation_Form_Gro_Loc_Lundi = Convert.ToInt32(command.ExecuteScalar().ToString());
        connection.Close();

        if (occupation_Form_Gro_Loc_Lundi == 0)
        {
            connection.Open();
            command.CommandText = string.Format("insert into  [db_gestion_achats3].[dbo].[OccupationLocal] values('{0}','{1}',{2},{3},'{4}','{5}' ,'{6}' , {7},'{8}')", local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, "GETDATE()", annee_formation);

            if (command.ExecuteNonQuery() > 0)
                insertion = true;

            connection.Close();
        }

        return insertion;
    }

    public bool InsertionLundi(string local, string matricule, string id_groupe, string mois, string jour, string heure_debut, string heure_fin, string annee_formation)
    {
        int occupation_Form_Gro_Loc_Samedi = 0;
        bool insertion = false;
        connection.Open();
        command.CommandText = string.Format("SELECT count(*)  FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  ([mois]={0} and Upper(jour)='SAMEDI' and ( [heurDebut] = '13:30:00' or [heurDebut] =  '16:00:00') and [AF]='{1}' and [formateur] ='{2}'  )", mois, annee_formation, matricule);
        occupation_Form_Gro_Loc_Samedi = Convert.ToInt32(command.ExecuteScalar().ToString());
        connection.Close();

        if (occupation_Form_Gro_Loc_Samedi == 0)
        {
            connection.Open();
            command.CommandText = string.Format("insert into  [db_gestion_achats3].[dbo].[OccupationLocal] values('{0}','{1}',{2},{3},'{4}','{5}' ,'{6}' , {7},'{8}')", local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, "GETDATE()", annee_formation);

            if (command.ExecuteNonQuery() > 0)
                insertion = true;

            connection.Close();
        }

        return insertion;
    }

    public int OccupationGroupeLocalFormateur(string mois, string jour, string heure_debut, string annee_formation, string matricule, string id_groupe, string local, double max_seance, double max_formateur, double max_groupe)
    {
        // command.CommandText = String.Format(" if((SELECT count(*)  FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  ([mois]={0} and [jour]='{1}' and [heurDebut]='{2}'  and AF='{3}') and ( [formateur] ='{4}' or [groupe]={5} or local = {6})) = 0 and (SELECT count(*)  FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  ([mois]={0} and [jour]='{1}'  and [AF]='{3}') and ( [formateur] ='{4}' and [groupe]= {5} ))<{7} and (SELECT IsNull(sum(cast ( (DATEDIFF(ss,heurDebut,heurFin) )/3600.00 as decimal(4,2) )),0) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{3}' and  [groupe]={5})<{8} and ( SELECT IsNull(sum(cast ( (DATEDIFF(ss,heurDebut,heurFin) )/3600.00 as decimal(4,2) )),0) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{3}' and  [formateur]='{4}')<{9}) select 1 else select 0 ", mois, jour, heure_debut, annee_formation, matricule, id_groupe, local, max_seance, max_groupe, max_formateur)

        command.CommandText = string.Format(" if((SELECT count(*)  FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  ([mois]={0} and [jour]='{1}' and [heurDebut]='{2}'  and AF='{3}') and ( [formateur] ='{4}' or [groupe]={5} or local = {6})) = 0 and (SELECT count(*)  FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  ([mois]={0} and [jour]='{1}'  and [AF]='{3}') and ( [formateur] ='{4}' and [groupe]= {5} ))< convert(float,REPLACE('{7}',',','.')) and (SELECT IsNull(sum(cast ( (DATEDIFF(ss,heurDebut,heurFin) )/3600.00 as decimal(4,2) )),0) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{3}' and  [groupe]={5})< convert(float,REPLACE('{8}',',','.')) and ( SELECT IsNull(sum(cast ( (DATEDIFF(ss,heurDebut,heurFin) )/3600.00 as decimal(4,2) )),0) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{3}' and  [formateur]='{4}')< convert(float,REPLACE('{9}',',','.'))) select 1 else select 0 ", mois, jour, heure_debut, annee_formation, matricule, id_groupe, local, max_seance, max_groupe, max_formateur);

        connection.Open();
        int occupation = Convert.ToInt32(command.ExecuteScalar().ToString());
        connection.Close();
        return occupation;
    }
    public void _2Heures_SansConditionGroupe(string filtre, string mois, string annee_formation, string etablissement)
    {
        dt_Planning = (DataTable)Session["dt_Planning"];

        string filtre_groupe = " And TypeEmploi_Groupe = 'P'";
        dt_Planning.DefaultView.RowFilter = filtre + filtre_groupe;

        if (dt_Planning.DefaultView.Count > 0)
        {
            string typeEmploi = dt_Planning.DefaultView[0]["TypeEmploi_Formateur"].ToString();
            dt_Horraire.DefaultView.RowFilter = " Type_Emploi = '" + typeEmploi + "'";
            dt_Formateur.DefaultView.RowFilter = filtre;
            dt_Formateur.DefaultView.Sort = "Sexe asc, MH desc";
            int f = 0;

            while (dt_Formateur.DefaultView.Count > f)
            {
                string sexe_Formateur = dt_Formateur.DefaultView[f]["Sexe"].ToString();
                string matricule = dt_Formateur.DefaultView[f][0].ToString();
                dt_Planning.DefaultView.RowFilter = filtre + " AND Matricule = '" + matricule + "'" + filtre_groupe;

                if (dt_Planning.DefaultView.Count > 0)
                {
                    dt_Planning.DefaultView.Sort = "MH desc";

                    for (int i = 0; i <= dt_Planning.DefaultView.Count - 1; i++)
                    {
                        string id_groupe = dt_Planning.DefaultView[i]["Groupe"].ToString();
                        string local = dt_Planning.DefaultView[i]["Local"].ToString();
                        string Mutualiser = dt_Planning.DefaultView[i]["Mutualiser"].ToString();
                        double MH_mois = double.Parse(dt_Planning.DefaultView[i]["MH_Mois"].ToString());
                        int j_horraire = 0;

                        while (j_horraire < dt_Horraire.DefaultView.Count && MH_mois > 0)
                        {
                            string jour = dt_Horraire.DefaultView[j_horraire]["Jour"].ToString();
                            string heure_debut = dt_Horraire.DefaultView[j_horraire]["Heure_Debut"].ToString();
                            string heure_fin = dt_Horraire.DefaultView[j_horraire]["Heure_Fin"].ToString();
                            double maxForm = 7.5;
                            double maxGroupe = 7.5;
                            if (jour.ToUpper() == "VENDREDI")
                                maxGroupe = maxForm = 8;

                            int toutEstDisponible_1erSeance = OccupationGroupeLocalFormateur(mois, jour, heure_debut, annee_formation, matricule, id_groupe, local, 2, maxForm, maxGroupe);
                            int conditionMutualiser = 0;
                            if (Mutualiser == "1")
                                conditionMutualiser = ConditionMutualiser1(matricule, jour, etablissement, heure_debut, heure_fin, annee_formation);

                            if (toutEstDisponible_1erSeance == 1 && conditionMutualiser == 0)
                            {
                                if (jour.ToUpper() == "SAMEDI" && (heure_debut == "13:30:00" || heure_debut == "16:00:00"))
                                {
                                    if (sexe_Formateur != "F")
                                    {
                                        if (InsertionSamedi(local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, annee_formation))
                                        {
                                            if (MH_mois * 10 % 25 != 0)
                                                MH_mois -= 2;
                                            else
                                                MH_mois -= 2.5;

                                            dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                        }
                                    }
                                }
                                else if (jour.ToUpper() == "LUNDI" && (heure_debut == "08:30:00" || heure_debut == "11:00:00"))
                                {
                                    if (InsertionLundi(local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, annee_formation))
                                    {
                                        if (MH_mois * 10 % 25 != 0)
                                            MH_mois -= 2;
                                        else
                                            MH_mois -= 2.5;

                                        dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                    }
                                }
                                else
                                {
                                    connection.Open();
                                    command.CommandText = string.Format("insert into  [db_gestion_achats3].[dbo].[OccupationLocal] values ('{0}','{1}',{2},{3},'{4}','{5}' ,'{6}' , {7},'{8}')", local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, "GETDATE()", annee_formation);

                                    if (command.ExecuteNonQuery() > 0)
                                    {
                                        if (MH_mois * 10 % 25 != 0)
                                            MH_mois -= 2;
                                        else
                                            MH_mois -= 2.5;

                                        dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                    }

                                    connection.Close();
                                }
                            }

                            j_horraire += 1;
                        }
                    }

                    DeleteView(dt_Planning.DefaultView);
                    RenouvellerMH(dt_Planning.DefaultView);
                }

                f += 1;
            }
        }

        dt_Planning.DefaultView.RowFilter = "";
        dt_Planning.DefaultView.Sort = "";
    }
    public void _2Heures_ConditionGroupe(string filtre, string mois, string annee_formation, string etablissement, bool ConditionNull, double max_formateur, double max_groupe)
    {
        dt_Planning =(DataTable)Session["dt_Planning"];

        string filtre_groupe = " And TypeEmploi_Groupe = 'P'";
        dt_Planning.DefaultView.RowFilter = filtre + filtre_groupe;

        if (dt_Planning.DefaultView.Count > 0)
        {
            string typeEmploi = dt_Planning.DefaultView[0]["TypeEmploi_Formateur"].ToString();
            dt_Horraire.DefaultView.RowFilter = " Type_Emploi = '" + typeEmploi + "'";
            dt_Formateur.DefaultView.RowFilter = filtre;
            dt_Formateur.DefaultView.Sort = "Sexe asc, MH desc";
            int f = 0;

            while (dt_Formateur.DefaultView.Count > f)
            {
                string sexe_Formateur = dt_Formateur.DefaultView[f]["Sexe"].ToString();
                string matricule = dt_Formateur.DefaultView[f][0].ToString();
                dt_Planning.DefaultView.RowFilter = filtre + " AND Matricule = '" + matricule + "'" + filtre_groupe;

                if (dt_Planning.DefaultView.Count > 0)
                {
                    dt_Planning.DefaultView.Sort = "MH desc";

                    for (int i = 0; i <= dt_Planning.DefaultView.Count - 1; i++)
                    {
                        string id_groupe = dt_Planning.DefaultView[i]["Groupe"].ToString();
                        string local = dt_Planning.DefaultView[i]["Local"].ToString();
                        string Mutualiser = dt_Planning.DefaultView[i]["Mutualiser"].ToString();
                        double MH_mois = double.Parse(dt_Planning.DefaultView[i]["MH_Mois"].ToString());
                        int j_horraire = 0;

                        while (j_horraire < dt_Horraire.DefaultView.Count && MH_mois > 0)
                        {
                            string jour = dt_Horraire.DefaultView[j_horraire]["Jour"].ToString();
                            string heure_debut = dt_Horraire.DefaultView[j_horraire]["Heure_Debut"].ToString();
                            string heure_fin = dt_Horraire.DefaultView[j_horraire]["Heure_Fin"].ToString();
                            double maxForm = max_formateur;
                            double maxGroupe = max_groupe;
                            if (jour.ToUpper() == "VENDREDI")
                                maxGroupe = maxForm = 8;
                            int toutEstDisponible_1erSeance = OccupationGroupeLocalFormateur(mois, jour, heure_debut, annee_formation, matricule, id_groupe, local, 2, maxForm, maxGroupe);
                            int conditionGroupeFormateur = 0;
                            int conditionMutualiser = 0;

                            if (ConditionNull)
                                conditionGroupeFormateur = ConditionGroupeFormateurNull(mois, jour, annee_formation, id_groupe, heure_fin, heure_debut, matricule);
                            else
                                conditionGroupeFormateur = ConditionGroupeFormateur1(mois, jour, annee_formation, id_groupe, heure_fin, heure_debut, matricule);

                            if (Mutualiser == "1")
                                conditionMutualiser = ConditionMutualiser1(matricule, jour, etablissement, heure_debut, heure_fin, annee_formation);

                            if (toutEstDisponible_1erSeance == 1 && conditionGroupeFormateur == 1 && conditionMutualiser == 0)
                            {
                                if (jour.ToUpper() == "SAMEDI" && (heure_debut == "13:30:00" || heure_debut == "16:00:00"))
                                {
                                    if (sexe_Formateur != "F")
                                    {
                                        if (InsertionSamedi(local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, annee_formation))
                                        {
                                            if (MH_mois * 10 % 25 != 0)
                                                MH_mois -= 2;
                                            else
                                                MH_mois -= 2.5;

                                            dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                        }
                                    }
                                }
                                else if (jour.ToUpper() == "LUNDI" && (heure_debut == "08:30:00" || heure_debut == "11:00:00"))
                                {
                                    if (InsertionLundi(local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, annee_formation))
                                    {
                                        if (MH_mois * 10 % 25 != 0)
                                            MH_mois -= 2;
                                        else
                                            MH_mois -= 2.5;

                                        dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                    }
                                }
                                else
                                {
                                    connection.Open();
                                    command.CommandText = string.Format("insert into  [db_gestion_achats3].[dbo].[OccupationLocal] values('{0}','{1}',{2},{3},'{4}','{5}' ,'{6}' , {7},'{8}')", local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, "GETDATE()", annee_formation);

                                    if (command.ExecuteNonQuery() > 0)
                                    {
                                        if (MH_mois * 10 % 25 != 0)
                                            MH_mois -= 2;
                                        else
                                            MH_mois -= 2.5;

                                        dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                    }

                                    connection.Close();
                                }
                            }

                            j_horraire += 1;
                        }
                    }

                    DeleteView(dt_Planning.DefaultView);
                    RenouvellerMH(dt_Planning.DefaultView);
                }

                f += 1;
            }
        }

        dt_Planning.DefaultView.RowFilter = "";
        dt_Planning.DefaultView.Sort = "";
    }


    public void _2Heures_BP_CDS(string filtre, string mois, string annee_formation, double max_seance)
    {
        dt_Planning = (DataTable)Session["dt_Planning"];

        dt_Planning.DefaultView.RowFilter = filtre;

        if (dt_Planning.DefaultView.Count > 0)
        {
            dt_Groupes.DefaultView.RowFilter = filtre;
            int f = 0;

            while (dt_Groupes.DefaultView.Count > f)
            {
                string id_groupe = dt_Groupes.DefaultView[f][0].ToString();
                dt_Planning.DefaultView.RowFilter = filtre + " AND Groupe = " + id_groupe + "";

                if (dt_Planning.DefaultView.Count > 0)
                {
                    string typeEmploi = dt_Planning.DefaultView[0]["TypeEmploi_Groupe"].ToString();
                    dt_Horraire.DefaultView.RowFilter = " Type_Emploi = '" + typeEmploi + "'";
                    dt_Planning.DefaultView.Sort = "MH desc";

                    for (int i = 0; i <= dt_Planning.DefaultView.Count - 1; i++)
                    {
                        string sexe_Formateur = dt_Planning.DefaultView[i]["Sexe"].ToString();
                        string matricule;
                        matricule = dt_Planning.DefaultView[i]["Matricule"].ToString();
                        string local = dt_Planning.DefaultView[i]["Local"].ToString();
                        double MH_mois = double.Parse(dt_Planning.DefaultView[i]["MH_Mois"].ToString());
                        int j_horraire = 0;

                        while (j_horraire < dt_Horraire.DefaultView.Count && MH_mois > 0)
                        {
                            string jour = dt_Horraire.DefaultView[j_horraire]["Jour"].ToString();
                            string heure_debut = dt_Horraire.DefaultView[j_horraire]["Heure_Debut"].ToString();
                            string heure_fin = dt_Horraire.DefaultView[j_horraire]["Heure_Fin"].ToString();
                            double max = max_seance;
                            if (jour.ToUpper() == "VENDREDI")
                                max = 8;
                            int toutEstDisponible_1erSeance = OccupationGroupeLocalFormateur(mois, jour, heure_debut, annee_formation, matricule, id_groupe, local, 2, max, max);
                            int conditionGroupeFormateur = 1;

                            if (toutEstDisponible_1erSeance == 1 && conditionGroupeFormateur == 1)
                            {
                                if (jour.ToUpper() == "SAMEDI" && (heure_debut == "13:30:00" || heure_debut == "16:00:00"))
                                {
                                    if (sexe_Formateur != "F")
                                    {
                                        if (InsertionSamedi(local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, annee_formation))
                                        {
                                            if (MH_mois * 10 % 25 != 0)
                                                MH_mois -= 2;
                                            else
                                                MH_mois -= 2.5;

                                            dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                        }
                                    }
                                }
                                else if (jour.ToUpper() == "LUNDI" && (heure_debut == "08:30:00" || heure_debut == "11:00:00"))
                                {
                                    if (InsertionLundi(local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, annee_formation))
                                    {
                                        if (MH_mois * 10 % 25 != 0)
                                            MH_mois -= 2;
                                        else
                                            MH_mois -= 2.5;

                                        dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                    }
                                }
                                else
                                {
                                    connection.Open();
                                    command.CommandText = string.Format("insert into  [db_gestion_achats3].[dbo].[OccupationLocal] values('{0}','{1}',{2},{3},'{4}','{5}' ,'{6}' , {7},'{8}')", local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, "GETDATE()", annee_formation);

                                    if (command.ExecuteNonQuery() > 0)
                                    {
                                        if (MH_mois * 10 % 25 != 0)
                                            MH_mois -= 2;
                                        else
                                            MH_mois -= 2.5;

                                        dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                    }

                                    connection.Close();
                                }
                            }

                            j_horraire += 1;
                        }
                    }

                    DeleteView(dt_Planning.DefaultView);
                    RenouvellerMH(dt_Planning.DefaultView);
                }

                f += 1;
            }
        }

        dt_Planning.DefaultView.RowFilter = "";
        dt_Planning.DefaultView.Sort = "";
    }

    public void _5HeuresSuccessif_1Groupe(string filtre, string mois, string annee_formation)
    {
        dt_Planning = (DataTable)Session["dt_Planning"];

        string filtre_groupe = " And TypeEmploi_Groupe = 'P'";
        dt_Planning.DefaultView.RowFilter = filtre + filtre_groupe;

        if (dt_Planning.DefaultView.Count > 0)
        {
            string typeEmploi = dt_Planning.DefaultView[0]["TypeEmploi_Formateur"].ToString();
            dt_Horraire.DefaultView.RowFilter = " Type_Emploi = '" + typeEmploi + "'";
            dt_Formateur.DefaultView.RowFilter = filtre;
            dt_Formateur.DefaultView.Sort = "Sexe asc, MH asc";
            int f = 0;

            while (dt_Formateur.DefaultView.Count > f)
            {
                string sexe_Formateur = dt_Formateur.DefaultView[f]["Sexe"].ToString();
                string matricule = dt_Formateur.DefaultView[f][0].ToString();
                dt_Planning.DefaultView.RowFilter = filtre + " AND Matricule = '" + matricule + "'" + filtre_groupe;

                if (dt_Planning.DefaultView.Count > 0)
                {
                    dt_Planning.DefaultView.Sort = "MH desc";

                    for (int i = 0; i <= dt_Planning.DefaultView.Count - 1; i++)
                    {
                        string id_groupe;
                        id_groupe = dt_Planning.DefaultView[i]["Groupe"].ToString();
                        string local = dt_Planning.DefaultView[i]["Local"].ToString();
                        double MH_mois = double.Parse(dt_Planning.DefaultView[i]["MH_Mois"].ToString());
                        int j_horraire = 0;

                        while (j_horraire < dt_Horraire.DefaultView.Count && MH_mois >= 4)
                        {
                            string jour = dt_Horraire.DefaultView[j_horraire]["Jour"].ToString();
                            string heure_debut = dt_Horraire.DefaultView[j_horraire]["Heure_Debut"].ToString();
                            string heure_fin = dt_Horraire.DefaultView[j_horraire]["Heure_Fin"].ToString();
                            double max_seance = 5;
                            if (jour.ToUpper() == "VENDREDI")
                                max_seance = 8;
                            int toutEstDisponible_1erSeance = OccupationGroupeLocalFormateur(mois, jour, heure_debut, annee_formation, matricule, id_groupe, local, 2, max_seance, max_seance);
                            int toutEstDisponible_2emeSeance = OccupationGroupeLocalFormateur(mois, jour, heure_fin, annee_formation, matricule, id_groupe, local, 1, (max_seance == 8) ? 6 : 2.5, (max_seance == 8) ? 6 : 2.5);

                            if (toutEstDisponible_1erSeance == 1 && toutEstDisponible_2emeSeance == 1)
                            {
                                string heure_fin_2emeSeance = dt_Horraire.DefaultView[j_horraire + 1]["Heure_Fin"].ToString();

                                if (jour.ToUpper() == "SAMEDI" && (heure_debut == "13:30:00" || heure_debut == "16:00:00"))
                                {
                                    if (sexe_Formateur != "F")
                                    {
                                        if (InsertionSamedi(local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, annee_formation))
                                        {
                                            if (MH_mois * 10 % 25 != 0)
                                                MH_mois -= 2;
                                            else
                                                MH_mois -= 2.5;

                                            dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                        }

                                        if (InsertionSamedi(local, matricule, id_groupe, mois, jour, heure_fin, heure_fin_2emeSeance, annee_formation))
                                        {
                                            if (MH_mois * 10 % 25 != 0)
                                                MH_mois -= 2;
                                            else
                                                MH_mois -= 2.5;

                                            dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                        }
                                    }
                                }
                                else if (jour.ToUpper() == "LUNDI" && (heure_debut == "08:30:00" || heure_debut == "11:00:00"))
                                {
                                    if (InsertionLundi(local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, annee_formation))
                                    {
                                        if (MH_mois * 10 % 25 != 0)
                                            MH_mois -= 2;
                                        else
                                            MH_mois -= 2.5;

                                        dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                    }

                                    if (InsertionLundi(local, matricule, id_groupe, mois, jour, heure_fin, heure_fin_2emeSeance, annee_formation))
                                    {
                                        if (MH_mois * 10 % 25 != 0)
                                            MH_mois -= 2;
                                        else
                                            MH_mois -= 2.5;

                                        dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                    }
                                }
                                else
                                {
                                    connection.Open();
                                    command.CommandText = string.Format("insert into  [db_gestion_achats3].[dbo].[OccupationLocal] values('{0}','{1}',{2},{3},'{4}','{5}' ,'{6}' , {7},'{8}')", local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, "GETDATE()", annee_formation);

                                    if (command.ExecuteNonQuery() > 0)
                                    {
                                        if (MH_mois * 10 % 25 != 0)
                                            MH_mois -= 2;
                                        else
                                            MH_mois -= 2.5;

                                        dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                    }

                                    connection.Close();
                                    connection.Open();
                                    command.CommandText = string.Format("insert into  [db_gestion_achats3].[dbo].[OccupationLocal] values('{0}','{1}',{2},{3},'{4}','{5}' ,'{6}' , {7},'{8}')", local, matricule, id_groupe, mois, jour, heure_fin, heure_fin_2emeSeance, "GETDATE()", annee_formation);

                                    if (command.ExecuteNonQuery() > 0)
                                    {
                                        if (MH_mois * 10 % 25 != 0)
                                            MH_mois -= 2;
                                        else
                                            MH_mois -= 2.5;

                                        dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                    }

                                    connection.Close();
                                }
                            }

                            j_horraire += 4;

                            if (j_horraire == dt_Horraire.DefaultView.Count)
                                j_horraire = 2;
                        }
                    }

                    DeleteView(dt_Planning.DefaultView);
                    RenouvellerMH(dt_Planning.DefaultView);
                }

                f += 1;
            }
        }

        dt_Planning.DefaultView.RowFilter = "";
        dt_Planning.DefaultView.Sort = "";
    }


    public void _5HeuresSuccessif_2Groupes(string filtre, string mois, string annee_formation, double max_groupe)
    {
        dt_Planning = (DataTable)Session["dt_Planning"];

        string filtre_groupe = " And TypeEmploi_Groupe = 'P'";
        dt_Planning.DefaultView.RowFilter = filtre + filtre_groupe;

        if (dt_Planning.DefaultView.Count > 0)
        {
            string typeEmploi = dt_Planning.DefaultView[0]["TypeEmploi_Formateur"].ToString();
            dt_Horraire.DefaultView.RowFilter = " Type_Emploi = '" + typeEmploi + "'";
            dt_Formateur.DefaultView.RowFilter = filtre;
            dt_Formateur.DefaultView.Sort = "Sexe asc, MH desc";
            int f = 0;

            while (dt_Formateur.DefaultView.Count > f)
            {
                string sexe_Formateur = dt_Formateur.DefaultView[f]["Sexe"].ToString();
                string matricule = dt_Formateur.DefaultView[f][0].ToString();
                dt_Planning.DefaultView.RowFilter = filtre + " AND Matricule = '" + matricule + "'" + filtre_groupe;

                if (dt_Planning.DefaultView.Count > 0)
                {
                    dt_Planning.DefaultView.Sort = "Local asc, MH desc";
                    int i = 0;

                    while (i < dt_Planning.DefaultView.Count)
                    {
                        string id_groupe;
                        id_groupe = dt_Planning.DefaultView[i]["Groupe"].ToString();
                        string local = dt_Planning.DefaultView[i]["Local"].ToString();
                        double MH_mois = double.Parse(dt_Planning.DefaultView[i]["MH_Mois"].ToString());
                        int j_horraire = 0;

                        while (j_horraire < dt_Horraire.DefaultView.Count && MH_mois > 0)
                        {
                            string jour = dt_Horraire.DefaultView[j_horraire]["Jour"].ToString();
                            string heure_debut = dt_Horraire.DefaultView[j_horraire]["Heure_Debut"].ToString();
                            string heure_fin = dt_Horraire.DefaultView[j_horraire]["Heure_Fin"].ToString();
                            double max_seance_formateur = 5;
                            double max_seance_groupe = max_groupe;
                            if (jour.ToUpper() == "VENDREDI")
                                max_seance_formateur = max_seance_groupe = 8;
                            int toutEstDisponible_1erSeance = OccupationGroupeLocalFormateur(mois, jour, heure_debut, annee_formation, matricule, id_groupe, local, 2, max_seance_formateur, max_seance_groupe);
                            int conditionGroupeFormateur = 0;
                            conditionGroupeFormateur = ConditionGroupeFormateurNull(mois, jour, annee_formation, id_groupe, heure_fin, heure_debut, matricule);

                            if (toutEstDisponible_1erSeance == 1 && conditionGroupeFormateur == 1)
                            {
                                int horraire_Apres_Avant;

                                if (j_horraire % 2 == 0)
                                    horraire_Apres_Avant = j_horraire + 1;
                                else
                                    horraire_Apres_Avant = j_horraire - 1;

                                string heure_debut_2emeSeance = dt_Horraire.DefaultView[horraire_Apres_Avant]["Heure_Debut"].ToString();
                                string heure_fin_2emeSeance = dt_Horraire.DefaultView[horraire_Apres_Avant]["Heure_Fin"].ToString();

                                for (int j = i + 1; j <= dt_Planning.DefaultView.Count - 1; j++)
                                {
                                    string id_groupe_2emeSeance;
                                    id_groupe_2emeSeance = dt_Planning.DefaultView[j]["Groupe"].ToString();
                                    string local_2emeSeance = dt_Planning.DefaultView[j]["Local"].ToString();
                                    double MH_mois_2emeSeance = double.Parse(dt_Planning.DefaultView[j]["MH_Mois"].ToString());
                                    int toutEstDisponible_2emeSeance = OccupationGroupeLocalFormateur(mois, jour, heure_debut_2emeSeance, annee_formation, matricule, id_groupe_2emeSeance, local_2emeSeance, 1, (max_seance_formateur == 8) ? 6 : 2.5, max_seance_groupe);
                                    int conditionGroupeFormateur_2emeSeance = 0;
                                    conditionGroupeFormateur_2emeSeance = ConditionGroupeFormateurNull(mois, jour, annee_formation, id_groupe_2emeSeance, heure_fin_2emeSeance, heure_debut_2emeSeance, matricule);

                                    if (toutEstDisponible_2emeSeance == 1 && MH_mois_2emeSeance > 0 && conditionGroupeFormateur_2emeSeance == 1)
                                    {
                                        if (jour.ToUpper() == "SAMEDI" && (heure_debut == "13:30:00" || heure_debut == "16:00:00"))
                                        {
                                            if (sexe_Formateur != "F")
                                            {
                                                if (InsertionSamedi(local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, annee_formation))
                                                {
                                                    if (MH_mois * 10 % 25 != 0)
                                                        MH_mois -= 2;
                                                    else
                                                        MH_mois -= 2.5;

                                                    dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                                }

                                                if (InsertionSamedi(local_2emeSeance, matricule, id_groupe_2emeSeance, mois, jour, heure_debut_2emeSeance, heure_fin_2emeSeance, annee_formation))
                                                {
                                                    if (MH_mois_2emeSeance * 10 % 25 != 0)
                                                        MH_mois_2emeSeance -= 2;
                                                    else
                                                        MH_mois_2emeSeance -= 2.5;

                                                    dt_Planning.DefaultView[j]["MH_Mois"] = MH_mois_2emeSeance;
                                                }
                                            }
                                        }
                                        else if (jour.ToUpper() == "LUNDI" && (heure_debut == "08:30:00" || heure_debut == "11:00:00"))
                                        {
                                            if (InsertionLundi(local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, annee_formation))
                                            {
                                                if (MH_mois * 10 % 25 != 0)
                                                    MH_mois -= 2;
                                                else
                                                    MH_mois -= 2.5;

                                                dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                            }

                                            if (InsertionLundi(local_2emeSeance, matricule, id_groupe_2emeSeance, mois, jour, heure_debut_2emeSeance, heure_fin_2emeSeance, annee_formation))
                                            {
                                                if (MH_mois_2emeSeance * 10 % 25 != 0)
                                                    MH_mois_2emeSeance -= 2;
                                                else
                                                    MH_mois_2emeSeance -= 2.5;

                                                dt_Planning.DefaultView[j]["MH_Mois"] = MH_mois_2emeSeance;
                                            }
                                        }
                                        else
                                        {
                                            connection.Open();
                                            command.CommandText = string.Format("insert into  [db_gestion_achats3].[dbo].[OccupationLocal] values('{0}','{1}',{2},{3},'{4}','{5}' ,'{6}' , {7},'{8}')", local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, "GETDATE()", annee_formation);

                                            if (command.ExecuteNonQuery() > 0)
                                            {
                                                if (MH_mois * 10 % 25 != 0)
                                                    MH_mois -= 2;
                                                else
                                                    MH_mois -= 2.5;

                                                dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                            }

                                            connection.Close();
                                            connection.Open();
                                            command.CommandText = string.Format("insert into  [db_gestion_achats3].[dbo].[OccupationLocal] values('{0}','{1}',{2},{3},'{4}','{5}' ,'{6}' , {7},'{8}')", local_2emeSeance, matricule, id_groupe_2emeSeance, mois, jour, heure_debut_2emeSeance, heure_fin_2emeSeance, "GETDATE()", annee_formation);

                                            if (command.ExecuteNonQuery() > 0)
                                            {
                                                if (MH_mois_2emeSeance * 10 % 25 != 0)
                                                    MH_mois_2emeSeance -= 2;
                                                else
                                                    MH_mois_2emeSeance -= 2.5;

                                                dt_Planning.DefaultView[j]["MH_Mois"] = MH_mois_2emeSeance;
                                            }

                                            connection.Close();
                                        }
                                    }
                                }
                            }

                            j_horraire += 1;
                        }

                        i += 1;
                    }

                    DeleteView(dt_Planning.DefaultView);
                    RenouvellerMH(dt_Planning.DefaultView);
                }

                f += 1;
            }
        }

        dt_Planning.DefaultView.RowFilter = "";
        dt_Planning.DefaultView.Sort = "";
    }


    public int ConditionGroupeFormateurNull(string mois, string jour, string annee_formation, string id_groupe, string heure_fin, string heure_debut, string formateur)
    {
        int condition_seance_groupe = 0;
        command.CommandText = string.Format("if((((SELECT  cast(min(heurDebut) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [groupe]={3}) is null) or (SELECT  cast(max(heurDebut) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [groupe]={3} ) = '{4}' or (SELECT  cast(min(heurDebut) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [groupe]={3}) = '{4}' or (SELECT  cast(max(heurFin) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [groupe]={3}) = '{5}' or (SELECT  cast(min(heurFin) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [groupe]={3}) = '{5}') AND (((SELECT  cast(min(heurDebut) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [formateur]='{6}') is null) or (SELECT  cast(max(heurDebut) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [formateur]='{6}') = '{4}' or (SELECT  cast(min(heurDebut) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [formateur]='{6}' ) = '{4}' or (SELECT  cast(max(heurFin) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [formateur]='{6}') = '{5}' or (SELECT  cast(min(heurFin) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [formateur]='{6}') = '{5}'))select 1 else select 0", mois, jour, annee_formation, id_groupe, heure_fin, heure_debut, formateur);
        connection.Open();
        condition_seance_groupe = Convert.ToInt32(command.ExecuteScalar().ToString());
        connection.Close();
        return condition_seance_groupe;
    }




    public int ConditionGroupeFormateur1(string mois, string jour, string annee_formation, string id_groupe, string heure_fin, string heure_debut, string formateur)
    {
        int condition_seance_groupe = 0;
        command.CommandText = string.Format("if(((SELECT  cast(max(heurDebut) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [groupe]={3} ) = '{4}' or (SELECT  cast(min(heurDebut) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [groupe]={3} ) = '{4}' or (SELECT  cast(max(heurFin) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [groupe]={3}) = '{5}' or (SELECT  cast(min(heurFin) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [groupe]={3}) = '{5}') AND ((SELECT  cast(max(heurDebut) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [formateur]='{6}' ) = '{4}' or (SELECT  cast(min(heurDebut) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [formateur]='{6}') = '{4}' or (SELECT  cast(max(heurFin) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [formateur]='{6}') = '{5}' or (SELECT  cast(min(heurFin) as time) FROM [db_gestion_achats3].[dbo].[OccupationLocal] where  [mois]={0} and [jour]='{1}' and [AF]='{2}' and  [formateur]='{6}') = '{5}'))select 1 else select 0", mois, jour, annee_formation, id_groupe, heure_fin, heure_debut, formateur);
        connection.Open();
        condition_seance_groupe = Convert.ToInt32(command.ExecuteScalar().ToString());
        connection.Close();
        return condition_seance_groupe;
    }








    protected void Button1_Click1(object sender, EventArgs e)
    {
        try
        {
            connection.Open();
            command.CommandText = string.Format("SELECT [Formateur] as 'Matricule',f.Nom as 'Formateur',Upper(f.sexe) as 'Sexe', Upper( IsNull(f.Type_Emploi, 'Permanent') ) as 'TypeEmploi_Formateur', cast(IsNull(Mutualiser,0) as varchar(1)) as Mutualiser,[Groupe],g.codeFiliere,g.nom,g.Annee,IsNull(g.Type_Emploi, 'P') as 'TypeEmploi_Groupe', [Local], MH as 'MH_Mois',[MH]  as 'MH' FROM [db_gestion_achats3].[dbo].[AlgoPlanning] ap inner join Formateurs f  on ap.formateur = f.matricule inner join Groupes g on g.idGroupe = ap.Groupe            WHERE ap.AnneeFormation='{0}' and Mois = {1} and ap.Etablissement='{2}' and type_formation='p' ", Session["Anneeformation"].ToString(), cbx_Mois.SelectedIndex + 1, Session["etablissement"].ToString());
            reader = command.ExecuteReader();
            dt_Planning = new DataTable();
            dt_Planning.Load(reader);
            dgv_PlanningOrigine.DataSource = dt_Planning;
            dgv_PlanningOrigine.DataBind();
            reader.Close();
            connection.Close();
            Session["dt_Planning"] = dt_Planning;

            if ((dgv_PlanningOrigine.Rows.Count > 0))
                Button3.Visible = true;
        }
        // Session("dt_PlanningTemp") = dt_PlanningTemp
        catch (Exception ex)
        {
            Label2.Visible = true;
            Label2.Text = ex.Message;
        }
    }
    protected void Button3_Click(object sender, EventArgs e)
    {
        try
        {
            // '*******
            command.CommandText = "select COUNT(*) from  OccupationLocal where AF='" + Session["anneeformation"].ToString() + "' and mois=" + cbx_Mois.SelectedIndex + 1 + " and local in (select id from local where type<>'fad')";
            connection.Open();

            int nbr = 0;
            nbr = command.ExecuteNonQuery();
            connection.Close();
            if (nbr <= 0)
            {

                // '******
                dt_Planning = (DataTable)Session["dt_Planning"];
                string mois = (cbx_Mois.SelectedIndex + 1).ToString();
                string etablissement = Session["etablissement"].ToString();
                string annee_formation = Session["Anneeformation"].ToString();
                dt_Formateur = new DataTable();
                connection.Open();
                command.CommandText = string.Format("SELECT distinct formateur,Upper(f.sexe) as 'Sexe', Upper( IsNull(Type_Emploi, 'Permanent') ) as 'TypeEmploi_Formateur',cast(IsNull(Mutualiser,0) as varchar(1)) as Mutualiser,sum(MH) as 'MH'from AlgoPlanning ap inner join Formateurs f  on ap.formateur = f.matricule where ap.Etablissement='{0}' and Mois ={1} and type_formation='P' and  AnneeFormation = '{2}' group by formateur,Upper(f.sexe), Upper( IsNull(Type_Emploi, 'Permanent') ),IsNull(Mutualiser,0) order by Upper(f.sexe) asc ,sum(MH) asc", etablissement, mois, annee_formation);
                reader = command.ExecuteReader();
                dt_Formateur.Load(reader);
                reader.Close();
                connection.Close();
                dt_Groupes = new DataTable();
                connection.Open();
                command.CommandText = string.Format("SELECT  distinct ap.Groupe,Upper( IsNull(Type_Emploi, 'P') ) as 'TypeEmploi_Groupe' from AlgoPlanning ap inner join Groupes g on g.idGroupe = ap.Groupe where ap.Etablissement='{0}' and Mois ={1} and  ap.AnneeFormation = '{2}' and type_formation='P' ", etablissement, mois, annee_formation);
                reader = command.ExecuteReader();
                dt_Groupes.Load(reader);
                reader.Close();
                connection.Close();
                dt_Horraire = new DataTable();
                connection.Open();
                command.CommandText = string.Format("SELECT  Type_Emploi , Jour, cast(Heure_Debut as varchar(8)) as Heure_Debut, cast(Heure_Fin as varchar(8)) as Heure_Fin FROM [db_gestion_achats3].[dbo].[HorraireAutorise] ");
                reader = command.ExecuteReader();
                dt_Horraire.Load(reader);
                reader.Close();
                connection.Close();
                _2Heures_ConditionGroupe("TypeEmploi_Formateur  <> 'EXTERNE2' and TypeEmploi_Formateur <> 'EXTERNE1' and  TypeEmploi_Formateur <> 'PERMANENT' ", mois, annee_formation, etablissement, true, 5, 5);
                _2Heures_ConditionGroupe("TypeEmploi_Formateur ='EXTERNE2' ", mois, annee_formation, etablissement, true, 5, 5);
                _2Heures_ConditionGroupe("TypeEmploi_Formateur = 'EXTERNE1' ", mois, annee_formation, etablissement, true, 5, 5);
                _5HeuresSuccessif_1Groupe("TypeEmploi_Formateur = 'PERMANENT' and Mutualiser=1", mois, annee_formation);
                _5HeuresSuccessif_2Groupes("TypeEmploi_Formateur = 'PERMANENT' and Mutualiser = 1 ", mois, annee_formation, 5);
                _5HeuresSuccessif_2Groupes("TypeEmploi_Formateur = 'PERMANENT' and Mutualiser = 1  ", mois, annee_formation, 7.5);
                _2Heures_ConditionGroupe("TypeEmploi_Formateur = 'PERMANENT' and Mutualiser = 1 ", mois, annee_formation, etablissement, false, 5, 5);
                _2Heures_ConditionGroupe("TypeEmploi_Formateur = 'PERMANENT' and Mutualiser = 1 ", mois, annee_formation, etablissement, false, 5, 7.5);
                _2Heures_ConditionGroupe("TypeEmploi_Formateur = 'PERMANENT' and Mutualiser = 1 ", mois, annee_formation, etablissement, true, 7.5, 5);
                _2Heures_ConditionGroupe("TypeEmploi_Formateur = 'PERMANENT' and Mutualiser = 1 ", mois, annee_formation, etablissement, true, 7.5, 7.5);
                _2Heures_SansConditionGroupe("Mutualiser = 1 ", mois, annee_formation, etablissement);
                _2Heures_BP_CDS("TypeEmploi_Groupe = 'CDS'", mois, annee_formation, 5);
                _2Heures_BP_CDS("TypeEmploi_Groupe <> 'P' and TypeEmploi_Groupe <> 'CDS'", mois, annee_formation, 5);
                _2Heures_BP_CDS("TypeEmploi_Groupe <> 'P' and TypeEmploi_Groupe <> 'CDS'", mois, annee_formation, 7.5);
                _5HeuresSuccessif_1Groupe("TypeEmploi_Formateur = 'PERMANENT' and Mutualiser<>1", mois, annee_formation);
                _5HeuresSuccessif_2Groupes("TypeEmploi_Formateur = 'PERMANENT'  and Mutualiser<>1", mois, annee_formation, 5);
                _5HeuresSuccessif_2Groupes("TypeEmploi_Formateur = 'PERMANENT' and Mutualiser<>1", mois, annee_formation, 7.5);
                _2Heures_ConditionGroupe("TypeEmploi_Formateur = 'PERMANENT' and Mutualiser<>1", mois, annee_formation, etablissement, true, 5, 5);
                _2Heures_ConditionGroupe("TypeEmploi_Formateur = 'PERMANENT' and Mutualiser<>1", mois, annee_formation, etablissement, true, 5, 7.5);
                _2Heures_ConditionGroupe("TypeEmploi_Formateur = 'PERMANENT' and Mutualiser<>1", mois, annee_formation, etablissement, false, 7.5, 5);
                _2Heures_ConditionGroupe("TypeEmploi_Formateur = 'PERMANENT' and Mutualiser<>1", mois, annee_formation, etablissement, false, 7.5, 7.5);
                _2Heures_SansConditionGroupe("TypeEmploi_Formateur <> '0' ", mois, annee_formation, etablissement);
                dt_Planning.DefaultView.RowFilter = "";

                for (int cas = 1; cas <= 2; cas++)
                {
                    if (dt_Planning.DefaultView.Count > 0)
                    {
                        for (int i = 0; i <= dt_Planning.DefaultView.Count - 1; i++)
                        {
                            string matricule, type_formateur, id_groupe, local;
                            matricule = dt_Planning.DefaultView[i]["Matricule"].ToString();
                            id_groupe = dt_Planning.DefaultView[i]["Groupe"].ToString();
                            type_formateur = dt_Planning.DefaultView[i]["TypeEmploi_Formateur"].ToString();
                            string Mutualiser = dt_Planning.DefaultView[i]["Mutualiser"].ToString();
                            string sexe_formateur = dt_Planning.DefaultView[i]["Sexe"].ToString();
                            double MH_mois = double.Parse(dt_Planning.DefaultView[i]["MH_Mois"].ToString());
                            List<string> List_LocauxPriorite = new List<string>();
                            List_LocauxPriorite = RemplirLocaux(etablissement, matricule, id_groupe);
                            int j = 0;
                            dt_Horraire.DefaultView.RowFilter = " Type_Emploi = '" + type_formateur + "'";

                            while (j < List_LocauxPriorite.Count && MH_mois > 0)
                            {
                                local = List_LocauxPriorite[j].ToString();
                                int j_horraire = 0;

                                while (j_horraire < dt_Horraire.DefaultView.Count && MH_mois > 0)
                                {
                                    string jour = dt_Horraire.DefaultView[j_horraire]["Jour"].ToString();
                                    string heure_debut = dt_Horraire.DefaultView[j_horraire]["Heure_Debut"].ToString();
                                    string heure_fin = dt_Horraire.DefaultView[j_horraire]["Heure_Fin"].ToString();
                                    double max = 7.5;
                                    if (jour.ToUpper() == "VENDREDI")
                                        max = 8;
                                    int occupation = OccupationGroupeLocalFormateur(mois, jour, heure_debut, annee_formation, matricule, id_groupe, local, 2, max, max);
                                    int conditionMutualiser = 0;
                                    if (Mutualiser == "1")
                                        conditionMutualiser = ConditionMutualiser1(matricule, jour, etablissement, heure_debut, heure_fin, annee_formation);

                                    if (occupation == 1 && conditionMutualiser == 0)
                                    {
                                        if (cas == 1 && jour.ToUpper() == "SAMEDI" && (heure_debut == "13:30:00" || heure_debut == "16:00:00"))
                                        {
                                            if (sexe_formateur != "F")
                                            {
                                                if (InsertionSamedi(local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, annee_formation))
                                                {
                                                    if (MH_mois * 10 % 25 != 0)
                                                        MH_mois -= 2;
                                                    else
                                                        MH_mois -= 2.5;

                                                    dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                                }
                                            }
                                        }
                                        else if (cas == 1 && jour.ToUpper() == "LUNDI" && (heure_debut == "08:30:00" || heure_debut == "11:00:00"))
                                        {
                                            if (InsertionSamedi(local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, annee_formation))
                                            {
                                                if (MH_mois * 10 % 25 != 0)
                                                    MH_mois -= 2;
                                                else
                                                    MH_mois -= 2.5;

                                                dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                            }
                                        }
                                        else
                                        {
                                            connection.Open();
                                            command.CommandText = string.Format("insert into  [db_gestion_achats3].[dbo].[OccupationLocal] values('{0}','{1}',{2},{3},'{4}','{5}' ,'{6}' , {7},'{8}')", local, matricule, id_groupe, mois, jour, heure_debut, heure_fin, "GETDATE()", annee_formation);

                                            if (command.ExecuteNonQuery() > 0)
                                            {
                                                if (MH_mois * 10 % 25 != 0)
                                                    MH_mois -= 2;
                                                else
                                                    MH_mois -= 2.5;

                                                dt_Planning.DefaultView[i]["MH_Mois"] = MH_mois;
                                            }

                                            connection.Close();
                                        }
                                    }

                                    j_horraire += 1;
                                }

                                j += 1;
                            }
                        }
                    }

                    dt_Planning.DefaultView.RowFilter = "";
                    DeleteView(dt_Planning.DefaultView);
                    RenouvellerMH(dt_Planning.DefaultView);
                }

                dt_Planning.DefaultView.RowFilter = "";
                dt_Planning.DefaultView.Sort = "";
                dgv_PlanningOrigine.DataSource = dt_Planning.DefaultView;
                dgv_PlanningOrigine.DataBind();
                // MessageBox.Show("C'est finis")
                Label2.Visible = true;
                Label2.ForeColor = System.Drawing.Color.Green;
                Label2.Text = "L'emploi du temps a été générer avec succès";
            }
            else
            {
                Label2.Visible = true;
                Label2.Text = "l'emploie du temps du mois " + cbx_Mois.SelectedIndex + 1 + " existe Déja";
            }
        }
        catch (Exception ex)
        {
            Label2.Visible = true;
            Label2.Text = ex.Message;
        }
    }
    public int ConditionMutualiser1(string matricule, string jour, string etablissement, string heur_debut, string heur_fin, string annee)
    {
        int condition_mutualiser = 1;
        command.CommandText = string.Format("if( (select count(distinct etablissement) from OccupationLocal O inner join Groupes G on O.groupe = G.idGroupe where Formateur = '{0}' and jour = '{1}' and AF='{5}' and etablissement <> '{2}')=0 or (select count(*) from OccupationLocal O inner join Groupes G on O.groupe = G.idGroupe where  formateur= '{0}' and jour='{1}' and Etablissement <> '{2}' and AF='{5}' and (heurDebut = '{3}' or heurFin = '{4}') )=0) select 0 else select 1", matricule, jour, etablissement, heur_fin, heur_debut, annee);
        connection.Open();
        condition_mutualiser = Convert.ToInt32(command.ExecuteScalar().ToString());
        connection.Close();
        return condition_mutualiser;
    }
}