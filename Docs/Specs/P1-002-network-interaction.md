# P1-002 Interaction réseau générique

## Intention

Prouver le patron technique qui servira ensuite à boire, refuser, réveiller un joueur et déclencher le retour : le joueur vise un objet, demande une interaction et l'hôte décide du nouvel état partagé.

## Comportement observable

Dans `NetworkTest.unity`, un cube interactif possède deux états visuels distincts. L'hôte ou le client le vise à courte portée et appuie sur Interagir. L'hôte valide la cible et la distance, puis change l'état. Les deux instances voient le même résultat.

## Travail demandé

1. Lire `AGENTS.md`, le rapport P1-001 et cette fiche.
2. Partir du commit de nettoyage qui suit `e8f7235` avec un arbre propre.
3. Réenregistrer les deux scripts P1-001 en UTF-8 et restaurer les accents des commentaires sans changer leur comportement.
4. Ajouter une action `Interact` à l'Input System existant, liée provisoirement à `E`.
5. Détecter la cible par un raycast court depuis la caméra du propriétaire.
6. Envoyer la demande à l'hôte. L'hôte revérifie la cible et la distance avant toute modification.
7. Répliquer l'état avec NGO et faire découler le visuel de cet état partagé.
8. Ajouter le cube interactif et un feedback minimal dans la scène de test.
9. Garder la solution minimale ; ne pas créer de framework généraliste.

## Critères d'acceptation

- L'hôte et le client peuvent chacun activer le cube.
- Une activation hors portée est refusée par l'hôte.
- Une pression produit un seul changement d'état.
- Les deux instances affichent toujours le même état.
- Chaque joueur lit uniquement ses propres entrées.
- Unity compile sans erreur.
- Dix activations alternées ne produisent aucune divergence ni erreur réseau bloquante.

## Hors périmètre

PNJ, dialogue, ivresse, maintien pour refuser, inventaire, interface finale, Relay et système générique pour toutes les interactions.

## Compte rendu attendu

Créer `Docs/Reports/P1-002-network-interaction-report.md` avec les fichiers modifiés, le protocole d'autorité, la procédure et le résultat du test à deux joueurs, les limites et le statut PASS, FAIL ou BLOCKED. Ne pas commit ni push sans autorisation explicite.
