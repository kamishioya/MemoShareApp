# ビルドステージ
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# プロジェクトファイルをコピーして依存関係を復元
COPY ["MemoShareApp.API/MemoShareApp.API.csproj", "MemoShareApp.API/"]
RUN dotnet restore "MemoShareApp.API/MemoShareApp.API.csproj"

# ソースコードをコピーしてビルド
COPY MemoShareApp.API/ MemoShareApp.API/
WORKDIR "/src/MemoShareApp.API"
RUN dotnet build "MemoShareApp.API.csproj" -c Release -o /app/build

# パブリッシュステージ
FROM build AS publish
RUN dotnet publish "MemoShareApp.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 実行ステージ
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# ビルド成果物をコピー
COPY --from=publish /app/publish .

# エントリーポイント
ENTRYPOINT ["dotnet", "MemoShareApp.API.dll"]
