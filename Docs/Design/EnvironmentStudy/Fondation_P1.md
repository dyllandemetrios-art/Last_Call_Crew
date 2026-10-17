# Fiche proposée — fondation environnementale P1

> **Reprise du 17 octobre 2026 : cette fiche décrit le premier incrément historique, déjà dépassé par les travaux P1-054 à P1-058. Ne pas la rejouer.** Pour la demande actuelle, utiliser [Bars_compacts.md](Bars_compacts.md) puis [Rues_et_impasses.md](Rues_et_impasses.md). La décision récente de ville ouverte et celle des petits bars thématiques remplacent les anciennes demandes de fermeture systématique et de réemploi intégral du bar ci-dessous. Préserver la scène et l'habillage construits par Claude.

**Statut : orientation retenue, fiche révisée le 14 octobre 2026.** Dyllan apprécie le plan et demande davantage de poches dans la ruelle, des offres de substances accessibles, y compris par le taxi, et une priorité absolue à la jouabilité en greybox. Auteur de l'étude : Codex. Implémentation ultérieure : Claude Code dans son checkout, après intégration de la fiche retenue par le chat principal. Ce retour ne change pas le mandat en lecture seule de cette étude. Aucun message envoyé à Claude et aucune mutation du jeu effectuée ici.

## Intention observable

À deux, quitter la coloc, atteindre le bar en moins d'une minute de marche guidée, identifier un chemin plus court au retour, aider un partenaire et rentrer avant zéro. La scène est continue et la ville fixe. La préparation à la modularité reste spatiale. Les temps sont des cibles de test ; ils ne deviennent pas des règles réseau.

**Décisions déjà validées par Dyllan :** scène continue, ville fixe P1, marche possible, taxi gagnant du temps dans la même scène, modules raccordables à terme, greybox prioritaire. Direction des Deux-Lanternes retenue avec davantage de rencontres dans la ruelle et des offres de substances accessibles, taxi compris ; lieux/PNJ/missions progressivement enrichis à terme. **Hypothèses réversibles :** trois poches de 6 × 6 m, emprise 220 × 130 m, métrages, réserve de roulage de 12 s, un seul bar ouvert, poches sans interaction dans le premier incrément environnemental et canal décoratif.

## Avant d'éditer

Relire AGENTS local, Git status/diff et le dernier rapport de Claude. Au début de l'étude : branche `feature/p1-001-network-foundation`, commit `38bfd78bcb5517eb7d5a736a3fa2794a73e466ad`. Téléphone P1-053 en cours dans la scène et les scripts ; cet état n'est pas un point de retour arrière propre. Le snapshot de fin d'étude est dans `preuves/git_fin.txt`. Recontrôler les valeurs de `Player.prefab`, `TuningConfig.asset` et la scène avant tout travail : Claude peut les faire évoluer entre-temps.

Unity 6000.3.8f1, URP 17.3.0, NGO 2.13.3. Aucun nouvel outil, package ou autre projet Unity. Si un outil d'éditeur est utilisé, lire sa vraie liste d'outils et identifier le projet ouvert avant mutation. Aucun MCP Unity n'était appelable dans la session de cette étude.

## Première étape courte : le trajet mesurable

Un seul incrément : **coloc conservée + bar déplacé + deux chemins piétons avec trois poches de ruelle + emprise du taxi tracée**. Les ruelles nord, réserves et façades détaillées de la carte restent des indications pour la suite, pas une exigence de ce premier incrément. Arrêter l'étape dès que les mesures et régressions ci-dessous sont disponibles. Les assets sont facultatifs : primitives et capsules identifiées suffisent ; aucun manque artistique ne bloque ce test.

1. Dans `Assets/_Project/Scenes/BarGreybox.unity`, conserver la maison, sa porte et `ReturnZone`, les spawns, les objets clés/portefeuille/téléphone et toutes les références gameplay. Maison actuelle : intérieur x=15…25, z=11…21 ; porte (15,16). Spawns `(21,1,15.2)` et `(21,1,16.8)`, yaw=270. **Aucun changement de `RestartRules` nécessaire.**
2. Réemployer le bar actuel de 17 × 16 m. Après vérification des coordonnées, translation proposée **(−131,5 ; 0 ; +28)** pour son sol, ses murs, meubles, Marcel, DrinkOffer, portes, tapis et lumières attachées au lieu ; ne pas déplacer la coloc, le sol global, la zone de retour ou les gestionnaires réseau. L'entrée actuelle centrée `(1.5,8)` devient `(-130,36)` en XZ ; la sortie est `(10,16)` devient `(-121.5,44)`. Pas de duplication de NetworkObject ou de deuxième implémentation d'un bar.
3. Étendre le sol plat et déplacer uniquement les anciennes clôtures qui bloquent les nouveaux trajets. Dessiner le chemin principal et le raccourci avec des primitives, puis fermer leurs îlots par des volumes visibles simples. Ajouter R1 (x −104…−98, z 32…38), R2 (x −94…−88, z 42…48), R3 (x −82…−76, z 32…38) : trois renfoncements de 6 × 6 m, ouverts sur la ruelle, passage central de 4 m inchangé. Reculer l'îlot nord au-delà de z=48 pour R2. Conserver un accès de 3 m à la sortie est. Aucun terrain ouvert autorisant la diagonale maison→bar à travers les îlots.
4. Poser les trois repères : maison verte existante, carrefour bleu, entrée ambre. Tracer A/B et l'emprise de route avec des primitives. **Pas de véhicule mobile, d'appel taxi ou de condition d'embarquement dans cet incrément.** Une route réservée ne valide pas le futur taxi.

