using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoIP_Server
{
    class DatabaseConn
    {
        string connectionString;
        public DatabaseConn()
        {
            connectionString = "Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename=" + Directory.GetCurrentDirectory() + "\\Database1.mdf; Integrated Security = True";
        }
        ~DatabaseConn()
        {
        }

        public void editUserPassword(string login, string newPassword)
        {
            //string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Piotrek\\source\\repos\\MemoryServer3\\MemoryServer\\MemoryDatabase.mdf;Integrated Security=True";
            string queryString = "UPDATE dbo.Table SET password = @password WHERE login=@login";

            if (newPassword.Length > 15 && newPassword.Length < 5)
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand update = new SqlCommand(queryString, conn))
                {
                    update.Parameters.AddWithValue("@login", login);
                    update.Parameters.AddWithValue("@password", newPassword);
                    conn.Open();
                    update.ExecuteNonQuery();
                }
            }
        }

        public bool checkUserData(string login, string password)
        {
            //string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Piotrek\\source\\repos\\MemoryServer3\\MemoryServer\\MemoryDatabase.mdf;Integrated Security=True";

            SqlConnection conn = new SqlConnection(connectionString);
            SqlDataAdapter sda = new SqlDataAdapter("SELECT COUNT(*) FROM dbo.Table WHERE login= '" + login + "' AND password= '" + password + "'", conn);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            if (dt.Rows[0][0].ToString() == "1")
            {
                return true;
            }
            else return false;
        }

        public bool registerUser(string login, string password)
        {
            //string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Piotrek\\source\\repos\\MemoryServer3\\MemoryServer\\MemoryDatabase.mdf;Integrated Security=True";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string queryString = "INSERT INTO dbo.Table (login, password) VALUES (@Login, @Password);";
                string loginCheck = "SELECT COUNT(*) from dbo.Table WHERE login= @Login";

                if (login.Length > 15 && login.Length < 3)
                {
                    return false;
                }
                if (password.Length > 15 && password.Length < 5)
                {
                    return false;
                }

                using (SqlCommand check = new SqlCommand(loginCheck, conn))
                {
                    check.Parameters.AddWithValue("@Login", login);
                    conn.Open();
                    int result = (int)check.ExecuteScalar();
                    if (result == 0)
                    {
                        using (SqlCommand register = new SqlCommand(queryString, conn))
                        {
                            try
                            {
                                register.Parameters.AddWithValue("@Login", login);
                                register.Parameters.AddWithValue("@Password", password);
                                register.ExecuteNonQuery();
                                System.Console.WriteLine("registered " + login);
                                return true;
                            }
                            catch (Exception ex)
                            {
                                Console.Write(ex);
                                return false;
                            }
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("login already exists");
                        return false;
                    }
                }

            }
        }

        public bool deleteUser(string login, string password)
        {

            if (checkUserData(login, password) == true)
            {
                string removeString = "DELETE FROM dbo.Table WHERE login= @Login";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand delete = new SqlCommand(removeString, conn))
                    {
                        try
                        {
                            conn.Open();
                            delete.Parameters.AddWithValue("@Login", login);
                            delete.ExecuteNonQuery();
                            System.Console.WriteLine("User " + login + " has been succesfully deleted");
                            return true;
                        }
                        catch (Exception exc)
                        {
                            Console.Write(exc);
                            return false;
                        }
                    }

                }

            }
            else return false;
        }
    }
}
