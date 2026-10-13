# Relais compact — boucle jouable P1

Mis à jour le 13 octobre 2026 après validation de P1-033.

## État de reprise

- Branche : `feature/p1-001-network-foundation`
- Dernier commit fonctionnel : `7e800b1` (`feat: P1-033 add shared summary and restart`)
- Unity : 6000.3.8f1, URP, Netcode for GameObjects 2.13.3, Multiplayer Play Mode 2.0.2
- Scène de travail : `Assets/_Project/Scenes/NetworkTest.unity`
- Autorité : règles et états de gameplay écrits par l'hôte ; entrées lues par le propriétaire
- Périmètre officiellement validé : deux joueurs. Certains parcours ont aussi été essayés à trois, sans en faire une exigence générale.

## Boucle jouable validée

Connexion locale, déplacement, boire/refuser, ivresse synchronisée, KO, portage, réveil, chronomètre, retour collectif, victoire/échec, bilan partagé et relance dans la même session. P1-001, P1-002, P1-011 à P1-013, P1-020 à P1-023 et P1-030 à P1-033 sont PASS. P1-033 totalise 157 tests EditMode réussis et deux relances manuelles validées par Dyllan.

## Contrôles actuels et décision future

Conserver les commandes contextuelles : boire/porter sur `E`, refuser au clic droit maintenu et réveiller sur `F` maintenu. P1-014 ne doit pas les refondre. Pour un PNJ dialogué seulement, `E` ouvre sa proposition puis `1`, `2`, `3`, etc. choisissent une réponse simple. Chaque PNJ pourra avoir ses propres choix plus tard ; ne pas créer de framework universel.

## Limites connues

- Relay et le build Windows à deux joueurs ne sont pas validés : P1-003 et P1-004 restent à faire. P1-040 dépend de ces deux éléments.
- Une reconnexion reçoit un nouvel identifiant ; le roster persistant et les déconnexions en cours de manche restent ouverts.
- Le ressenti de perception (flou, oscillation, couleurs, temps altéré) est différé jusqu'à stabilisation de la boucle et des substances.
- L'impossibilité de refuser à forte ivresse et la confusion progressive des choix sont des idées de design différées.
- L'interface actuelle est un HUD de prototype en IMGUI ; elle est lisible mais n'est pas l'interface finale.
- NGO peut recréer `Assets/DefaultNetworkPrefabs.asset` : supprimer ce doublon avant commit et conserver `Assets/_Project/Prefabs/DefaultNetworkPrefabs.asset`.

## Prochaine décision

Ne pas commencer P1-040 : il est bloqué par P1-003/P1-004. Les prochaines voies utiles sont :

1. P1-003/P1-004 pour sécuriser Relay et les builds réels ; cela demande que Dyllan lie le projet à Unity Cloud.
2. P1-014 pour éprouver le dialogue à choix numérotés et le ton comique.
3. P1-010 pour remplacer la scène de test par une greybox de lieu lisible.

Lire ensuite uniquement la fiche choisie, le rapport précédent pertinent et le diff Git. Aucun push implicite.
