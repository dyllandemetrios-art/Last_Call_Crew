# P1-013 Ivresse synchronisée

## Intention

Donner à chaque joueur une valeur d'ivresse autoritaire comprise entre `0` et `100`, alimentée par les verres acceptés de P1-012 et diminuée passivement. L'hôte calcule cette valeur et son palier ; toutes les instances affichent le même état.

Cette tâche prouve la règle et sa synchronisation. Elle ne modifie pas encore le déplacement, la caméra ou les actions du joueur selon son ivresse et ne déclenche pas le comportement de KO.

## Comportement observable

Dans `NetworkTest.unity`, chaque joueur commence à `0` (`Sobre`). Lorsqu'un joueur accepte un verre :

- son ivresse augmente du gain configuré, sans modifier celle de l'autre joueur ;
- un refus ne modifie aucune ivresse ;
- l'ivresse redescend passivement selon la valeur configurée ;
- la valeur reste bornée entre le minimum et le seuil KO ;
- les paliers affichés sont identiques sur l'hôte et le client.

Un HUD de test indique pour les deux joueurs leur identifiant, leur valeur d'ivresse et leur palier. Il sert uniquement au prototype et à la validation réseau.

## Paliers provisoires

Les seuils viennent exclusivement de l'asset `TuningConfig` :

- `Sobre` : valeur inférieure au seuil Pompette ;
- `Pompette` : valeur supérieure ou égale au seuil Pompette et inférieure au seuil Ivre ;
- `Ivre` : valeur supérieure ou égale au seuil Ivre et inférieure au seuil KO ;
- `Seuil KO` : valeur égale au seuil KO.

Atteindre `Seuil KO` ne désactive ni le mouvement ni les interactions dans cette tâche. P1-020 implémentera le KO.

## Autorité et données réseau

- Ajouter un composant réseau dédié, par exemple `PlayerIntoxication`, sur le prefab joueur.
- Chaque joueur possède sa propre valeur répliquée.
- Seul l'hôte peut modifier cette valeur.
- Le client propriétaire ne demande pas directement un gain d'ivresse : le gain découle uniquement d'une acceptation déjà validée par l'hôte dans P1-012.
- Un refus, une demande rejetée ou une acceptation invalide ne produit aucun gain.
- La décroissance est calculée par l'hôte avec le temps écoulé, puis répliquée à une fréquence raisonnable. Éviter une écriture réseau à chaque image si la valeur affichée n'a pas changé de manière utile.
- Les nouveaux clients doivent recevoir la valeur actuelle grâce à l'état réseau, sans RPC de rattrapage spécifique.

## Travail demandé

1. Lire `AGENTS.md`, `Docs/Backlog.md`, les rapports P1-011 et P1-012, puis cette fiche.
2. Vérifier que le commit de départ est `e4eba22` et que l'arbre est propre.
3. Ajouter des règles pures pour borner une valeur, ajouter un verre, appliquer une décroissance et déterminer le palier à partir de `TuningConfig`.
4. Ajouter `PlayerIntoxication` au prefab joueur avec une référence explicite à l'asset `TuningConfig`, sans singleton ni service global.
5. Répliquer la valeur d'ivresse avec NGO en écriture serveur uniquement et lecture par tous.
6. Relier l'acceptation validée de P1-012 au composant d'ivresse du joueur qui a accepté. Conserver la décision et le gain dans le même chemin autoritaire.
7. Appliquer la décroissance passive sur l'hôte seulement, en tenant compte du temps écoulé et des bornes configurées.
8. Ajouter un HUD de test lisible sur chaque instance montrant l'état des deux joueurs. Ne pas construire l'interface finale.
9. Ajouter des tests EditMode ciblés pour les calculs purs et les limites.
10. Tester avec un hôte et un client que les gains restent indépendants et que les valeurs/paliers convergent sur les deux instances.

## Critères d'acceptation

- Chaque joueur apparaît à la valeur minimale configurée et au palier `Sobre`.
- Une acceptation valide ajoute exactement `GainPerDrink` au joueur concerné uniquement.
- Un refus et toute demande rejetée n'ajoutent rien.
- La décroissance passive utilise `PassiveDecayPerSecond`, est calculée uniquement par l'hôte et ne descend jamais sous le minimum.
- Les gains ne dépassent jamais le seuil KO.
- Les passages `Sobre` → `Pompette` → `Ivre` → `Seuil KO` respectent les seuils configurés.
- Après plusieurs gains et au moins une période de décroissance, l'hôte et le client affichent la même valeur à la précision du HUD et le même palier pour les deux joueurs.
- Le joueur qui ne boit pas ne reçoit pas le gain de l'autre.
- Le mouvement et le choix boire/refuser validés précédemment ne régressent pas.
- Unity compile sans erreur et les tests pertinents passent.

## Validation manuelle demandée

Avec deux instances :

1. vérifier que les deux joueurs commencent à `0 — Sobre` sur les deux écrans ;
2. faire accepter un verre par l'hôte et vérifier que seul son joueur gagne `20` points ;
3. attendre la disponibilité, faire accepter un verre par le client et vérifier que seul son joueur gagne `20` points ;
4. vérifier qu'un refus complet ne donne aucun point ;
5. observer pendant au moins dix secondes que les valeurs diminuent et restent cohérentes sur les deux écrans ;
6. atteindre successivement les seuils `Pompette`, `Ivre` et `Seuil KO`, puis vérifier les libellés et la borne à `100` ;
7. vérifier qu'à `100` le joueur peut encore se déplacer et interagir, puisque le KO appartient à P1-020.

Claude peut accélérer ce protocole avec des outils de test ou des entrées injectées, mais le rapport doit distinguer les preuves automatisées de la validation visuelle de Dyllan.

## Hors périmètre

Effets de caméra, modification de vitesse ou de contrôle, animation, son, vomissement, eau, KO fonctionnel, portage, réveil, profils de personnage, sauvegarde, interface finale et Relay.

## Compte rendu attendu

Créer `Docs/Reports/P1-013-synchronized-intoxication-report.md` avec : branche et commit de départ, fichiers modifiés, modèle d'autorité, fréquence de réplication choisie et son coût, raccordement à P1-012, tests automatiques et hôte/client, valeurs observées, erreurs de console, limites, statut PASS, FAIL ou BLOCKED et prochaine action. Ne pas commit ni push sans autorisation explicite.
