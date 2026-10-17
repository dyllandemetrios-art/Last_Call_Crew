# Contexte et limites de preuve — 14 octobre 2026

## Périmètre respecté

Sources lues : `D:/AI-GameWorkflow/AGENTS.md`, `Docs/Engines.md`, `Docs/Tools.md` ; puis `D:/REPOSITORY/Last_Call_Crew/AGENTS.md`, `CLAUDE.md`, README, Backlog, contexte Unity et relais P1, fiches P1-010/P1-030/P1-050 et rapports utiles P1-010/P1-050. Inspection ciblée des réglages, scène et contrôleurs. Les commandes Git utilisent `--no-optional-locks` pour éviter le rafraîchissement facultatif de l'index.

Les seules créations/éditions de cette étude sont dans `D:/REPOSITORY/LCC_Environment_Study`. Aucun appel Unity, lancement de Play Mode, import de pack, installation, compilation, commit, push ou message inter-agent. Les sources ZIP ont été ouvertes en lecture ; seuls leurs inventaires, licences et aperçus ont été copiés dans ce dossier. Les prévisualisations source ont été assemblées, sans prétendre les avoir rendues dans Unity.

## Git observé au départ

Branche : `feature/p1-001-network-foundation`.
HEAD : `38bfd78bcb5517eb7d5a736a3fa2794a73e466ad`.

Modifiés :

- `Assets/_Project/Scenes/BarGreybox.unity`
- `Assets/_Project/Scripts/Runtime/Network/IntoxicationHud.cs`
- `Assets/_Project/Scripts/Runtime/Network/NightRound.cs`
- `Assets/_Project/Scripts/Runtime/Network/PlayerInteractor.cs`

Non suivis : `Phone.mat` et son meta ; `HousePhone.cs`, `PhoneRules.cs`, `PhoneRulesTests.cs` et leurs meta ; `Docs/Reports/P1-053-house-phone-report.md`. Diff initial suivi : 4 fichiers, 416 insertions et 35 suppressions. Le diff `NightRound` ajoute la remise à zéro du téléphone. Le diff de la scène comprend ce nouvel objet. Ces changements sont le contexte de travail existant de Claude, pas un résultat de l'étude.

Pendant l'étude, `Docs/Backlog.md` est aussi apparu modifié. Au contrôle final, HEAD était devenu `302e7ab574b489caacdc68d5cb81a107526a5e6a` et le status/diff étaient vides. L'étude n'a effectué aucun commit : le checkout a évolué extérieurement pendant son utilisation par Claude. [Snapshot final](git_fin.txt) conservé ; ne pas réinitialiser ces changements. Les valeurs vitesse, capsule, nuit, portage et réveil ont été relues à ce nouveau HEAD et restent identiques.

## Faits techniques utilisés

