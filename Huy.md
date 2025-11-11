# Huy – Ghi chú chi tiết về JWT, đăng ký/đăng nhập, Ban User và API Gateway

## 1. Kiến trúc tổng thể
- **AccountAPI** chịu trách nhiệm nhận/đáp ứng mọi yêu cầu auth (đăng ký, đăng nhập, ban, quên mật khẩu). JWT được ký tại đây và mọi endpoint (trừ `[AllowAnonymous]`) bảo vệ bằng `[Authorize]`. Cấu hình ở `AccountAPI/Program.cs:15-105`.
- **APIGateways** (Ocelot) là lớp biên: nó đọc `APIGateways/Ocelot.json` và forward request tới đúng service, đồng thời áp chính sách JWT/CORS/HTTPS chung (xem `APIGateways/Program.cs:11-72`).
- **MVCApplication** là web app cho người dùng cuối. Nó gọi tới gateway bằng `HttpClient`, lưu JWT vào cookie claims và dùng `AccessTokenHandler` để tự động đính `Authorization: Bearer ...` cho mọi request ra ngoài (`MVCApplication/Infrastructure/AccessTokenHandler.cs:9-44`).

## 2. Luồng JWT (server)
1. **Cấu hình key/issuer/audience** nằm trong `AccountAPI/appsettings.Development.json:16-21`. Đây là nguồn duy nhất để tạo/kiểm tra token.
2. **Đăng ký JWT middleware**:
   ```csharp
   // AccountAPI/Program.cs:51-90
   builder.Services
       .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateIssuerSigningKey = true,
               ValidateLifetime = true,
               ValidIssuer = jwtSection["Issuer"],
               ValidAudience = jwtSection["Audience"],
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
               ClockSkew = TimeSpan.FromMinutes(1) // tránh lệch giờ
           };
           options.Events = new JwtBearerEvents
           {
               OnAuthenticationFailed = ctx =>
               {
                   var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                   logger.LogError(ctx.Exception, "JWT failed for {Path}", ctx.HttpContext.Request.Path);
                   return Task.CompletedTask;
               }
           };
       });
   ```
   Middleware này cộng với `app.UseAuthentication()` bảo vệ controller `AccountsController` (được đánh `[Authorize]` ở `AccountAPI/Controllers/AccountsController.cs:9-12`).
3. **Phát token**:
   ```csharp
   // AccountAPI/Services/AccountService.cs:68-119
   private (string Token, DateTime ExpiresAtUtc) GenerateJwtToken(User user)
   {
       var claims = new List<Claim>
       {
           new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()), // Subject = UserId
           new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
           new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
           new Claim(ClaimTypes.Name, user.Username)
       };

       var normalizedRole = /* chuẩn hóa Role để downstream xử lý */
           string.IsNullOrWhiteSpace(user.Role)
               ? "User"
               : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(user.Role.Trim().ToLowerInvariant());
       claims.Add(new Claim(ClaimTypes.Role, normalizedRole));
       claims.Add(new Claim("role", normalizedRole)); // cho client dễ đọc

       if (!string.IsNullOrWhiteSpace(user.Email))
           claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
       if (!string.IsNullOrWhiteSpace(user.FullName))
           claims.Add(new Claim("full_name", user.FullName));
       if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
           claims.Add(new Claim("avatar_url", user.AvatarUrl));

       var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
       var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
       var token = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: credentials);
       return (new JwtSecurityTokenHandler().WriteToken(token), expires);
   }
   ```
4. **Chặn login nếu bị ban**: `AccountService.LoginAsync` kiểm tra `user.IsBanned` trước khi phát token (`AccountAPI/Services/AccountService.cs:44-61`). >> nếu user bị ban, trả `null` → controller trả `401`.

## 3. Luồng JWT bên MVC (client)
1. Người dùng submit form -> `MVCApplication/Controllers/AccountsController.cs:27-87` gọi `_service.LoginAsync`.
2. Kết quả thành công được kiểm chứng lại bằng `JwtSecurityTokenHandler().ReadJwtToken(auth.AccessToken)` để chắc chắn token hợp lệ trước khi ghi cookie.
3. Claims được ghép lại và lưu vào cookie auth:
   ```csharp
   // MVCApplication/Controllers/AccountsController.cs:46-80
   var claims = new List<Claim>(jwt.Claims)
   {
       new Claim("access_token", auth.AccessToken),           // để DelegatingHandler tái sử dụng
       new Claim("avatar_url", auth.User.AvatarUrl ?? ""),
       new Claim("full_name", auth.User.FullName ?? ""),
       new Claim("username", auth.User.Username)
   };
   claims.RemoveAll(c => c.Type == ClaimTypes.Role);          // dọn role cũ trong JWT
   claims.Add(new Claim(ClaimTypes.Role, normalizedRole));    // role chuẩn hóa cho ASP.NET Core

+  await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
+      new AuthenticationProperties { IsPersistent = false, ExpiresUtc = auth.ExpiresAtUtc });
   ```
