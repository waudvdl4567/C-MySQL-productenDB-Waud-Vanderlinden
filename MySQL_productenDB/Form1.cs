using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MySQL_productenDB
{
    public partial class Form1 : Form
    {
        ConnectionStringSettingsCollection connectionStringSettings = new ConnectionStringSettingsCollection();
        Dictionary<string, string> connStringsDict = new Dictionary<string, string>();

        //MySQL Connectie variabelen
        string mySqlConnStr = null;
        MySqlConnection mySqlConn;
        MySqlCommand mySqlComm;
        DataTable myDatatable;
        MySqlDataAdapter myAdapter;


        public Form1()
        {
            InitializeComponent();
            connectionStringSettings = GetConnectionStrings();
            connStringsDict = UpdateConnectionsComboBox(serversComboBox, connectionStringSettings);
            mySqlConnStr = serversComboBox.SelectedValue.ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private Dictionary<string, string> UpdateConnectionsComboBox(ComboBox cb, ConnectionStringSettingsCollection cssc)
        {
            Dictionary<string, string> csd = new Dictionary<string, string>();

            if (cssc != null)
            {
                foreach (ConnectionStringSettings cs in cssc)
                {
                    csd.Add(cs.Name, cs.ConnectionString);
                }

                cb.DataSource = new BindingSource(csd, null);
                cb.DisplayMember = "Key";
                cb.ValueMember = "Value";
            }
            else
            {
                cb.Enabled = false;
            }
            return csd;
        }

        private ConnectionStringSettingsCollection GetConnectionStrings()
        {
            ConnectionStringSettingsCollection settings = new ConnectionStringSettingsCollection();

            try
            {
                settings = ConfigurationManager.ConnectionStrings;
            }
            catch (ConfigurationErrorsException err)
            {
                MessageBox.Show(err.Message, "Configuratie", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return settings;
        }

        private void openConnectionButton_Click(object sender, EventArgs e)
        {
            try
            {
                mySqlConn = OpenMySQLverbinding(mySqlConnStr);

                try
                {
                    if (mySqlConn.State == ConnectionState.Open)
                    {
                        MessageBox.Show("Verbinding met database gemaakt", "DATABASE VERBINING");
                    }
                    else
                    {
                        MessageBox.Show("Geen verbinding met database", "DATABASE VERBINING");
                    }
                }
                catch (NullReferenceException err)
                {
                    MessageBox.Show(err.Message, "MySQL Connectie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (MySqlException)
            {
                
                MessageBox.Show("fout bij het verbinden met database", "DATABASE VERBINING");
            }

        }

        private MySqlConnection OpenMySQLverbinding(string connectieString)
        {
            MySqlConnection mijnVerbinding = null;


            try
            {
                mijnVerbinding = new MySqlConnection(connectieString);

                try
                {
                    mijnVerbinding.Open();
                }
                catch (MySqlException)
                {
                    MessageBox.Show("Fout bij het maken van verbinding met database", "MySQL Connectie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (mijnVerbinding != null)
                    {
                        mijnVerbinding.Dispose();
                    }
                }
            }
            catch (ArgumentException err)
            {
                MessageBox.Show(err.Message, "SQL-verbinding", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (mijnVerbinding != null)
                {
                    mijnVerbinding.Dispose();
                }
            }

            return mijnVerbinding;
        }

        private void serversComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            mySqlConnStr = serversComboBox.SelectedValue.ToString();
            String[] strlist = mySqlConnStr.Split(';');
            string str = string.Empty;

            foreach (String s in strlist)
            {
                if (!s.Contains("password"))
                {
                    str += s + " , ";
                }
            }
        }

        private bool SluitMySQLverbinding(MySqlConnection mijnVerbinding)
        {
            bool succes = false;

            if (mijnVerbinding.State == ConnectionState.Open)
            {
                try
                {
                    mijnVerbinding.Close();
                    succes = true;
                }
                catch (MySqlException)
                {
                    MessageBox.Show("Fout bij het sluiten van verbinding met database", "MySQL Connectie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Geen open SQL-verbinding", "MySQL Connectie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return succes;
        }

        private void closeConnectionButton_Click(object sender, EventArgs e)
        {
            if (SluitMySQLverbinding(mySqlConn))
            {
               
                MessageBox.Show("Verbinding met database gesloten", "DATABASE VERBINDING");
                updateButtonsOnCloseConnection();
            }
            else
            {
                
                MessageBox.Show("Verbinding kon niet worden verbroken", "DATABASE VERBINDING");
            }
        }
        private void updateButtonsOnCloseConnection()
        {
            openConnectionButton.Enabled = true;
            closeConnectionButton.Enabled = false;
            //lblSqlVerbindingsStatus.Visible = false;
        }

        private void readTableOrders_Click(object sender, EventArgs e)
        {
            //Dictionary<int, string> csd = new Dictionary<int, string>();
            try
            {
                using (mySqlConn = OpenMySQLverbinding(mySqlConnStr))
                {
                    if (mySqlConn.State == ConnectionState.Open)
                    {
                        //LogtekstToevoegen(tbLoggingText, "verbinding met database gemaakt. Start lezen data...");

                        mySqlComm = new MySqlCommand();
                        mySqlComm.Connection = mySqlConn;
                        mySqlComm.CommandText = "select * from orders;";
                        mySqlComm.CommandType = CommandType.Text;


                        using (MySqlDataReader mySqlDr = mySqlComm.ExecuteReader())
                        {
                            while (mySqlDr.Read())
                            {
                                printTextBox.Text += (int)mySqlDr[0] + "\t"
                                    + Convert.ToString((DateTime)mySqlDr[1]) + "\t"
                                    + (int)mySqlDr[2] + "\t"
                                    + Convert.ToByte(mySqlDr[3]) + "\t"
                                    + (int)mySqlDr[4] + "\t"
                                    + mySqlDr[5] + "\r\n";
                            }

                            mySqlDr.Close();

                        }
                        readTableOrders.Enabled = true;
                    }
                    else
                    {
                        //LogtekstToevoegen(tbLoggingText, "Geen verbinding met database");
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is NullReferenceException || ex is MySqlException)
                {
                    MessageBox.Show(ex.Message, "MySQL Connectie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else throw;
            }
        }

        private void addProductButton_Click(object sender, EventArgs e)
        {
            using (mySqlConn = OpenMySQLverbinding(mySqlConnStr))
            {
                if (ConnectionState.Open == mySqlConn.State)
                {
                    using (mySqlComm = new MySqlCommand())
                    {
                        mySqlComm = new MySqlCommand();
                        mySqlComm.Connection = mySqlConn;
                        mySqlComm.CommandText = "insert into producten(productNaam, productStock)" +
                            "values" +
                            "(@productNaam, @productStock);";
                        mySqlComm.Parameters.AddWithValue("@productNaam", productNaamTextBox.Text);
                        mySqlComm.Parameters.AddWithValue("@productStock", productStockTextBox.Text);
                        mySqlComm.CommandType = CommandType.Text;
                        mySqlComm.ExecuteNonQuery();
                        //connectionStatusLabel.Text = "Product is toegevoegd!"
                    }
                }
            }
        }

        //private void readOrdersButton_Click(object sender, EventArgs e)
        //{
        //    DataGridViewSelectedRowCollection geselecteerdeRecords = dataGridView.SelectedRows;

        //    try
        //    {
        //        using (mySqlConn = OpenMySQLverbinding(mySqlConnStr))
        //        {
        //            if (mySqlConn.State == ConnectionState.Open)
        //            {
        //                //LogtekstToevoegen(tbLoggingText, "verbinding met database gemaakt. Start verwijderen data...");

        //                foreach (DataGridViewRow r in geselecteerdeRecords)
        //                {
        //                    int selectedPersoon_VaardigheidID = (int)r.Cells[0].Value;
        //                    DialogResult dialogResult = MessageBox.Show("Record met persoon_vaardigheidID = '" + selectedPersoon_VaardigheidID + "' verwijderen?", "Record verwijderen", MessageBoxButtons.YesNo);

        //                    if (dialogResult == DialogResult.Yes)
        //                    {

        //                        using (mySqlComm = new MySqlCommand())
        //                        {
        //                            mySqlComm = new MySqlCommand();
        //                            mySqlComm.Connection = mySqlConn;
        //                            mySqlComm.CommandText = "DELETE FROM vaardigheden_personen WHERE persoon_vaardigheidID = @vaardigheidID;";
        //                            mySqlComm.Parameters.AddWithValue("@vaardigheidID", selectedPersoon_VaardigheidID);
        //                            mySqlComm.CommandType = CommandType.Text;
        //                            mySqlComm.ExecuteNonQuery();
        //                            //LogtekstToevoegen(tbLoggingText, "Record " + selectedPersoon_VaardigheidID + " is verwijderd uit tabel vaardigheid_personen.");

        //                        }
        //                    }
        //                    else if (dialogResult == DialogResult.No)
        //                    {
        //                        //doe iets anders...nu niets dus...
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                MessageBox.Show("Geen connectie met database ", "MySQL Connectie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            }
        //        }

        //        LeesData();
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is NullReferenceException || ex is MySqlException)
        //        {
        //            MessageBox.Show(ex.Message, "MySQL Connectie", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //        else throw;
        //    }
        //}

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection geselecteerdeRecords = dataGridView.SelectedRows;

            deleteProductButton.Enabled = (geselecteerdeRecords.Count > 0) ? true : false;

            foreach (DataGridViewRow r in geselecteerdeRecords)
            {
                Console.WriteLine("geselecteerde 'persoon_vaardigheidID' = " + r.Cells[0].Value + " , ");
            }
        }

        //private void LeesData()
        //{
        //    dataGridView.SelectionChanged -= dataGridView_SelectionChanged;
        //    myDatatable = new DataTable();

        //    try
        //    {
        //        using (mySqlConn = OpenMySQLverbinding(mySqlConnStr))
        //        {
        //            if (mySqlConn.State == ConnectionState.Open)
        //            {
        //                //LogtekstToevoegen(tbLoggingText, "verbinding met database gemaakt. Start lezen data...");

        //                using (mySqlComm = new MySqlCommand())
        //                {
        //                    //mySqlComm.Connection = mySqlConn;
        //                    //mySqlComm.CommandText = "GetPersonenMetVaardigheden";
        //                    //mySqlComm.CommandType = CommandType.StoredProcedure;

        //                    //mySqlComm.Parameters.Add(new MySqlParameter("vaardigheid", MySqlDbType.VarChar));
        //                    //mySqlComm.Parameters["vaardigheid"].Value = cmbVaardigheden.Text;
        //                    //mySqlComm.Parameters["vaardigheid"].Direction = ParameterDirection.Input;

        //                    //LogtekstToevoegen(tbLoggingText, "Zoeken naar vaardigheid : " + cmbVaardigheden.Text);

        //                    myDatatable.Load(mySqlComm.ExecuteReader());
        //                    dataGridView.DataSource = myDatatable;
        //                    //LogtekstToevoegen(tbLoggingText, "Gegevens uit database geladen. " + myDatatable.Rows.Count + " rijen gevonden");
        //                }
        //            }
        //            else
        //            {
        //                //LogtekstToevoegen(tbLoggingText, "Geen verbinding met database");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is NullReferenceException || ex is MySqlException)
        //        {
        //            MessageBox.Show(ex.Message, "MySQL Connectie", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //        else throw;
        //    }

        //    dataGridView.ClearSelection();
        //    dataGridView.SelectionChanged += dataGridView_SelectionChanged;
        //}







        private void readOrdersButton_Click(object sender, EventArgs e)
        {
            InlezenEnWegSchrijvenInTabel();
        }
        
        private void InlezenEnWegSchrijvenInTabel()
        {
            try
            {
                using (mySqlConn = OpenMySQLverbinding(mySqlConnStr))
                {
                    
                    dataGridView.ColumnCount = 4;
                    dataGridView.Columns[0].Name = "productID";
                    dataGridView.Columns[1].Name = "productNaam";
                    dataGridView.Columns[2].Name = "productStock";
                    dataGridView.Columns[3].Name = "Beschikbaarheid";
                    dataGridView.Rows.Clear();
                    if (mySqlConn.State == ConnectionState.Open)
                    {
                        mySqlComm = new MySqlCommand();
                        mySqlComm.Connection = mySqlConn;
                        mySqlComm.CommandText = "select * from producten;";
                        mySqlComm.CommandType = CommandType.Text;

                        //SetConnectionLabelTextAndColor("VERBONDEN", Color.Green);

                        using (MySqlDataReader mySqlDr = mySqlComm.ExecuteReader())
                        {
                            while (mySqlDr.Read())
                            {
                                dataGridView.Rows.Add(mySqlDr[0].ToString(), mySqlDr[1].ToString(), mySqlDr[2].ToString(), mySqlDr[3].ToString());
                            }

                            mySqlDr.Close();
                        }
                        dataGridView.Visible = true;
                        mySqlConn.Close();
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is NullReferenceException || ex is MySqlException)
                {
                    MessageBox.Show(ex.Message, "MySQL Connectie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else throw;
            }
        }

        private void deleteProductButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 1)
            {
                int productID = Convert.ToInt32(dataGridView.SelectedRows[0].Cells["ProductIDCol"].Value);

                DialogResult boxResult = MessageBox.Show("WILT U HET PRODUCT:" + Convert.ToString(dataGridView.SelectedRows[0].Cells["ProductNaamCol"].Value).ToUpper() + " VERWIJDEREN?", "INFO", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (boxResult == DialogResult.Yes)
                {
                    dataGridView.Rows.Clear();
                    try
                    {
                        using (mySqlConn = OpenMySQLverbinding(mySqlConnStr))
                        {
                            if (mySqlConn.State == ConnectionState.Open)
                            {
                                using (mySqlComm = new MySqlCommand())
                                {
                                    mySqlComm.Connection = mySqlConn;
                                    mySqlComm.Parameters.AddWithValue("@productID", productID);
                                    mySqlComm.CommandText = "DELETE FROM producten WHERE productID = @productID";
                                    mySqlComm.CommandType = CommandType.Text;

                                    mySqlComm.ExecuteNonQuery();

                                    mySqlConn.Close();
                                    InlezenEnWegSchrijvenInTabel();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Geen connectie met database ", "MySQL Connectie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is NullReferenceException || ex is MySqlException)
                        {
                            MessageBox.Show(ex.Message, "MySQL Connectie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else throw;
                    }
                }
            }
            else
                if (dataGridView.SelectedRows.Count == 0)
                MessageBox.Show("U MOET EERST EEN RIJ SELECTEREN", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                if (dataGridView.SelectedRows.Count > 1)
                MessageBox.Show("VERWIJDER RIJ PER RIJ", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
