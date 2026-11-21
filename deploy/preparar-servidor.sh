#!/usr/bin/env bash
# Prepara um servidor Ubuntu novo para o AgroArca. Rode UMA vez, como root:
#   curl -fsSL https://raw.githubusercontent.com/Matheuscunha20/SistemaDeGerenciamentoDeGranja/master/deploy/preparar-servidor.sh | bash -s -- seu-dominio.com.br
# Sem domínio ainda? Rode sem o domínio: o sistema fica em https://<ip>.sslip.io (endereço grátis).
set -euo pipefail

REPO_RAW="https://raw.githubusercontent.com/Matheuscunha20/SistemaDeGerenciamentoDeGranja/master"
PASTA=/opt/agroarca
USUARIO=deploy

if [ "$(id -u)" -ne 0 ]; then echo "Rode como root (ou com sudo)."; exit 1; fi

IP=$(curl -fsS https://api.ipify.org || hostname -I | awk '{print $1}')
DOMINIO="${1:-${IP//./-}.sslip.io}"

echo "==> 1/7 Atualizando o sistema e instalando o básico"
export DEBIAN_FRONTEND=noninteractive
apt-get update -y
apt-get upgrade -y
apt-get install -y ca-certificates curl ufw unattended-upgrades openssl
dpkg-reconfigure -f noninteractive unattended-upgrades   # atualizações de segurança automáticas

echo "==> 2/7 Instalando o Docker"
if ! command -v docker >/dev/null; then
  curl -fsSL https://get.docker.com | sh
fi
systemctl enable --now docker

echo "==> 3/7 Firewall: só SSH, HTTP e HTTPS"
ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw allow 443/udp
ufw --force enable

echo "==> 4/7 Memória extra (swap de 2 GB) para picos"
if ! swapon --show | grep -q /swapfile; then
  fallocate -l 2G /swapfile
  chmod 600 /swapfile
  mkswap /swapfile
  swapon /swapfile
  echo '/swapfile none swap sw 0 0' >> /etc/fstab
fi

echo "==> 5/7 Usuário '$USUARIO' para a publicação automática (com chave própria)"
if ! id "$USUARIO" >/dev/null 2>&1; then
  adduser --disabled-password --gecos "" "$USUARIO"
fi
usermod -aG docker "$USUARIO"
install -d -m 700 -o "$USUARIO" -g "$USUARIO" "/home/$USUARIO/.ssh"
if [ ! -f "/home/$USUARIO/.ssh/id_publicacao" ]; then
  sudo -u "$USUARIO" ssh-keygen -t ed25519 -N "" -C "github-publicacao" -f "/home/$USUARIO/.ssh/id_publicacao"
  cat "/home/$USUARIO/.ssh/id_publicacao.pub" >> "/home/$USUARIO/.ssh/authorized_keys"
  chown "$USUARIO:$USUARIO" "/home/$USUARIO/.ssh/authorized_keys"
  chmod 600 "/home/$USUARIO/.ssh/authorized_keys"
fi

echo "==> 6/7 Pasta do AgroArca e senhas"
mkdir -p "$PASTA"
curl -fsSL "$REPO_RAW/deploy/docker-compose.servidor.yml" -o "$PASTA/docker-compose.servidor.yml"
curl -fsSL "$REPO_RAW/deploy/Caddyfile" -o "$PASTA/Caddyfile"
if [ ! -f "$PASTA/.env" ]; then
  cat > "$PASTA/.env" <<ENV
# Configurações do AgroArca neste servidor. NÃO compartilhe este arquivo.
DOMINIO=$DOMINIO
MYSQL_ROOT_PASSWORD=$(openssl rand -hex 24)
MYSQL_PASSWORD=$(openssl rand -hex 24)
# E-mail (preencha depois, ex.: Brevo): smtp-relay.brevo.com / 587
EMAIL_SERVIDOR=
EMAIL_PORTA=587
EMAIL_USUARIO=
EMAIL_SENHA=
EMAIL_REMETENTE=
ENV
  chmod 600 "$PASTA/.env"
else
  echo "    .env já existe: mantido (as senhas não mudam)."
fi
chown -R "$USUARIO:$USUARIO" "$PASTA"

echo "==> 7/7 Pronto!"
echo
echo "================================================================"
echo " Endereço do sistema: https://$DOMINIO"
echo " IP do servidor:      $IP"
echo
echo " Agora, no GitHub (Settings > Secrets and variables > Actions),"
echo " crie estes 2 segredos:"
echo
echo "   SERVIDOR_IP         = $IP"
echo "   SERVIDOR_CHAVE_SSH  = (todo o texto entre as linhas abaixo)"
echo "----------------------------------------------------------------"
cat "/home/$USUARIO/.ssh/id_publicacao"
echo "----------------------------------------------------------------"
echo " Depois rode a publicação no GitHub (Actions > Publicar > Run workflow)."
echo "================================================================"
