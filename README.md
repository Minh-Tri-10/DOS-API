# PRN232 â€“ DrinkOrder System (DOS) Microservices

Repo nay chua nhieu dich vu .NET 8 tach biet (Account, Categories, Cart, Order, Payment), mot API Gateway dang Ocelot va mot ung dung MVC lam frontend.

## Má»¥c lá»¥c
- Giá»›i thiá»‡u nhanh
- Kiáº¿n trÃºc & thÃ nh pháº§n
- CÃ´ng nghá»‡ sá»­ dá»¥ng
- YÃªu cáº§u há»‡ thá»‘ng
- Cáº¥u hÃ¬nh mÃ´i trÆ°á»ng (appsettings, secrets)
- Cháº¡y dá»± Ã¡n (CLI/Visual Studio)
- API Gateway (Ocelot)
- Swagger & Ä‘Æ°á»ng dáº«n truy cáº­p
- CÃ´ng cá»¥ há»— trá»£: ConnKeySwitcher
- Ghi chÃº CSDL
- ÄÃ³ng gÃ³p & phÃ¡t triá»ƒn

## Giá»›i thiá»‡u nhanh
- Nhiá»u service ASP.NET Core Web API Ä‘á»™c láº­p, giao tiáº¿p qua HTTP.
- EF Core káº¿t ná»‘i SQL Server, dÃ¹ng AutoMapper cho mapping DTO, Swagger Ä‘á»ƒ thá»­ API.
- EmailJS Ä‘Æ°á»£c dÃ¹ng á»Ÿ AccountAPI cho luá»“ng quÃªn/máº·t kháº©u; CategoriesAPI dÃ¹ng Cloudinary cho media.
- MVCApplication tiÃªu thá»¥ cÃ¡c API vÃ  cung cáº¥p giao diá»‡n ngÆ°á»i dÃ¹ng.

## Kiáº¿n trÃºc & thÃ nh pháº§n
CÃ¡c dá»± Ã¡n chÃ­nh trong solution:
- AccountAPI: quáº£n lÃ½ tÃ i khoáº£n, Ä‘Äƒng kÃ½/Ä‘Äƒng nháº­p, Ä‘á»•i/quÃªn máº­t kháº©u (EmailJS), quáº£n lÃ½ profile, khÃ³a tÃ i khoáº£n. Tham kháº£o cáº¥u hÃ¬nh: `AccountAPI/Program.cs` vÃ  `AccountAPI/appsettings.json`.
- CategoriesAPI: manages categories and product catalog, integrates with Cloudinary for media.
- CartAPI: giá» hÃ ng.
- OrderAPI: handles orders and reporting; fetches catalog data from CategoriesAPI and user info from AccountAPI via HttpClient.
- PaymentAPI: thanh toÃ¡n (mock), cáº­p nháº­t tráº¡ng thÃ¡i thanh toÃ¡n.
- APIGateways: API Gateway dÃ¹ng Ocelot, gom cÃ¡c route xuá»‘ng cÃ¡c service.
- MVCApplication: frontend MVC .NET hiá»ƒn thá»‹ vÃ  thao tÃ¡c vá»›i cÃ¡c service.


## CÃ´ng nghá»‡ sá»­ dá»¥ng
- .NET 8, ASP.NET Core Web API, MVC
- Entity Framework Core (SQL Server)
- AutoMapper
- Swagger/Swashbuckle
- Ocelot API Gateway
- BCrypt (bÄƒm máº­t kháº©u)
- MemoryCache (token reset máº­t kháº©u)
- EmailJS (gá»­i mail), Cloudinary (media)

## YÃªu cáº§u há»‡ thá»‘ng
- .NET SDK 8.x
- SQL Server (local/remote)
- Visual Studio 2022 hoáº·c VS Code + C# extension

