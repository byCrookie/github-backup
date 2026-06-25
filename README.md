# github-backup

Back up your GitHub account locally using GitHub’s migration exports.

## Install

Download the binary for your platform from the GitHub Releases page.

* **Windows:** Add the executable to your `PATH`.
* **macOS / Linux:** Make it executable:

```bash
chmod +x ghb
```

## Usage

```bash
ghb --help
```

## Authentication

Tokens are resolved in this order:

1. `--token`
2. `GITHUB_BACKUP_TOKEN`
3. GitHub device flow

## Scheduled backups

Use `--interval` to run backups regularly.

## Contributing

Issues and pull requests are welcome.
