# github-backup

Back up your GitHub repositories locally, including their full Git history.

## Install

Download the appropriate binary from the GitHub Releases page.

* **Windows:** Add the executable to your `PATH`.
* **macOS / Linux:** Make it executable:

```bash
chmod +x ghb
```

## Usage

```bash
ghb --help
```

### Authentication

Tokens are resolved in this order:

1. `--token`
2. `GITHUB_BACKUP_TOKEN`
3. GitHub device flow

Device-flow tokens are stored temporarily while valid.

### Scheduled backups

Use `--interval` to run backups periodically.

## Contributing

Issues and pull requests are welcome.
