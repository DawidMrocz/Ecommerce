# 🛒 Ecommerce – mikroserwisy na .NET 10

Przykładowa platforma e-commerce zbudowana w architekturze mikroserwisowej: API Gateway, serwisy tożsamości, produktów i koszyka, komunikacja asynchroniczna przez RabbitMQ oraz gotowe manifesty do uruchomienia w Dockerze i Kubernetesie.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)
![Kubernetes](https://img.shields.io/badge/Kubernetes-manifests-326CE5?logo=kubernetes&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-MassTransit-FF6600?logo=rabbitmq&logoColor=white)
![Terraform](https://img.shields.io/badge/Terraform-Azure-7B42BC?logo=terraform&logoColor=white)

## ✨ Funkcje

- **API Gateway** (`Gateway.Api`) – jeden punkt wejścia, routing do pozostałych serwisów, integracja z KSeF (demo).
- **Identity** (`Identity.Api`) – rejestracja, logowanie, odświeżanie tokenów, zdjęcia profilowe; JWT podpisywany parą kluczy RSA.
- **Product** (`Product.Api`) – CRUD produktów, zdjęcia produktów w Azure Blob Storage (sekrety z Azure Key Vault).
- **Cart** (`Cart.Api`) – koszyk użytkownika i jego pozycje.
- **Zdarzenia** – synchronizacja stanu między serwisami przez RabbitMQ (MassTransit): dodanie do koszyka, usunięcie/zmiana ilości pozycji, utworzenie/usunięcie produktu.
- **MroczwareFramework** – wspólna biblioteka: uwierzytelnianie, e-mail, cache, logowanie (log4net), pliki, szablony.
- **ECommerceFunctions** – Azure Functions.
- **infra/** – Terraform (Resource Group, Storage Account, Key Vault) oraz manifesty K8s w katalogu głównym.

## 🏗️ Architektura

```
            ┌──────────────┐
  Klient ──▶│  Gateway.Api │
            └──────┬───────┘
      ┌────────────┼─────────────┐
      ▼            ▼             ▼
┌───────────┐ ┌───────────┐ ┌──────────┐
│Identity.Api│ │Product.Api│ │ Cart.Api │
└─────┬─────┘ └─────┬─────┘ └────┬─────┘
      │             └──── RabbitMQ ────┘   (MassTransit)
      ▼
  SQL Server            Azure Blob Storage + Key Vault
```

## 🧰 Stos technologiczny

| Obszar | Technologie |
|---|---|
| Backend | ASP.NET Core (.NET 10), Entity Framework Core, Swagger/OpenAPI |
| Baza danych | Microsoft SQL Server |
| Messaging | RabbitMQ + MassTransit |
| Auth | JWT (RS256), BCrypt |
| Chmura | Azure Blob Storage, Azure Key Vault, Azure Functions |
| DevOps | Docker, Docker Compose, Kubernetes, Terraform |

## 📁 Struktura repozytorium

```
Cart.Api/               serwis koszyka
Gateway.Api/            API Gateway (+ KSeF)
Identity.Api/           tożsamość i użytkownicy
Product.Api/            produkty i zdjęcia
Contracts/              wspólne modele i kontrakty zdarzeń
MroczwareFramework/     wspólna biblioteka
ECommerceFunctions/     Azure Functions
infra/terraform/        infrastruktura jako kod
*.yaml                  manifesty Kubernetes
docker-compose.yml      lokalne uruchomienie całości
```

## 🚀 Uruchomienie lokalne

### Wymagania

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- Docker + Docker Compose
- SQL Server (dostępny z kontenerów)

### 1. Konfiguracja sekretów

Repozytorium **nie zawiera** żadnych haseł ani kluczy. Wszystkie wartości wrażliwe podajesz przez plik `.env`:

```bash
cp .env.example .env
```

Uzupełnij `.env` (connection string do SQL Server, dane SMTP, hasła RabbitMQ itd.).

Parę kluczy RSA do podpisywania JWT (sam base64, bez nagłówków PEM) możesz wygenerować tak:

```bash
openssl genrsa -traditional -out jwt.pem 2048
openssl rsa -in jwt.pem -RSAPublicKey_out -out jwt.pub.pem
# wartości do .env (bez nagłówków i łamania linii):
grep -v -- '-----' jwt.pem | tr -d '\n'       # JWT_PRIVATE_KEY_PEM
grep -v -- '-----' jwt.pub.pem | tr -d '\n'   # JWT_PUBLIC_KEY_PEM
```

> Pliki `*.pem`, `*.pfx`, `.env` są w `.gitignore` – nie commituj ich.

### 2. Start w Dockerze

```bash
docker compose up --build
```

| Serwis | Adres |
|---|---|
| Gateway.Api | http://localhost:8000/swagger |
| Cart.Api | http://localhost:8001/swagger |
| Identity.Api | http://localhost:8002/swagger |
| Product.Api | http://localhost:8003/swagger |
| RabbitMQ UI | http://localhost:15672 |

### 3. Bez Dockera

Ustaw te same zmienne (np. przez `dotnet user-secrets` lub zmienne środowiskowe, np. `ConnectionStrings__DefaultConnection`, `JWT__PublicKeyPem`) i uruchom wybrany projekt:

```bash
dotnet run --project Identity.Api
```

## ☸️ Kubernetes

```bash
cp secrets.example.yaml secrets.yaml   # uzupełnij wartości
kubectl apply -f secrets.yaml -f configs.yaml -f rabbit.yaml
kubectl apply -f identityapi.yaml -f productapi.yaml -f cartapi.yaml -f gatewayapi.yaml
```

Obrazy są pobierane z Docker Hub (`dawid064/*`) – zmień nazwy obrazów na własne rejestry, jeśli budujesz je samodzielnie.

## ☁️ Infrastruktura (Terraform)

```bash
cd infra/terraform
cp terraform.tfvars.example terraform.tfvars   # uzupełnij wartości
terraform init && terraform plan
```

## Azure Functions

```bash
cp ECommerceFunctions/local.settings.example.json ECommerceFunctions/local.settings.json
```

Uzupełnij `AzureWebJobsStorage` własnym kontem Storage (lub użyj emulatora Azurite).

## 🔐 Bezpieczeństwo

- Sekrety trzymaj wyłącznie w `.env`, `secrets.yaml`, `local.settings.json`, `terraform.tfvars` lub w Azure Key Vault – wszystkie są ignorowane przez git.
- W repozytorium znajdują się tylko szablony: `*.example`.
- Wartości domyślne (np. `guest`/`guest` dla RabbitMQ) służą wyłącznie do lokalnego developmentu – zmień je w środowiskach współdzielonych.

## 📄 Licencja

Dodaj plik `LICENSE` (np. MIT), jeśli chcesz udostępnić kod na otwartej licencji.
