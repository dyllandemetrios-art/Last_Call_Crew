using LastCallCrew.Night;
using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Retour d'un joueur dans la zone de sortie (P1-031). Donnée répliquée, écrite par l'hôte uniquement
    /// lors des transitions d'entrée et de sortie : l'instant serveur du début de la présence actuelle dans la zone
    /// (ReturnRules.NotReturned tant que le joueur n'y est pas). P1-032 compare cet instant à l'échéance du chronomètre.
    /// </summary>
    public class PlayerReturn : NetworkBehaviour
    {
        readonly NetworkVariable<double> _returnTime = new NetworkVariable<double>(ReturnRules.NotReturned);

        public bool IsReturned => ReturnRules.IsReturned(_returnTime.Value);
        /// <summary>Temps serveur de l'entrée actuelle dans la zone ; ReturnRules.NotReturned s'il n'y est pas.</summary>
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
        /// Hôte, apparition d'un joueur (P1-050) : sa pose de départ n'est pas encore arrivée sur l'hôte ; sa position de
        /// passage ne doit compter ni comme retour ni comme sortie de la maison. Ne touche pas aux NetworkVariable
        /// (appelé pendant le spawn).
        /// </summary>
        public void ServerExpectPose(Vector3 startPosition, double serverTimeNow)
        {
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

        /// <summary>
        /// Hôte uniquement (P1-031a). Applique la présence réelle dans la zone : l'instant d'entrée est conservé tant que le
        /// joueur reste dedans, effacé à la sortie, renouvelé à une nouvelle entrée. N'écrit la NetworkVariable que lors
        /// d'une transition.
        /// </summary>
        public void ServerSetPresence(bool present, double serverTimeNow)
        {
            if (!IsServer)
                return;
            double next = ReturnRules.Next(_returnTime.Value, present, serverTimeNow);
            if (next == _returnTime.Value)
                return;
            _returnTime.Value = next;
            Debug.Log(present
                ? $"[P1-031] host: client {OwnerClientId} entered return zone at server time {next:0.00}"
                : $"[P1-031a] host: client {OwnerClientId} left return zone");
        }
    }
}
