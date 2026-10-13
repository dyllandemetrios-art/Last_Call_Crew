# Rapport P1-014 — Premier dialogue absurde

**Statut : PASS avec réserve** (hôte + client, Multiplayer Play Mode). Dyllan valide la phase et le dialogue le 13 octobre 2026. Le refus au clic droit ne fonctionne pas dans son essai et le pointeur peut sortir de la Game View, ce qui rend le contrôle de la caméra pénible. Ce défaut est consigné pour correction séparée et ne remet pas en cause le dialogue P1-014.

## Départ

- Branche `feature/p1-001-network-foundation`, commit de départ `db8981a`, arbre propre. Unity 6000.3.8f1 ; aucun package ajouté.

## Fichiers

Créés :
- `Scripts/Runtime/Dialogue/LastCallCrew.Dialogue.asmdef` et `DialogueRules.cs` : validation pure (un PNJ, trois réponses, pas de framework).
- `Scripts/Runtime/Network/NpcDialogue.cs` : le PNJ (textes et réaction répliquée).
- `Tests/EditMode/DialogueRulesTests.cs` : 17 cas.
- `.meta` correspondants.

Modifiés :
- `Assets/InputSystem_Actions.inputactions` : actions `DialogueChoice1/2/3` (touches `1`/`2`/`3` et pavé numérique) et `DialogueCancel` (`Échap`).
- `Scenes/NetworkTest.unity` : objet `NpcDialogue` (capsule bleue en (−3 ; 1 ; 4), `NetworkObject` de scène, +172 lignes, ajouts uniquement) ; référence optionnelle depuis `NightRound`.
- `PlayerInteractor.cs` : visée du PNJ, `E` ouvre, `1/2/3/Échap`, RPC hôte.
- `PlayerCarry.cs` : porter un partenaire annule un dialogue ouvert côté hôte.
- `NightRound.cs` : remise à zéro du PNJ à la relance.
- `IntoxicationHud.cs` : invite, carte de dialogue, réaction.
- `LastCallCrew.Tests.EditMode.asmdef`.
- Non modifiés : commandes et logique boire / refuser / porter / réveiller, `TuningConfig`.

## Textes configurés (champs de `NpcDialogue`, modifiables dans l'Inspector)

- PNJ : « Marcel » ; proposition : « Psst… je vends des cravates pour canards. T'en veux une ? »
- 1 — Acheter trois cravates → « MERCI ! Le canard sera très élégant ce soir. »
- 2 — Demander l'avis du canard → « Le canard a dit « coin ». Je prends ça pour un oui. »
- 3 — Refuser, j'ai déjà un canard → « Pas grave. Il portera un nœud papillon alors. »
- Durée d'affichage de la réaction : 3 s (champ `reactionSeconds`).

## Modèle d'autorité

- Le propriétaire lit `E`, `1/2/3`, `Échap` et envoie trois RPC vers l'hôte (dans `PlayerInteractor`, donc avec la propriété requise) : ouvrir, choisir, fermer.
- L'hôte valide avec `DialogueRules` : émetteur, cible, gel de manche, KO, porté, porteur, portée (même portée serveur que les autres interactions), et, pour un choix, **dialogue ouvert côté hôte** et numéro valide. Un choix accepté ferme le dialogue côté hôte avant d'enregistrer la réaction : accepté une seule fois.
- La réaction est répliquée (`NetworkVariable` de numéro de réponse, de compteur et de joueur) ; chaque instance démarre localement le même minuteur. Un nouvel arrivant ne reçoit aucune réaction ancienne. La carte de choix reste locale au joueur concerné.
- Aucune indisponibilité globale ni cooldown ; aucun maintien.

## Interruptions

- **Locales** (aucune réaction) : `Échap`, perte de la cible ou éloignement (au-delà de la portée du raycast), KO, portage d'un partenaire, résultat de nuit (gel), nouvelle manche (numéro de manche mémorisé).
- **Hôte** : le drapeau « ouvert » est effacé à l'entrée en KO, au portage d'un partenaire et à la relance (`ServerCancelRefusal`), et tout rejet d'un choix l'efface aussi.

## Résultats (journaux hôte, valeurs inspectées, captures minimales)

