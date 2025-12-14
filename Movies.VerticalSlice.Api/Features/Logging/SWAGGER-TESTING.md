# Testing API with Swagger

## How to Authenticate in Swagger

### Step 1: Register or Login

1. **Register a new user** (if you don't have one):
   - Find the `POST /api/users/register` endpoint (no padlock - doesn't require auth)
   - Click "Try it out"
   - Enter:
     ```json
     {
       "userName": "testuser",
       "email": "test@example.com",
       "password": "YourPassword123!"
     }
     ```
   - Click "Execute"

2. **Login to get a JWT token**:
   - Find the `POST /api/users/login` endpoint (no padlock - doesn't require auth)
   - Click "Try it out"
   - Enter:
     ```json
     {
       "email": "test@example.com",
       "password": "YourPassword123!"
     }
     ```
   - Click "Execute"
   - **Copy the `token` value from the response**

### Step 2: Authorize in Swagger

1. Click the **"Authorize"** button at the top right of the Swagger page (or click any padlock icon)
2. In the "Value" field, enter: `Bearer YOUR_TOKEN_HERE`
   - Example: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
   - **Important**: Include the word "Bearer" followed by a space, then your token
3. Click "Authorize"
4. Click "Close"

### Step 3: Test Protected Endpoints

Now you can test any endpoint that has a padlock icon:
- `GET /api/movies` - Get all movies
- `POST /api/movies` - Create a movie
- `DELETE /api/movies/{id}` - Delete a movie
- etc.

## Visual Indicators in Swagger

### ?? No Padlock = Public Endpoint
These endpoints don't require authentication:
- `POST /api/users/register` - Register new user
- `POST /api/users/login` - Login
- `POST /api/logs` - Create log (anonymous logging allowed)

### ?? Padlock = Protected Endpoint
These endpoints require authentication (JWT token):
- All movie CRUD operations
- All rating operations
- User profile operations
- `GET /api/logs` - View logs

## Testing User Logging in Database

1. **Login** using the login endpoint to get a token
2. **Authorize** in Swagger using your token
3. **Make any API request** (e.g., GET /api/movies)
4. **Check the database**:
   ```sql
   SELECT TOP 10 
       Timestamp,
       Message,
       UserId,
       UserName,
       RequestPath
   FROM ApplicationLogs
   WHERE Category = 'ApiRequest'
   ORDER BY Timestamp DESC
   ```

You should now see:
- ? Your **UserId** (GUID from JWT token)
- ? Your **UserName** (e.g., "testuser")
- ? Request details

## Troubleshooting

### "Unauthorized" Response
- Make sure you copied the entire token
- Make sure you included "Bearer " before the token
- Check if your token has expired (default: 60 minutes)
- Try logging in again to get a fresh token

### User Info Not Logged
- Make sure you're **authenticated** when making the request
- Check that the middleware is placed **after** `UseAuthentication()` in Program.cs
- Look for debug logs in the output window showing "Authenticated user - UserId: ..."

### Can't See Which Endpoints Require Auth
- Endpoints with a ?? padlock icon require authentication
- Endpoints without a padlock are public
- After authenticating, all padlocks should show as "Authorized"
