set -euo pipefail

BASE_URL="${BASE_URL:-http://127.0.0.1:3000}"
USERNAME="${USERNAME:-idrizzogaj}"
EMAIL="${EMAIL:-test1@example.com}"

METHOD="${1:-}"
PATH_IN="${2:-}"
DATA="${3:-}"

if [ -z "${METHOD}" ] || [ -z "${PATH_IN}" ]; then
  echo "Usage:"
  echo "  BASE_URL=http://127.0.0.1:3000 USERNAME=testuser1 EMAIL=test1@example.com bash /Users/idrizzogaj/Projects/memotopia/backend/mt.sh get /me"
  echo "  bash /Users/idrizzogaj/Projects/memotopia/backend/mt.sh post /user-statistics/time-played '{}'"
  exit 1
fi

if [ "${PATH_IN#"/"}" = "${PATH_IN}" ]; then
  PATH_IN="/${PATH_IN}"
fi

read -rs -p "Password for ${USERNAME}: " PASSWORD
printf "\n"

tmp="$(/usr/bin/mktemp)"
code="$(/usr/bin/curl -sS -o "${tmp}" -w "%{http_code}" -X POST "${BASE_URL}/auth/signin" -H "Content-Type: application/json" -d "{\"username\":\"${USERNAME}\",\"password\":\"${PASSWORD}\"}")"
body="$(/bin/cat "${tmp}")"
rm -f "${tmp}"

if [ "${code}" = "401" ]; then
  tmp="$(/usr/bin/mktemp)"
  create_code="$(/usr/bin/curl -sS -o "${tmp}" -w "%{http_code}" -X POST "${BASE_URL}/users" -H "Content-Type: application/json" -d "{\"email\":\"${EMAIL}\",\"username\":\"${USERNAME}\",\"password\":\"${PASSWORD}\"}")"
  create_body="$(/bin/cat "${tmp}")"
  rm -f "${tmp}"

  if [ "${create_code}" != "201" ] && [ "${create_code}" != "409" ]; then
    echo "Create user failed (HTTP ${create_code}):"
    echo "${create_body}"
    exit 1
  fi

  tmp="$(/usr/bin/mktemp)"
  code="$(/usr/bin/curl -sS -o "${tmp}" -w "%{http_code}" -X POST "${BASE_URL}/auth/signin" -H "Content-Type: application/json" -d "{\"username\":\"${USERNAME}\",\"password\":\"${PASSWORD}\"}")"
  body="$(/bin/cat "${tmp}")"
  rm -f "${tmp}"
fi

if [ "${code}" != "200" ] && [ "${code}" != "201" ]; then
  echo "Signin failed (HTTP ${code}):"
  echo "${body}"
  exit 1
fi

TOKEN="$(printf "%s" "${body}" | /usr/bin/python3 -c "import sys,json; print(json.load(sys.stdin).get('accessToken','') or json.load(sys.stdin))")"

if [ -z "${TOKEN}" ]; then
  echo "No accessToken in response:"
  echo "${body}"
  exit 1
fi

if [ "${METHOD}" = "get" ] || [ "${METHOD}" = "GET" ]; then
  /usr/bin/curl -i "${BASE_URL}${PATH_IN}" -H "Authorization: Bearer ${TOKEN}"
  exit 0
fi

if [ "${METHOD}" = "post" ] || [ "${METHOD}" = "POST" ]; then
  if [ -z "${DATA}" ]; then
    DATA="{}"
  fi
  /usr/bin/curl -i -X POST "${BASE_URL}${PATH_IN}" -H "Authorization: Bearer ${TOKEN}" -H "Content-Type: application/json" -d "${DATA}"
  exit 0
fi

echo "Method must be get or post"
exit 1
