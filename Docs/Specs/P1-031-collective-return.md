# P1-031 Retour collectif

## Intention

Permettre à chaque joueur d'être enregistré comme rentré avant la fin de la nuit, y compris lorsqu'il franchit la sortie en étant porté. Cette tâche crée l'état de retour ; P1-032 décidera ensuite victoire ou échec.

## Comportement observable

- Une zone de retour clairement visible existe dans `NetworkTest.unity`.
- Lorsqu'un joueur entre dans la zone, l'hôte l'enregistre comme rentré avec l'heure serveur.
- Si ce joueur porte son partenaire KO, les deux sont enregistrés au même passage.
- Le HUD affiche pour chaque joueur `ENCORE DEHORS` ou `RENTRÉ`.
- L'état est irréversible pour cette partie et identique sur toutes les instances.

P1-031 ne bloque pas encore les contrôles et n'affiche aucun résultat final. Cela évite un verrouillage prématuré avant P1-032.

## Autorité réseau

- Ajouter un composant joueur dédié portant un instant de retour répliqué ; utiliser une valeur sentinelle explicite pour « pas encore rentré ».
- Seul l'hôte écrit cet instant, une seule fois par joueur.
- La zone détecte le passage côté hôte et ne fait confiance à aucune demande client.
- Pour un porteur, l'hôte résout le joueur porté depuis l'état P1-021 et marque les deux.
- Conserver l'instant serveur, pas seulement un booléen, afin que P1-032 puisse comparer le retour à l'échéance du chronomètre.
- Un nouvel arrivant reçoit les états existants sans RPC de rattrapage.

## Présentation minimale

- Créer une sortie de greybox visible et identifiable, avec un trigger distinct de son visuel.
- Ajouter au HUD un état de retour compact sur chaque carte joueur.
- Afficher un feedback bref lorsque le joueur local ou son partenaire rentre.
- Ne pas créer d'écran de victoire ou d'échec.

## Critères d'acceptation

- Hôte et client peuvent chacun être marqués rentrés en entrant dans la zone.
- Un passage marque exactement une fois le joueur et conserve son premier instant de retour.
- Un porteur entrant dans la zone marque aussi son partenaire porté, même si le collider de celui-ci est désactivé.
- Un joueur KO non porté peut être marqué s'il se trouve réellement dans la zone.
- Les états et instants sont identiques sur les instances.
- Un rejoin tardif reçoit les retours déjà enregistrés.
- Le chronomètre, le KO, le portage et le réveil ne régressent pas.
- L'expiration du chronomètre ne déclenche encore aucun résultat dans cette tâche.
- Compilation, tests et consoles restent sans nouvelle erreur.

## Validation demandée

1. Retour individuel de l'hôte puis du client.
2. Nouvelle partie de test : retour du porteur avec le partenaire KO porté.
3. Entrées répétées dans la zone : l'instant initial ne change pas.
4. Rejoin après un retour enregistré.
5. Passage avant et après l'expiration pour préparer la preuve de P1-032.

## Hors périmètre

Victoire, échec, écran de résultat, blocage des contrôles, relance, maison détaillée, porte animée, chargement de scène, Relay et build autonome.

## Compte rendu attendu

Créer `Docs/Reports/P1-031-collective-return-report.md` avec : commit de départ, fichiers modifiés, modèle d'autorité, représentation de l'instant, zone et trigger, retour individuel, retour porté, rejoin, interaction avec le chronomètre, tests, erreurs, limites et statut PASS, FAIL ou BLOCKED. Ne pas commit ni push.
