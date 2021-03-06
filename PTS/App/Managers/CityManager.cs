﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using MySql.Data.MySqlClient;
using PTS.App.DataBase;
using PTS.App.Objects;

namespace PTS.App.Managers
{
    public class CityManager
    {
        public City GetCity(string cityName, string cityZIP)
        {
            //Create a request to get the city
            using MySqlCommand rqst = DataBaseManager.Connection.CreateCommand();

            //Fill the request
            rqst.CommandText = "SELECT ville_nom_reel, " +
            "ville_code_postal, " +
            "ville_longitude_deg, " +
            "ville_latitude_deg " +
            "FROM `villes_france_free` " +
            $"WHERE UPPER(ville_nom_simple) = UPPER('{ cityName.Replace('\'', '-') }')" +
            $"AND ville_code_postal = '{ cityZIP }'";

            City city = null;

            //Running the request  
            using (DbDataReader reader = rqst.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    //Count the number of records get by the request
                    //should be equal to 1
                    int rowCount = 1;
                    while (reader.Read())
                    {
                        //If rowCount >1
                        if (rowCount > 1)
                        {
                            //Free ressources
                            rqst.DisposeAsync();
                            reader.DisposeAsync();

                            //Raise error
                            throw new NotImplementedException("Too much records returned for the same City name and Zip code");
                        }

                        int cityNameOrd = reader.GetOrdinal("ville_nom_simple");
                        int longitudeOrd = reader.GetOrdinal("ville_nom_simple");
                        int latitudeOrd = reader.GetOrdinal("ville_nom_simple");
                        int zipOrd = reader.GetOrdinal("ville_code_postal");

                        string name = reader.GetString(cityNameOrd);
                        double longitude = reader.GetDouble(longitudeOrd);
                        double latitude = reader.GetDouble(latitudeOrd);
                        string cityZip = reader.GetString(zipOrd);

                        city = new City(name, cityZip, longitude, latitude);

                        rowCount++;
                    }
                }
            }

            return city;
        }

        public List<City> GetCities(Dictionary<string, string> cities)
        {
            //Create a request to get the cities
            using MySqlCommand rqst = DataBaseManager.Connection.CreateCommand();

            //Fill the request
            rqst.CommandText = "SELECT ville_nom_reel, " +
                "ville_code_postal, " +
                "ville_longitude_deg, " +
                "ville_latitude_deg " +
                "FROM `villes_france_free` " +
                "WHERE";

            foreach (var item in cities)
            {
                string where = $"(ville_code_postal LIKE '%{item.Key.Split('.')[0]}%' " +
                    $"AND UPPER(ville_nom_reel) = UPPER('{item.Value.Replace('\'','-')}')) " +
                    $"OR";

                rqst.CommandText += " " + where;
            }

            //Remove the last ,
            rqst.CommandText = rqst.CommandText.Remove(rqst.CommandText.Length - 2);

            //End the request
            rqst.CommandText += ";";

            //Create the list of cities
            List<City> citiesList = new List<City>();

            //Running the request  
            using (DbDataReader reader = rqst.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {

                        int cityNameOrd = reader.GetOrdinal("ville_nom_reel");
                        int longitudeOrd = reader.GetOrdinal("ville_longitude_deg");
                        int latitudeOrd = reader.GetOrdinal("ville_latitude_deg");
                        int zipOrd = reader.GetOrdinal("ville_code_postal");
                        
                        string name = reader.GetString(cityNameOrd);
                        double longitude = reader.GetDouble(longitudeOrd);
                        double latitude = reader.GetDouble(latitudeOrd);
                        string cityZip = reader.GetString(zipOrd);

                        citiesList.Add(new City(name, cityZip, longitude, latitude));
                    }
                }
            }
            return citiesList;
        }

        public List<City> GetAllCitiesName()
        {
            //Create a request to get the cities
            using MySqlCommand rqst = DataBaseManager.Connection.CreateCommand();

            //Fill the request
            rqst.CommandText = "SELECT ville_nom_reel, " +
                "ville_code_postal " +
                "FROM `villes_france_free`;";


            //Create the list of cities
            List<City> citiesList = new List<City>();

            //Running the request  
            using (DbDataReader reader = rqst.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {

                        int cityNameOrd = reader.GetOrdinal("ville_nom_reel");
                        int zipOrd = reader.GetOrdinal("ville_code_postal");

                        string cityName = reader.GetString(cityNameOrd);
                        string cityZip = reader.GetString(zipOrd);

                        citiesList.Add(new City(cityName, cityZip));
                    }
                }
            }
            return citiesList;
        }

        public List<City> GetCitiesName(string text)
        {
            //using MySqlConnection connection = DataBaseManager.GetNewConnection();
            //Create a request to get the cities
            using MySqlCommand rqst = DataBaseManager.Connection.CreateCommand();

            //Fill the request
            rqst.CommandText = "SELECT ville_nom_reel, " +
                "ville_code_postal " +
                "FROM `villes_france_free` " +
                "WHERE UPPER(CONCAT(ville_nom_reel,' (',ville_code_postal,')')) LIKE UPPER('%" + text + "%')" +
                "LIMIT 5";


            //Create the list of cities
            List<City> citiesList = new List<City>();
            //Running the request
            bool exception = false;
            do
            {
                try
                {
                    exception = false;
                                       
                    using (DbDataReader reader = rqst.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {

                                int cityNameOrd = reader.GetOrdinal("ville_nom_reel");
                                int zipOrd = reader.GetOrdinal("ville_code_postal");

                                string cityName = reader.GetString(cityNameOrd);
                                string cityZip = reader.GetString(zipOrd);

                                citiesList.Add(new City(cityName, cityZip));
                            }
                        }

                    }
                }
                catch (MySql.Data.MySqlClient.MySqlException e)
                {
                    //Exception thrown when mutiple request at the same time are sent 
                    //so we wait until the request are processed
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    exception = true;
                }

            } while (exception);

            return citiesList;
        }

        public static Dictionary<string, string> GetCitiesNumber(int number)
        {
            //Create a request to get the cities
            using MySqlCommand rqst = DataBaseManager.Connection.CreateCommand();

            //Fill the request

            rqst.CommandText = "SELECT SQL_NO_CACHE ville_nom_reel, " +
                "ville_code_postal FROM villes_france_free " +
                "WHERE ville_code_postal in (SELECT ville_code_postal FROM villes_france_free " +
                "group by ville_code_postal " +
                "HAVING count(*) = 1) AND RAND() > 0.9 ORDER BY RAND() LIMIT " + number + ";";
            //Create the dictonnary of cities
            Dictionary<string, string> citiesList = new Dictionary<string, string>();

            //Running the request  
            using (DbDataReader reader = rqst.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {

                        int cityNameOrd = reader.GetOrdinal("ville_nom_reel");
                        int zipOrd = reader.GetOrdinal("ville_code_postal");

                        string cityName = reader.GetString(cityNameOrd);
                        string cityZip = reader.GetString(zipOrd);

                        citiesList.Add(cityZip,cityName);
                    }
                }
            }
            return citiesList;
        }
    }
}
