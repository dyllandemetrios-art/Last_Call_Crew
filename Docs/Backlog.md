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
| P1-010 | Créer la greybox | Deux joueurs apparaissent dans un petit lieu avec entrée et sortie lisibles | PASS validé par Dyllan le 13 octobre 2026 |
| P1-011 | Créer le TuningConfig | Gain et décroissance d'ivresse, seuils, KO, refus et réveil sont réglables | PASS au commit `4757202` |
| P1-012 | Boire et refuser | Le choix est autorisé par l'hôte et possède un feedback sur les deux instances | PASS validé par Dyllan le 13 octobre 2026 |
| P1-013 | Ivresse synchronisée | L'état 0–100 et ses paliers restent cohérents chez l'hôte et le client | PASS validé par Dyllan le 13 octobre 2026 |
| P1-014 | Dialogue absurde | Une situation complète produit une réponse et une réaction comiques | PASS validé par Dyllan le 13 octobre 2026 ; défaut de souris consigné |
| P1-015 | Capture souris et refus | La vue reste contrôlable et le clic droit maintenu fonctionne dans chaque instance de test | PASS validé par Dyllan le 13 octobre 2026 |

## Entraide

| ID | Tâche | Critère de fin | Statut |
| --- | --- | --- | --- |
| P1-020 | KO | À 100, le joueur tombe, perd ses actions et ne bloque pas la partie de l'autre | PASS validé par Dyllan le 13 octobre 2026 |
| P1-021 | Portage | Un joueur transporte puis lâche son partenaire KO avec un état réseau cohérent | PASS validé par Dyllan le 13 octobre 2026 |
| P1-022 | Réveil | Maintenir l'interaction relève le joueur et applique la valeur d'ivresse prévue | PASS validé par Dyllan le 13 octobre 2026 |
| P1-023 | Playtest entraide | Dyllan valide ou demande une correction du feeling de portage et de réveil | PASS le 13 octobre 2026 |

## Fin de nuit

| ID | Tâche | Critère de fin | Statut |
| --- | --- | --- | --- |
| P1-030 | Chronomètre autoritaire | Les deux clients partagent la même fin de partie | PASS validé par Dyllan le 13 octobre 2026 |
| P1-031 | Retour collectif | Chaque joueur peut être marqué rentré, y compris lorsqu'il est porté | PASS validé par Dyllan le 13 octobre 2026 |
| P1-031a | Présence réelle au retour | Un joueur sorti de la zone redevient dehors et ne peut pas provoquer une fausse victoire | PASS validé par Dyllan le 13 octobre 2026 |
| P1-032 | Victoire et échec | Victoire si les deux sont rentrés avant zéro, échec sinon | PASS validé par Dyllan le 13 octobre 2026 |
| P1-033 | Bilan simple | Les deux joueurs voient la même conclusion et peuvent relancer | PASS validé par Dyllan le 13 octobre 2026 |

## Stabilisation

| ID | Tâche | Critère de fin | Statut |
| --- | --- | --- | --- |
| P1-040 | Cas réseau critiques | Code invalide, client parti, hôte parti et session pleine ont un résultat documenté | Bloqué par P1-004 |
| P1-041 | Tests de règles | Seuils d'ivresse, KO et victoire disposent de tests automatisés utiles | Bloqué par les règles correspondantes |
| P1-042 | Playtest externe | Un nouveau joueur comprend l'objectif en moins d'une minute | Bloqué par P1-033 |
| P1-043 | Candidate P1 | Build identifié, critères prouvés, défauts connus consignés | Bloqué par P1-040 à P1-042 |

## Conditionnel après boucle complète

Maison simple, clés, portefeuille, téléphone et taxi minimal. Aucun de ces éléments ne doit retarder la boucle boire, KO, entraide et retour.

## Décision d'interaction pour P1-014

Conserver les commandes contextuelles existantes : boire/porter sur `E`, refuser au clic droit maintenu et réveiller sur `F` maintenu. P1-014 ne crée pas de système universel et ne modifie pas ces commandes. Pour un PNJ dialogué, `E` ouvre sa proposition puis `1`, `2`, `3`, etc. sélectionnent une réponse simple. Chaque PNJ pourra proposer ses propres bêtises plus tard.

## Après P1

Voix de proximité, police, missions, économie, substances supplémentaires, statistiques complètes, cartes supplémentaires, quatre joueurs, compagnon IA, cosmétiques et housing.
