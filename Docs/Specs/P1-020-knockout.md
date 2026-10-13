# P1-020 KO autoritaire

## Intention

Transformer le franchissement stable du seuil d'ivresse en un véritable état KO partagé. À `100`, le joueur tombe, perd ses actions et reste visible comme KO sur toutes les instances, tandis que son partenaire continue à jouer normalement.

Cette tâche crée l'état et ses conséquences immédiates. Le portage, le réveil à l'eau et le réveil automatique restent respectivement dans P1-021 et P1-022.

## Comportement observable

Lorsqu'un joueur atteint `KoThreshold` :

- l'hôte le place une seule fois dans l'état KO ;
- son corps est représenté couché sur le sol ;
- il ne peut plus se déplacer, regarder, boire ou refuser ;
- toute interaction déjà en cours est annulée sans produire de décision ;
- son écran affiche clairement `VOUS ÊTES KO` ;
- le HUD indique son état KO sur les deux instances ;
- l'autre joueur conserve son déplacement, sa caméra et ses interactions.

Le joueur reste KO indéfiniment dans P1-020. Aucun bouton ou délai ne permet encore de se relever.

## Autorité réseau

- Ajouter un composant dédié, par exemple `PlayerKnockout`, sur le prefab joueur.
- L'état KO est une donnée durable répliquée, écrite uniquement par l'hôte et lisible par tous.
- L'hôte déclenche le KO lorsque l'ivresse autoritaire atteint le seuil configuré.
- Le déclenchement est idempotent : rester à `100` ou recevoir un verre supplémentaire ne rejoue pas l'entrée en KO et ne crée pas de spam réseau ou de logs.
- Les nouveaux clients reçoivent l'état KO actuel par la donnée répliquée, sans RPC de rattrapage.
- Les RPC d'acceptation et de refus vérifient l'état KO côté hôte. Bloquer seulement les entrées locales n'est pas une validation suffisante.

## Présentation minimale

Ne pas ajouter de ragdoll ou de physique dynamique. La racine réseau et le collider du joueur restent stables et verticaux afin de ne pas perturber le `NetworkTransform` ni préparer prématurément le portage.

Créer ou utiliser un enfant visuel distinct pour la capsule :

- état normal : position et rotation habituelles ;
- état KO : visuel tourné à l'horizontale et rapproché du sol ;
- la transition inverse doit pouvoir être appliquée proprement lorsque P1-022 ajoutera le réveil, même si elle n'est pas déclenchée dans cette tâche.

Pour le propriétaire KO, afficher un voile sombre léger et un message central lisible. Ne pas construire l'interface finale et ne pas ajouter d'animation complexe.

## Blocage des actions

Quand le joueur est KO :

- `PlayerController` ne traite plus déplacement, rotation de caméra ou auto-walk ;
- `PlayerInteractor` ne démarre plus d'acceptation ni de refus ;
- un maintien de refus commencé avant le KO est annulé sans décision ;
- l'hôte rejette une acceptation, un début de refus ou une fin de refus provenant de ce joueur ;
- le HUD ne montre pas l'invite boire/refuser au joueur KO.

Le joueur non KO ne doit subir aucun de ces blocages.

## Travail demandé

1. Lire `AGENTS.md`, `Docs/Backlog.md`, le rapport P1-013 et cette fiche.
2. Vérifier que le commit de départ est `1dd9ffb` et que l'arbre est propre.
3. Ajouter des règles pures minimales pour déterminer l'entrée en KO si elles apportent une vraie couverture de test.
4. Ajouter l'état KO autoritaire au prefab joueur avec une responsabilité distincte de l'ivresse.
5. Relier le franchissement du seuil dans `PlayerIntoxication` au KO sans permettre au client de décider lui-même.
6. Bloquer les entrées locales et ajouter les validations serveur décrites ci-dessus.
7. Annuler proprement un refus en cours si le KO survient pendant le maintien.
8. Ajouter la représentation couchée sans modifier la rotation de la racine réseau ni la forme du collider.
9. Étendre l'interface de debug existante sans recréer de styles ou ajouter de panneaux concurrents.
10. Ajouter les tests EditMode pertinents et effectuer une validation hôte/client.

## Critères d'acceptation

- Atteindre le seuil KO place le bon joueur en KO une seule fois.
- L'état KO est identique sur l'hôte, le client propriétaire et l'observateur.
- Le corps apparaît couché sur toutes les instances sans renverser la racine réseau ni le collider.
- Le joueur KO ne peut plus se déplacer, tourner sa caméra, boire ou refuser.
- Un refus interrompu par le KO ne produit aucune décision.
- Une demande forcée depuis un joueur KO est rejetée par l'hôte et ne modifie ni la proposition ni l'ivresse.
- Le joueur non KO continue à se déplacer, regarder, boire et refuser.
- Un verre supplémentaire à `100` ne retrigger pas le KO.
- Un client qui rejoint après le KO reçoit immédiatement l'état correct.
- L'interface indique clairement le KO sans masquer les informations utiles du partenaire.
- Unity compile sans erreur, les tests pertinents passent et aucune erreur réseau nouvelle n'apparaît.

## Validation manuelle demandée

Avec deux instances :

1. monter l'hôte à `100` et vérifier que seul son joueur tombe et perd ses actions ;
2. vérifier que le client continue à se déplacer et à interagir ;
3. recommencer avec le client comme joueur KO ;
4. commencer un refus puis atteindre le KO pendant le maintien à l'aide d'un outil de test contrôlé ; vérifier qu'aucune décision n'est produite ;
5. tenter mouvement, caméra, boire et refuser depuis le joueur KO ;
6. observer sur les deux écrans la capsule couchée, le HUD et le message local ;
7. laisser l'état actif au moins dix secondes et vérifier qu'il reste stable sans spam de console.

Les appels directs utilisés pour accélérer l'ivresse doivent être consignés comme injection de test, séparément des entrées humaines.

## Hors périmètre

Ragdoll, portage, lâcher, eau, réveil par le partenaire, réveil automatique, réduction d'ivresse après réveil, animation finale, audio, effets de caméra élaborés, déconnexion de l'hôte, Relay et build autonome.

## Compte rendu attendu

Créer `Docs/Reports/P1-020-knockout-report.md` avec : branche et commit de départ, fichiers modifiés, modèle d'autorité, chemin exact du déclenchement, actions bloquées côté propriétaire et côté hôte, présentation visuelle, tests automatiques et hôte/client, cas du maintien interrompu, rejoin tardif, erreurs de console, limites, statut PASS, FAIL ou BLOCKED et prochaine action. Ne pas commit ni push sans autorisation explicite.
