using LastCallCrew.Dialogue;
using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Premier PNJ à dialogue (P1-014) : une proposition absurde et trois réponses. Pas de framework : les textes
    /// sont de simples champs modifiables dans l'Inspector. L'hôte décide (via PlayerInteractor) et écrit la réaction
    /// répliquée (numéro de réponse et compteur) ; chaque instance en déduit la réaction affichée un court instant.
    /// Aucune indisponibilité globale ni cooldown : deux joueurs peuvent parler successivement.
    /// </summary>
    public class NpcDialogue : NetworkBehaviour
    {
        [Header("Textes de prototype (modifiables)")]
        [SerializeField] string npcName = "Marcel";
        [TextArea(1, 3)]
        [SerializeField] string prompt = "Psst… je vends des cravates pour canards. T'en veux une ?";
        [SerializeField] string[] choices =
        {
            "Acheter trois cravates",
            "Demander l'avis du canard",
            "Refuser, j'ai déjà un canard",
        };
        [SerializeField] string[] reactions =
        {
            "MERCI ! Le canard sera très élégant ce soir.",
            "Le canard a dit « coin ». Je prends ça pour un oui.",
            "Pas grave. Il portera un nœud papillon alors.",
        };

        [Header("Réglages")]
        [Tooltip("Durée (s) d'affichage de la réaction, identique chez les deux joueurs.")]
        [SerializeField] float reactionSeconds = 3f;
        [SerializeField] Collider npcCollider;
        [SerializeField] Renderer npcRenderer;
        [SerializeField] Color npcColor = new Color(0.3f, 0.55f, 0.95f);

        // Réaction répliquée : numéro de réponse (0 = aucune) et compteur de réponses ; écrits par l'hôte seulement.
        readonly NetworkVariable<byte> _reaction = new NetworkVariable<byte>();
        readonly NetworkVariable<int> _serial = new NetworkVariable<int>();
        readonly NetworkVariable<ulong> _speakerPlusOne = new NetworkVariable<ulong>();

        float _reactionUntil;
        byte _shownReaction;

        public string NpcName => npcName;
        public string Prompt => prompt;
        public string Choice(int number) => number >= 1 && number <= choices.Length ? choices[number - 1] : string.Empty;
        public string ReactionText(int number) => number >= 1 && number <= reactions.Length ? reactions[number - 1] : string.Empty;

        /// <summary>Texte de la réaction en cours (même sur toutes les instances) ou vide.</summary>
        public string ActiveReaction => _shownReaction != 0 && Time.unscaledTime < _reactionUntil ? ReactionText(_shownReaction) : string.Empty;
        /// <summary>Joueur ayant provoqué la réaction en cours, ou null.</summary>
        public ulong? ActiveSpeaker => _shownReaction != 0 && Time.unscaledTime < _reactionUntil && _speakerPlusOne.Value != 0
            ? _speakerPlusOne.Value - 1 : (ulong?)null;
        public int Serial => _serial.Value;
        public int ReactionNumber => _reaction.Value;

        public Vector3 ClosestPoint(Vector3 from) => npcCollider.ClosestPoint(from);

        public override void OnNetworkSpawn()
        {
            if (npcRenderer != null)
            {
                var block = new MaterialPropertyBlock();
                block.SetColor("_BaseColor", npcColor);
                npcRenderer.SetPropertyBlock(block);
            }
            _serial.OnValueChanged += OnSerialChanged;
            _reaction.OnValueChanged += OnReactionChanged;
        }

        public override void OnNetworkDespawn()
        {
            _serial.OnValueChanged -= OnSerialChanged;
            _reaction.OnValueChanged -= OnReactionChanged;
        }

        /// <summary>Hôte uniquement, après validation : enregistre la réaction à la réponse choisie.</summary>
        public void ServerReact(ulong speakerClientId, int choice)
        {
            if (!IsServer || !DialogueRules.IsValidChoice(choice))
                return;
            _speakerPlusOne.Value = speakerClientId + 1;
            _reaction.Value = (byte)choice;
            _serial.Value++;
            Debug.Log($"[P1-014] host: client {speakerClientId} chose {choice} -> \"{ReactionText(choice)}\"");
        }

        /// <summary>Hôte uniquement (NightRound) : plus aucune réaction après une relance.</summary>
        public void ServerResetForRound()
        {
            if (!IsServer)
                return;
            _reaction.Value = 0;
            _speakerPlusOne.Value = 0;
        }

        void OnSerialChanged(int previous, int current)
        {
            // La réaction (déjà écrite avant le compteur) démarre son minuteur local : même durée partout.
            _shownReaction = _reaction.Value;
            _reactionUntil = Time.unscaledTime + reactionSeconds;
        }

        void OnReactionChanged(byte previous, byte current)
        {
            if (current == 0)
            {
                _shownReaction = 0;
                _reactionUntil = 0f;
            }
        }
    }
}
