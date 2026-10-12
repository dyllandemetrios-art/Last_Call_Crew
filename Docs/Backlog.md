# Backlog P1

Échéance cible : **30 novembre 2026**.

## Socle réseau

| ID | Tâche | Critère de fin | Statut |
| --- | --- | --- | --- |
| P1-000 | Établir la référence Unity | Projet ouvert avec Unity 6000.3.8f1, compilation sans erreur bloquante | PASS |
| P1-001 | Créer la fondation réseau locale | Un hôte et un client contrôlent chacun une capsule et voient les deux déplacements pendant 5 minutes | PASS le 12 octobre 2026 |
| P1-002 | Ajouter une interaction réseau générique | Un joueur active un objet ; l'hôte autorise le changement et les deux joueurs voient le même état | PASS au commit `9f13a46` |
| P1-003 | Configurer Unity Cloud et Relay | Une session produit un code que la seconde instance peut rejoindre | Bloqué par P1-002 et action Dyllan |
| P1-004 | Valider un build Windows à deux joueurs | Deux processus rejoignent la même partie via Relay sans erreur bloquante | Bloqué par P1-003 |

## Tranche jouable

| ID | Tâche | Critère de fin | Statut |
| --- | --- | --- | --- |
| P1-010 | Créer la greybox | Deux joueurs apparaissent dans un petit lieu avec entrée et sortie lisibles | Bloqué par P1-002 |
| P1-011 | Créer le TuningConfig | Gain et décroissance d'ivresse, seuils, KO, refus et réveil sont réglables | Prêt pour Claude |
| P1-012 | Boire et refuser | Le choix est autorisé par l'hôte et possède un feedback sur les deux instances | Bloqué par P1-002 |
| P1-013 | Ivresse synchronisée | L'état 0–100 et ses paliers restent cohérents chez l'hôte et le client | Bloqué par P1-011 et P1-012 |
| P1-014 | Dialogue absurde | Une situation complète produit une réponse et une réaction comiques | Bloqué par P1-012 |

## Entraide

| ID | Tâche | Critère de fin | Statut |
| --- | --- | --- | --- |
| P1-020 | KO | À 100, le joueur tombe, perd ses actions et ne bloque pas la partie de l'autre | Bloqué par P1-013 |
| P1-021 | Portage | Un joueur transporte puis lâche son partenaire KO avec un état réseau cohérent | Bloqué par P1-020 |
| P1-022 | Réveil | Maintenir l'interaction relève le joueur et applique la valeur d'ivresse prévue | Bloqué par P1-020 |
| P1-023 | Playtest entraide | Dyllan valide ou demande une correction du feeling de portage et de réveil | Bloqué par P1-021 et P1-022 |

## Fin de nuit

| ID | Tâche | Critère de fin | Statut |
| --- | --- | --- | --- |
| P1-030 | Chronomètre autoritaire | Les deux clients partagent la même fin de partie | Bloqué par P1-001 |
| P1-031 | Retour collectif | Chaque joueur peut être marqué rentré, y compris lorsqu'il est porté | Bloqué par P1-021 et P1-030 |
| P1-032 | Victoire et échec | Victoire si les deux sont rentrés avant zéro, échec sinon | Bloqué par P1-031 |
| P1-033 | Bilan simple | Les deux joueurs voient la même conclusion et peuvent relancer | Bloqué par P1-032 |

## Stabilisation

| ID | Tâche | Critère de fin | Statut |
| --- | --- | --- | --- |
| P1-040 | Cas réseau critiques | Code invalide, client parti, hôte parti et session pleine ont un résultat documenté | Bloqué par P1-004 |
| P1-041 | Tests de règles | Seuils d'ivresse, KO et victoire disposent de tests automatisés utiles | Bloqué par les règles correspondantes |
| P1-042 | Playtest externe | Un nouveau joueur comprend l'objectif en moins d'une minute | Bloqué par P1-033 |
| P1-043 | Candidate P1 | Build identifié, critères prouvés, défauts connus consignés | Bloqué par P1-040 à P1-042 |

## Conditionnel après boucle complète

Maison simple, clés, portefeuille, téléphone et taxi minimal. Aucun de ces éléments ne doit retarder la boucle boire, KO, entraide et retour.

## Après P1

Voix de proximité, police, missions, économie, substances supplémentaires, statistiques complètes, cartes supplémentaires, quatre joueurs, compagnon IA, cosmétiques et housing.
