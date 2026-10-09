#!/bin/bash
# Memotopia server update: level number as XP and video badge keys (round 2). Run on the Mac.
set -e
S=root@46.225.130.128
D="$(cd "$(dirname "$0")" && pwd)"
echo "1/4 Backup on the server"
ssh "$S" 'bash -s' <<'REMOTE'
set -e
cd /opt/memotopia
mkdir -p backups
STAMP=$(date +%Y%m%d-%H%M%S)
docker exec memotopia-postgres-1 sh -c 'pg_dump -U "$POSTGRES_USER" "$POSTGRES_DB"' | gzip > backups/db-pre-xp2-$STAMP.sql.gz
chmod 600 backups/db-pre-xp2-$STAMP.sql.gz
tar czf backups/backend-src-pre-xp2-$STAMP.tgz backend/src
ls -l backups | tail -4
REMOTE
echo "2/4 Copy the changed files"
tar czf - -C "$D/files" . | ssh "$S" 'tar xzf - -C /opt/memotopia/backend'
echo "3/4 Build and restart the backend"
ssh "$S" 'cd /opt/memotopia && docker compose up -d --build backend 2>&1 | tail -15'
echo "4/4 Check"
sleep 20
ssh "$S" 'cd /opt/memotopia && docker compose ps && docker compose logs --tail=15 backend'
echo "DONE. Tell Claude: server updated."
