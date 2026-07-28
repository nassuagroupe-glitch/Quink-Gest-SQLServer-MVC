using System.Windows;
using QuinkGest.Models.Database;

namespace QuinkGest
{
    /// <summary>
    /// Point d'entrée de l'application : crée la base SQL Server "QuinkGest"
    /// (si absente) et ses tables avant l'affichage de la première fenêtre.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DatabaseHelper.Initialiser();
        }
    }
}