| Source locale dans Last_Call_Crew | Constat |
|---|---|
| `ProjectSettings/ProjectVersion.txt` | Unity 6000.3.8f1, révision 1c7db571dde0. |
| `Packages/manifest.json` | URP 17.3.0, Input System 1.18.0, NGO 2.13.3, Multiplayer Services 2.4.0, MPPM 2.0.2, tests 1.6.0, AI Assistant 2.20.0-pre.2, Navigation 2.0.10. Packages présents ≠ validation de leur usage dans cette carte. |
| `ProjectSettings/EditorBuildSettings.asset` | Trois scènes activées : SampleScene, NetworkTest, BarGreybox. Ne pas confondre « une scène de monde continue » avec « un seul asset Scene dans le projet ». Ne pas conclure au démarrage effectif du build depuis ces données seules. |
| `Assets/_Project/Prefabs/Player.prefab` | `moveSpeed: 5`, rayon 0,5, hauteur 2. Référence du tuning à GUID `93f73e323e74ac042a759a485eee3001`. |
| `Assets/_Project/Data/Tuning/TuningConfig.asset` et `.meta` | Même GUID. Nuit 300 s, portage 0,6, réveil eau 3 s, auto 45 s, décroissance ivresse 0,2/s. `BarGreybox` référence bien ce tuning pour NightTimer. |
| `Scripts/Runtime/Network/PlayerController.cs` | Déplacement horizontal par Transform, facteur de portage. Poussée via capsule, trois passes et buffer de 16 ; MeshCollider non convexe ignoré. Aucune pente/gravité pilotée par ce contrôleur. |
| `Scripts/Runtime/Network/NightTimer.cs` et `Night/NightTimerRules.cs` | L'hôte démarre à au moins deux connexions ; pas au franchissement de la porte. |
| `Scripts/Runtime/Network/PlayerWake.cs`, `PlayerKnockout.cs`, `PlayerCarry.cs` | Réveil auto vérifié même porté ; réveil libère le lien de portage. Collider du porté désactivé ; ne pas déduire les passages du cumul de deux capsules, mais tester le visuel et le lâcher. |
| `Scripts/Runtime/Night/RestartRules.cs` | Poses actuelles dans la maison : x21, y1, z15,2+1,6×slot, yaw270. Garder cette ancre évite de changer le code pour la carte. |
| `Scripts/Runtime/Network/ReturnZone.cs` et fiche P1-050 | Activation après sortie de tous, présence vérifiée par l'hôte. Visite du bar non requise. |
| Rapport P1-010 | Salle 17×16, entrée sud de 3m, sortie est de 3m, bar existant réutilisable. Anciennes positions de spawn du rapport remplacées depuis P1-050. |
| Rapport P1-050 | Maison 10×10, porte ouest3m, trigger9×3×9 ; feeling de cette nouvelle porte alors différé. Ce rapport ne prouve pas la future ville. |

`Docs/AI/UnityProjectContext.md` et le relais compact sont obsolètes sur plusieurs faits (absence de gameplay/NGO, Relay restant à faire, scène de travail). Le Backlog récent et les fichiers réels prévalent pour cette étude. Le rapport P1-043/Backlog indiquent une candidate technique et un playtest externe différé : ne pas l'assimiler à une validation du nouveau level design.

MCP : package AI Assistant présent et fichiers de projet AI.MCP repérés, mais **aucun outil Unity appelable** dans cette session. Projet actuellement ouvert dans l'éditeur non vérifié ; aucun appel métier tenté. L'étude n'avait pas besoin d'une connexion moteur.

## Contrôles effectués dans le dossier d'étude

- `tools/audit_archives.py` : lecture des dix ZIP, inventaire des extensions, SHA-256, licences, dimensions de 500 OBJ ; inspection structurelle de `character-a.glb` et des deux GLB Quaternius. Aucun import FBX ni test URP.
- Profils de bord OBJ comparés pour `road-straight`, `road-straight-half`, `road-intersection`, `road-crossroad` ; mêmes couples hauteur/largeur aux extrémités X, au seuil d'arrondi 0,0001. Cela ne couvre pas toutes les pièces du pack.
- `tools/draw_map.py` : export PNG et SVG d'un même plan, calcul des longueurs euclidiennes des segments orthogonaux et des temps. Les JSON stockent les axes. La dernière image a été relue pour vérifier les textes, la légende, les portes et les parcours.
- Plan Rennes récupéré par HTTP public et rendu avec Poppler, p.9. Les erreurs d'accès aux deux autres vieux plans sont conservées dans `sources_plans.json`. Les sources non lues ne fondent pas les observations.
- Les scripts s'exécutent avec le Python fourni par le runtime Codex : `C:/Users/Boite/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe`. Le Python système n'avait pas Pillow ; aucune installation n'a été faite.

**Résultat : livrables d'étude vérifiés, carte non construite. Validation gameplay : BLOCKED jusqu'à validation de la proposition puis implémentation et essai hôte/client.** Aucun défaut en jeu n'a été observé par cette étude ; les conflits de rythme sont des déductions explicites des règles et des dimensions proposées.

## Révision 2 — retour de Dyllan

