using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.SqlClient;

namespace Woodic.Modelo
{
    public static class DatabaseHelper
    {
        private static string? _connectionString;
        private static readonly object _lock = new();

        public static string GetDatabaseFilePath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, "WoodicData.mdf");
        }

        public static string GetConnectionString()
        {
            if (_connectionString != null) return _connectionString;

            lock (_lock)
            {
                if (_connectionString != null) return _connectionString;

                string mdfPath = GetDatabaseFilePath();
                string dir = Path.GetDirectoryName(mdfPath) ?? AppDomain.CurrentDomain.BaseDirectory;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                EnsureDatabaseCreated(mdfPath);

                _connectionString = $"Server=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={mdfPath};Database=WoodicDb;Integrated Security=True;Connect Timeout=30;TrustServerCertificate=True;";
                return _connectionString;
            }
        }

        public static SqlConnection CreateConnection()
        {
            string connStr = GetConnectionString();
            return new SqlConnection(connStr);
        }

        private static void EnsureDatabaseCreated(string mdfPath)
        {
            if (!File.Exists(mdfPath))
            {
                try
                {
                    string dir = Path.GetDirectoryName(mdfPath) ?? AppDomain.CurrentDomain.BaseDirectory;
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    string logPath = Path.ChangeExtension(mdfPath, "_log.ldf");

                    // Conectar a master en LocalDB para crear el archivo .mdf
                    string masterConnStr = "Server=(LocalDB)\\MSSQLLocalDB;Database=master;Integrated Security=True;Connect Timeout=30;TrustServerCertificate=True;";
                    using (var masterConn = new SqlConnection(masterConnStr))
                    {
                        masterConn.Open();

                        // Verificar si ya existe una base catalogada con ese nombre para desasociarla
                        string checkSql = @"
                            IF DB_ID('WoodicDb') IS NOT NULL
                            BEGIN
                                ALTER DATABASE [WoodicDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                DROP DATABASE [WoodicDb];
                            END";
                        try
                        {
                            using var checkCmd = new SqlCommand(checkSql, masterConn);
                            checkCmd.ExecuteNonQuery();
                        }
                        catch { }

                        string createDbSql = $@"
                            CREATE DATABASE [WoodicDb] ON 
                            (NAME = N'WoodicDb_Data', FILENAME = N'{mdfPath}')
                            LOG ON 
                            (NAME = N'WoodicDb_Log', FILENAME = N'{logPath}')";

                        using (var createCmd = new SqlCommand(createDbSql, masterConn))
                        {
                            createCmd.ExecuteNonQuery();
                        }

                        // Desasociar para permitir que AttachDbFilename se encargue
                        try
                        {
                            string detachSql = "ALTER DATABASE [WoodicDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; EXEC sp_detach_db 'WoodicDb';";
                            using var detachCmd = new SqlCommand(detachSql, masterConn);
                            detachCmd.ExecuteNonQuery();
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al crear MDF en LocalDB: {ex.Message}");
                }
            }
        }

        public static void InitializeSchema(string? mdfPath = null)
        {
            mdfPath ??= GetDatabaseFilePath();
            EnsureDatabaseCreated(mdfPath);

            string connStr = $"Server=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={mdfPath};Database=WoodicDb;Integrated Security=True;Connect Timeout=30;TrustServerCertificate=True;";

            using var conn = new SqlConnection(connStr);
            conn.Open();

            string schemaSql = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'cliente')
                BEGIN
                    CREATE TABLE cliente (
                        CONTACTO BIGINT NOT NULL PRIMARY KEY,
                        NOMBRE NVARCHAR(100) NULL,
                        DIRECCION NVARCHAR(200) NULL
                    );
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'placa')
                BEGIN
                    CREATE TABLE placa (
                        idPLACA INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        LINEA NVARCHAR(50) NULL,
                        COMPUESTO NVARCHAR(50) NULL,
                        COLOR NVARCHAR(50) NULL,
                        BETA BIT NULL,
                        PRECIOPLAC DECIMAL(10,2) NULL,
                        PROVEEDOR NVARCHAR(100) NULL,
                        ANCHO INT DEFAULT 1830,
                        LARGO INT DEFAULT 2400
                    );
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'pedido')
                BEGIN
                    CREATE TABLE pedido (
                        idPEDIDO INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        CANTIDADMODULOS INT NOT NULL,
                        PRECIO DECIMAL(10,2) NULL,
                        placa_idPLACA INT NOT NULL,
                        cliente_CONTACTO BIGINT NOT NULL,
                        FECHA DATETIME DEFAULT GETDATE(),
                        CONSTRAINT FK_pedido_cliente FOREIGN KEY (cliente_CONTACTO) REFERENCES cliente (CONTACTO) ON DELETE CASCADE,
                        CONSTRAINT FK_pedido_placa FOREIGN KEY (placa_idPLACA) REFERENCES placa (idPLACA)
                    );
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'modulo')
                BEGIN
                    CREATE TABLE modulo (
                        idMODULO INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        ANCHO INT NOT NULL,
                        ALTO INT NOT NULL,
                        PROFUNDO INT NOT NULL,
                        pedido_idPEDIDO INT NOT NULL,
                        DESCRIPCION NVARCHAR(100) NULL,
                        CONSTRAINT FK_modulo_pedido FOREIGN KEY (pedido_idPEDIDO) REFERENCES pedido (idPEDIDO) ON DELETE CASCADE
                    );
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'componente')
                BEGIN
                    CREATE TABLE componente (
                        idCOMPONENTE INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        NOMBRECOMPONENTE NVARCHAR(50) NULL,
                        ANCHOCOMP INT NULL,
                        LARGOCOMP INT NULL,
                        CANTIDADCOMP INT NULL,
                        modulo_idMODULO INT NOT NULL,
                        CONSTRAINT FK_componente_modulo FOREIGN KEY (modulo_idMODULO) REFERENCES modulo (idMODULO) ON DELETE CASCADE
                    );
                END;
            ";

            using (var cmd = new SqlCommand(schemaSql, conn))
            {
                cmd.ExecuteNonQuery();
            }

            // Sembrar placas iniciales si la tabla está vacía
            using (var countCmd = new SqlCommand("SELECT COUNT(*) FROM placa", conn))
            {
                int count = Convert.ToInt32(countCmd.ExecuteScalar());
                if (count == 0)
                {
                    SeedPlacas(conn);
                }
            }
        }

        public static void SeedPlacas(SqlConnection conn)
        {
            string insertSql = @"
                INSERT INTO placa (LINEA, COMPUESTO, COLOR, BETA, PRECIOPLAC, PROVEEDOR, ANCHO, LARGO) VALUES
                ('Clásica', 'Aglomerado', 'Blanco', 0, 22000.00, 'Faplac', 1830, 2400),
                ('Clásica', 'Aglomerado', 'Gris Humo', 0, 24500.00, 'Faplac', 1830, 2400),
                ('Naturaleza', 'Aglomerado', 'Roble Dakar', 1, 28000.00, 'Egger', 1830, 2400),
                ('Naturaleza', 'Aglomerado', 'Nogal', 1, 29500.00, 'Egger', 1830, 2400),
                ('Fibroplus', 'MDF', 'Blanco', 0, 31000.00, 'Masisa', 1830, 2400),
                ('Especial', 'MDF', 'Haya Catedral', 1, 34000.00, 'Masisa', 1830, 2400),
                ('Especial', 'MDF', 'Wengue', 1, 36500.00, 'Egger', 1830, 2400),
                ('Urbana', 'MDF', 'Gris Ceniza', 0, 33000.00, 'Faplac', 1830, 2400);
            ";

            using var cmd = new SqlCommand(insertSql, conn);
            cmd.ExecuteNonQuery();
        }

        public static void ResetDatabase()
        {
            using var conn = CreateConnection();
            conn.Open();

            string cleanSql = @"
                DELETE FROM componente;
                DELETE FROM modulo;
                DELETE FROM pedido;
                DELETE FROM cliente;
                DELETE FROM placa;
            ";

            using (var cmd = new SqlCommand(cleanSql, conn))
            {
                cmd.ExecuteNonQuery();
            }

            SeedPlacas(conn);
        }

        public static Placa? GetCheapestPlaca()
        {
            using var conn = CreateConnection();
            conn.Open();

            string sql = "SELECT TOP 1 idPLACA, LINEA, COMPUESTO, COLOR, BETA, PRECIOPLAC, PROVEEDOR, ANCHO, LARGO FROM placa ORDER BY PRECIOPLAC ASC";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Placa
                {
                    IdPlaca = reader.GetInt32(0),
                    Linea = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Compuesto = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Color = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Beta = !reader.IsDBNull(4) && reader.GetBoolean(4),
                    Precio = reader.IsDBNull(5) ? 0 : Convert.ToDouble(reader.GetDecimal(5)),
                    Proveedor = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    Ancho = reader.IsDBNull(7) ? 1830 : reader.GetInt32(7),
                    Largo = reader.IsDBNull(8) ? 2400 : reader.GetInt32(8)
                };
            }

            return null;
        }

        public static List<Placa> GetAllPlacas()
        {
            var list = new List<Placa>();
            using var conn = CreateConnection();
            conn.Open();

            string sql = "SELECT idPLACA, LINEA, COMPUESTO, COLOR, BETA, PRECIOPLAC, PROVEEDOR, ANCHO, LARGO FROM placa ORDER BY idPLACA";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Placa
                {
                    IdPlaca = reader.GetInt32(0),
                    Linea = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Compuesto = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Color = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Beta = !reader.IsDBNull(4) && reader.GetBoolean(4),
                    Precio = reader.IsDBNull(5) ? 0 : Convert.ToDouble(reader.GetDecimal(5)),
                    Proveedor = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    Ancho = reader.IsDBNull(7) ? 1830 : reader.GetInt32(7),
                    Largo = reader.IsDBNull(8) ? 2400 : reader.GetInt32(8)
                });
            }
            return list;
        }

        public static bool InsertPlaca(Placa placa)
        {
            using var conn = CreateConnection();
            conn.Open();

            string sql = @"INSERT INTO placa (LINEA, COMPUESTO, COLOR, BETA, PRECIOPLAC, PROVEEDOR, ANCHO, LARGO) 
                           VALUES (@linea, @compuesto, @color, @beta, @precio, @proveedor, @ancho, @largo)";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@linea", placa.Linea ?? "");
            cmd.Parameters.AddWithValue("@compuesto", placa.Compuesto ?? "");
            cmd.Parameters.AddWithValue("@color", placa.Color ?? "");
            cmd.Parameters.AddWithValue("@beta", placa.Beta);
            cmd.Parameters.AddWithValue("@precio", Convert.ToDecimal(placa.Precio));
            cmd.Parameters.AddWithValue("@proveedor", placa.Proveedor ?? "");
            cmd.Parameters.AddWithValue("@ancho", placa.Ancho > 0 ? placa.Ancho : 1830);
            cmd.Parameters.AddWithValue("@largo", placa.Largo > 0 ? placa.Largo : 2400);

            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool DeletePlaca(int idPlaca)
        {
            using var conn = CreateConnection();
            conn.Open();

            string sql = "DELETE FROM placa WHERE idPLACA = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idPlaca);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
