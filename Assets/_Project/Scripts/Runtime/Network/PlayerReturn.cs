using LastCallCrew.Night;
using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Retour d'un joueur dans la zone de sortie (P1-031). Donnée durable répliquée, écrite par l'hôte uniquement,
    /// une seule fois : l'instant serveur du premier retour (ReturnRules.NotReturned tant qu'il n'est pas rentré).
    /// Aucune décision de victoire ou d'échec : P1-032 comparera cet instant à l'échéance du chronomètre.
    /// </summary>
    public class PlayerReturn : NetworkBehaviour
    {
        readonly NetworkVariable<double> _returnTime = new NetworkVariable<double>(ReturnRules.NotReturned);

        public bool IsReturned => ReturnRules.IsReturned(_returnTime.Value);
        /// <summary>Temps serveur du premier retour ; ReturnRules.NotReturned s'il n'est pas rentré.</summary>
        public double ReturnServerTime => _returnTime.Value;

        /// <summary>Hôte uniquement. Idempotent : le premier instant est conservé, les appels suivants ne changent rien.</summary>
        public bool ServerMarkReturned(double serverTimeNow)
        {
            if (!IsServer || !ReturnRules.ShouldRecord(_returnTime.Value))
                return false;
            _returnTime.Value = ReturnRules.Record(_returnTime.Value, serverTimeNow);
            Debug.Log($"[P1-031] host: client {OwnerClientId} returned at server time {_returnTime.Value:0.00}");
            return true;
        }
    }
}
