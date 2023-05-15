# ZeroLogger
Service providing an api for logging.

## Authorization

To access the log viewer, the user must provide an access token in the X-Token request header.
An authorized user can only view logs that he/she is allowed to view.
Permission to view logs is granted by linking the source to the user's account.
The right to create new users and manage their access has a built-in administrator account.

### Access token request

```bash
curl --location 'http://<host>:57717/api/auth' \
--header 'Content-Type: application/json' \
--data '{
    "username": "loggerAdmin",
    "password": "loggerAdminPassword"
}'
```

A successful request will return a string containing the access token.

### User creation

```bash
curl --location 'http://<host>:57717/api/user' \
--header 'X-Token: my_access_token' \
--header 'Content-Type: application/json' \
--data '{
    "UserName":"username",
    "Password":"password",
    "Sources": ["MyApp1", "MyApp2", "MyApp3"]
}'
```

The password is stored as a hash:
```csharp
private static byte[] Hash(string pwd)
{
    var pbkdf2 = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(pwd), salt, 17813);
    return pbkdf2.GetBytes(20);
}
```

### Getting a list of users

```bash
curl --location 'http://<host>:57717/api/user' --header 'X-Token: my_access_token'
```

### Blocking the user
```bash
curl --location --request DELETE 'http://<host>:57717/api/user/1' --header 'X-Token: my_access_token'
```

### Unlocking a user
```bash
curl --location --request PUT 'http://<host>:57717/api/user/1' --header 'X-Token: my_access_token'
```

### Adding a source to the user's account
```bash
curl --location --request PUT 'http://<host>:57717/api/user/source' \
--header 'X-Token: my_access_token' \
--header 'Content-Type: application/json' \
--data '{
    "id": 1,
    "source": "app1"
}'
```

### Remove a source from the user's account
```bash
curl --location --request DELETE 'http://<host>:57717/api/user/source' \
--header 'X-Token: my_access_token' \
--header 'Content-Type: application/json' \
--data '{
    "id": 1,
    "source": "app1"
}'
```


## Working with logs

### Saving a log entry
```bash
curl --location 'http://<host>:57717/api/log' \
--header 'X-Token: my_access_token' \
--header 'Content-Type: application/json' \
--data '{
    "source": "MyApp1",
    "tag": "module1",
    "text": "Nothing happened today",
    "logLevel": "critical"
}'
```

### Getting the record by identifier
```bash
curl --location 'http://<host>:57717/api/log/1' --header 'X-Token: my_access_token'
```

### Search for records by filter
```bash
curl --location 'http://<host>:57717/api/search' \
--header 'X-Token: my_access_token' \
--header 'Content-Type: application/json' \
--data '{
    "id": null,
    "tag": null,
    "source": "MyApp1",
    "text": null,
    "logLevel": null,
    "start": "2023-03-01 17:00:00",
    "end": null
}'
```

### Get the most recent records for the period
```bash
curl --location 'http://<host>:57717/api/last?seconds=1720' --header 'X-Token: my_access_token'
```

### Obtaining the list of sources available to the user
```bash
curl --location 'http://<host>:57717/api/sources' --header 'X-Token: my_access_token'
```

### Getting the list of logging levels
```bash
curl --location 'http://<host>:57717/api/loglevels' --header 'X-Token: my_access_token'
```