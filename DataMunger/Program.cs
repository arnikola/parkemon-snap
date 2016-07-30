using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;

namespace DataMunger
{
    class Program
    {
        static void Main(string[] args)
        {
            const string DatabaseFileName = "parks.db";
            SQLiteConnection.CreateFile(DatabaseFileName);
            var connection = new SQLiteConnection($"Data Source={DatabaseFileName};Version=3");
            connection.Open();

            using (var transaction = connection.BeginTransaction())
            {
                new SQLiteCommand(
                    @"
create table points (
    lat NUMERIC,
    lon NUMERIC,
    id INTEGER
);

create table parks (
    name    text,
    area    numeric,
    area_type text
);

CREATE INDEX idx_points_lat ON points(lat);
CREATE INDEX idx_points_lon ON points(lon);
", connection, transaction).ExecuteNonQuery();

                var features = JsonConvert.DeserializeObject<FeatureCollection>(File.ReadAllText("Reserves.geojson"));
                var x = features.Features.Count;
                var insertPark =
                new SQLiteCommand(
                    "insert into parks (name, area, area_type) values(@name, @area, @area_type)",
                    connection,
                    transaction);
                insertPark.Prepare();
                var insertPoint =
                new SQLiteCommand(
                    "insert into points (lat, lon, id) values(@lat, @lon, @id)",
                    connection,
                    transaction);
                insertPoint.Prepare();
                foreach (var feature in features.Features)
                {
                    insertPark.Parameters.AddWithValue("name", feature.Properties["NAME"] as string);
                    insertPark.Parameters.AddWithValue("area", feature.Properties["Shape_Area"] as double?);
                    insertPark.Parameters.AddWithValue("area_type", feature.Properties["AREA_TYPE"] as string);
                    insertPark.ExecuteNonQuery();
                    var id = connection.LastInsertRowId;
                    foreach (var point in ((MultiPolygon) feature.Geometry).Coordinates.SelectMany(_=>_.Coordinates).SelectMany(_=>_.Coordinates).Cast<GeographicPosition>())
                    {
                        insertPoint.Parameters.AddWithValue("lat", point.Latitude);
                        insertPoint.Parameters.AddWithValue("lon", point.Longitude);
                        insertPoint.Parameters.AddWithValue("id", id);
                        insertPoint.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
        }
    }
}
