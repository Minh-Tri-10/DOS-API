# PRN232 – DrinkOrder System (DOS) Microservices

Dự án cuối môn PRN232 xây dựng theo kiến trúc Microservices cho hệ thống đặt đồ uống (DrinkOrder System). Repo này chứa nhiều dịch vụ .NET 8 tách biệt (Account, Product, Categories, Cart, Order, Payment), một API Gateway dùng Ocelot và một ứng dụng MVC làm frontend.

## Mục lục
- Giới thiệu nhanh
- Kiến trúc & thành phần
- Công nghệ sử dụng
- Yêu cầu hệ thống
- Cấu hình môi trường (appsettings, secrets)
- Chạy dự án (CLI/Visual Studio)
- API Gateway (Ocelot)
- Swagger & đường dẫn truy cập
- Công cụ hỗ trợ: ConnKeySwitcher
- Ghi chú CSDL
- Đóng góp & phát triển

## Giới thiệu nhanh
- Nhiều service ASP.NET Core Web API độc lập, giao tiếp qua HTTP.
- EF Core kết nối SQL Server, dùng AutoMapper cho mapping DTO, Swagger để thử API.
- EmailJS được dùng ở AccountAPI cho luồng quên/mặt khẩu; CategoriesAPI dùng Cloudinary cho media.
- MVCApplication tiêu thụ các API và cung cấp giao diện người dùng.

## Kiến trúc & thành phần
Các dự án chính trong solution:
- AccountAPI: quản lý tài khoản, đăng ký/đăng nhập, đổi/quên mật khẩu (EmailJS), quản lý profile, khóa tài khoản. Tham khảo cấu hình: `AccountAPI/Program.cs` và `AccountAPI/appsettings.json`.
- ProductAPI: quản lý sản phẩm.
- CategoriesAPI: quản lý danh mục, tích hợp Cloudinary cho ảnh.
- CartAPI: giỏ hàng.
- OrderAPI: đơn hàng và số liệu thống kê; gọi Product/Categories/Account qua HttpClient.
- PaymentAPI: thanh toán (mock), cập nhật trạng thái thanh toán.
- APIGateways: API Gateway dùng Ocelot, gom các route xuống các service.
- MVCApplication: frontend MVC .NET hiển thị và thao tác với các service.


## Công nghệ sử dụng
- .NET 8, ASP.NET Core Web API, MVC
- Entity Framework Core (SQL Server)
- AutoMapper
- Swagger/Swashbuckle
- Ocelot API Gateway
- BCrypt (băm mật khẩu)
- MemoryCache (token reset mật khẩu)
- EmailJS (gửi mail), Cloudinary (media)

## Yêu cầu hệ thống
- .NET SDK 8.x
- SQL Server (local/remote)
- Visual Studio 2022 hoặc VS Code + C# extension

## Cấu hình môi trường
1) Connection strings (SQL Server)
- Mỗi service có `appsettings.json` chứa `ConnectionStrings` với nhiều key ví dụ: `HuyConnection`, `TriConnection`, `WeiConnection`, `DefaultConnection`, ...
- Code thường lấy chuỗi kết nối bằng tên key (ví dụ `HuyConnection`). Bạn có thể:
  - Sửa giá trị tương ứng trong từng `appsettings.json`, hoặc
  - Dùng tool ConnKeySwitcher (bên dưới) để đồng bộ nhanh tên key dùng trong `Program.cs` của tất cả service.

2) Frontend base URL
- AccountAPI gửi link reset password dựa trên `Frontend:BaseUrl`.
- Đặt trùng với HTTPS của MVCApplication (mặc định `https://localhost:7223`).

3) EmailJS (AccountAPI)
- Cấu hình tại `AccountAPI/appsettings.json` mục `Email` với `ServiceId`, `TemplateId`, `PublicKey`, `AccessToken`, `FromName`, `Origin`.
- Khuyến nghị: không commit secret thật; dùng `appsettings.Development.json` hoặc UserSecrets.

4) Cloudinary (CategoriesAPI)
- Cấu hình tại `CategoriesAPI/appsettings.json` mục `CloudinarySettings` (`CloudName`, `ApiKey`, `ApiSecret`).
- Khuyến nghị: dùng UserSecrets trong quá trình dev.

