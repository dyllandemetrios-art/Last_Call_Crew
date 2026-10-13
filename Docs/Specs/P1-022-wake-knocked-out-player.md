# P1-022 Réveil d'un partenaire KO

## Intention

Fermer la boucle d'entraide : un partenaire peut réveiller manuellement un joueur KO avec de l'eau, tandis qu'un joueur abandonné finit par se réveiller automatiquement. L'hôte décide du réveil et de la nouvelle valeur d'ivresse.

## Comportement observable

- En visant un partenaire KO au sol, maintenir `F` remplit une jauge de réveil.
- Après la durée configurée, le joueur se relève avec `DrunkennessAfterWaterWake`.
- Relâcher trop tôt, perdre la cible ou sortir de portée annule le réveil.
- Un joueur KO non aidé se réveille automatiquement après `AutoWakeSeconds` avec `DrunkennessAfterAutoWake`.
- Le joueur réveillé retrouve mouvement, caméra et interactions.

`E` reste réservé au portage. Le joueur doit être lâché avant un réveil manuel.

## Autorité réseau

- Ajouter une action Input System `Wake`, liée provisoirement à `F`.
- Le client propriétaire signale début et fin du maintien ; l'hôte mesure lui-même la durée.
- L'hôte valide : émetteur debout, cible distincte et KO, cible non portée, portée correcte et maintien suffisant.
- L'instant du KO sert de référence au réveil automatique côté hôte.
- Le réveil modifie dans un même chemin autoritaire l'état KO et l'ivresse.
- Un seul réveil peut gagner si manuel et automatique arrivent presque simultanément.
- Les nouveaux clients reçoivent l'état courant sans RPC de rattrapage.

## Règles

- Réveil à l'eau : maintien `WaterWakeHoldSeconds`, ivresse finale `DrunkennessAfterWaterWake`.
- Réveil automatique : délai `AutoWakeSeconds`, ivresse finale `DrunkennessAfterAutoWake`.
- Aucune valeur n'est dupliquée en littéral hors valeurs techniques d'affichage.
- Un réveil remet la décroissance passive en fonctionnement à partir de la nouvelle valeur.
- Un joueur réveillé n'est plus porté et retrouve son collider et son `NetworkTransform`.
- Eau illimitée pour ce prototype : aucun inventaire ni consommation d'objet.

## Présentation

- HUD : `MAINTENIR F — RÉVEILLER AVEC DE L'EAU` en visant un KO au sol.
- Pendant le maintien : grande barre et libellé `RÉVEIL EN COURS`.
- Joueur KO : afficher le temps restant avant le réveil automatique.
- Succès : feedback bref pour les deux joueurs ; annulation : feedback local clair.
- La transition visuelle inverse et le retour caméra utilisent P1-020a.

## Critères d'acceptation

- Hôte et client peuvent chacun réveiller l'autre par maintien complet.
- Un relâchement anticipé, une perte de cible, une sortie de portée ou une cible portée annule sans réveil.
- La durée est mesurée par l'hôte et vient du `TuningConfig`.
- Le réveil manuel fixe exactement l'ivresse à `50` avec la configuration actuelle.
- Sans aide, le réveil automatique survient à `45` secondes et fixe l'ivresse à `90`.
- Manuel et automatique ne déclenchent jamais deux réveils.
- Après réveil : joueur debout, contrôles restaurés, caméra première personne, collider et synchronisation actifs.
- L'autre joueur reste libre pendant tout le processus.
- Portage, KO, boire/refuser et ivresse ne régressent pas.
- Compilation, tests et consoles restent sans nouvelle erreur.

## Validation demandée

1. Réveil manuel hôte vers client, puis client vers hôte.
2. Relâchement avant trois secondes et perte de visée.
3. Tentative sur joueur porté, debout, hors portée et depuis un joueur KO.
4. Réveil automatique complet avec le délai réel de 45 secondes.
5. Course entre fin du maintien manuel et réveil automatique.
6. Vérification des valeurs 50 et 90 sur les deux instances.
7. Déplacement, caméra et interaction après chaque réveil.

## Hors périmètre

Inventaire d'eau, animation, son, particules, quantité limitée, autres objets de soin, revive après déconnexion, Relay, build autonome et effets visuels d'ivresse.

## Compte rendu attendu

Créer `Docs/Reports/P1-022-wake-report.md` avec : commit de départ, fichiers modifiés, autorité, mesure du maintien, arbitrage manuel/automatique, valeurs d'ivresse, restauration des systèmes, tests hôte/client, erreurs, limites et statut PASS, FAIL ou BLOCKED. Ne pas commit ni push.
