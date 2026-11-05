#!/bin/bash
set -e
cd /opt/memotopia
F=backend/src/modules/achievements/achievements.constants.ts
cp "$F" "$F.bak-20261105"
cp /tmp/achievements.constants.ts "$F"
grep -c "athlete-one-heart-says" "$F"
docker compose up -d --build backend 2>&1 | tail -n 4
sleep 12
docker compose ps --format '{{.Name}} {{.Status}}' | head -4
curl -s -o /dev/null -w "root %{http_code}\n" http://localhost:3000/ || true
docker compose logs backend --since 2m 2>&1 | grep -i -E "error|started" | cut -c1-200 | tail -n 4