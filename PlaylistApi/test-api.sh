#!/usr/bin/env bash
# Prueba el contrato de la API (sirve igual contra NestJS o ASP.NET Core).
#
#   ./test-api.sh                              -> prueba http://localhost:5080/api  (ASP.NET Core)
#   ./test-api.sh http://localhost:3000/api    -> prueba NestJS, para comparar
#
# Requiere solo bash y curl (funciona en Git Bash de Windows).

BASE="${1:-http://localhost:5080/api}"
FAILS=0
SUFFIX="$RANDOM$RANDOM"
EMAIL_A="usera_$SUFFIX@test.com"
EMAIL_B="userb_$SUFFIX@test.com"
PASS="123456"

# req METODO RUTA TOKEN BODY  -> deja el código en $STATUS y el cuerpo en $BODY
req() {
  local method=$1 path=$2 token=$3 body=$4
  local args=(-s -w $'\n%{http_code}' -X "$method" "$BASE$path" -H "Content-Type: application/json")
  [ -n "$token" ] && args+=(-H "Authorization: Bearer $token")
  [ -n "$body" ] && args+=(-d "$body")
  local out
  out=$(curl "${args[@]}")
  STATUS=${out##*$'\n'}
  BODY=${out%$'\n'*}
}

check() { # descripción, código esperado
  if [ "$STATUS" = "$2" ]; then
    echo "PASS  $1 ($STATUS)"
  else
    echo "FAIL  $1 -> esperado $2, llegó $STATUS | $BODY"
    FAILS=$((FAILS + 1))
  fi
}

contains() { # descripción, texto que debe aparecer en $BODY
  if [[ "$BODY" == *"$2"* ]]; then
    echo "PASS  $1"
  else
    echo "FAIL  $1 -> no aparece '$2' en: $BODY"
    FAILS=$((FAILS + 1))
  fi
}

echo "Probando contra: $BASE"
echo "-------------------------------------------"

echo "## Registro y login"
req POST /auth/register "" "{\"email\":\"$EMAIL_A\",\"password\":\"$PASS\"}"
check "Registrar usuario A" 201
contains "  respuesta trae id y email (camelCase)" '"email"'

req POST /auth/register "" "{\"email\":\"$EMAIL_A\",\"password\":\"$PASS\"}"
check "Registrar correo repetido" 409

req POST /auth/register "" "{\"email\":\"no-es-correo\",\"password\":\"$PASS\"}"
check "Registrar con correo inválido" 400

req POST /auth/register "" "{\"email\":\"corta_$SUFFIX@test.com\",\"password\":\"123\"}"
check "Registrar con contraseña corta" 400

req POST /auth/login "" "{\"email\":\"$EMAIL_A\",\"password\":\"incorrecta\"}"
check "Login con contraseña incorrecta" 401

req POST /auth/login "" "{\"email\":\"$EMAIL_A\",\"password\":\"$PASS\"}"
check "Login usuario A" 200
contains "  respuesta trae accessToken" '"accessToken"'
TOKEN_A=$(echo "$BODY" | sed -E 's/.*"accessToken":"([^"]+)".*/\1/')

echo
echo "## Protección con JWT"
req GET /songs "" ""
check "Listar SIN token" 401
req POST /songs "" '{"name":"X","singer":"Y"}'
check "Crear SIN token" 401
req GET /songs "token.invalido.xxx" ""
check "Listar con token inválido" 401

echo
echo "## CRUD"
req POST /songs "$TOKEN_A" '{"name":"Bohemian Rhapsody","singer":"Queen"}'
check "Crear canción" 201
contains "  respuesta trae userId" '"userId"'
SONG_ID=$(echo "$BODY" | sed -E 's/.*"id":([0-9]+).*/\1/')

req POST /songs "$TOKEN_A" '{"name":"","singer":"Queen"}'
check "Crear con nombre vacío" 400

req GET "/songs/$SONG_ID" "$TOKEN_A" ""
check "Obtener canción por id" 200

req PATCH "/songs/$SONG_ID" "$TOKEN_A" '{"name":"Bohemian Rhapsody (Remastered)"}'
check "Editar canción (PATCH parcial)" 200
contains "  el nombre cambió" 'Remastered'
contains "  el cantante se conservó" '"singer":"Queen"'

echo
echo "## Paginación y búsqueda (creando 11 canciones más)"
for i in $(seq 1 11); do
  req POST /songs "$TOKEN_A" "{\"name\":\"Cancion $i\",\"singer\":\"Artista $i\"}" >/dev/null
done
req GET "/songs?page=1&limit=5" "$TOKEN_A" ""
check "Listar página 1, limit 5" 200
contains "  meta.total = 12" '"total":12'
contains "  meta.totalPages = 3" '"totalPages":3'

req GET "/songs?page=3&limit=5" "$TOKEN_A" ""
check "Listar página 3" 200
contains "  la última página tiene page=3" '"page":3'

req GET "/songs?search=bohemian" "$TOKEN_A" ""
check "Buscar 'bohemian' (minúsculas)" 200
contains "  encuentra la canción sin importar mayúsculas" 'Remastered'
contains "  meta.total = 1" '"total":1'

echo
echo "## Aislamiento entre usuarios"
req POST /auth/register "" "{\"email\":\"$EMAIL_B\",\"password\":\"$PASS\"}"
req POST /auth/login "" "{\"email\":\"$EMAIL_B\",\"password\":\"$PASS\"}"
TOKEN_B=$(echo "$BODY" | sed -E 's/.*"accessToken":"([^"]+)".*/\1/')

req GET /songs "$TOKEN_B" ""
check "Usuario B lista" 200
contains "  B no ve canciones de A (total 0)" '"total":0'
req GET "/songs/$SONG_ID" "$TOKEN_B" ""
check "B pide una canción de A" 404
req PATCH "/songs/$SONG_ID" "$TOKEN_B" '{"name":"hack"}'
check "B intenta editar una canción de A" 404
req DELETE "/songs/$SONG_ID" "$TOKEN_B" ""
check "B intenta borrar una canción de A" 404

echo
echo "## Eliminar"
req DELETE "/songs/$SONG_ID" "$TOKEN_A" ""
check "A elimina su canción" 200
req GET "/songs/$SONG_ID" "$TOKEN_A" ""
check "La canción eliminada ya no existe" 404

echo "-------------------------------------------"
if [ "$FAILS" -eq 0 ]; then
  echo "TODO OK: el contrato coincide."
else
  echo "$FAILS prueba(s) fallaron."
  exit 1
fi
