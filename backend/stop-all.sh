cd /Users/idrizzogaj/Projects/memotopia/backend || exit 1
PID3000="$(lsof -tiTCP:3000 -sTCP:LISTEN 2>/dev/null | tr '\n' ' ')"
if [ -n "$PID3000" ]; then kill -9 $PID3000; fi
docker stop memotopia-postgres2 >/dev/null 2>&1 || true
echo "Stopped: backend(3000) + memotopia-postgres2"
lsof -nP -iTCP:3000 -sTCP:LISTEN 2>/dev/null || echo "3000 free"
docker ps --filter "name=memotopia-postgres2" --format "table {{.Names}}\t{{.Status}}" || true
