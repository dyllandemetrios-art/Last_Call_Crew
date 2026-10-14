# P1-015a Curseur libre et boutons du HUD

## Défaut observé

Dans le build Windows P1-004, `Échap` libère correctement le curseur. Mais le clic gauche destiné au bouton `ARRÊTER` recapture immédiatement la souris dans `PlayerController` avant que le bouton IMGUI reçoive son relâchement. Le bouton n'est donc utilisable que lorsque le bilan de manche a déjà libéré la souris.

## Comportement attendu

- Curseur capturé : `Échap` le libère, sauf lorsque cette pression vient de fermer un dialogue.
- Curseur libre : `Échap` le recapture.
- Un clic gauche lorsque le curseur est libre ne le recapture plus automatiquement ; il reste disponible pour les boutons et champs du HUD.
- Le message d'aide indique clairement `Échap : reprendre la souris` lorsque le curseur est libre.
- Le bilan de manche continue à libérer le curseur et la relance continue à le recapturer comme avant.

Cette correction ne modifie aucune règle réseau, aucun RPC, aucune interaction de gameplay et aucun flux Relay.

## Critères d'acceptation

- En jeu, `Échap` libère la souris puis un clic réel sur `ARRÊTER` déclenche l'arrêt dès la première tentative.
- Quand le curseur est libre, cliquer dans le champ du code, coller avec `Ctrl+V`, puis cliquer sur `REJOINDRE` fonctionne.
- `Échap` recapture ensuite la souris et le regard fonctionne normalement.
- Un `Échap` qui ferme un dialogue ne libère pas la souris dans la même frame.
- Le bouton `RELANCER` du bilan fonctionne toujours et la souris est recapturée après relance.
- Compilation et tests existants sans nouvelle erreur.

## Validation

Tester au minimum dans un build Windows, car le défaut a été observé hors éditeur. Ajouter le résultat dans une section P1-015a du rapport `Docs/Reports/P1-004-windows-relay-build-report.md`. Ne pas commit ni push.
