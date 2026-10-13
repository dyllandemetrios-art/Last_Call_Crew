# P1-031a Présence réelle dans la zone de retour

## Défaut observé

Dans `BarGreybox.unity`, un joueur entre dans la zone verte puis en ressort. Le HUD continue de l'afficher « RENTRÉ ». Lorsque le second joueur entre ensuite, la partie annonce une victoire alors qu'un joueur se trouve physiquement dehors.

Comportement attendu : la victoire exige que tous les joueurs soient réellement dans la zone de retour au même moment, avant ou à l'échéance.

## Cause confirmée

`PlayerReturn.ServerMarkReturned` conserve volontairement le premier instant de retour et n'offre aucune transition inverse. `ReturnZone.Update` marque les joueurs présents, mais ne remet jamais dehors ceux qui ont quitté le volume. `NightOutcome` évalue donc des instants historiques au lieu de la présence actuelle.

## Règle corrigée

- Entrer dans la zone enregistre l'instant serveur du début de la présence actuelle.
- Rester dans la zone conserve cet instant sans écriture répétée.
- Sortir avant le résultat efface le retour et le HUD repasse à `ENCORE DEHORS`.
- Rentrer de nouveau enregistre un nouvel instant serveur.
- Un partenaire porté compte dans la zone uniquement tant que son porteur y est réellement.
- Un joueur KO non porté suit sa propre position.
- Dès que la victoire est décidée, P1-033 gèle le résultat et les états comme auparavant.

Conserver l'instant plutôt qu'un simple booléen afin que P1-032 puisse toujours départager proprement les entrées à l'échéance.

## Autorité et coût réseau

L'hôte calcule la présence depuis les positions et l'état de portage existants. Aucun client ne demande à être marqué rentré ou dehors. Écrire la `NetworkVariable` uniquement lors d'une transition entrée/sortie, pas à chaque frame.

Après une relance, conserver la protection existante contre l'ancienne position du client tant que sa pose de départ n'est pas arrivée.

## Critères d'acceptation

- Joueur A entre puis sort : A apparaît dehors sur les deux instances.
- A dehors, joueur B entre : aucun résultat de victoire.
- A rejoint ensuite B avant zéro : victoire.
- A entre, sort puis rentre : son nouvel instant remplace l'ancien.
- Un porteur entre avec son partenaire KO : les deux sont rentrés ; s'il ressort avant la décision, les deux redeviennent dehors.
- Un KO non porté est marqué selon sa position réelle.
- Entrée exactement à l'échéance reste une victoire ; entrée après l'échéance reste un échec.
- Relance, rejoin et HUD ne régressent pas.
- Aucune écriture réseau par frame lorsque la présence ne change pas.
- Compilation, tests et consoles restent sans nouvelle erreur.

## Validation demandée

Tester avec hôte et client dans `BarGreybox.unity` :

1. A entre, ressort complètement, puis B entre ; vérifier l'absence de victoire.
2. A rentre de nouveau pendant que B reste dedans ; vérifier la victoire.
3. Rejouer avec A portant B, puis ressortir avant que le résultat puisse être décidé dans un scénario contrôlé.
4. Vérifier un KO non porté, la relance et un client rejoignant un état en cours.
5. Ajouter des tests de règles ciblés pour entrée, maintien, sortie et nouvelle entrée.

## Hors périmètre

Refonte du volume, nouvelle zone, identité persistante après déconnexion, Relay, modification du nombre de joueurs ou changement du gel terminal.

## Compte rendu attendu

Créer `Docs/Reports/P1-031a-live-return-presence-report.md` avec : reproduction initiale, cause, fichiers modifiés, transitions autoritaires, coût réseau, tests automatiques et manuels, scénarios portage/KO/échéance/rejoin, erreurs, limites et statut PASS, FAIL ou BLOCKED. Ne rien commit ni push.
