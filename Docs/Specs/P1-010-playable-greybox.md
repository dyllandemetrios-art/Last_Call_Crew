# P1-010 Greybox jouable

## Intention

Remplacer l'arène technique comme expérience principale par un petit lieu lisible où deux joueurs peuvent parcourir toute la boucle P1 : entrer dans un bar, interagir, gérer un KO, transporter un partenaire et rejoindre ensemble la zone de retour.

La priorité est la circulation et la compréhension. Utiliser uniquement des primitives, couleurs et matériaux simples. Les packs conservés dans `D:\Assets\LastCallCrew` ne sont pas encore importés.

## Stratégie de scène

Conserver `NetworkTest.unity` comme banc de régression. Créer une scène distincte sous `Assets/_Project/Scenes/` en dupliquant proprement le socle fonctionnel, puis construire la greybox autour de lui. Éviter deux implémentations divergentes : les scripts, prefabs réseau et configurations restent partagés.

La nouvelle scène devient la scène de playtest P1. Ne modifier les Build Settings que si c'est nécessaire pour l'ouvrir en build, et consigner ce changement.

## Parcours minimal

Construire un parcours compact comprenant :

- une zone de départ extérieure clairement séparée de la zone de retour ;
- une entrée de bar immédiatement identifiable ;
- une salle principale avec comptoir, quelques volumes de tables et assez d'espace pour circuler à deux ;
- Marcel et la proposition de boisson, accessibles sans bloquer un passage ;
- une sortie lisible depuis l'intérieur ;
- un chemin extérieur court jusqu'à la zone de retour collective ;
- deux points de départ qui ne se chevauchent pas et ne marquent personne comme rentré.

Utiliser les proportions suivantes comme point de départ réversible : passage principal d'au moins 2,5 m, portes d'au moins 2 m, aucun étranglement empêchant le portage. Ajuster si les dimensions actuelles des capsules l'exigent.

## Lisibilité

- Sol, murs, obstacles, zone de départ, entrée/sortie et zone de retour utilisent des couleurs distinctes.
- La zone de retour reste verte et possède un repère vertical visible de loin.
- Le bar doit être compréhensible sans texte explicatif ni asset final.
- Les volumes purement visuels ne doivent pas créer de collisions invisibles ; les colliders utiles correspondent à leur silhouette simple.
- Les panneaux existants du HUD restent lisibles sur les fonds choisis.

## Intégration des systèmes existants

Tous les comportements validés restent disponibles : déplacement et regard, capture souris, boire/refuser, ivresse, dialogue avec Marcel, KO, portage/lâcher, réveil, chronomètre, retour, bilan et relance.

Ne modifier aucune règle réseau ni durée de gameplay pour compenser le niveau. Les références de scène doivent être vérifiées après la duplication, notamment `NetworkManager`, prefab joueur, `NightTimer`, `NightOutcome`, `NightRound`, zone de retour, HUD et objets réseau de scène.

Après une relance, les joueurs réapparaissent aux points de départ de la greybox et aucun ancien état ne subsiste.

## Critères d'acceptation

- Hôte et client apparaissent sans chevauchement, hors de la zone de retour.
- Les deux comprennent visuellement où entrer, où sortir et où rentrer.
- Chaque joueur peut atteindre la boisson, Marcel et la zone de retour sans saut ni blocage.
- Un joueur debout peut porter un partenaire KO depuis la salle jusqu'à la zone de retour sans rester coincé.
- Un joueur KO non porté peut être réveillé dans la salle.
- Les joueurs ne traversent pas les murs et ne restent pas bloqués contre les volumes simples.
- Une nuit peut se conclure par victoire puis être relancée correctement dans la greybox.
- La scène `NetworkTest.unity` reste disponible et fonctionnelle comme référence technique.
- Compilation, tests et consoles restent sans nouvelle erreur.

## Validation demandée

Avec un hôte et un client :

1. parcourir séparément départ → bar → sortie → retour ;
2. boire et dialoguer depuis les emplacements prévus ;
3. provoquer un KO dans la salle, porter le partenaire jusqu'au retour et obtenir la victoire ;
4. relancer et vérifier les deux points de départ ;
5. réveiller un joueur KO près d'une table et d'un mur pour vérifier l'espace ;
6. tenter les angles, portes et obstacles susceptibles de coincer une capsule.

Faire au maximum trois captures utiles : vue générale, intérieur du bar, parcours de sortie/retour. Le feeling, les distances et la compréhension spatiale finale restent à valider par Dyllan.

## Hors périmètre

Import des packs graphiques, décoration finale, éclairage artistique, cycle jour/nuit visuel, animations, audio, NavMesh, PNJ supplémentaires, nouvelles règles de gameplay, agrandissement de la carte, Relay et optimisation finale.

## Compte rendu attendu

Créer `Docs/Reports/P1-010-playable-greybox-report.md` avec : commit de départ, stratégie de scène, plan et dimensions finales, objets/références modifiés, collisions, parcours testés, systèmes rejoués, captures minimales, tests, erreurs, limites et statut PASS, FAIL ou BLOCKED. Ne rien commit ni push.
