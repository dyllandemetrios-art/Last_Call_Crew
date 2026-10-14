# P1-052 Portefeuille

## Intention

Ajouter le portefeuille à la préparation collective du départ et préparer son futur usage pour payer le taxi, sans encore implémenter l'argent, le taxi ou un inventaire général.

## Décision de prototype

Comme les clés, le portefeuille appartient au groupe pour la durée de la manche. Une personne le prend, tout le monde voit qu'il est récupéré et l'état reste acquis malgré un KO ou une déconnexion.

Le portefeuille ne bloque pas encore la sortie, le retour ou la victoire. Son futur rôle de prérequis du taxi sera traité dans la fiche du taxi.

## Comportement observable

- Un portefeuille greybox est posé dans la maison à un emplacement distinct des clés.
- En le visant à courte portée : `E — PRENDRE LE PORTEFEUILLE`.
- Après validation par l'hôte, il disparaît sur toutes les instances.
- Le HUD regroupe la préparation sous une ligne compacte : état des clés et du portefeuille.
- Feedback partagé : `PORTEFEUILLE RÉCUPÉRÉ`.
- À la relance, le portefeuille réapparaît.
- Un client arrivant après la prise reçoit directement l'état correct.

## Architecture

Réutiliser les conventions de P1-051 : raycast propriétaire, RPC vers l'hôte, reconstruction de la cible et de la distance, `NetworkVariable` écrite par l'hôte, visuel dérivé de l'état.

Ne pas refondre `HouseKeys` uniquement pour rendre les noms génériques. Une petite règle et un composant parallèles sont acceptables pour cette étape. Extraire un élément commun seulement si cela réduit réellement la duplication sans modifier le comportement validé des clés ni créer un système d'inventaire.

## Validations hôte

Refuser : mauvais émetteur, mauvaise cible, hors portée, manche gelée, joueur KO, joueur porté, joueur portant un partenaire et portefeuille déjà pris. Une seule demande concurrente peut réussir.

## Cycle de manche

- La relance remet le portefeuille à disposition dans le même ordre sûr que les clés.
- La déconnexion du joueur demandeur ne change pas l'état collectif.
- Les clés et le portefeuille se réinitialisent tous les deux, indépendamment.
- Aucun de ces objets ne conditionne encore l'activation du retour.

## Tests et validation

Ajouter les tests purs utiles puis vérifier :

1. prise par l'hôte puis par le client sur une nouvelle manche ;
2. concurrence entre deux demandes ;
3. rejets principaux sans transition ;
4. late join après prise ;
5. déconnexion du demandeur ;
6. relance : clés et portefeuille visibles ;
7. prendre l'un ne modifie pas l'autre ;
8. boire et prendre les clés restent fonctionnels.

Les demandes peuvent être injectées comme pour P1-051. Ne pas demander à Dyllan de rejouer la boucle complète.

## Critères d'acceptation

- Portefeuille visible, ciblable et synchronisé.
- Une seule prise par manche.
- État indépendant des clés et réinitialisation correcte.
- HUD compact et lisible avec les deux objets.
- Aucun inventaire, monnaie ou taxi ajouté.
- Compilation sans erreur, tests pertinents réussis et aucune nouvelle erreur bloquante.

## Hors périmètre

Argent, contenu du portefeuille, propriété individuelle, perte, échange, modèle final, téléphone, appel et taxi.

## Compte rendu attendu

Créer `Docs/Reports/P1-052-house-wallet-report.md` avec : commit de départ, fichiers/scène, éventuelle réutilisation de code, autorité, indépendance clés/portefeuille, relance, reconnexion, tests, limites et statut PASS, FAIL ou BLOCKED. Ne pas commit ni push après l'implémentation.
