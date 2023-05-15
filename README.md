# ZeroLogger
Service providing an api for logging.

## Authorization.

To access the log viewer, the user must provide an access token in the X-Token request header.
An authorized user can only view logs that he/she is allowed to view.
Permission to view logs is granted by linking the source to the user's account.
The right to create new users and manage their access has a built-in administrator account.

### Access token request.

```bash
curl --location 'http://<host>:57717/api/auth' \
--header 'Content-Type: application/json' \
--data '{
    "username": "loggerAdmin",
    "password": "loggerAdminPassword",
    "source": "Administration"
}'
```

A successful request will return a string containing the access token.