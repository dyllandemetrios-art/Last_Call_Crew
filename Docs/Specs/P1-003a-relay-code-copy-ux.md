# P1-003a Copie du code Relay

## Retour de playtest

La connexion Relay fonctionne au clic réel. En revanche, le bouton `COPIER` est inaccessible lorsque la souris est capturée par la vue du joueur. Le champ peut recevoir le focus avec `Tab`, puis être copié avec `Ctrl+C`, mais ce parcours n'est pas assez évident.

## Correction attendue

- Dès que l'hôte reçoit un nouveau code Relay, le copier automatiquement une seule fois dans le presse-papiers.
- Afficher près du code une confirmation courte : `CODE COPIÉ`.
- Conserver le code visible et le champ sélectionnable au clavier comme solution de secours.
- Supprimer le bouton `COPIER` s'il ne peut pas être utilisé de façon fiable avec la capture actuelle.
- Ne pas libérer automatiquement la souris : le regard doit rester immédiatement jouable et `Échap` conserve son comportement existant.
- Ne modifier ni le flux Relay, ni NGO, ni les règles réseau.

## Critères d'acceptation

- Après `HÉBERGER EN LIGNE`, le presse-papiers contient exactement le nouveau code affiché.
- La confirmation visuelle est lisible sans chevauchement à 1280×720.
- Créer une seconde session remplace le presse-papiers par le nouveau code une seule fois.
- Le champ reste accessible avec `Tab` et `Ctrl+C`.
- La connexion avec le code copié fonctionne encore.
- Compilation et tests existants sans nouvelle erreur.

## Compte rendu

Ajouter une courte section P1-003a dans `Docs/Reports/P1-003-relay-join-code-report.md`. Ne pas créer de nouveau rapport, ne pas commit ni push.
