# Welcome in Mentra AI

<div align="center">

<h2> ⚠️ you must update the base_url in the ```studio_app.py```, ```main.py``` file</h2>

<h2> and you must intializae .env file</h2>
<h3> In ```.env``` of [```src``` , ```docker```] folder</h3>

</div>

---

# 🤖 To let the project work
## In Fastapi
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

