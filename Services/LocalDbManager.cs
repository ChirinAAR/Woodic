using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace Woodic.Services
{
    public static class LocalDbManager
    {
        private const string LocalDbDownloadUrl = "https://download.microsoft.com/download/3/8/d/38de7036-2433-4207-8eae-06e247e17b25/SqlLocalDB.msi";

        /// <summary>
        /// Comprueba si Microsoft SQL Server LocalDB está instalado en el equipo.
        /// </summary>
        public static bool IsLocalDbInstalled()
        {
            try
            {
                // 1. Probar el comando sqllocaldb si está en el PATH
                var psi = new ProcessStartInfo
                {
                    FileName = "sqllocaldb",
                    Arguments = "info",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var proc = Process.Start(psi);
                if (proc != null)
                {
                    proc.WaitForExit(3000);
                    if (proc.ExitCode == 0) return true;
                }
            }
            catch { }

            try
            {
                // 2. Comprobar rutas habituales en Archivos de Programa
                string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string progFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                string[] versions = { "160", "150", "140", "130", "120", "110" };

                foreach (var v in versions)
                {
                    if (File.Exists(Path.Combine(progFiles, "Microsoft SQL Server", v, "Tools", "Binn", "SqlLocalDB.exe")) ||
                        File.Exists(Path.Combine(progFilesX86, "Microsoft SQL Server", v, "Tools", "Binn", "SqlLocalDB.exe")))
                    {
                        return true;
                    }
                }
            }
            catch { }

            try
            {
                // 3. Comprobar en el Registro de Windows
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions");
                if (key != null && key.SubKeyCount > 0)
                {
                    return true;
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Busca si existe el instalador SqlLocalDB.msi en la carpeta de la aplicación o directorios padre.
        /// </summary>
        public static string? FindLocalInstallerMsi()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string target = Path.Combine(baseDir, "SqlLocalDB.msi");
            if (File.Exists(target)) return target;

            // Revisar subcarpeta instalador o complementos
            string sub1 = Path.Combine(baseDir, "instalador", "SqlLocalDB.msi");
            if (File.Exists(sub1)) return sub1;

            // Subir hacia carpetas padre (por si se ejecuta desde bin/Debug)
            var cur = new DirectoryInfo(baseDir);
            while (cur?.Parent != null)
            {
                string cand = Path.Combine(cur.FullName, "SqlLocalDB.msi");
                if (File.Exists(cand)) return cand;
                cur = cur.Parent;
            }

            return null;
        }

        /// <summary>
        /// Asegura que la instancia predeterminada MSSQLLocalDB esté creada e iniciada.
        /// </summary>
        public static void EnsureInstanceStarted()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "sqllocaldb",
                    Arguments = "start MSSQLLocalDB",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit(6000);
            }
            catch
            {
                // Intentar buscar la ruta absoluta si no está en PATH
                try
                {
                    string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    string[] versions = { "160", "150", "140", "130", "120", "110" };
                    foreach (var v in versions)
                    {
                        string exe = Path.Combine(progFiles, "Microsoft SQL Server", v, "Tools", "Binn", "SqlLocalDB.exe");
                        if (File.Exists(exe))
                        {
                            var psi = new ProcessStartInfo
                            {
                                FileName = exe,
                                Arguments = "start MSSQLLocalDB",
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };
                            using var proc = Process.Start(psi);
                            proc?.WaitForExit(6000);
                            break;
                        }
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Ejecuta el instalador MSI de LocalDB solicitando permisos de administrador.
        /// </summary>
        public static async Task<bool> InstallLocalDbMsiAsync(string msiPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "msiexec.exe",
                        Arguments = $"/i \"{msiPath}\" /qb IACCEPTSQLLOCALDBLICENSETERMS=YES",
                        UseShellExecute = true,
                        Verb = "runas"
                    };

                    using var proc = Process.Start(psi);
                    if (proc == null) return false;

                    proc.WaitForExit();
                    return proc.ExitCode == 0 || proc.ExitCode == 3010;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al ejecutar instalador LocalDB: {ex.Message}");
                    return false;
                }
            });
        }

        /// <summary>
        /// Descarga el instalador oficial de LocalDB desde los servidores de Microsoft.
        /// </summary>
        public static async Task<string?> DownloadLocalDbInstallerAsync()
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "SqlLocalDB_Woodic.msi");
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(5);

                using var response = await client.GetAsync(LocalDbDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await stream.CopyToAsync(fileStream);

                return tempPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error descargando LocalDB: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Pregunta al usuario e instala automáticamente Microsoft SQL Server LocalDB si no está disponible.
        /// </summary>
        public static async Task<bool> PromptAndInstallLocalDbAsync(Window owner)
        {
            var result = MessageBox.Show(
                owner,
                "Para almacenar datos y pedidos, Woodic requiere Microsoft SQL Server LocalDB.\n\n" +
                "Este componente no fue detectado en su equipo.\n\n" +
                "¿Desea instalarlo automáticamente ahora? (Se solicitarán permisos de Administrador)",
                "Componente requerido - Woodic",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
            {
                return false;
            }

            string? msiPath = FindLocalInstallerMsi();

            if (msiPath == null || !File.Exists(msiPath))
            {
                var dlResult = MessageBox.Show(
                    owner,
                    "El archivo de instalación de LocalDB no se encontró en la carpeta local.\n\n" +
                    "¿Desea descargarlo automáticamente desde los servidores oficiales de Microsoft e instalarlo?",
                    "Descargar componente",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (dlResult != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlgDescarga = new Vistas.DialogoConfigurando
                {
                    Owner = owner,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                dlgDescarga.Show();

                try
                {
                    msiPath = await DownloadLocalDbInstallerAsync();
                }
                finally
                {
                    dlgDescarga.Close();
                }

                if (msiPath == null || !File.Exists(msiPath))
                {
                    MessageBox.Show(
                        owner,
                        "No se pudo descargar el instalador de Microsoft LocalDB. Verifique su conexión a Internet.",
                        "Error de descarga",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return false;
                }
            }

            // Ejecutar instalación
            bool installSuccess = await InstallLocalDbMsiAsync(msiPath);

            if (installSuccess)
            {
                EnsureInstanceStarted();
                MessageBox.Show(
                    owner,
                    "Microsoft SQL Server LocalDB se instaló correctamente.\nWoodic continuará iniciando el sistema.",
                    "Instalación exitosa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                return true;
            }
            else
            {
                MessageBox.Show(
                    owner,
                    "La instalación fue cancelada o no se completó.\n" +
                    "Puede ejecutar manualmente el archivo 'SqlLocalDB.msi' en cualquier momento.",
                    "Instalación no completada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return false;
            }
        }
    }
}
