# Welcome in Mentra AI

## to let the project work Fastapi
```bash
python -m venv .venv
.venv\Scripts\activate
cd src
pip install -r requirements.txt
```
### If Using UV 🚀
```bash
uv venv
./.venv/Scripts/activate
cd src
uv pip install -r requirements.txt
uvicorn main:app --port 8001 -- reload
```

## to use docker
```bash
cd ..
cd docker 
docker compose up -d --build
```

## to stop docker
```bash
cd ..
cd docker 
docker compose down
```

## to use langgraph studio 🤖
```bash
cd ..
cd src
langgraph dev
```