4. Bất kỳ `HttpClient` nào được tạo trong MVC đều đi qua `AccessTokenHandler` (`MVCApplication/Infrastructure/AccessTokenHandler.cs:9-44`):
   ```csharp
   var token = httpContext?.User?.FindFirst("access_token")?.Value;
   if (!string.IsNullOrWhiteSpace(token) && request.Headers.Authorization == null)
   {
       request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); // đính JWT vào request tới Gateway
   }
   ```
   => Gateway và các API đằng sau luôn nhận đúng JWT mà AccountAPI phát.

## 4. Luồng Đăng ký & Validation
### 4.1 Phía MVC (form đầu vào)
- `RegisterViewModel` (`MVCApplication/Models/RegisterViewModel.cs:5-25`) đặt toàn bộ `[Required]`, `[StringLength]`, `[Compare]`, `[EmailAddress]` → dữ liệu không hợp lệ bị ModelState chặn trước khi gửi xuống backend.
- `MVCApplication/Controllers/AccountsController.cs:93-132`:
  - Nếu `ModelState.IsValid == false` → trả lại view ngay.
  - Khi `_service.RegisterAsync` báo conflict, `InvalidOperationException` bị catch và đưa thông báo cho người dùng (`ModelState.AddModelError`).

### 4.2 Phía AccountAPI
1. **Controller**: `POST /api/accounts/register` (`AccountAPI/Controllers/AccountsController.cs:21-34`) nhận `RegisterDTO`. `[ApiController]` tự động bật model validation → request invalid trả 400 cùng `ModelState`.
2. **DTO**: `AccountAPI/DTOs/RegisterDTO.cs:7-24` chứa cùng tập DataAnnotations (username ≥4, password ≥6, email/phone chuẩn).
3. **Service**:
   ```csharp
   // AccountAPI/Services/AccountService.cs:122-143
   dto.Username = dto.Username?.Trim() ?? string.Empty;
   if (string.IsNullOrWhiteSpace(dto.Username))
       throw new InvalidOperationException("Tên đăng nhập là bắt buộc.");
   if (await _repo.GetByUsernameAsync(dto.Username) != null)
       throw new InvalidOperationException("Tên đăng nhập đã được sử dụng.");
   if (!string.IsNullOrWhiteSpace(dto.Email) &&
       await _repo.GetByEmailAsync(dto.Email) != null)
       throw new InvalidOperationException("Email đã được sử dụng.");

   user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password); // hashing trước khi lưu
   await _repo.AddAsync(user);
   ```
4. **Repository**: `AccountAPI/Repositories/UserRepository.cs:10-34` dùng EF Core và lưu xuống SQL Server.

=> Validation đủ 3 lớp: MVC ViewModel (client), DTO (API) và kiểm tra nghiệp vụ (service).

## 5. Luồng Đăng nhập & Validation
1. **MVC validation**: `LoginViewModel` (`MVCApplication/Models/LoginViewModel.cs:5-13`) yêu cầu đủ username/password và hiển thị thông báo localized.
2. **API service**:
   ```csharp
   // AccountAPI/Services/AccountService.cs:44-61
   var user = await _repo.GetByUsernameAsync(dto.Username);
   if (user == null || (user.IsBanned ?? false))
       return null;                               // ẩn chi tiết để tránh dò tài khoản
   if (!IsBcryptHash(user.PasswordHash) ||
       !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
       return null;
   return new AuthResponseDTO { AccessToken = token, ExpiresAtUtc = expiresAtUtc, User = mapped };
   ```
3. **Controller**: `AccountAPI/Controllers/AccountsController.cs:36-43` trả `401` với thông báo chung “Invalid username or password” để tránh lộ thông tin.
4. **MVC phản hồi**: `AccountsController.Login` nhận `null` và set `ViewBag.Error = "Sai tai khoan hoac mat khau."` (`MVCApplication/Controllers/AccountsController.cs:36-44`).

## 6. Ban / Unban User
1. **Endpoint**: `PATCH /api/accounts/{id}/ban` (`AccountAPI/Controllers/AccountsController.cs:67-73`). Action bị bảo vệ bởi `[Authorize(Roles = "Admin")]`.
2. **Payload**: `AccountAPI/DTOs/BanRequestDTO.cs:5-11` chỉ chứa `bool IsBanned` + `[Required]`.
3. **Service**:
   ```csharp
   // AccountAPI/Services/AccountService.cs:189-197
   var user = await _repo.GetByIdAsync(userId);
   if (user == null) return false;
   user.IsBanned = isBanned;
   user.UpdatedAt = DateTime.UtcNow;
   await _repo.UpdateAsync(user);
   ```
