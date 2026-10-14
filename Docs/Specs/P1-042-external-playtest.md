# P1-042 Playtest externe

## Intention

Vérifier qu'une personne qui n'a pas suivi le développement comprend rapidement le but de la nuit et peut commencer à jouer sans explication détaillée de Dyllan.

## Participant

Une personne qui n'a jamais joué à cette version et n'a pas lu le GDD ni les rapports. Dyllan peut jouer le partenaire et aider uniquement à rejoindre la session Relay.

## Build

Utiliser le dernier build Windows validé après P1-040/P1-015a. Noter son chemin et son identifiant affiché. Si un nouveau build est nécessaire, ne changer aucun gameplay pour le playtest.

## Protocole

1. Aider le participant uniquement à lancer le jeu, coller le code Relay et rejoindre.
2. À partir du moment où il contrôle son joueur, démarrer un chronomètre d'observation de 60 secondes.
3. Ne pas expliquer l'objectif, les commandes ou les systèmes pendant cette minute. Répondre seulement aux problèmes matériels extérieurs au jeu.
4. Noter la première action, ce qu'il lit, les touches essayées, les hésitations et toute question posée.
5. À 60 secondes, demander : `À ton avis, quel est le but de cette partie ?`
6. Continuer ensuite 10 à 15 minutes. Dyllan peut alors jouer normalement, mais sans présenter chaque mécanique à l'avance.
7. À la fin, poser quatre questions courtes :
   - Qu'est-ce que tu pensais devoir faire ?
   - Qu'est-ce qui t'a aidé à le comprendre ?
   - Qu'est-ce qui t'a bloqué ou semblé peu clair ?
   - Qu'est-ce qui t'a donné envie d'essayer quelque chose ?

## Critère principal

PASS si, avant ou lors de la question à 60 secondes, le participant formule l'idée essentielle : **faire des bêtises/boire puis rentrer ensemble avant la fin de la nuit**, avec au minimum la contrainte de retour collectif et le temps limité.

Une formulation imparfaite est acceptable si son comportement montre clairement qu'il cherche à rentrer avec son partenaire avant zéro. Ne pas compter une information donnée par Dyllan comme une compréhension autonome.

## Observations secondaires

Noter sans en faire automatiquement un FAIL :

- compréhension des invites `E`, clic droit et `F` ;
- repérage du bar et de la zone de retour ;
- compréhension de l'ivresse et du KO ;
- compréhension du portage et du réveil ;
- lisibilité du chronomètre et du bilan ;
- moments amusants, surprises et temps morts ;
- défauts techniques ou réseau.

## Compte rendu attendu

Créer `Docs/Reports/P1-042-external-playtest-report.md` avec : date, build, profil anonymisé du participant (`Participant A` suffit), durée, niveau d'aide fourni, chronologie de la première minute, réponse exacte ou fidèlement reformulée, observations secondaires, défauts, statut PASS/FAIL et prochaine correction recommandée.

Ne pas enregistrer de nom, voix, image ou autre donnée personnelle sans accord du participant. Aucun commit ni push automatique.
