# Booking Villa - Hybrid Application

Dự án hiện tại là **Hybrid Application** kết hợp cả **Razor Pages (Web UI)** và **Web API với JWT Authentication** trong cùng một project.

## Cấu trúc

- **Razor Pages** (`/Pages/*`): Web UI truyền thống sử dụng **Session-based authentication**
- **Web API** (`/api/*`): RESTful API endpoints sử dụng **JWT Bearer Token authentication**
- **Swagger UI**: Truy cập tại `/swagger` để test API endpoints

Cả 2 phần cùng hoạt động trong cùng một application và chia sẻ:
- Database Context
- Repositories và Business Logic
- Services

## Authentication

### Razor Pages (Web UI)
- Sử dụng **Session-based authentication** như cũ
- Session được lưu trong cookie
- Endpoints: `/Auth/Login`, `/Auth/Register`, etc.

### Web API
- Sử dụng **JWT Bearer Token authentication**
- Token được gửi qua header: `Authorization: Bearer {token}`
- Endpoints: `/api/auth/login`, `/api/auth/register`, etc.

## Cấu hình

### 1. Cấu hình JWT trong appsettings.json

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "BookingVilla",
    "Audience": "BookingVillaUsers",
    "ExpiryMinutes": "60"
  }
}
```

**Lưu ý**: Trong production, nên sử dụng một SecretKey mạnh và lưu trữ trong biến môi trường hoặc Azure Key Vault.

### 2. Connection String

Đảm bảo ConnectionString trong `appsettings.json` đúng với database của bạn:

```json
{
  "ConnectionStrings": {
    "BookingVillaPRN": "Server=(local);Database=BookingVillaPRN;User Id=sa;Password=1;Encrypt=False;TrustServerCertificate=True;"
  }
}
```

## Chạy ứng dụng

1. Restore packages:
```bash
dotnet restore
```

2. Chạy ứng dụng:
```bash
dotnet run --project BookingVilla
```

3. Truy cập Swagger UI (Development mode):
- URL: `https://localhost:{port}/swagger`
- Hoặc: `http://localhost:{port}/swagger`

## API Endpoints

### Authentication

#### 1. Đăng nhập
```
POST /api/auth/login
Content-Type: application/json

{
  "username": "string",
  "password": "string"
}

Response:
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "userInfo": {
      "id": 1,
      "name": "User Name",
      "email": "user@example.com",
      "role": "us",
      "userType": "Customer"
    }
  }
}
```

#### 2. Đăng ký
```
POST /api/auth/register
Content-Type: application/json

{
  "username": "string",
  "password": "string",
  "confirmPassword": "string",
  "name": "string",
  "email": "string",
  "phone": "string"
}
```

#### 3. Lấy thông tin user hiện tại
```
GET /api/auth/me
Authorization: Bearer {token}
```

### Villa

#### 1. Lấy danh sách Villa
```
GET /api/villa?page=1&sortBy=price
Authorization: Not required
```

#### 2. Lấy Villa theo ID
```
GET /api/villa/{id}
Authorization: Not required
```

#### 3. Tìm kiếm Villa
```
GET /api/villa/search?name={name}
Authorization: Not required
```

#### 4. Lấy Villa có sẵn
```
GET /api/villa/available?from=2024-01-01&to=2024-01-05&amountRoom=2&amountPeople=4
Authorization: Not required
```

#### 5. Thêm Villa (Admin only)
```
POST /api/villa
Authorization: Bearer {token} (Role: ad)
Content-Type: application/json

{
  "name": "string",
  "describe": "string",
  ...
}
```

#### 6. Cập nhật Villa (Admin only)
```
PUT /api/villa/{id}
Authorization: Bearer {token} (Role: ad)
Content-Type: application/json
```

### Booking

#### 1. Tạo Booking
```
POST /api/booking
Authorization: Bearer {token} (Role: us, ad)
Content-Type: application/json

{
  "villaId": 1,
  "checkinDate": "2024-01-01T00:00:00",
  "checkoutDate": "2024-01-05T00:00:00",
  "amountOfPeople": 4,
  "serviceIds": [1, 2]
}
```

#### 2. Tính giá Booking
```
GET /api/booking/calculate-price?villaId=1&checkinDate=2024-01-01&checkoutDate=2024-01-05&amountOfPeople=4
Authorization: Not required
```

#### 3. Lấy Booking theo ID
```
GET /api/booking/{id}
Authorization: Bearer {token}
```

## Sử dụng JWT Token

### 1. Lấy Token sau khi Login

Sau khi login thành công, bạn sẽ nhận được token trong response:
```json
{
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

### 2. Sử dụng Token trong Request

Thêm token vào header của mọi request yêu cầu authentication:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 3. Ví dụ với curl

```bash
# Login
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"user1","password":"pass123"}'

# Get Villa (no auth required)
curl -X GET "https://localhost:5001/api/villa"

# Create Booking (auth required)
curl -X POST "https://localhost:5001/api/booking" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{"villaId":1,"checkinDate":"2024-01-01T00:00:00","checkoutDate":"2024-01-05T00:00:00","amountOfPeople":4}'
```

### 4. Ví dụ với JavaScript/Fetch

```javascript
// Login
const loginResponse = await fetch('https://localhost:5001/api/auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    username: 'user1',
    password: 'pass123'
  })
});

const loginData = await loginResponse.json();
const token = loginData.data.token;

// Use token for authenticated requests
const villaResponse = await fetch('https://localhost:5001/api/villa/1', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
```

## Authorization Policies

Các policies đã được cấu hình:
- `AdminOnly`: Chỉ Admin (role: "ad")
- `UserOnly`: Chỉ User (role: "us")
- `EmployeeOnly`: Chỉ Employee (role: "em")
- `UserOrAdmin`: User hoặc Admin (role: "us", "ad")

Sử dụng trong Controller:
```csharp
[Authorize(Roles = "ad")] // Chỉ admin
[Authorize(Roles = "us,ad")] // User hoặc Admin
[Authorize(Policy = "AdminOnly")] // Sử dụng policy
```

## CORS

CORS đã được cấu hình để cho phép requests từ frontend. 

Hiện tại đang sử dụng policy `AllowAll` trong development. Để chuyển sang production:

1. Thay đổi trong `Program.cs`:
```csharp
app.UseCors("AllowSpecificOrigins"); // Thay vì "AllowAll"
```

2. Cập nhật origins trong CORS policy configuration.

## Notes

- Các endpoint không yêu cầu authentication được đánh dấu với `[AllowAnonymous]`
- Tất cả responses đều sử dụng format `ApiResponse<T>`
- Token có thời hạn (mặc định 60 phút), cần refresh hoặc login lại khi hết hạn
- Password hiện tại không được hash, nên hash password trong production

## Next Steps

Để hoàn thiện migration:
1. Thêm các Controllers khác (ServiceController, AccountController, etc.)
2. Thêm password hashing (BCrypt hoặc Identity PasswordHasher)
3. Thêm refresh token mechanism
4. Thêm validation và error handling tốt hơn
5. Thêm logging
6. Thêm unit tests