Orientation du plan retenue ; trois poches de 6 × 6 m ajoutées à la ruelle courte, offres de substances dans la ruelle et par le taxi consignées comme direction de gameplay, extension progressive lieux/PNJ/missions et priorité aux placeholders confirmées. Carte PNG/SVG régénérée et relue ; axes et longueurs inchangés. Fiche de fondation et audit complétés. Aucune nouvelle inspection des ZIP nécessaire, aucun travail dans Unity. Git relu : HEAD `302e7ab574b489caacdc68d5cb81a107526a5e6a`, status vide. L'obstacle documentaire sur la direction générale est levé ; le feeling reste non testé, en attente de construction et de playtest.

## Révision 3 — rues et impasses nommées, 17 octobre 2026

Le contexte précédent est historique. Nouvelle lecture des consignes, du statut/diff Git, de la fondation environnementale du jeu, des spécifications P1-056a/P1-057 et du rapport P1-058. Branche `feature/p1-001-network-foundation`, HEAD `1b953f7a1115f36793ad0699859c03425cb7e017`. Checkout occupé : scène, quatre scripts et documentation suivis modifiés, plus fichiers P1-056c/P1-057/P1-058 non suivis. Le dernier statut confirme ce travail en cours ; aucune écriture de l'étude dans le checkout.

La ville est désormais construite et ouverte à pied hors obstacles visibles. Cette décision remplace les fermetures systématiques de parcelles de la première étude. Les positions des futurs lieux X1/X2 proviennent du rapport P1-058 ; ce ne sont pas des mesures de l'éditeur réalisées par Codex. Les valeurs sur disque restent 5 m/s, portage ×0,6 et nuit 300 s. Correction après retour de Dyllan : le GDD de travail conservé dans `D:\AI-GameWorkflow\outputs\Last-Call-Crew-Asset-List.md` nomme bien Poudre de Perlimpinpin, Champignons de Mémé, Bonbons Câlins, Fumée de Dragon et Tisane de Morphée. La recherche initiale avait été limitée à tort au dépôt Unity.

Ajouts : rue de la Soif entre les trois lieux, passage à l'ouest du bar, prolongement de la boucle nord, deux impasses avec courts fonds fermés visibles et plaques proposées. Les poches R1–R3 restent présentes. `Rues_et_impasses.md` fournit les coordonnées, le statut des noms et une reprise courte sur la scène existante. Les anciens livrables signalent cette révision pour éviter de rejouer la première implantation.

Carte PNG/SVG régénérée avec `tools/draw_map.py`, image finale relue : noms du GDD dans la légende, connexions visibles, titre dégagé de X1. Longueurs exportées dans `trajets.json` : Soif 66 m, passage 26 m, prolongement 36 m, impasses 16/28 m. Validation documentaire uniquement ; aucune exécution Unity, aucun test hôte/client, aucun PASS gameplay pour ces ajouts. Le feeling et les colliders restent à éprouver dans le jeu.

## Révision 4 — petits bars thématiques, 17 octobre 2026

Retour de Dyllan : le bar de 17 × 16 m paraît deux fois trop volumineux ; le quartier doit proposer plusieurs petits bars dont les boissons visibles sur le comptoir expriment le thème. Proposition : division exacte de l'ancienne coque en B1 Bières et B2 Shooters, 8,5 × 16 m chacun ; réduction de l'ancien X2 en B3 Cocktails de 10 × 12 m. B1 seul reste jouable en P1, B2/B3 sont des réserves fermées. X1 reste la boîte future.

L'inventaire local confirme `mug`, `glass`, `cocktail`, `glass-wine`, vins, soda, canettes, citron, orange et tonneau dans le Kenney Food Kit. Il ne prouve pas la présence de tireuses, de verres à shot ou d'un shaker dédiés : placeholders indiqués. La nouvelle entrée de B1 ramène les axes de référence à 220,75 m et 180,75 m ; `trajets.json` et `calculs_trajets.txt` ont été régénérés.

Carte PNG/SVG relue après réduction des libellés intérieurs à B1/B2/B3. Aucun fichier Unity modifié et aucun essai en jeu. La capacité à quatre, le portage, la sortie est et la lisibilité des trois thèmes restent à valider dans la scène réelle.
