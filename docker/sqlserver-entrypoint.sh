#!/bin/sh
set -eu

if [ -f /run/secrets/mssql_sa_password ]; then
  export MSSQL_SA_PASSWORD="$(tr -d '\r\n' < /run/secrets/mssql_sa_password)"
elif [ -n "${MSSQL_SA_PASSWORD_FILE:-}" ] && [ -f "$MSSQL_SA_PASSWORD_FILE" ]; then
  export MSSQL_SA_PASSWORD="$(tr -d '\r\n' < "$MSSQL_SA_PASSWORD_FILE")"
fi

if [ -z "${MSSQL_SA_PASSWORD:-}" ]; then
  echo "MSSQL_SA_PASSWORD is not set. Mount /run/secrets/mssql_sa_password or set MSSQL_SA_PASSWORD." >&2
  exit 1
fi

exec /opt/mssql/bin/sqlservr
