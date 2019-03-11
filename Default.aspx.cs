using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Newtonsoft.Json;


public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void btnStart_Click(object sender, EventArgs e)
    {
        string rh = "";
        string nome = "";
        string endereco = "";
        string numero = "";
        string bairro = "";
        string cidade = "";
        string uf = "";
        string cep = "";
        double latitude = 0.0;
        double longitude = 0.0;

        try
        {

            using (SqlConnection cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionStringIsolamento"].ToString()))
            {
                SqlCommand cmm = cnn.CreateCommand();
                cmm.CommandText = "SELECT [rh],[nome],[dt_nasc],[sexo],[obito],[dtobito] FROM [Isolamento].[dbo].[Paciente] ";
                cnn.Open();
                SqlDataReader dr = cmm.ExecuteReader();

                while (dr.Read())
                {

                    rh = dr.GetInt32(0).ToString();

                    using (OdbcConnection cnn2 = new OdbcConnection(ConfigurationManager.ConnectionStrings["HospubConn"].ToString()))
                    {

                        OdbcCommand cmm2 = cnn2.CreateCommand();
                        cmm2.CommandText = "SELECT ib6regist, concat(ib6pnome,ib6compos) as nome, ib6lograd, ib6numero, ib6bairro, ib6municip, ib6uf, ib6cep FROM intb6 WHERE ib6regist =" + rh;
                        cnn2.Open();
                        OdbcDataReader dr2 = cmm2.ExecuteReader();

                        while (dr2.Read())
                        {
                            nome = dr2.GetString(1);
                            endereco = dr2.GetString(2);
                            numero = dr2.GetString(3).TrimStart('0');
                            bairro = dr2.GetString(4);
                            // Encontrar a cidade através do seu código
                            cidade = encontrarCidade(dr2.GetString(5));
                            uf = dr2.GetString(6);
                            cep = dr2.GetString(7);
                            if (cep.Equals("99999999"))
                            {
                                //Procurar o CEP no Google e a latitude e a longitude
                                using (var webClient = new WebClient())
                                {
                                    string rawJason =webClient.DownloadString( "https://maps.google.com/maps/api/geocode/json?address=" + endereco + "," + numero + "," + cidade + "," + uf + "components=country:BR%20&key=AIzaSyBOo3iqhE-w8xZaVC-PGJfvk8Rrx51suVg");
                                    // string url = @"https://maps.google.com/maps/api/geocode/json?address=Rua+Tenente+%20Galdino%20+%20Pinheiro%20+%20Franco,264,+Mogi+das%20+%20Cruzes%20,SP%20&components=country:BR%20&key=AIzaSyBOo3iqhE-w8xZaVC-PGJfvk8Rrx51suVg";

                                   

                                    RootObject root = JsonConvert.DeserializeObject<RootObject>(rawJason);
                                    foreach (var item in root.results)
                                    {
                                        cep = item.address_components[6].short_name;
                                        latitude = item.geometry.location.lat;
                                        longitude = item.geometry.location.lng;
                                    }
                                  
                                }
                            }
                            else
                            {
                                //Procurar o CEP no Google e a latitude e a longitude
                                using (var webClient = new WebClient())
                                {
                                    string rawJason = webClient.DownloadString("https://maps.google.com/maps/api/geocode/json?address=" + endereco + "," + numero + "," + cidade + "," + uf + "components=country:BR%20&key=AIzaSyBOo3iqhE-w8xZaVC-PGJfvk8Rrx51suVg");
                                    // string url = @"https://maps.google.com/maps/api/geocode/json?address=Rua+Tenente+%20Galdino%20+%20Pinheiro%20+%20Franco,264,+Mogi+das%20+%20Cruzes%20,SP%20&components=country:BR%20&key=AIzaSyBOo3iqhE-w8xZaVC-PGJfvk8Rrx51suVg";



                                    RootObject root = JsonConvert.DeserializeObject<RootObject>(rawJason);
                                    foreach (var item in root.results)
                                    {
                                        latitude = item.geometry.location.lat;
                                        longitude = item.geometry.location.lng;
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }
        catch(Exception erro)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Atenção! Erro encontrado: " + erro.Message + " ');", true);
        }
    }

    public string encontrarCidade( string cod)
    {
        string cidade= "";
        string  pCod = cod.Substring(0, 2);
        string sCod = cod.Substring(2, 5);

        try
        {

            using (OdbcConnection cnn2 = new OdbcConnection(ConfigurationManager.ConnectionStrings["HospubConn"].ToString()))
            {

                OdbcCommand cmm2 = cnn2.CreateCommand();
                cmm2.CommandText = "SELECT id4descricao FROM intd4 WHERE id4uf = '" + pCod + "' and id4compos='" + sCod + "'";
                cnn2.Open();
                OdbcDataReader dr2 = cmm2.ExecuteReader();

                if (dr2.Read())
                {
                    cidade = dr2.GetString(0);
                }
                dr2.Close();
            }
        }
        catch(Exception  erro)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Atenção! Erro encontrado no código da cidade: " + erro.Message + " ');", true);
            

        }
        return cidade;
    }
}