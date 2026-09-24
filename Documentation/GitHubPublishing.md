# Publish MobilityLab VR publicly on GitHub

These steps create a public repository without rewriting authorship or adding
generated Unity caches. They do not upload anything until the final `git push`.

## 1. Verify the folder

From Terminal, change into the extracted project directory and confirm the
expected root files:

```bash
cd ~/Downloads/MobilityLabVR
pwd
ls
```

You should see `Assets`, `Packages`, `ProjectSettings`, `Analysis`,
`Documentation`, `README.md`, and `LICENSE`.

Before publishing, run Unity and Python tests, add genuine screenshots, and
update `Documentation/Verification.md` with the observed results.

## 2. Create the empty GitHub repository

1. Sign in to [GitHub](https://github.com/).
2. Select **+ > New repository**.
3. Owner: `dhairyagamdha2006-bit`.
4. Repository name: `MobilityLabVR`.
5. Description: `VR micromobility safety and transportation simulator built with Unity, C#, OpenXR, and Python.`
6. Select **Public** so professors and recruiters can view it.
7. Do **not** add a README, `.gitignore`, or license on GitHub; this project
   already includes them.
8. Select **Create repository**.

## 3. Initialize and inspect Git locally

Use your own configured identity. These repository-local values match the
project owner information supplied for this portfolio; replace the email with a
GitHub-verified or GitHub noreply address if preferred.

```bash
cd ~/Downloads/MobilityLabVR
git init
git config user.name "Dhairya Gamdha"
git config user.email "dhairyagamdha2006@gmail.com"
git status --short
git check-ignore -v Library Temp Logs Obj Builds Analysis/output Analysis/models 2>/dev/null || true
```

If any generated folder or real telemetry appears under `git status`, stop and
fix `.gitignore` before staging.

## 4. Make the first commit

```bash
git add .
git status --short
git diff --cached --stat
git commit -m "Build MobilityLab VR research simulator"
git branch -M main
```

Read the staged file list before committing. It must not contain secrets,
participant telemetry, `Library`, `Temp`, `Logs`, `Obj`, builds, Python virtual
environments, or fabricated media.

## 5. Connect and push

```bash
git remote add origin https://github.com/dhairyagamdha2006-bit/MobilityLabVR.git
git remote -v
git push -u origin main
```

GitHub may open a browser or request a personal access token through its normal
credential helper. Never paste a password or token into a source file.

If `origin` already exists, inspect it before changing anything:

```bash
git remote -v
```

Only if it is definitely the wrong URL:

```bash
git remote set-url origin https://github.com/dhairyagamdha2006-bit/MobilityLabVR.git
git push -u origin main
```

## 6. Make the repository easy to discover

On the GitHub repository page:

1. Confirm the repository badge says **Public** (Settings > General > Danger
   Zone controls visibility if it was created private).
2. Add topics: `unity`, `csharp`, `virtual-reality`, `openxr`, `transportation`,
   `micromobility`, `simulation`, `research`, `python`, `machine-learning`.
3. Add the portfolio/demo URL in **About** when available.
4. Pin the repository on the GitHub profile.
5. Create a release only after Unity verification. Tag it, for example:

```bash
git tag -a v0.1.0 -m "MobilityLab VR verified portfolio release"
git push origin v0.1.0
```

Do not attach the Unity `Library` folder. Attach a tested desktop build as a
separate release artifact only if its platform, Unity version, and test status
are documented.

## 7. Future updates

```bash
git status --short
git add <reviewed-files>
git commit -m "Describe the verified change"
git push
```

Use selective `git add` for later work, preserve other contributors’ commit
authorship, and never rewrite shared history merely to change attribution.