## Cáº¥u hÃ¬nh mÃ´i trÆ°á»ng
1) Connection strings (SQL Server)
- Má»—i service cÃ³ `appsettings.json` chá»©a `ConnectionStrings` vá»›i nhiá»u key vÃ­ dá»¥: `HuyConnection`, `TriConnection`, `WeiConnection`, `DefaultConnection`, ...
- Code thÆ°á»ng láº¥y chuá»—i káº¿t ná»‘i báº±ng tÃªn key (vÃ­ dá»¥ `HuyConnection`). Báº¡n cÃ³ thá»ƒ:
  - Sá»­a giÃ¡ trá»‹ tÆ°Æ¡ng á»©ng trong tá»«ng `appsettings.json`, hoáº·c
  - DÃ¹ng tool ConnKeySwitcher (bÃªn dÆ°á»›i) Ä‘á»ƒ Ä‘á»“ng bá»™ nhanh tÃªn key dÃ¹ng trong `Program.cs` cá»§a táº¥t cáº£ service.

2) Frontend base URL
- AccountAPI gá»­i link reset password dá»±a trÃªn `Frontend:BaseUrl`.
- Äáº·t trÃ¹ng vá»›i HTTPS cá»§a MVCApplication (máº·c Ä‘á»‹nh `https://localhost:7223`).

3) EmailJS (AccountAPI)
- Cáº¥u hÃ¬nh táº¡i `AccountAPI/appsettings.json` má»¥c `Email` vá»›i `ServiceId`, `TemplateId`, `PublicKey`, `AccessToken`, `FromName`, `Origin`.
- Khuyáº¿n nghá»‹: khÃ´ng commit secret tháº­t; dÃ¹ng `appsettings.Development.json` hoáº·c UserSecrets.

4) Cloudinary (CategoriesAPI)
- Cáº¥u hÃ¬nh táº¡i `CategoriesAPI/appsettings.json` má»¥c `CloudinarySettings` (`CloudName`, `ApiKey`, `ApiSecret`).
- Khuyáº¿n nghá»‹: dÃ¹ng UserSecrets trong quÃ¡ trÃ¬nh dev.

LÆ°u Ã½: Repo hiá»‡n cÃ³ má»™t sá»‘ sample secret máº·c Ä‘á»‹nh phá»¥c vá»¥ demo. Khi triá»ƒn khai/thá»­ nghiá»‡m thá»±c táº¿, vui lÃ²ng thay toÃ n bá»™ secret báº±ng giÃ¡ trá»‹ vÃ  KHÃ”NG commit.

## Cháº¡y dá»± Ã¡n
### CÃ¡ch 1: DÃ²ng lá»‡nh (CLI)
- KhÃ´i phá»¥c & build
```bash
# Táº¡i thÆ° má»¥c gá»‘c repo
dotnet restore
dotnet build DOS.sln -c Debug
```
- Cháº¡y tá»«ng service (má»Ÿ nhiá»u terminal tab)
```bash
# API Gateway
dotnet run -p APIGateways/APIGateways.csproj

# CÃ¡c microservice
dotnet run -p AccountAPI/AccountAPI.csproj
dotnet run -p CategoriesAPI/CategoriesAPI.csproj
dotnet run -p CartAPI/CartAPI.csproj
dotnet run -p OrderAPI/OrderAPI.csproj
dotnet run -p PaymentAPI/PaymentAPI.csproj

# Frontend MVC
dotnet run -p MVCApplication/MVCApplication.csproj
```
- Cá»•ng máº·c Ä‘á»‹nh (theo launchSettings)
  - AccountAPI: https://localhost:7005
  - CategoriesAPI: https://localhost:7021
  - CartAPI: https://localhost:7143
  - OrderAPI: https://localhost:7269
  - PaymentAPI: https://localhost:7011
  - APIGateways (Ocelot): https://localhost:7001
  - MVCApplication: https://localhost:7223

