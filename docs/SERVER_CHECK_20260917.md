# Server check — 2026-09-17

Read-only checks completed over existing SSH key/known host to root@46.225.130.128. Hetzner metadata endpoint confirmed an instance. Client host: https://memotopia-eu-1.duckdns.org. No production changes or deployment performed.

- HTTPS health endpoint: 200, approximately 0.34 seconds from this Mac. `/me` without credentials: 401 as expected.
- Health metadata: uptime about 87 days, environment `local`, release `development`, version `unknown`, buildTime 2026-06-21T13:23:50.203Z. This endpoint checks process availability, not database or complete gameplay.
- Host uptime 155 days; load average 0.00; root filesystem 26% used, 27 GB available; memory available approximately 3 GB of 3.8 GB.
- No failed systemd units. Docker active. Caddy, backend and PostgreSQL containers up about two months, zero recorded restarts and no OOM flags. Nginx inactive is expected here because Caddy serves the app.
- PostgreSQL accepts connections; read-only SELECT 1 succeeded.
- HTTPS certificate validated normally and expires 2026-11-08. Public API documentation `/api` returns 200; review production environment settings before TestFlight.
- No backend log lines matching error/exception/fatal in the 24-hour sample. PostgreSQL had 30 syntax-error lines in the initial sample, with additional statements appearing during inspection. Sanitized statement inspection confirmed challenge queries containing empty `IN ()` lists. Local source matches this failure path in backend/src/modules/challenge/participant.repository.ts: it maps results into challenge IDs and unconditionally builds an IN query, including when empty. Empty challenge lists need an early empty result, then testing and a separately authorized deployment.

Server is reachable and resources/services look healthy, but challenge-query errors and development-mode configuration prevent calling the whole backend fully verified. Authenticated gameplay, purchases, backup restoration and all data integrity were not tested by this health check. No raw user records or secrets were retrieved.