4. **Ảnh hưởng**: mọi lần đăng nhập đi qua `LoginAsync` (xem mục 5.2) sẽ trả `null` nếu `IsBanned == true`, nghĩa là token không thể được cấp lại cho user bị khóa.
5. **MVC quản lý**: service `_service.SetBanAsync` (`MVCApplication/Services/AccountService.cs:44-69`) gửi `PATCH` qua gateway; nếu HTTP != 204/200 sẽ log warning để admin biết.

## 7. Gateway & quản lý API
1. **Middleware chung** (JWT, CORS, HTTPS) được bật ở `APIGateways/Program.cs:15-67`. Gateway cũng bật Swagger để test nhanh tất cả route.
2. **JWT check tại cổng**: Gateway dùng cùng issuer/audience/key như AccountAPI → nếu request từ MVC kèm token sai sẽ bị chặn ngay trước khi tới microservice đích.
3. **Routing với Ocelot**:
   ```json
   {
     "UpstreamPathTemplate": "/api/product",
     "UpstreamHttpMethod": [ "GET" ],
     "AuthenticationOptions": {
       "AuthenticationProviderKey": "Bearer",
       "AllowedScopes": []
     },
     "DownstreamPathTemplate": "/api/Product",
     "DownstreamScheme": "https",
     "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 7021 }]
   }
   ```
   - Trích từ `APIGateways/Ocelot.json:6-26`. Gateway nhận `GET https://localhost:7001/api/product`, kiểm tra JWT (nhờ `AuthenticationOptions`), sau đó forward tới service Product ở `https://localhost:7021/api/Product`.
4. **Route không cần token** (ví dụ đăng nhập/đăng ký) không khai báo `AuthenticationOptions`, vì vậy client có thể gọi thẳng:
   ```json
   // APIGateways/Ocelot.json:28-57
   {
     "UpstreamPathTemplate": "/api/accounts/login",
     "UpstreamHttpMethod": [ "POST" ],
     "DownstreamScheme": "https",
     "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 7005 }]
   }
   ```
5. **Tự reload cấu hình**: `builder.Configuration.AddJsonFile("Ocelot.json", optional: false, reloadOnChange: true);` (`APIGateways/Program.cs:15-16`) → chỉ cần sửa `Ocelot.json`, gateway tự nạp lại mà không phải restart ứng dụng.
6. **Tích hợp với MVC**: mọi `HttpClient` trong `Program.cs` của MVC đều đặt `BaseAddress = https://localhost:7001/` (gateway). Vì `AccessTokenHandler` tự gắn token, MVC chỉ cần gọi route “ảo” như `api/accounts/login`, `api/orders/...` mà không biết service cụ thể.

## 8. Tài liệu nhanh (cheat sheet)
- **JWT config**: `AccountAPI/appsettings.Development.json:16-21`.
- **JWT middleware**: `AccountAPI/Program.cs:51-90` & `APIGateways/Program.cs:22-45`.
- **Phát token**: `AccountAPI/Services/AccountService.cs:68-119`.
- **Đăng ký**: Controller `AccountAPI/Controllers/AccountsController.cs:21-34`, DTO `AccountAPI/DTOs/RegisterDTO.cs:7-24`, Service `AccountAPI/Services/AccountService.cs:122-143`, MVC ViewModel `MVCApplication/Models/RegisterViewModel.cs:5-25`.
- **Đăng nhập**: Controller `AccountAPI/Controllers/AccountsController.cs:36-43`, Service `AccountAPI/Services/AccountService.cs:44-61`, MVC Controller `MVCApplication/Controllers/AccountsController.cs:27-87`.
- **Ban**: Endpoint `AccountAPI/Controllers/AccountsController.cs:67-73`, DTO `AccountAPI/DTOs/BanRequestDTO.cs:5-11`, Service `AccountAPI/Services/AccountService.cs:189-197`, MVC service client `MVCApplication/Services/AccountService.cs:44-69`.
- **Gateway routes**: `APIGateways/Ocelot.json`, reload logic & middleware `APIGateways/Program.cs:15-67`.

> Nhìn chung, chuỗi bảo mật là: người dùng → MVC (validate form) → Gateway → AccountAPI (phát/kiểm tra JWT) → JWT trả về được lưu trong cookie (MVC) → mọi request sau đó gắn token nhờ `AccessTokenHandler` → Gateway xác thực lại trước khi forward tới các microservice khác.

