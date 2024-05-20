using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
public partial class Generer_ET_FAD : System.Web.UI.Page
{
    private SqlConnection connection;
    private SqlCommand command;
    private SqlDataReader reader;
    private DataTable dt_Planning;
    private DataTable dt_Formateur;
    
    
    private DataTable dt_Horraire;
    private DataTable dt_code_grpmnt_byformateur;
    private DataTable  dt_groupes_par_code_grpmnt;
    protected void Page_Load(object sender, EventArgs e)
    {
        connection = new SqlConnection("Data Source=DESKTOP-0HNH2FK\\DESKTOP;Initial Catalog=db_gestion_achats3;Integrated Security=True");
        command = new SqlCommand();
        command.Connection = connection;
        if (Page.IsPostBack == false)
            cbx_Mois.SelectedIndex = DateTime.Now.Month - 1;

    }
    protected void Button1_Click1(object sender, EventArgs e)
    {
        try
        {
            Session["anneeformation"] = "2021/2022";
            Session["etablissement"] = "p200";
            connection.Open();
            command.CommandText = string.Format("SELECT [Formateur] as 'Matricule',f.Nom as 'Formateur',Upper(f.sexe) as 'Sexe', Upper( IsNull(f.Type_Emploi, 'Permanent') ) as 'TypeEmploi_Formateur', cast(IsNull(Mutualiser,0) as varchar(1)) as Mutualiser,[Groupe],g.codeFiliere,g.nom,g.Annee,IsNull(g.Type_Emploi, 'P') as 'TypeEmploi_Groupe', [Local], MH as 'MH_Mois',[MH]  as 'MH' FROM [db_gestion_achats3].[dbo].[AlgoPlanning] ap inner join Formateurs f  on ap.formateur = f.matricule inner join Groupes g on g.idGroupe = ap.Groupe            WHERE ap.AnneeFormation='{0}' and type_formation='FAD' and Mois = {1} and ap.Etablissement='{2}' ", Session["Anneeformation"].ToString(), cbx_Mois.SelectedIndex + 1, Session["etablissement"].ToString());
            reader = command.ExecuteReader();
            dt_Planning = new DataTable();
            dt_Planning.Load(reader);
            dgv_PlanningOrigine.DataSource = dt_Planning;
            dgv_PlanningOrigine.DataBind();
            reader.Close();
            connection.Close();

            Session["dt_Planning"] = dt_Planning;

            if (dgv_PlanningOrigine.Rows.Count > 0)
                Button3.Visible = true;
        }
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

            

            dt_Planning = (DataTable) Session["dt_Planning"];
            int mois = (cbx_Mois.SelectedIndex + 1);
            string etablissement = Session["etablissement"].ToString();
            string annee_formation = Session["Anneeformation"].ToString();
            dt_Formateur = new DataTable();
            connection.Open();

            command.CommandText = string.Format("select count(*) from OccupationLocal where  Mois ={0} and  af = '{1}' and  local in (select id from local where type='fad') ", mois, annee_formation);
            int cnt =(int) command.ExecuteScalar();

            if (cnt > 0)
            {
                Label2.Visible = true;
                Label2.Text = "l'emploie du Temps du mois " + mois + " existe Déja ";
            }
            else
            {


                command.CommandText = string.Format("SELECT distinct formateur ,isnull(Type_Emploi,'Permanent') as TypeEmploi_Formateur  from AlgoPlanning ap inner join Formateurs on Matricule=Formateur where ap.Etablissement='{0}' and Mois ={1} and  AnneeFormation = '{2}' and type_formation='fad' ", etablissement, mois, annee_formation);
                reader = command.ExecuteReader();
                dt_Formateur.Load(reader);
                reader.Close();
                connection.Close();

                // dt_Formateur.DefaultView.RowFilter = "TypeEmploi_Formateur  <> 'EXTERNE2' and TypeEmploi_Formateur <> 'EXTERNE1' and  TypeEmploi_Formateur <> 'PERMANENT'"
                procedure_Men(mois, annee_formation, etablissement);
                procedure_externe("externe2", "TypeEmploi_Formateur  = 'EXTERNE2'", mois, annee_formation, etablissement);
                procedure_externe("externe1", "TypeEmploi_Formateur  = 'EXTERNE1'", mois, annee_formation, etablissement);
                procedure_externe("Permanent", "TypeEmploi_Formateur  = 'Permanent'", mois, annee_formation, etablissement);

                dt_Planning.Rows.Clear();
                dgv_PlanningOrigine.DataSource = dt_Planning;
                dgv_PlanningOrigine.DataBind();

                Label2.Visible = true;
                Label2.ForeColor = System.Drawing.Color.Green;
                Label2.Text = "l'emoloie du Temps du formation à distance à été générer avec succés";
            }
        }
        catch (Exception ex)
        {
            Label2.Visible = true;
            Label2.Text = ex.Message;
        }
    }
     
    public void procedure_Men(int mois, string af, string etab)
    {
        dt_Formateur.DefaultView.RowFilter = "TypeEmploi_Formateur  <> 'EXTERNE2' and TypeEmploi_Formateur <> 'EXTERNE1' and  TypeEmploi_Formateur <> 'PERMANENT'";
        if (dt_Formateur.DefaultView.Count > 0)
        {
            for (int i = 0; i <= dt_Formateur.DefaultView.Count - 1; i++)
            {
                command.CommandText = string.Format("select jour,heure_debut,heure_fin from HorraireAutorise where type_emploi='{0}'", dt_Formateur.DefaultView[i][1]);
                connection.Open();
                reader = command.ExecuteReader();
                dt_Horraire = new DataTable();
                dt_Horraire.Load(reader);

                command.CommandText = "select distinct code_groupement_fad from ModuleFormateurGroupe inner join Groupes on idGroupe=Groupe where Type_Formation='fad' and code_groupement_fad is not null and AnneeFormation=@af and Formateur=@for and Etablissement=@etab";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@for", dt_Formateur.Rows[i][0].ToString());
                command.Parameters.AddWithValue("@af", af);
                command.Parameters.AddWithValue("@etab", etab);
                // command.Parameters.AddWithValue("@mois", cbx_Mois.SelectedIndex + 1)

             


                reader = command.ExecuteReader();
                dt_code_grpmnt_byformateur = new DataTable();
                dt_code_grpmnt_byformateur.Load(reader);



                if (dt_code_grpmnt_byformateur.Rows.Count > 0)
                {
                  
                    for (var j = 0; j <= dt_code_grpmnt_byformateur.Rows.Count - 1; j++)
                    {
                        command.CommandText = "select distinct Groupe from ModuleFormateurGroupe where code_groupement_fad=@code";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@code", dt_code_grpmnt_byformateur.Rows[j][0]);
                        reader = command.ExecuteReader();
                        dt_groupes_par_code_grpmnt = new DataTable();
                        dt_groupes_par_code_grpmnt.Load(reader);
                        

                        DataTable dt_horraire_deja_inserer = new DataTable();
                        dt_horraire_deja_inserer.Columns.Add("jour");
                        dt_horraire_deja_inserer.Columns.Add("heure_debut");
                        dt_horraire_deja_inserer.Columns.Add("heure_fin");
                        float mh_mois;
                        int fois = 0;

                        for (int k = 0; k <= dt_groupes_par_code_grpmnt.Rows.Count - 1; k++)
                        {
                            dt_Planning.DefaultView.RowFilter = "matricule='" + dt_Formateur.Rows[i][0].ToString() + "' and groupe='" + int.Parse(dt_groupes_par_code_grpmnt.Rows[k][0].ToString()) + "'";
                            mh_mois =float.Parse( dt_Planning.DefaultView[0]["MH_MOIS"].ToString());

                            if ((dt_groupes_par_code_grpmnt.Rows.Count > 1))
                            {
                                while (mh_mois >= 2)
                                {
                                    if ((k == 0))
                                    {
                                        for (int t = 0; t <= dt_Horraire.Rows.Count - 1; t++)
                                        {
                                            //

                                           
                                            command.CommandText = "valider_si_tous_groupes_disponible_horire";
                                            command.Parameters.Clear();

                                            command.CommandType = CommandType.StoredProcedure;
                                            command.Parameters.AddWithValue("@code", dt_code_grpmnt_byformateur.Rows[j][0]);
                                            command.Parameters.AddWithValue("@mois", mois);
                                            command.Parameters.AddWithValue("@af", af);
                                            command.Parameters.AddWithValue("@jour", dt_Horraire.Rows[t][0].ToString());
                                            command.Parameters.AddWithValue("@hd", dt_Horraire.Rows[t][1].ToString());
                                            command.Parameters.AddWithValue("@hf", dt_Horraire.Rows[t][2].ToString());
                                            SqlParameter p1=new SqlParameter();
                                            p1.Direction=ParameterDirection.ReturnValue;
                                            p1.SqlDbType = SqlDbType.Int;
                                            command.Parameters.Add(p1);
                                            
                                             command.ExecuteNonQuery();

                                             if (int.Parse(p1.Value.ToString()) == 1)
                                             {

                                                 insertion_occupationlocal(etab, dt_Formateur.DefaultView[i][0].ToString(), int.Parse(dt_groupes_par_code_grpmnt.Rows[k][0].ToString()), dt_Horraire.Rows[t][0].ToString(), dt_Horraire.Rows[t][1].ToString(), dt_Horraire.Rows[t][2].ToString(), af);

                                                 DataRow dr = dt_horraire_deja_inserer.NewRow();
                                                 dr["jour"] = dt_Horraire.Rows[t][0];
                                                 dr["heure_debut"] = dt_Horraire.Rows[t][1];
                                                 dr["heure_fin"] = dt_Horraire.Rows[t][2];

                                                 dt_horraire_deja_inserer.Rows.Add(dr);
                                                 dt_Horraire.Rows[t].Delete();

                                                 mh_mois = mh_mois - 2.5f;
                                                  break;
                                             }
                                            
                                    }
                                    }
                                    else if (dt_horraire_deja_inserer.Rows.Count > 0)
                                    {

                                        insertion_occupationlocal(etab, dt_Formateur.DefaultView[i][0].ToString(), int.Parse(dt_groupes_par_code_grpmnt.Rows[k][0].ToString()), dt_horraire_deja_inserer.Rows[0][0].ToString(), dt_horraire_deja_inserer.Rows[0][1].ToString(), dt_horraire_deja_inserer.Rows[0][2].ToString(), af);
                                        dt_horraire_deja_inserer.Rows[0].Delete();
                                        mh_mois = mh_mois - 2.5f;
                                    }
                                    else
                                    {
                                        for (int t = 0; t <= dt_Horraire.Rows.Count - 1; t++)
                                        {
                                            //


                                            command.CommandText = "valider_si_tous_groupes_disponible_horire";
                                            command.Parameters.Clear();

                                            command.CommandType = CommandType.StoredProcedure;
                                            command.Parameters.AddWithValue("@code", dt_code_grpmnt_byformateur.Rows[j][0]);
                                            command.Parameters.AddWithValue("@mois", mois);
                                            command.Parameters.AddWithValue("@af", af);
                                            command.Parameters.AddWithValue("@jour", dt_Horraire.Rows[t][0].ToString());
                                            command.Parameters.AddWithValue("@hd", dt_Horraire.Rows[t][1].ToString());
                                            command.Parameters.AddWithValue("@hf", dt_Horraire.Rows[t][2].ToString());
                                            SqlParameter p1 = new SqlParameter();
                                            p1.Direction = ParameterDirection.ReturnValue;
                                            p1.SqlDbType = SqlDbType.Int;
                                            command.Parameters.Add(p1);
                                            
                                            command.ExecuteNonQuery();

                                            if (int.Parse(p1.Value.ToString()) == 1)
                                            {
                                                insertion_occupationlocal(etab, dt_Formateur.DefaultView[i][0].ToString(), int.Parse(dt_groupes_par_code_grpmnt.Rows[k][0].ToString()), dt_Horraire.Rows[t][0].ToString(), dt_Horraire.Rows[t][1].ToString(), dt_Horraire.Rows[t][2].ToString(), af);
                                                dt_Horraire.Rows[0].Delete();
                                                mh_mois = mh_mois - 2.5f;
                                                break;
                                            }
                                            
                                        }
                                    }
                                }
                            }
                            else
                                while (mh_mois >= 2)
                                {
                                    for (int t = 0; t <= dt_Horraire.Rows.Count - 1; t++)
                                    {
                                        //


                                        command.CommandText = "valider_si_tous_groupes_disponible_horire";
                                        command.Parameters.Clear();

                                        command.CommandType = CommandType.StoredProcedure;
                                        command.Parameters.AddWithValue("@code", dt_code_grpmnt_byformateur.Rows[j][0]);
                                        command.Parameters.AddWithValue("@mois", mois);
                                        command.Parameters.AddWithValue("@af", af);
                                        command.Parameters.AddWithValue("@jour", dt_Horraire.Rows[t][0].ToString());
                                        command.Parameters.AddWithValue("@hd", dt_Horraire.Rows[t][1].ToString());
                                        command.Parameters.AddWithValue("@hf", dt_Horraire.Rows[t][2].ToString());
                                        SqlParameter p1 = new SqlParameter();
                                        p1.Direction = ParameterDirection.ReturnValue;
                                        p1.SqlDbType = SqlDbType.Int;
                                        command.Parameters.Add(p1);

                                        command.ExecuteNonQuery();

                                        if (int.Parse(p1.Value.ToString()) == 1)
                                        {
                                            insertion_occupationlocal(etab, dt_Formateur.DefaultView[i][0].ToString(), int.Parse(dt_groupes_par_code_grpmnt.Rows[k][0].ToString()), dt_Horraire.Rows[t][0].ToString(), dt_Horraire.Rows[t][1].ToString(), dt_Horraire.Rows[t][2].ToString(), af);
                                            dt_Horraire.Rows[0].Delete();
                                            mh_mois = mh_mois - 2.5f;
                                            break;

                                        }
                                    }
                                }
                        }
                    }
                }
                connection.Close();
            }
        }
    }

    public void procedure_externe(string type, string filtre, int mois, string af, string etab)
    {
        dt_Formateur.DefaultView.RowFilter = filtre;
        if (dt_Formateur.DefaultView.Count > 0)
        {
            for (int i = 0; i <= dt_Formateur.DefaultView.Count - 1; i++)
            {
                command.CommandText = string.Format("select jour,heure_debut,heure_fin from HorraireAutorise where type_emploi='" + type + "' and Heure_Debut<>'11:00:00' and Heure_Debut<>'13:30:00' and Heure_Debut<>'14:30:00' and Heure_Debut<>'10:30:00'");
                connection.Open();
                reader = command.ExecuteReader();
                dt_Horraire = new DataTable();
                dt_Horraire.Load(reader);

                command.CommandText = "select distinct code_groupement_fad from ModuleFormateurGroupe inner join Groupes on idGroupe=Groupe where Type_Formation='fad' and code_groupement_fad is not null and AnneeFormation=@af and Formateur=@for and Etablissement=@etab";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@for", dt_Formateur.Rows[i][0].ToString());
                command.Parameters.AddWithValue("@af", af);
                command.Parameters.AddWithValue("@etab", etab);




                reader = command.ExecuteReader();
                dt_code_grpmnt_byformateur = new DataTable();
                dt_code_grpmnt_byformateur.Load(reader);
                if (dt_code_grpmnt_byformateur.Rows.Count > 0)
                {
                    for (var j = 0; j <= dt_code_grpmnt_byformateur.Rows.Count - 1; j++)
                    {
                        command.CommandText = "select distinct Groupe from ModuleFormateurGroupe where code_groupement_fad=@code";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@code", dt_code_grpmnt_byformateur.Rows[j][0]);
                        reader = command.ExecuteReader();
                        dt_groupes_par_code_grpmnt = new DataTable();
                        dt_groupes_par_code_grpmnt.Load(reader);


                        DataTable dt_horraire_deja_inserer = new DataTable();
                        dt_horraire_deja_inserer.Columns.Add("jour");
                        dt_horraire_deja_inserer.Columns.Add("heure_debut");
                        dt_horraire_deja_inserer.Columns.Add("heure_fin");
                        double mh_mois;
                        
                        for (int k = 0; k <= dt_groupes_par_code_grpmnt.Rows.Count - 1; k++)
                        {
                            int fois = 0;
                            dt_Planning.DefaultView.RowFilter = "matricule='" + dt_Formateur.Rows[i][0].ToString() + "' and groupe='" + int.Parse(dt_groupes_par_code_grpmnt.Rows[k][0].ToString()) + "'";
                            if (dt_Planning.DefaultView.Count > 0)
                            {
                                mh_mois = Double.Parse(dt_Planning.DefaultView[0]["MH_MOIS"].ToString());

                                if ((dt_groupes_par_code_grpmnt.Rows.Count > 1))
                                {
                                    while (mh_mois >= 2)
                                    {
                                        if ((k == 0))
                                        {
                                            for (int t = 0; t <= dt_Horraire.Rows.Count - 1; t++)
                                            {
                                                //


                                                command.CommandText = "valider_si_tous_groupes_disponible_horire";
                                                command.CommandType = CommandType.StoredProcedure;
                                                command.Parameters.Clear();
                                                command.Parameters.AddWithValue("@code", dt_code_grpmnt_byformateur.Rows[j][0]);
                                                command.Parameters.AddWithValue("@mois", mois);
                                                command.Parameters.AddWithValue("@af", af);
                                                command.Parameters.AddWithValue("@jour", dt_Horraire.Rows[t][0].ToString());
                                                command.Parameters.AddWithValue("@hd", dt_Horraire.Rows[t][1].ToString());
                                                command.Parameters.AddWithValue("@hf", dt_Horraire.Rows[t][2].ToString());
                                                SqlParameter p1 = new SqlParameter();
                                                p1.Direction = ParameterDirection.ReturnValue;
                                                p1.SqlDbType = SqlDbType.Int;
                                                command.Parameters.Add(p1);
                                                
                                                command.ExecuteNonQuery();

                                                if (int.Parse(p1.Value.ToString()) == 1)
                                                {
                                                    insertion_occupationlocal(etab, dt_Formateur.DefaultView[i][0].ToString(), int.Parse(dt_groupes_par_code_grpmnt.Rows[k][0].ToString()), dt_Horraire.Rows[t][0].ToString(), dt_Horraire.Rows[t][1].ToString(), dt_Horraire.Rows[t][2].ToString(), af);

                                                    DataRow dr = dt_horraire_deja_inserer.NewRow();
                                                    dr["jour"] = dt_Horraire.Rows[t][0];
                                                    dr["heure_debut"] = dt_Horraire.Rows[t][1];
                                                    dr["heure_fin"] = dt_Horraire.Rows[t][2];

                                                    dt_horraire_deja_inserer.Rows.Add(dr);
                                                    dt_Horraire.Rows[t].Delete();
                                                    dt_Horraire.AcceptChanges();

                                                    mh_mois = mh_mois - 2.5;
                                                    break;
                                                }
                                            }
                                        }
                                        else if (dt_horraire_deja_inserer.Rows.Count > 0)
                                        {
                                            if (k == dt_groupes_par_code_grpmnt.Rows.Count - 1)
                                                fois = 0;
                                            
                                            insertion_occupationlocal(etab, dt_Formateur.DefaultView[i][0].ToString(), int.Parse(dt_groupes_par_code_grpmnt.Rows[k][0].ToString()), dt_horraire_deja_inserer.Rows[fois][0].ToString(), dt_horraire_deja_inserer.Rows[fois][1].ToString(), dt_horraire_deja_inserer.Rows[fois][2].ToString(), af);
                                            if (k == dt_groupes_par_code_grpmnt.Rows.Count - 1)
                                                dt_horraire_deja_inserer.Rows[0].Delete();
                                            
                                                fois++;

                                            mh_mois = mh_mois - 2.5;
                                        }
                                        else
                                        {
                                            for (int t = 0; t <= dt_Horraire.Rows.Count - 1; t++)
                                            {
                                                //


                                                command.CommandText = "valider_si_tous_groupes_disponible_horire";
                                                command.Parameters.Clear();
                                                command.CommandType = CommandType.StoredProcedure;
                                                command.Parameters.AddWithValue("@code", dt_code_grpmnt_byformateur.Rows[j][0]);
                                                command.Parameters.AddWithValue("@mois", mois);
                                                command.Parameters.AddWithValue("@af", af);
                                                command.Parameters.AddWithValue("@jour", dt_Horraire.Rows[t][0].ToString());
                                                command.Parameters.AddWithValue("@hd", dt_Horraire.Rows[t][1].ToString());
                                                command.Parameters.AddWithValue("@hf", dt_Horraire.Rows[t][2].ToString());
                                                SqlParameter p1 = new SqlParameter();
                                                p1.Direction = ParameterDirection.ReturnValue;
                                                p1.SqlDbType = SqlDbType.Int;
                                                command.Parameters.Add(p1);
                                                
                                                command.ExecuteNonQuery();

                                                if (int.Parse(p1.Value.ToString()) == 1)
                                                {
                                                    insertion_occupationlocal(etab, dt_Formateur.DefaultView[i][0].ToString(), int.Parse(dt_groupes_par_code_grpmnt.Rows[k][0].ToString()), dt_Horraire.Rows[t][0].ToString(), dt_Horraire.Rows[t][1].ToString(), dt_Horraire.Rows[t][2].ToString(), af);
                                                    dt_Horraire.Rows[0].Delete();
                                                    dt_Horraire.AcceptChanges();

                                                    mh_mois = mh_mois - 2.5;
                                                    break;
                                                }
                                                

                                            }
                                        }
                                    }
                                }
                                else
                                    while (mh_mois >= 2)
                                    {
                                        for (int t = 0; t <= dt_Horraire.Rows.Count - 1; t++)
                                        {
                                            //


                                            command.CommandText = "valider_si_tous_groupes_disponible_horire";
                                            command.Parameters.Clear();

                                            command.CommandType = CommandType.StoredProcedure;
                                            command.Parameters.AddWithValue("@code", dt_code_grpmnt_byformateur.Rows[j][0]);
                                            command.Parameters.AddWithValue("@mois", mois);
                                            command.Parameters.AddWithValue("@af", af);
                                            command.Parameters.AddWithValue("@jour", dt_Horraire.Rows[t][0].ToString());
                                            command.Parameters.AddWithValue("@hd", dt_Horraire.Rows[t][1].ToString());
                                            command.Parameters.AddWithValue("@hf", dt_Horraire.Rows[t][2].ToString());
                                            SqlParameter p1 = new SqlParameter();
                                            p1.Direction = ParameterDirection.ReturnValue;
                                            p1.SqlDbType = SqlDbType.Int;
                                            command.Parameters.Add(p1);

                                            command.ExecuteNonQuery();

                                            if (int.Parse(p1.Value.ToString()) == 1)
                                            {
                                                insertion_occupationlocal(etab, dt_Formateur.DefaultView[i][0].ToString(), int.Parse(dt_groupes_par_code_grpmnt.Rows[k][0].ToString()), dt_Horraire.Rows[t][0].ToString(), dt_Horraire.Rows[t][1].ToString(), dt_Horraire.Rows[t][2].ToString(), af);
                                                dt_Horraire.Rows[0].Delete();
                                                dt_Horraire.AcceptChanges();

                                                mh_mois = mh_mois - 2.5;
                                                break;
                                            }
                                            
                                        }
                                    }
                            }
                        }
                        //dt_horraire_deja_inserer.Rows.Clear();
                    }
                }
                connection.Close();
            }
        }
    }

    public void insertion_occupationlocal(string etab, string Form, int grp, string jour, string hd, string hf, string af)
    {
        int local;
        //command = new SqlCommand();
        command.CommandText = string.Format("select id from local where etablissement='{0}' and type='FAD'", etab);
        command.CommandType = CommandType.Text;
        local = int.Parse(command.ExecuteScalar().ToString());


        command.CommandText = string.Format("insert into OccupationLocal values({0},'{1}',{2},{3},'{4}','{5}','{6}',{7},'{8}')", local, Form, grp, cbx_Mois.SelectedIndex + 1, jour, hd, hf, "GETDATE()", af);
        command.ExecuteNonQuery();
    }
}


