# Base de données — tables et règles de liaison

> **Convention de nommage (rappel).** Tout identifiant est en **anglais** : tables et
> colonnes SQL en `snake_case`, code C# en `PascalCase`, et **noms de pages — routes
> `@page` ET fichiers `.razor` — en anglais** (ex. `/profile`, `/download`, `/training`,
> `/catalog`, `/client-area`, `/access-denied`, `/forgot-password`). Colonne de langue =
> `language` partout. Détail des conventions : voir `ARCHITECTURE.md`.

> Ce fichier centralise, **en clair**, la structure des tables et surtout les
> **règles de liaison** (clés étrangères) entre elles. La **source de vérité**
> reste la base elle-même (voir « Comment vérifier » en bas) ; ce document est un
> résumé lisible, à tenir à jour quand on modifie le schéma.

## C'est quoi une « règle de liaison » ?

Une règle de liaison = une **clé étrangère** (*foreign key*, FK). C'est une colonne
d'une table « enfant » qui pointe vers la clé primaire d'une table « parent ».
La règle dit aussi **ce qui arrive à l'enfant quand on supprime le parent** :

- **`ON DELETE CASCADE`** : supprimer le parent supprime automatiquement les enfants.
- **`ON DELETE RESTRICT`** : interdit de supprimer le parent tant qu'il a des enfants.
- **`ON DELETE SET NULL`** : la colonne de l'enfant est remise à `NULL`.

⚠️ Ces règles sont **stockées dans la base** (dans le schéma de la table), **pas**
dans le code C#. EF Core ne les invente pas : il les suit.

## Tables actuelles

| Table | Rôle | Gérée par |
|---|---|---|
| `users` | Comptes (login, rôle). | À la main (modèle `User` mappe la table existante). |
| `email_templates` | Modèles d'emails (FR/EN/ES). **Global**, pas lié à un user. | SQL. |
| `email_tokens` | Jetons temporaires (confirmation, reset). | SQL. |
| `trainings` | Formations vendues (**une par langue**, colonne `language`). **Global**, pas lié à un user. | SQL. |
| `schema_migrations` | Historique de migrations (origine Supabase). À nettoyer un jour. | — |

## Règles de liaison actuelles

| Enfant | Colonne | → Parent | Colonne | ON DELETE |
|---|---|---|---|---|
| `email_tokens` | `user_id` | `users` | `id` | **CASCADE** |

➡️ **Conséquence pratique** : supprimer un utilisateur efface **automatiquement**
ses lignes dans `email_tokens`. Aucune autre table ne dépend de `users`
aujourd'hui, donc la suppression d'un compte est **propre et complète**.

> `email_templates` et `trainings` ne référencent **pas** `users` : ce sont des
> données globales, elles ne sont pas concernées par la suppression d'un compte.

## Comment vérifier (source de vérité)

Pour relister toutes les clés étrangères réelles de la base :

```sql
SELECT
  kcu.table_name              AS enfant,
  kcu.column_name             AS colonne,
  kcu.referenced_table_name   AS parent,
  rc.delete_rule              AS on_delete
FROM information_schema.key_column_usage kcu
JOIN information_schema.referential_constraints rc
  ON rc.constraint_schema = kcu.table_schema
 AND rc.constraint_name   = kcu.constraint_name
WHERE kcu.table_schema = 'portfolio_arbiter'
  AND kcu.referenced_table_name IS NOT NULL
ORDER BY kcu.referenced_table_name, kcu.table_name;
```

(À exécuter via `mysql.exe`, voir « Accéder à la DB en ligne de commande » ci-dessous.)

## Accéder à la DB en ligne de commande

- Identifiants dans `.env` : `DB_HOST=127.0.0.1`, `DB_PORT=13306`, `DB_NAME=portfolio_arbiter`,
  `DB_USER=portfolio`, `DB_PASSWORD=123`. ⚠️ **`DB_HOST=127.0.0.1`** obligatoire (le user MySQL
  `portfolio` refuse `localhost`).
- **`mysql.exe`** est dans `C:\Program Files\MariaDB 11.8\bin\mysql.exe` (PAS dans le PATH → ne pas
  taper `mysql` directement).
- **Requêter** (via PowerShell, les guillemets posent problème sous cmd.exe) :
  ```powershell
  powershell -Command "& 'C:\Program Files\MariaDB 11.8\bin\mysql.exe' -h 127.0.0.1 -P 13306 -u portfolio -p123 portfolio_arbiter -e 'SELECT ...;'"
  ```
- **Alternative fiable** : un fichier `.sql` appelé par un `.bat` (`mysql.exe ... < query.sql`).
- **Colonne `key`** : mot réservé SQL → toujours la backtiquer : `` `key` ``.
- ⚠️ Pas de migrations EF : on **crée/modifie les tables en SQL** ici, puis on mappe le modèle.
  La connexion n'est **pas** testée au démarrage (version serveur fixée dans `Program.cs`) →
  l'app démarre même sans base lancée.

## À retenir quand on ajoute une table liée aux users

Si une future table doit être **rattachée à un utilisateur** (ex. `achats`,
`messages_forum`), il faut décider de sa règle `ON DELETE` au moment du
`CREATE TABLE`, puis **mettre à jour ce document**. Recommandé dans la plupart
des cas : `ON DELETE CASCADE` (supprimer le user efface ses données associées).
