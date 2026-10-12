# P1-011 TuningConfig

## Intention

Centraliser les chiffres de la boucle P1 afin que Dyllan puisse régler l'ivresse, le KO et l'entraide dans l'Inspector sans modifier le code. Cette tâche définit les données et leur validation, sans implémenter les mécaniques.

## Comportement observable

Un asset `TuningConfig` existe sous `Assets/_Project/Data/Tuning/`. Ses champs sont organisés et compréhensibles dans l'Inspector. Modifier une valeur puis relancer Unity conserve cette valeur. Les valeurs invalides sont refusées ou corrigées de manière explicite.

## Valeurs provisoires du GDD

### Ivresse

- minimum : `0`
- maximum et seuil KO : `100`
- seuil Pompette : `30`
- seuil Ivre : `60`
- gain par verre : `20`
- décroissance passive : `0.2` point par seconde
- eau hors réveil : réduction de `15`

### Refus

- durée normale : `2` secondes
- multiplicateur du profil Sociable : `2`
- délai minimal entre deux propositions du même PNJ : `20` secondes

### KO et entraide

- durée de maintien pour réveiller avec de l'eau : `3` secondes
- ivresse après réveil avec de l'eau : `50`
- réveil automatique : `45` secondes
- ivresse après réveil automatique : `90`
- multiplicateur de vitesse pendant le portage : `0.6`

Ces nombres restent provisoires. La tâche ne doit pas transformer leur présence dans l'asset en validation définitive du game design.

## Travail demandé

1. Lire `AGENTS.md`, `README.md`, `Docs/Backlog.md` et cette fiche.
2. Vérifier le commit de départ indiqué par Codex et un arbre propre.
3. Créer `Assets/_Project/Scripts/Runtime/Config/TuningConfig.cs` et son `.meta`.
4. Créer l'asset `Assets/_Project/Data/Tuning/TuningConfig.asset` avec ses dossiers et `.meta`.
5. Utiliser un `ScriptableObject` simple, sans singleton global, service locator ou système de chargement générique.
6. Organiser l'Inspector avec des sections cohérentes, des unités explicites et des tooltips courts.
7. Exposer les valeurs en lecture publique sans permettre à n'importe quel système de les modifier au runtime. Préférer des champs sérialisés privés avec propriétés en lecture seule.
8. Valider les invariants : minimum inférieur au seuil Pompette, seuil Pompette inférieur au seuil Ivre, seuil Ivre inférieur au KO, valeurs temporelles positives, multiplicateur de portage compris entre 0 et 1, valeurs de réveil dans l'intervalle d'ivresse.
9. Ajouter des tests EditMode ciblés pour les valeurs par défaut et les invariants si cela peut se faire sans tester des détails de l'Inspector.
10. Ne pas connecter encore cet asset au joueur, au réseau ou à une scène de production.

## Critères d'acceptation

- Le projet compile sans erreur.
- L'asset existe et affiche toutes les valeurs provisoires ci-dessus.
- Les noms, tooltips et unités permettent à Dyllan de comprendre ce qu'il règle.
- Les systèmes futurs peuvent lire les valeurs sans les modifier directement.
- Une configuration incohérente ne peut pas être utilisée silencieusement.
- Les tests pertinents passent et leur commande ou exécution est consignée.
- Aucun comportement P1-001 ou P1-002 ne régresse.

## Hors périmètre

Jauge d'ivresse, consommation, dégrisement, dialogue, KO, portage, réveil, profils de personnage, interface finale, synchronisation réseau et Relay.

## Compte rendu attendu

Créer `Docs/Reports/P1-011-tuning-config-report.md` avec : commit de départ, fichiers créés ou modifiés, choix de validation, valeurs par défaut, tests exécutés et résultats, compilation, limites, statut PASS, FAIL ou BLOCKED et prochaine action. Ne pas commit ni push sans autorisation explicite.
