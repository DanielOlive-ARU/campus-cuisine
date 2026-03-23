# Backend Setup

Run these commands from the `backend/` directory.

## Install
```bash
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
cp .env.example .env
```

On Windows PowerShell, activate the environment with:
```powershell
.\.venv\Scripts\Activate.ps1
```

## Run
```bash
uvicorn app.main:app --reload
```

## Test
```bash
pytest
```
