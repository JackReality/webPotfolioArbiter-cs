# Processus de travail (workflow)

> Annexe de [`CLAUDE.md`](CLAUDE.md). Règles strictes de suivi des modifications, pour la
> traçabilité et la validation par l'utilisateur.

## Boucle de développement
1. **Objectif** : l'utilisateur donne un objectif (pas forcément découpé).
2. **Proposition** : Claude découpe en tâches/sous-étapes et les présente pour validation (sans coder).
3. **Validation** : une fois d'accord, Claude **ajoute les tâches dans `STATUS.md`** (section « À faire »).
4. **Exécution** : Claude code **UNE tâche à la fois**, vérifie la compilation, puis **s'arrête et fait
   le point**. Il ne passe à la suivante qu'après le **feu vert** de l'utilisateur. Jamais tout un lot d'un coup.

## Vérification de compilation (OBLIGATOIRE)
Après **toute** modification de code, vérifier dans les **logs du `dotnet watch`** que la compilation a
réussi (`Build succeeded`) **avant** de demander un test. Si elle échoue, corriger immédiatement.
Interdit de demander un test sur une version non compilée (l'utilisateur servirait l'ancien cache).
⚠️ Après un changement structurel (nouvelle route `@page`, nouveau fichier), **redémarrer le `watch`**
(le hot-reload ne l'applique pas).

## Suivi des lots dans `STATUS.md`
- Avant de lancer un lot : le **noter**. Une fois codé : indiquer le **statut** (« codé, à tester »).
  Une fois **validé par l'utilisateur** : le **cocher** comme validé.
- Ne pas enchaîner plusieurs lots distincts sans validation intermédiaire, sauf instruction explicite.

## Format de `STATUS.md`
**7 chapitres maximum.** Le chapitre 1 est « À faire ». Les chapitres 2 à 7 sont « Fait le [YYYY-MM-DD] ».
À chaque nouveau lot terminé : ajouter un chapitre « Fait le [date] » et supprimer le plus ancien
(toujours exactement 7 chapitres).
