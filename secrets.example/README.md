Copy these files to a local `secrets/` directory before running Docker Compose:

```bash
mkdir -p secrets
cp secrets.example/* secrets/
```

Docker Compose mounts each file at `/run/secrets/<name>` inside containers. Do not commit the `secrets/` directory.
