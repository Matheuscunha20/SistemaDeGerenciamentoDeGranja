#!/usr/bin/env bash
# Cria uma conta de teste no AgroArca (fazenda + usuário) pelo formulário de cadastro.
# Confere que o banco grava de verdade: fazenda, usuário, espécies, raças, vacinas e categorias.
# Uso: testar-cadastro.sh http://localhost:8080
set -euo pipefail
URL="${1:-http://localhost:8080}"
COOKIES=$(mktemp)

PAGINA=$(curl -fsS -c "$COOKIES" "$URL/Identity/Account/Register")
TOKEN=$(echo "$PAGINA" | grep -o '<input[^>]*__RequestVerificationToken[^>]*>' | head -1 | sed -n 's/.*value="\([^"]*\)".*/\1/p')
if [ -z "$TOKEN" ]; then echo "Não achei o token do formulário."; exit 1; fi

CODIGO=$(curl -s -o /tmp/resposta-cadastro.html -w "%{http_code}" -b "$COOKIES" -c "$COOKIES" \
  --data-urlencode "__RequestVerificationToken=$TOKEN" \
  --data-urlencode "Input.NomeFazenda=Fazenda Teste CI" \
  --data-urlencode "Input.Email=teste-ci@exemplo.com.br" \
  --data-urlencode "Input.Senha=Rebanho@2026" \
  --data-urlencode "Input.ConfirmarSenha=Rebanho@2026" \
  "$URL/Identity/Account/Register")

echo "Cadastro -> $CODIGO"
if [ "$CODIGO" != "302" ]; then
  echo "O cadastro não redirecionou (esperado 302). Erros na página:"
  grep -o 'alert-danger[^<]*<[^>]*>[^<]*' /tmp/resposta-cadastro.html | head -5 || true
  exit 1
fi
echo "Conta de teste criada com sucesso."
