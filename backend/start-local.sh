cd /Users/idrizzogaj/Projects/memotopia/backend || exit 1
export PATH="/opt/homebrew/opt/node@18/bin:$PATH"

docker info >/dev/null 2>&1 || { echo "Docker inte igång"; exit 1; }

docker volume create memotopia-pgdata >/dev/null 2>&1 || true

if docker ps -a --format '{{.Names}}' | grep -qx memotopia-postgres2; then
  docker start memotopia-postgres2 >/dev/null
else
  docker run -d --name memotopia-postgres2 -e POSTGRES_USER=memotopia -e POSTGRES_PASSWORD=memotopia -e POSTGRES_DB=memotopia -p 55432:5432 -v memotopia-pgdata:/var/lib/postgresql/data postgres:14 >/dev/null
fi

i=0
while [ $i -lt 30 ]; do
  docker exec memotopia-postgres2 pg_isready -U memotopia -d memotopia >/dev/null 2>&1 && break
  sleep 1
  i=$((i+1))
done

set -a
source /Users/idrizzogaj/Projects/memotopia/backend/.env
set +a

yarn -s typeorm:cli migration:run >/dev/null 2>&1 || true

docker exec -i memotopia-postgres2 psql -U memotopia -d memotopia -c "insert into game (\"gameName\") values ('Flash'),('Pairs'),('Boxes') on conflict (\"gameName\") do nothing;" >/dev/null 2>&1 || true

PID3000="$(lsof -tiTCP:3000 -sTCP:LISTEN 2>/dev/null | head -n 1)"
if [ -n "$PID3000" ]; then kill -9 "$PID3000"; fi

rm -f /Users/idrizzogaj/Projects/memotopia/backend/backend.log

TS_NODE_TRANSPILE_ONLY=1 nohup /opt/homebrew/opt/node@18/bin/node -r ts-node/register -r tsconfig-paths/register /Users/idrizzogaj/Projects/memotopia/backend/src/main.ts > /Users/idrizzogaj/Projects/memotopia/backend/backend.log 2>&1 &

sleep 2
LANIP="$(ipconfig getifaddr en0 2>/dev/null || ipconfig getifaddr en1 2>/dev/null)"
echo "Backend: http://127.0.0.1:3000/"
echo "iPhone:  http://$LANIP:3000/"
tail -n 20 /Users/idrizzogaj/Projects/memotopia/backend/backend.log