Lưu ý: Repo hiện có một số sample secret mặc định phục vụ demo. Khi triển khai/thử nghiệm thực tế, vui lòng thay toàn bộ secret bằng giá trị và KHÔNG commit.

## Chạy dự án
### Cách 1: Dòng lệnh (CLI)
- Khôi phục & build
```bash
# Tại thư mục gốc repo
dotnet restore
dotnet build DOS.sln -c Debug
```
- Chạy từng service (mở nhiều terminal tab)
```bash
# API Gateway
dotnet run -p APIGateways/APIGateways.csproj

# Các microservice
dotnet run -p AccountAPI/AccountAPI.csproj
dotnet run -p CategoriesAPI/CategoriesAPI.csproj
dotnet run -p ProductAPI/ProductAPI.csproj     
dotnet run -p CartAPI/CartAPI.csproj
dotnet run -p OrderAPI/OrderAPI.csproj
dotnet run -p PaymentAPI/PaymentAPI.csproj

# Frontend MVC
dotnet run -p MVCApplication/MVCApplication.csproj
```
- Cổng mặc định (theo launchSettings)
  - AccountAPI: https://localhost:7005
  - CategoriesAPI: https://localhost:7021
  - ProductAPI: (demo Program, port thực tế tùy cấu hình khi thêm csproj)
  - CartAPI: https://localhost:7143
  - OrderAPI: https://localhost:7269
  - PaymentAPI: https://localhost:7011
  - APIGateways (Ocelot): https://localhost:7001
  - MVCApplication: https://localhost:7223

### Cách 2: Visual Studio 2022
- Mở `DOS.sln`.
- Chỉnh “Multiple startup projects” để khởi động các service cần thiết (Gateway, các API và MVCApplication).
- Chạy ở cấu hình Debug.

## API Gateway (Ocelot)
- Cấu hình route tại: `APIGateways/Ocelot.json`.
- `GlobalConfiguration.BaseUrl` mặc định: `http://localhost:7000` 


## Swagger & đường dẫn truy cập nhanh
- Mỗi service bật Swagger ở môi trường Development:
  - AccountAPI: https://localhost:7005/swagger
  - CategoriesAPI: https://localhost:7021/swagger
  - CartAPI: https://localhost:7143/swagger
  - OrderAPI: https://localhost:7269/swagger
  - PaymentAPI: https://localhost:7011/swagger
- Frontend MVC: https://localhost:7223
- API Gateway (Ocelot): https://localhost:7001 (Gateway không có Swagger tổng hợp mặc định)

## Công cụ hỗ trợ: ConnKeySwitcher
- Mục tiêu: đổi nhanh tên key dùng trong `GetConnectionString("...")` của tất cả `Program.cs` theo 1 key đã chọn.
- Vị trí: `Tools/ConnKeySwitcher`.
- Cách dùng nhanh:
  1. Build: `dotnet build Tools/ConnKeySwitcher/ConnKeySwitcher.csproj -c Release`
  2. Chạy file `ConnKeySwitcher.exe` trong `Tools/ConnKeySwitcher/bin/Release/net8.0-windows/`.
  3. Chọn thư mục repo và key (ví dụ `HuyConnection`).
  4. Scan để xem trước, Apply để áp dụng. Tool sẽ tạo `.bak` cạnh file `Program.cs` đã chỉnh.
- Chi tiết: xem `Tools/ConnKeySwitcher/README.md`.

## Ghi chú CSDL
- Dự án dùng EF Core kiểu database-first (không kèm migrations). Cần có sẵn database `DrinkOrderDB` với các bảng tương ứng (ví dụ `Users` trong AccountAPI).
- Hãy chỉnh chuỗi kết nối tới SQL Server chứa DB này, hoặc tự tạo/khôi phục DB theo yêu cầu môn học.

## Đóng góp & phát triển
- Quy ước code: tuân thủ phong cách sẵn có của từng service, sử dụng AutoMapper cho mapping.
- Bảo mật: đưa secret (EmailJS, Cloudinary, connection strings thật) vào UserSecrets hoặc biến môi trường; không commit.
- Vấn đề biết trước:
  - `APIGateways/Ocelot.json` là ví dụ minh họa, cần đồng bộ port downstream theo cổng thực tế của service.
  - `ProductAPI` trong repo hiện có `Program.cs` demo; nếu cần chạy độc lập, bổ sung file `.csproj` và cấu trúc chuẩn.
