# AD WebApp

En webapplikation til administration og visning af Active Directory-data, bygget med en Vue 3-frontend og en ASP.NET Core-backend.

## Teknologier

| Lag | Stack |
|-----|-------|
| Frontend | Vue 3, TypeScript, Vite, Pinia, Vue Router |
| Backend | ASP.NET Core (.NET 10), LDAP / System.DirectoryServices |
| Auth | OpenID Connect |
| Styling | SCSS, Buefy, Font Awesome |

## Projektstruktur

```
AD-WebApp/
├── AD.Client/   # Vue 3 frontend (Vite)
└── AD.Server/   # ASP.NET Core API + LDAP-integration
```

## Kom i gang

### Frontend

```bash
cd AD.Client
npm install
npm run dev
```

### Backend

```bash
cd AD.Server
dotnet run
```

> Konfigurer LDAP-forbindelsen og OIDC-indstillinger i `AD.Server/appsettings.json` inden opstart.
