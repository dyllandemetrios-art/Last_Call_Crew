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

        // Hôte : après une relance, la pose du joueur est remise par son propriétaire (autorité propriétaire) ;
        // tant que l'hôte voit encore l'ancienne position, aucun retour ne doit être enregistré.
        bool _awaitingPose;
        Vector3 _poseTarget;
        double _poseDeadline;
        const float PoseTolerance = 1f;
        const double PoseTimeoutSeconds = 3d;

        /// <summary>Hôte uniquement, relance (P1-033) : efface le retour pour la manche suivante.</summary>
        public void ServerResetForRound(Vector3 startPosition, double serverTimeNow)
        {
            if (!IsServer)
                return;
            _returnTime.Value = ReturnRules.NotReturned;
            _awaitingPose = true;
            _poseTarget = startPosition;
            _poseDeadline = serverTimeNow + PoseTimeoutSeconds;
        }

        /// <summary>
        /// Hôte : faux tant que la pose de départ de la relance n'est pas arrivée sur l'hôte (position proche du point
        /// de départ, ou délai écoulé). Évite d'enregistrer un retour à partir d'une position de la manche précédente.
        /// </summary>
        public bool ServerPoseSettled(Vector3 currentPosition, double serverTimeNow)
        {
            if (!_awaitingPose)
                return true;
            if ((currentPosition - _poseTarget).sqrMagnitude <= PoseTolerance * PoseTolerance || serverTimeNow >= _poseDeadline)
                _awaitingPose = false;
            return !_awaitingPose;
        }

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
