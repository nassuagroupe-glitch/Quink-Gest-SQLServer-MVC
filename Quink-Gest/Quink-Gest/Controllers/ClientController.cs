using System;
using System.Collections.Generic;
using QuinkGest.Models;
using QuinkGest.Models.Repository;

namespace QuinkGest.Controllers
{
    public class ClientController
    {
        private readonly ClientRepository _repository = new ClientRepository();

        public List<Client> ChargerTousLesClients() => _repository.ListerTout();

        public (bool succes, string message) AjouterClient(Client client)
        {
            if (string.IsNullOrWhiteSpace(client.Nom))
                return (false, "Le nom du client est obligatoire");

            try
            {
                _repository.Ajouter(client);
                return (true, $"Client \"{client.Nom}\" ajouté");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors de l'ajout : {ex.Message}");
            }
        }

        public (bool succes, string message) ModifierClient(Client client)
        {
            if (string.IsNullOrWhiteSpace(client.Nom))
                return (false, "Le nom du client est obligatoire");

            try
            {
                _repository.Modifier(client);
                return (true, "Client mis à jour");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors de la modification : {ex.Message}");
            }
        }

        public (bool succes, string message) SupprimerClient(int clientId)
        {
            try
            {
                _repository.Supprimer(clientId);
                return (true, "Client supprimé");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors de la suppression : {ex.Message}");
            }
        }
    }
}
