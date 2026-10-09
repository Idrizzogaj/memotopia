# Server: reset all accounts + log/backup retention (prepared 2026-10-07, NOT run)

Server: `root@46.225.130.128`, project in `/opt/memotopia` (docker compose: postgres, backend, caddy). Reached through the Mac. **Nothing below has been run.** Run it only after the owner has seen the account list and said "yes, delete everything".

## A. Reset: delete all accounts and start from zero

Step 0 – look first (read only):
```sql
select u.id, u.username, u."paymentStatus", coalesce(s.xp,0) xp, u."createdAt" from "user" u left join user_statistics s on s."userId"=u.id order by u.id;
```
Check specially for rows with `paymentStatus` other than `NONE`: those users pay and would lose Premium on this server.

Step 1 – fresh backup right before (same command as on 2026-10-06):
```bash
docker exec memotopia-postgres-1 sh -c 'pg_dump -U "$POSTGRES_USER" "$POSTGRES_DB"' | gzip > /opt/memotopia/backups/db-before-reset-$(date +%Y%m%d-%H%M%S).sql.gz && chmod 600 /opt/memotopia/backups/db-before-reset-*.sql.gz
```
Step 2 – delete, in ONE transaction (the `game` table with Pairs/Flash/Boxes is NOT touched):
```sql
BEGIN;
DELETE FROM favorites;          -- no cascade exists for favorites
DELETE FROM challenge;          -- participants cascade
DELETE FROM "user";             -- user_statistics, restrictions, achievement, game_level, participant cascade
-- reset id counters so the next account is #1 (check sequence names with \ds first):
ALTER SEQUENCE user_id_seq RESTART WITH 1;
ALTER SEQUENCE challenge_id_seq RESTART WITH 1;
COMMIT;
```
Step 3 – verify: `select count(*) from "user";` is 0, `select count(*) from game;` unchanged. Then register a test account in the app.

Note: existing JWT tokens in people's phones stop working (their users are gone); the apps send them to the login screen.

## B. Log retention (policy says logs: 30 days)

Docker json logs cannot be limited by age, only by size. Proposal (backend + caddy only, postgres is not touched):

1. Back up `/opt/memotopia/docker-compose.yml`, then add to the `backend` and `caddy` services:
```yaml
    logging:
      driver: json-file
      options:
        max-size: "10m"
        max-file: "3"
```
2. Apply: `cd /opt/memotopia && docker compose up -d backend caddy` (recreates those two containers; a few seconds of downtime).
3. Weekly clean-up so nothing stays longer than the policy allows (`crontab -e` as root):
```
0 3 * * 0  for c in memotopia-backend-1 memotopia-caddy-1; do truncate -s 0 "$(docker inspect $c --format '{{.LogPath}}')"; done
```
   (Result: container logs are at most about 7 days old. Rotated files are limited by size.)

## C. Backups (policy says backups: 30 days)

- **There is no scheduled database backup today** – only the manual dumps in `/opt/memotopia/backups`. Recommended: either enable Hetzner server backups (about +20 % of the server price) or a daily `pg_dump` cron.
- Delete old manual dumps after 30 days, for example daily:
```
30 3 * * *  find /opt/memotopia/backups -maxdepth 1 -type f -name 'db-*.sql.gz' -mtime +30 -delete
```
  (Do this only once a real automatic backup exists, otherwise the last restore point disappears after 30 days.)
- The dumps from 2026-10-06 contain the old user data; they will be removed by this rule on about 2026-11-05.