| Scénario | Résultat |
|---|---|
| Chiffre `2` sans dialogue ouvert (hôte) | aucun effet (`serial=0`, aucun journal) |
| Hôte ouvre puis `Échap` ; client idem | `Opened` puis fermeture sans réaction, sur chaque rôle |
| Hôte choisit 1, client choisit 2, hôte choisit 3 | réaction n°1, n°2, n°3, `serial` 1 → 3 identique sur les deux instances ; texte exact « MERCI… », « …coin… », « …nœud papillon… » |
| Réaction visible par les deux joueurs | `active='Le canard a dit… '` identique sur l'hôte et le client |
| Hôte et client ouverts en même temps | les deux cartes ouvertes ; l'hôte qui choisit n'affecte pas l'autre |
| Choix forcé sans dialogue ouvert (client, hôte) | `RejectedNotOpen` |
| Client ouvre puis s'éloigne (z = 25), choix forcé | `RejectedOutOfRange` |
| Hôte ouvre puis devient KO ; choix forcé | fermeture locale ; `RejectedKnockedOut` |
| Hôte ouvre puis porte le partenaire KO ; choix forcé | fermeture locale ; `RejectedCarrying` |
| Client ouvre puis se détourne | fermeture locale, aucune réaction (`serial` inchangé) |
| Client ouvre puis la manche se termine (échec) ; choix forcé | fermeture locale ; `RejectedFrozen` |
| Relance avec une réaction active (durée portée à 60 s par réflexion pour le test) | `reaction=0`, `active=''`, cartes fermées sur les deux instances |
| Nouveau dialogue après relance (choix 3 par le client) | réaction n°3 sur les deux instances |

Les RPC forcés, l'échéance de manche et les KO sont injectés par réflexion/script ; les ouvertures, annulations et choix utilisent de vraies touches `E`, `1`, `2`, `3`, `Échap` injectées par l'Input System.

## Captures

Deux, 1280×720 : carte de choix ouverte (hôte) et réaction validée visible par l'autre joueur (client), toutes deux relues ; carte lisible, sans chevauchement avec NUIT ni le panneau Ivresse.

## Régressions vérifiées

- Boire : `E` sur la proposition de boisson → `Accepted` avant le dialogue **et** après la relance suivant un dialogue.
- Porter : `E` → `carry request -> Started`, `E` → lâcher ; réveiller : `F` maintenu 3,6 s → `WokenByWater (held 3,00 s)`.
- Refuser (clic droit maintenu) : non rejoué avec une entrée réelle (code non modifié) ; les RPC de refus restent couverts par les rejets de gel de P1-033.
- Le dialogue d'un joueur ne bloque pas l'autre : le client a ouvert, annulé et choisi pendant que l'hôte avait sa carte ouverte.

## Tests

- Compilation : 0 erreur. **174 tests EditMode sur 174 réussis** (157 + 17). Console : aucune erreur de jeu.

## Erreurs rencontrées

- Un nom de membre en double (`OpenDialogue`) a bloqué la première compilation ; corrigé.
- Mes premiers tests de RPC forcés échouaient silencieusement (erreur de compilation du script d'évaluation masquée) : corrigés et rejoués, les résultats ci-dessus sont ceux du second passage.

## Limites

- Pas de test à trois joueurs ni de build autonome ; Relay hors périmètre.
- Le dialogue ne se ferme pas lorsqu'on perd la cible par distance exacte entre la portée du raycast (3 m) et celle du serveur (4 m) : côté propriétaire c'est la portée du raycast qui ferme.
- Les touches `1/2/3` ne sont lues que dialogue ouvert ; elles ne sont liées à aucune autre action, mais les futurs usages des chiffres devront rester cohérents.
- Pas de bulle dans le monde ni d'animation du PNJ ; la réaction s'affiche sur le HUD (bas de l'écran), identique pour les deux joueurs.
- `Échap` peut aussi libérer le curseur de l'éditeur Unity en Play Mode ; sans effet sur la logique.
- Refuser non rejoué avec une entrée réelle (voir Régressions).
- Validation Dyllan : clic droit de refus non fonctionnel et pointeur non capturé dans la Game View en Multiplayer Play Mode. Lien causal non établi ; à reproduire avant correction.

## Nettoyage

`Assets/DefaultNetworkPrefabs.asset` (recréé par NGO) et `Assets/Temp` (captures) supprimés ; aucun changement de `ProjectSettings`.

## Validation manuelle de Dyllan

Le dialogue et son rythme sont acceptés. La phase peut continuer malgré la réserve de contrôle souris ci-dessus.

## Prochaine action recommandée

Reproduire et corriger la capture du pointeur et le clic droit dans une petite tâche dédiée, puis reprendre la construction de la greybox.
