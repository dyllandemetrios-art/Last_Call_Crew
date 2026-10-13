# P1-030 Chronomètre autoritaire

## Intention

Créer la pression temporelle de la nuit avec un chronomètre partagé. L'hôte décide du départ et de l'échéance ; les deux joueurs voient le même temps restant sans répliquer une valeur à chaque image.

## Règles provisoires

- Ajouter `NightDurationSeconds` au `TuningConfig`, valeur provisoire `300` secondes.
- Le chronomètre démarre une seule fois lorsque deux joueurs sont connectés.
- Une déconnexion client ne met pas le temps en pause.
- Un client qui rejoint en cours de partie reçoit l'échéance actuelle.
- À zéro, l'état devient `Expired` et reste stable.
- P1-030 ne décide ni victoire ni échec et ne bloque pas les contrôles.

La durée de cinq minutes sert au prototype et reste réglable par Dyllan.

## Autorité réseau

- Ajouter un `NightTimer` réseau dans `NetworkTest.unity`.
- L'hôte écrit un état durable minimal : attente, en cours ou expiré, ainsi que l'échéance en temps serveur.
- Les clients calculent le temps restant avec `NetworkManager.ServerTime` ; ne pas répliquer un compteur chaque frame.
- Le démarrage et l'expiration sont idempotents.
- Un nouvel arrivant obtient l'état par les `NetworkVariable`, sans RPC de rattrapage.

## Présentation

- Étendre le HUD existant avec un chronomètre `MM:SS` centré en haut.
- Avant deux joueurs : `EN ATTENTE DU PARTENAIRE`.
- Pendant la partie : temps restant clairement lisible.
- À zéro : `TEMPS ÉCOULÉ`.
- Les changements visuels des dix dernières secondes restent simples et non agressifs ; aucun son requis.

## Critères d'acceptation

- À deux joueurs, le chronomètre démarre une seule fois à la durée configurée.
- Hôte et client affichent le même temps à une seconde près.
- Le temps atteint zéro sans devenir négatif et l'état expiré est identique partout.
- Une déconnexion du client ne remet ni en pause ni à zéro le chronomètre.
- Un rejoin tardif reçoit le temps restant actuel.
- Aucun trafic réseau continu n'est produit pour chaque seconde affichée.
- Les systèmes boire, ivresse, KO, portage et réveil continuent à fonctionner pendant et après l'expiration.
- Compilation, tests et consoles restent sans nouvelle erreur.

## Validation demandée

1. Démarrage avec hôte seul puis arrivée du client.
2. Comparaison hôte/client pendant au moins trente secondes.
3. Test accéléré jusqu'à zéro sans modifier définitivement l'asset.
4. Déconnexion puis reconnexion pendant le chronomètre.
5. Vérification d'une interaction après expiration.

## Hors périmètre

Condition de retour, victoire, échec, écran de résultat, relance, pause, synchronisation avec l'heure réelle, Relay et build autonome.

## Compte rendu attendu

Créer `Docs/Reports/P1-030-authoritative-night-timer-report.md` avec : commit de départ, fichiers modifiés, valeur provisoire, modèle réseau, démarrage, expiration, déconnexion/rejoin, coût réseau, tests, erreurs, limites et statut PASS, FAIL ou BLOCKED. Ne pas commit ni push.