### CÃ¡ch 2: Visual Studio 2022
- Má»Ÿ `DOS.sln`.
- Chá»‰nh â€œMultiple startup projectsâ€ Ä‘á»ƒ khá»Ÿi Ä‘á»™ng cÃ¡c service cáº§n thiáº¿t (Gateway, cÃ¡c API vÃ  MVCApplication).
- Cháº¡y á»Ÿ cáº¥u hÃ¬nh Debug.

## API Gateway (Ocelot)
- Cáº¥u hÃ¬nh route táº¡i: `APIGateways/Ocelot.json`.
- `GlobalConfiguration.BaseUrl` máº·c Ä‘á»‹nh: `http://localhost:7000` 


## Swagger & Ä‘Æ°á»ng dáº«n truy cáº­p nhanh
- Má»—i service báº­t Swagger á»Ÿ mÃ´i trÆ°á»ng Development:
  - AccountAPI: https://localhost:7005/swagger
  - CategoriesAPI: https://localhost:7021/swagger
  - CartAPI: https://localhost:7143/swagger
  - OrderAPI: https://localhost:7269/swagger
  - PaymentAPI: https://localhost:7011/swagger
- Frontend MVC: https://localhost:7223
- API Gateway (Ocelot): https://localhost:7001 (Gateway khÃ´ng cÃ³ Swagger tá»•ng há»£p máº·c Ä‘á»‹nh)

## CÃ´ng cá»¥ há»— trá»£: ConnKeySwitcher
- Má»¥c tiÃªu: Ä‘á»•i nhanh tÃªn key dÃ¹ng trong `GetConnectionString("...")` cá»§a táº¥t cáº£ `Program.cs` theo 1 key Ä‘Ã£ chá»n.
- Vá»‹ trÃ­: `Tools/ConnKeySwitcher`.
- CÃ¡ch dÃ¹ng nhanh:
  1. Build: `dotnet build Tools/ConnKeySwitcher/ConnKeySwitcher.csproj -c Release`
  2. Cháº¡y file `ConnKeySwitcher.exe` trong `Tools/ConnKeySwitcher/bin/Release/net8.0-windows/`.
  3. Chá»n thÆ° má»¥c repo vÃ  key (vÃ­ dá»¥ `HuyConnection`).
  4. Scan Ä‘á»ƒ xem trÆ°á»›c, Apply Ä‘á»ƒ Ã¡p dá»¥ng. Tool sáº½ táº¡o `.bak` cáº¡nh file `Program.cs` Ä‘Ã£ chá»‰nh.
- Chi tiáº¿t: xem `Tools/ConnKeySwitcher/README.md`.

## Ghi chu CSDL
- Moi service nay da duoc tach sang mot database rieng: `DOSAccountDb`, `DOSCartDb`, `DOSCatalogDb`, `DOSOrderDb`, `DOSPaymentDb`. Connection string mau trong tung `appsettings.json` giu nguyen ten key (vd `HuyConnection`) de nguoi dung tool cu van hoat dong.
- Tham khao `docs/database-separation.md` de tao schema moi va script copy du lieu tu database hop nhat cu (`DrinkOrderDB`) sang tung database doc lap.

##  ÄÃ³ng gÃ³p & phÃ¡t triá»ƒn
- Quy Æ°á»›c code: tuÃ¢n thá»§ phong cÃ¡ch sáºµn cÃ³ cá»§a tá»«ng service, sá»­ dá»¥ng AutoMapper cho mapping.
- Báº£o máº­t: Ä‘Æ°a secret (EmailJS, Cloudinary, connection strings tháº­t) vÃ o UserSecrets hoáº·c biáº¿n mÃ´i trÆ°á»ng; khÃ´ng commit.
- Váº¥n Ä‘á» biáº¿t trÆ°á»›c:
  - `APIGateways/Ocelot.json` lÃ  vÃ­ dá»¥ minh há»a, cáº§n Ä‘á»“ng bá»™ port downstream theo cá»•ng thá»±c táº¿ cá»§a service.


