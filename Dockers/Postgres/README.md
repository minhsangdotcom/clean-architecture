
Create 2 folders to storage data in docker container

```
mkdir pgadmin_data && mkdir db_data

```

Run this command to fix "permission dined" error

```
sudo chown -R 5050:5050 pgadmin_data
```