Fichiers visés : la scène de playtest et éventuellement des matériaux greybox simples, plus le rapport intégré ultérieurement à `Docs/Reports/` par l'auteur. Les prefabs, règles, timer et `NetworkTest.unity` ne nécessitent pas de changement. Si une référence ou une contrainte gameplay empêche cette approche, documenter le blocage au lieu de refondre le système.

### Axes à construire (X ; Z), en mètres

- Principal : `(15,16) → (0,16) → (0,40) → (−70,40) → (−70,60) → (−110,60) → (−110,30) → (−130,30) → (−130,36)` : **225 m**.
- Court : `(15,16) → (0,16) → (0,40) → (−70,40) → (−110,40) → (−110,30) → (−130,30) → (−130,36)` : **185 m**.
- Sortie est : `(-121.5,44) → (-110,44)`, rejoint la rue puis la ruelle : **164,5 m jusqu'à la porte de la coloc**.
- Route réservée : A `(0,16) → (0,−4) → (−130,−4) → (−130,16)` B. Axe de **170 m**, à arrondir ultérieurement aux virages. Aucun pilotage ou test de roulage requis maintenant.
- Porte du bar→B : 20 m ; porte coloc→A : 15 m. A/B sont les nœuds métriques, les positions exactes d'embarquement seront latérales dans des dégagements de 6 × 8 m.

Largeurs : promenade 6 m, rue nocturne 8 m, ruelle 4 m, portes et raccord de sortie 3 m. Route 6 m + trottoirs 3 m de chaque côté. Sol plat, racines joueurs à la hauteur actuelle. Colliders simples, décor fin sans collision.

## QA définie avant construction

Direction ultérieure à préserver : rencontres et offres de Poudre de Perlimpinpin, Champignons de Mémé, Bonbons Câlins, Fumée de Dragon et Tisane de Morphée dans la ruelle et par le taxi ; davantage de lieux/PNJ pourront fournir conversations et missions. Pour cet incrément, tester seulement leur capacité spatiale avec des capsules inertes si utile. Les effets, transactions et missions feront l'objet de règles distinctes ; aucune consommation obligatoire pour traverser ou prendre le taxi n'est proposée.

| Contrôle | Preuve à conserver / critère |
|---|---|
| Parcours à deux avec entrées réelles | Hôte et client parcourent maison→entrée principale→sortie est→maison, sans saut, mur traversé ou blocage. Ne pas remplacer cette preuve par une téléportation. |
| Chronométrage seul | Une mesure par axe, du seuil au seuil, sans pause volontaire. Attentes de calcul : 45 s principal, 37 s court, 33 s sortie est. Écart de plus de 15 % = analyser géométrie/contrôle, pas modifier le timer. Une mesure utilisateur plus lente est consignée séparément. |
| Entraide | KO provoqué au bar par le mécanisme de test existant, prise en charge, sortie de 3 m, virage et ruelle. Noter l'instant du réveil auto ; poursuivre debout si nécessaire. Test court de portage/lâcher/réveil à un angle et à la porte de la coloc avant les 45 s. |
| Poches R1–R3 | Avec une capsule de PNJ dans chaque poche, deux joueurs peuvent s'arrêter, déposer/reprendre un partenaire et rejoindre la ruelle. Un joueur qui continue garde 4 m de passage, sans interaction forcée ni traversée possible entre les poches par l'îlot. |
| Règles de maison et relance | Pas de victoire au spawn ; retour activé après sortie de tous ; présence réelle simultanée au retour ; sortie du trigger efface la présence ; relance remet joueurs et objets domestiques dans leur état prévu. |
| Lisibilité, Dyllan | Sans carte ouverte, retrouver la coloc depuis la sortie du bar ; expliquer où est le taxi et distinguer une façade fermée d'une entrée jouable. Relever chaque hésitation de plus de 5 s et les sections ressenties comme vides. Ce jugement n'est pas un test automatique. |
| Compilation / réseau | Console Unity sans nouvelle erreur ; références scène/prefab intactes ; hôte et client sur la même scène. Si régression de règles observée, rejouer les tests concernés avec la commande déjà validée dans le projet. Ne pas inventer de commande CLI. |

Rapport demandé : commit/branche réellement utilisés, fichiers et positions modifiés, version Unity, durée des parcours, manipulations exactes, résultats hôte/client, chemin des logs/rapports, une vue du dessus et au maximum une capture d'un point problématique. Si compilation/tests sont exécutés : consigner commande exacte ou opération d'éditeur réelle, résultat et fichier de rapport. Ne pas copier les anciens PASS comme résultat de ce niveau.

**PASS** seulement si tous les critères de l'incrément sont prouvés ; **FAIL** pour un défaut observé ; **BLOCKED** pour outil/accès/validation manquants. Si le feeling reste à apprécier, l'écrire explicitement, sans PASS global gameplay. La conduite du taxi, le rendu des packs et la génération procédurale ne sont pas validés par cet incrément.

## Retour arrière et relais

Avant construction, conserver une sauvegarde locale de la scène correspondant à l'état de Claude, hors du jeu si nécessaire, ou un checkpoint ciblé seulement s'il est autorisé. En cas de retour arrière, restaurer uniquement les changements de cet incrément avec l'auteur ; jamais `reset --hard`, nettoyage global ou restauration vers le commit de début d'étude qui écraserait P1-053. Aucun commit/push implicite.

Après mesure, la prochaine décision utile est simplement : **garder ces distances, les raccourcir ou les rallonger**. L'habillage vient ensuite. La modularité signifie ici des morceaux partageant des passages et dimensions compatibles ; elle n'exige pas un générateur.
