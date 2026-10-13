# P1-014 Premier dialogue absurde

## Intention

Valider une interaction courte et comique avec un PNJ : le joueur vise le PNJ, ouvre sa proposition avec `E`, choisit une réponse avec `1`, `2` ou `3`, puis voit une réaction claire. Cette fiche éprouve le rythme et la lisibilité d'un dialogue, pas un système de quêtes ni un framework générique.

## Décision de commandes

Ne pas modifier les commandes existantes :

- boire et porter restent sur `E` selon la cible ;
- refuser reste sur le clic droit maintenu ;
- réveiller reste sur `F` maintenu ;
- leurs durées et validations actuelles ne changent pas.

Sur le PNJ de P1-014 uniquement, `E` ouvre la proposition. `1`, `2` ou `3` choisit immédiatement une réponse. Aucun maintien n'est requis dans ce premier dialogue.

## Situation de test

Ajouter un PNJ greybox distinct de la proposition de boisson, dans `NetworkTest.unity`. Il présente une courte proposition absurde et trois réponses très brèves aux intentions différentes : accepter la bêtise, la détourner, ou refuser avec humour.

Les textes sont du contenu de prototype : les exposer dans le composant du PNJ ou dans une petite donnée locale afin que Dyllan puisse les modifier sans toucher à la logique. Ne pas créer de base de données de dialogues ni de graphe narratif.

## Comportement observable

- À portée et en visant le PNJ, une invite lisible indique `E — PARLER`.
- `E` ouvre une carte avec la proposition et trois choix numérotés.
- `1`, `2` ou `3` ferme les choix et affiche brièvement la réaction correspondante du PNJ.
- Un choix valide est accepté une seule fois et apparaît de manière cohérente chez les deux joueurs.
- Le joueur qui n'est pas dans le dialogue reste libre de bouger et d'agir.
- Le joueur dialoguant peut fermer sans choisir avec `Échap`, en s'éloignant ou en perdant la cible ; aucune réaction n'est alors produite.
- Le dialogue se ferme si le joueur devient KO, est porté, si la manche se termine ou si une relance commence.

## Autorité et portée réseau

Le propriétaire lit `E`, `1`, `2`, `3` et `Échap`. L'hôte valide le choix en vérifiant au minimum l'émetteur, le PNJ ciblé, la portée, l'état du joueur et l'état de la manche. Le client ne décide pas seul de la réaction.

La proposition et les choix sont affichés seulement au joueur concerné. La réaction validée du PNJ est répliquée et visible par les deux joueurs pendant une courte durée. Deux joueurs peuvent parler au PNJ successivement ; aucune indisponibilité globale ni cooldown n'est ajouté.

Réutiliser les contrôles de portée, d'identité, de KO/portage et de gel existants. Ne pas modifier l'autorité des systèmes déjà validés.

## Interface minimale

Réutiliser le style du HUD de prototype : carte centrée en bas, proposition courte, puis trois lignes `1 — ...`, `2 — ...`, `3 — ...`. La carte NUIT, le panneau Ivresse, les messages KO et le bilan restent lisibles. Ne pas reconstruire l'interface entière.

Les captures sont limitées à deux : une carte de choix ouverte et une réaction validée, à la résolution la plus contraignante utile. Les tests et journaux servent pour les autres preuves.

## Critères d'acceptation

- L'hôte et le client peuvent chacun ouvrir le dialogue et sélectionner les trois réponses.
- Chaque choix produit exactement la réaction qui lui correspond, identique sur les deux instances.
- Une touche numérique sans dialogue ouvert ne produit rien.
- Un choix hors portée, par un joueur KO, porté, gelé ou avec une cible invalide est rejeté par l'hôte.
- Fermer ou interrompre le dialogue ne produit aucune réaction.
- Le dialogue d'un joueur ne bloque ni les déplacements ni les interactions de l'autre.
- Boire, refuser, porter et réveiller conservent leurs commandes et comportements actuels.
- La relance de manche efface toute carte ou réaction résiduelle.
- Unity compile sans erreur, les tests ciblés passent et les consoles ne présentent aucune nouvelle erreur.

## Validation demandée

Avec un hôte et un client :

1. ouvrir puis annuler le dialogue sur chaque rôle ;
2. sélectionner chacun des trois choix et vérifier la réaction sur les deux écrans ;
3. tenter un chiffre sans dialogue, puis un choix hors portée ;
4. interrompre par perte de cible, KO, portage et fin de manche ;
5. vérifier les interactions existantes avant et après le dialogue ;
6. relancer une manche et confirmer l'absence d'état résiduel.

Le caractère réellement drôle des textes et le rythme de lecture restent une validation de Dyllan, distincte du PASS technique.

## Hors périmètre

Framework général de dialogue, arbre de conversation, quêtes, conséquences de gameplay, animations, voix, caméra cinématique, localisation, nouveaux maintiens, modification des choix selon l'ivresse, confusion des choix, inventaire, sauvegarde et Relay.

## Compte rendu attendu

Créer `Docs/Reports/P1-014-first-npc-dialogue-report.md` avec : commit de départ, fichiers modifiés, textes configurés, modèle d'autorité, interruptions, tests, captures minimales, régressions vérifiées, erreurs, limites et statut PASS, FAIL ou BLOCKED. Ne rien commit ni push.
