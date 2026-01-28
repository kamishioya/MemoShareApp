# VPSデプロイ手順

## 前提条件
- Ubuntu 20.04以上 または CentOS 8以上
- Docker と Docker Compose がインストール済み
- ドメインまたはIPアドレス

## 1. Dockerのインストール（未インストールの場合）

### Ubuntu
```bash
sudo apt update
sudo apt install -y docker.io docker-compose-v2
sudo systemctl enable docker
sudo systemctl start docker
sudo usermod -aG docker $USER
```

### CentOS/RHEL
```bash
sudo yum install -y docker docker-compose
sudo systemctl enable docker
sudo systemctl start docker
sudo usermod -aG docker $USER
```

## 2. プロジェクトのデプロイ

```bash
# プロジェクトをクローン
git clone https://github.com/kamishioya/MemoShareApp.git
cd MemoShareApp

# 環境変数ファイルを作成
cp .env.example .env

# .envファイルを編集（パスワードとシークレットキーを変更）
nano .env
```

## 3. 環境変数の設定

`.env`ファイルを編集して以下を設定：

```env
POSTGRES_PASSWORD=your_strong_password_here
JWT_SECRET_KEY=your_very_long_and_random_secret_key_here
```

**重要**: 本番環境では必ず強力なパスワードとシークレットキーを使用してください！

## 4. アプリケーションの起動

```bash
# コンテナをビルドして起動
sudo docker compose up -d --build

# ログを確認
sudo docker compose logs -f

# 起動確認
curl http://localhost:5000/api/auth/login
```

## 5. HTTPS設定（Nginxリバースプロキシ + Let's Encrypt）

### Nginxのインストール
```bash
sudo apt install -y nginx certbot python3-certbot-nginx
```

### Nginx設定ファイルを作成
```bash
sudo nano /etc/nginx/sites-available/memoshare
```

以下を記述：
```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### 設定を有効化
```bash
sudo ln -s /etc/nginx/sites-available/memoshare /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

### SSL証明書の取得（Let's Encrypt）
```bash
sudo certbot --nginx -d your-domain.com
```

## 6. ファイアウォール設定

```bash
# UFW（Ubuntu）
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable

# firewalld（CentOS）
sudo firewall-cmd --permanent --add-service=http
sudo firewall-cmd --permanent --add-service=https
sudo firewall-cmd --reload
```

## 7. 動作確認

```bash
# APIの動作確認
curl https://your-domain.com/api/auth/login

# Swaggerにアクセス（開発環境のみ）
# https://your-domain.com/swagger
```

## 8. メンテナンスコマンド

```bash
# ログ確認
sudo docker compose logs -f api
sudo docker compose logs -f postgres

# コンテナ再起動
sudo docker compose restart

# コンテナ停止
sudo docker compose down

# データベースバックアップ
sudo docker compose exec postgres pg_dump -U postgres memosharedb > backup.sql

# データベースリストア
sudo docker compose exec -T postgres psql -U postgres memosharedb < backup.sql
```

## 9. 自動起動設定

```bash
# Dockerサービスの自動起動を有効化
sudo systemctl enable docker

# システム起動時にコンテナも起動
# docker-compose.ymlで restart: unless-stopped を設定済み
```

## 10. セキュリティ対策

- SSH設定: ポート変更、鍵認証、root ログイン無効化
- 定期的なセキュリティアップデート
- ファイアウォールの適切な設定
- `.env`ファイルのパーミッション: `chmod 600 .env`
- データベースバックアップの定期実行

## トラブルシューティング

### データベース接続エラー
```bash
# PostgreSQLの状態確認
sudo docker compose ps
sudo docker compose logs postgres

# データベース接続テスト
sudo docker compose exec postgres psql -U postgres -d memosharedb
```

### APIが起動しない
```bash
# APIログ確認
sudo docker compose logs api

# コンテナ再ビルド
sudo docker compose up -d --build --force-recreate
```

## API エンドポイント

### 認証
- POST `/api/auth/register` - ユーザー登録
- POST `/api/auth/login` - ログイン

### メモ
- GET `/api/memos/my` - 自分のメモ一覧
- GET `/api/memos/shared` - 共有メモ一覧
- GET `/api/memos/{id}` - メモ詳細
- POST `/api/memos` - メモ作成
- PUT `/api/memos/{id}` - メモ更新
- DELETE `/api/memos/{id}` - メモ削除
- POST `/api/memos/{id}/share` - メモ共有
- DELETE `/api/memos/{id}/share/{userId}` - 共有解除

全てのメモエンドポイントには `Authorization: Bearer {token}` ヘッダーが必要です。
