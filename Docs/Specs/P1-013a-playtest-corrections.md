# P1-013a Corrections issues du playtest

## Décision de Dyllan

P1-013 n'est pas validé dans son état actuel. Le playtest du 13 octobre 2026 relève trois défauts bloquants :

1. une décision rend la proposition indisponible pour les deux joueurs ;
2. le délai de 20 secondes casse le rythme et laisse l'ivresse redescendre entre deux verres ;
3. l'interface de test est trop petite, dispersée et illisible pour permettre une validation humaine fiable.

La correction reste dans P1-013 et doit être faite sur les changements non commités existants. Ne pas commencer P1-014 ou P1-020.

## Cause confirmée

`DrinkOffer` porte un unique `_unavailableUntil` répliqué. Cette disponibilité appartient donc à l'objet partagé : dès qu'un joueur accepte ou refuse, le même verrou s'applique aux deux joueurs pendant `NpcOfferCooldownSeconds`.

Ce modèle ne correspond pas au prototype demandé. L'objet de test doit permettre d'enchaîner immédiatement les décisions et ne doit jamais bloquer l'autre joueur.

## Correction de la proposition

- Retirer le cooldown de la proposition de verre de `NetworkTest.unity`.
- Supprimer de `DrinkOffer` l'état réseau, le calcul, le visuel et les messages liés à l'indisponibilité temporisée.
- La proposition reste disponible immédiatement après une acceptation ou un refus, pour l'hôte comme pour le client.
- Conserver les validations d'identité, de cible, de portée et de maintien du refus.
- Conserver la dernière décision et le compteur répliqués si cela reste utile au diagnostic.
- `NpcOfferCooldownSeconds` reste dans `TuningConfig`, mais n'est plus consommé par cet objet de prototype. Il sera réévalué avec les vrais PNJ et leur logique de proposition.
- Mettre à jour les règles et tests devenus obsolètes : aucune attente de `RejectedUnavailable` ne doit subsister dans le chemin de cette proposition.

## Correction du seuil KO

À `KoThreshold`, figer l'ivresse à la valeur maximale : la décroissance passive ne reprend pas tant qu'un futur système de réveil n'a pas explicitement modifié la valeur.

P1-013 ne désactive toujours ni le mouvement ni les interactions. Cette règle rend seulement le franchissement stable et observable ; P1-020 ajoutera l'état KO et ses conséquences.

Ajouter ou ajuster les tests purs pour prouver qu'une valeur au seuil KO ne décroît pas et ne dépasse jamais le maximum.

## Interface de validation obligatoire

Rester sur une interface de debug IMGUI pour cette correction, sans construire l'interface finale, mais remplacer l'empilement actuel par une présentation cohérente et lisible :

- fond sombre semi-opaque derrière chaque zone de texte ;
- textes nettement plus grands et contrastés ;
- panneau réseau compact en haut à gauche ;
- panneau Ivresse en haut à droite, avec une ligne ou carte par joueur ;
- pour chaque joueur : libellé `VOUS` ou `AUTRE`, valeur numérique sur 100, nom du palier et barre de progression colorée ;
- invite d'interaction unique en bas au centre : `E — BOIRE` et `MAINTENIR CLIC DROIT — REFUSER` ;
- pendant le refus, remplacer l'invite par une barre de progression large et un libellé clair ;
- feedback bref et lisible après acceptation, refus ou rejet ;
- supprimer les textes techniques en double et tout compte à rebours de disponibilité ;
- adapter les rectangles à la taille de l'écran et vérifier au minimum en 1920×1080 et 1280×720 ;
- mettre les `GUIStyle` en cache, sans allocation de styles à chaque `OnGUI`.

Une solution minimale acceptable consiste à faire de `IntoxicationHud` le présentateur visuel commun et à lui exposer l'état local nécessaire. Éviter une refonte d'architecture : le but est un écran de test lisible, pas un framework UI.

## Critères d'acceptation

- L'hôte peut boire deux fois de suite sans attendre et gagne deux fois le gain configuré.
- Le client peut boire pendant ou immédiatement après la décision de l'hôte.
- La décision d'un joueur ne rend jamais la proposition indisponible pour l'autre.
- Les refus restent autoritaires et un relâchement anticipé ne produit aucune décision.
- Aucune mention de cooldown ou d'indisponibilité n'apparaît dans l'interface de cette scène.
- À `100`, la valeur reste à `100` jusqu'à modification explicite ; les deux instances affichent durablement `Seuil KO`.
- L'hôte et le client voient les mêmes valeurs, paliers et décisions.
- À 1920×1080 et 1280×720, les panneaux ne se chevauchent pas, le texte est lisible et l'invite est immédiatement identifiable.
- Les tests automatiques pertinents passent, Unity compile sans erreur et les consoles hôte/client ne présentent aucune nouvelle erreur.

## Validation demandée à Claude

1. Deux acceptations immédiates successives par l'hôte.
2. Une acceptation du client immédiatement après une acceptation de l'hôte.
3. Un refus complet puis un refus interrompu pour chaque rôle.
4. Montée durable à `100` et observation pendant au moins cinq secondes.
5. Captures ou inspection visuelle de l'interface en 1920×1080 et 1280×720 sur les deux rôles.
6. Régression du mouvement, de la visée, de l'acceptation et du refus.

## Compte rendu attendu

Mettre à jour `Docs/Reports/P1-013-synchronized-intoxication-report.md` : remplacer les anciennes preuves de cooldown, consigner le retour de Dyllan, décrire la correction, reporter les nouveaux tests et indiquer explicitement que la validation visuelle finale par Dyllan reste requise. Ne pas commit ni push.
