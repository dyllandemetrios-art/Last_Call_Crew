# Instructions de travail

Lire `README.md`, `Docs/Backlog.md` et `Docs/AI/UnityProjectContext.md` avant toute modification importante.

## Responsabilités

- Dyllan décide du gameplay, des priorités et des compromis structurants.
- Codex maintient le backlog, les spécifications, les preuves et les revues.
- Claude Code implémente une fiche précise de `Docs/Specs/` à la fois et rédige son compte rendu.
- Un seul auteur travaille sur le checkout et les assets Unity à la fois.

## Règles Unity

- Utiliser Unity `6000.3.8f1`. Ne pas changer de version sans validation.
- Base choisie : Universal 3D avec MonoBehaviours et Netcode for GameObjects. Ne pas introduire ECS, Netcode for Entities ou un framework d'injection.
- Préserver `Assets/`, `Packages/`, `ProjectSettings/`, tous les `.meta` et leurs GUID.
- Ne pas éditer les scènes, prefabs ou assets YAML à la main lorsque l'éditeur peut faire l'opération de manière sûre.
- Toute installation ou mise à jour de package doit correspondre à une fiche validée et être consignée.
- Garder les paramètres de gameplay réglables dans l'Inspector ou un `ScriptableObject` dédié.
- Ne jamais enregistrer d'identifiant secret Unity Gaming Services dans Git.

## Validation

- Vérifier `git status` avant et après la tâche.
- Compiler après chaque incrément significatif et lire la console Unity.
- Une fonction réseau exige au minimum une preuve hôte plus client.
- Séparer test local, test Relay et build autonome.
- Ne jamais annoncer un PASS sans commande ou comportement réellement observé.
- Dyllan valide le feeling en jeu ; les tests automatiques ne le remplacent pas.

## Relais

Le compte rendu doit préciser : branche et commit de départ, fichiers modifiés, décisions prises, commandes et résultats, tests réalisés, limites, changements non commités et prochaine action recommandée. Aucun push implicite.

## Sobriété de travail

- Limiter les lectures, sorties et prompts aux éléments nécessaires à l'étape en cours ; référencer les rapports existants au lieu de les recopier.
- Ne pas créer de sous-agent ou lancer de validation large lorsqu'un contrôle ciblé suffit.
- Laisser l'application compacter automatiquement le contexte. À une frontière de milestone ou si un nouveau chat devient utile, produire un relais court dans `Docs/` avec commit, état Git, décision, preuves et prochaine action, sans dupliquer tout l'historique.
- Un nouveau chat traite une étape clairement délimitée ; ne pas multiplier les conversations pour de petites corrections.
