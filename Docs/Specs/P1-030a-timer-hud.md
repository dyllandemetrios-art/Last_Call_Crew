# P1-030a Lisibilité du chronomètre

## Retour de playtest

Le chronomètre est fonctionnel et synchronisé, y compris lors d'un essai à trois joueurs, mais il n'est pas assez visible. Les nouveaux systèmes doivent désormais compléter progressivement la même interface cohérente.

## Correction demandée

Modifier uniquement la présentation dans `IntoxicationHud` et `DebugGuiKit` si nécessaire :

- carte `NUIT` centrée en haut, avec le même fond, les mêmes marges et la même typographie que les autres panneaux ;
- temps `MM:SS` en grand format ;
- barre horizontale montrant la proportion de temps restant ;
- état d'attente clairement libellé ;
- dix dernières secondes en orange puis rouge, sans clignotement ;
- état expiré affichant `TEMPS ÉCOULÉ` sur toute la carte ;
- le bandeau de portage et les autres feedbacks restent visibles sans chevauchement ;
- vérifier 1920×1080 et 1280×720, avec deux puis trois cartes joueur.

Exposer depuis `NightTimer` la durée totale configurée si le HUD en a besoin. Ne modifier ni le démarrage, ni l'échéance, ni les `NetworkVariable`, ni les règles réseau.

## Critères d'acceptation

- Le chronomètre attire immédiatement l'œil sans masquer le jeu.
- La barre correspond au temps restant et atteint zéro à l'expiration.
- Attente, déroulement, dix dernières secondes et expiration sont distincts.
- Aucun chevauchement avec Ivresse, réseau, portage, réveil ou KO aux deux résolutions demandées.
- Les valeurs restent identiques entre hôte et clients.
- Compilation, tests et consoles restent sans nouvelle erreur.

## Rapport

Mettre à jour `Docs/Reports/P1-030-authoritative-night-timer-report.md` avec le retour de Dyllan, les captures aux deux résolutions et la confirmation qu'aucune règle réseau n'a changé. Ne pas commit ni push.

## Cycle jour nuit

Aucun asset jour/nuit n'est requis pour cette correction. Le cycle visuel sera étudié après la condition de retour et la durée de partie, d'abord avec la lumière directionnelle et le ciel existants pilotés par la progression du chronomètre.
