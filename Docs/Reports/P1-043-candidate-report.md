# P1-043 — Candidate P1 interne

**Statut : READY WITH LIMITATION.** La boucle P1 est jouable et validée techniquement. Le playtest par une personne extérieure (P1-042) est différé faute de participant disponible ; la candidate ne prétend donc pas prouver que l'objectif est compris en moins d'une minute sans accompagnement.

## Version identifiée

- Source : branche `feature/p1-001-network-foundation`, commit `1be60c0`.
- Unity : `6000.3.8f1`.
- Build Windows Development validé : `D:/REPOSITORY/LCC_TestBuild/P1-040/LastCallCrew.exe`.
- Dernier identifiant de build consigné : `d9a23080`.
- Archive de playtest préparée hors Git : `D:/REPOSITORY/LCC_TestBuild/RentrerVivant-P1-Playtest-Windows-x64.zip`.

## Preuves principales

- Relay DTLS avec code : hôte et client réels, nouvelles tentatives et arrêt propre.
- Build Windows : deux processus, vraies entrées système, session stable plus de cinq minutes.
- Cas critiques : jusqu'à trois processus, session pleine, départ client, départ hôte, reconnexion et relance.
- Boucle jouable : déplacement, boisson/refus, ivresse, KO, portage, réveil, chronomètre, retour collectif, victoire/échec et relance.
- Tests : dernière suite EditMode complète, `199/199` réussis ; compilation sans erreur.
- Rapports détaillés : P1-003, P1-004, P1-040 et P1-041 dans `Docs/Reports/`.

## Défauts et limites connus

- Aucun playtest externe naïf : compréhension autonome de l'objectif non prouvée.
- Tests Relay réalisés sur une seule machine et un seul réseau, même si le trafic passe réellement par Relay.
- Une rupture brutale peut demander environ 30 secondes avant détection par le transport.
- Pas de migration d'hôte, reprise d'identité, reconnexion automatique, Lobby ou Matchmaker.
- Interface de prototype en IMGUI, pas une interface finale ; certains libellés secondaires restent serrés dans une fenêtre 960×540.
- Greybox et capsules sans direction artistique finale, animations ou effets de perception avancés.
- Build Development non signé, sans installateur et non destiné à une publication publique.

## Décision

Ces limites n'empêchent pas de poursuivre le développement du contenu du prototype. P1-042 reste à exécuter dès qu'un participant extérieur est disponible, sans immobiliser l'équipe entre-temps. Toute correction issue de ce futur test sera priorisée séparément.
