# P1-020a Caméra KO

## Retour de playtest

Le KO est fonctionnel et lisible, mais le propriétaire ne ressent pas la chute : la caméra reste dans la position première personne et le message masque le fait que son corps est au sol. L'observateur voit correctement la capsule couchée.

La mécanique qui empêche de refuser à forte ivresse n'appartient pas à P1-020. Elle reste reportée à une future itération sur les choix altérés.

## Correction demandée

Au passage en KO, déplacer progressivement la caméra du propriétaire vers une vue extérieure reculée qui cadre son propre corps couché :

- transition courte et lisible, provisoirement autour de `0,35` seconde ;
- caméra derrière et au-dessus du joueur, orientée vers le corps ;
- corps du propriétaire visible dans le cadre ;
- aucun contrôle de caméra pendant le KO ;
- aucun changement de caméra pour l'autre joueur ;
- aucun changement de la racine réseau, du collider ou de l'autorité ;
- paramètres de position, rotation et durée sérialisés pour permettre un réglage simple ;
- restauration propre de la pose première personne lors de `ServerExitKnockout()`, afin de préparer P1-022.

La caméra peut rester enfant du joueur pour ce prototype. Ne pas ajouter de système de collision caméra, Cinemachine, animation, ragdoll ou nouvelle dépendance.

## Interface

Conserver le voile, le HUD et le texte `VOUS ÊTES KO`, mais vérifier que le texte ne cache pas entièrement le corps. Réduire ou déplacer légèrement le message si nécessaire. Ne pas reconstruire l'interface.

## Critères d'acceptation

- Le propriétaire voit la caméra reculer au moment où il devient KO.
- La vue finale montre clairement sa propre capsule couchée.
- La transition ne produit ni saut de la racine réseau, ni déplacement du collider, ni contrôle résiduel.
- L'autre joueur conserve sa caméra habituelle.
- L'observateur voit toujours la capsule couchée.
- Une sortie de KO injectée restaure la caméra première personne et ses contrôles.
- Le résultat reste lisible en 1920×1080 et 1280×720.
- Unity compile, les tests existants passent et les consoles restent sans nouvelle erreur.

## Rapport

Mettre à jour `Docs/Reports/P1-020-knockout-report.md` avec le retour de Dyllan, les paramètres retenus, les preuves hôte/client et la restauration de caméra. Ne pas commit ni push.
